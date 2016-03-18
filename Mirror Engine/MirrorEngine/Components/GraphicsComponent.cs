/**
 * @file GraphicsComponent.cs
 * @author SIGGD, Purdue ACM <siggd.purdue@gmail.com>
 * @version 2.0
 */

using System;
using System.Collections.Generic;
using Tao.Sdl;
using Tao.OpenGl;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace Engine
{
    /*
     * Manages the camera and window, performs the standard draw routine, contains the GUI, 
     * manages lighting and fading, and provides rudimentary drawing functions
     */
    public class GraphicsComponent : Component
    {
        //Window
        private IntPtr vidSurface;  //Pointer to current video surface
        private IntPtr pixFmt;      //Pointer to current pixel format
        public const int DEFAULTWIDTH = 700;
        public const int DEFAULTHEIGHT = 400;
        public bool fullscreen;                         //Whether or not the window is fullscreen
        public int width { get; private set; }          //width of the screen
        public int height { get; private set; }         //height of the screen
        public int windowWidth { get; private set; }    //width of the window when not fullscreen
        public int windowHeight { get; private set; }   //height of the window when not fullscreen
        
        //Screen
        public Camera camera;                       //The camera that views the world
        public GUI gui;                             //The gui to be displayed
        
        //Fading
        public Color tint = new Color(0, 0, 0, 0);  //Global tint, with opacity
        private int fadebegin;                      //Number of ms since beginning of fade
        private int fadeend;                        //Number of ms at end of fade (zero if not fading)
        public delegate void Action();              //Action to fire when fade is over
        private Action fadedone;                    //Called when the fade finishes

        private Texture2D lastTex;  //The last texture to be rendered

        /**
        * Constructor. Initializes openGL and creates a new window.
        *
        * @param engine the engine
        * @param width standard width of the window
        * @param height standard height of the window
        * @param fullscreen whether to start in fullscreen
        */
        public GraphicsComponent(MirrorEngine engine, int windowWidth = DEFAULTWIDTH, int windowHeight = DEFAULTHEIGHT, bool fullscreen = false)
            : base(engine)
        {
            this.windowWidth = windowWidth;
            this.windowHeight = windowHeight;
            this.fullscreen = fullscreen;
        }


        /// <summary>
        /// Sets widow icon, with transparency where color matches sample point if
        /// mask is enabled. According to SDL documentation icon should be set
        /// before first call to SDL_SetVideoMode which occurs during the openGL
        /// initialization procedure when initializing GraphicsComponent.
        /// 
        /// Image must be a 32x32 256 color bmp. In gimp this is is achieved by 
        /// setting mode to indexed.
        /// </summary>
        /// <param name="file">Relative path from content folder</param>
        /// <param name="makeMask">Rather or not to generate a mask</param>
        /// <param name="mSample_x">X coordinate referenced to determine alpha</param>
        /// <param name="mSample_y">Y coordinate referenced to determine alpha</param>
        public void SetIcon(string file, bool makeMask=false, int mSample_x=0, int mSample_y=0)
        {
            // initialize SDL System (SDL must be started before can set WM icon, but normally would
            // not be before initialize is called)
            initializeSDLSystem();

            Debug.WriteLine("Setup Game Icon (should be before first call to SDL_SetVideoMode");
            //Set the icon of the game window
            IntPtr icon = Sdl.SDL_LoadBMP(file);
            byte[] mask = null;

            if( makeMask ) {
                // Get image data from internal native struct to generate mask
                Sdl.SDL_Surface img = (Sdl.SDL_Surface)Marshal.PtrToStructure(icon, typeof(Sdl.SDL_Surface));

                int len = img.h * img.w;
                byte[] pixels = new byte[len];

                Marshal.Copy(img.pixels, pixels, 0, len);

                // reference used to determine if pixel is alpha, scanline for pixels
                // is img.w
                byte alpha_ref = pixels[mSample_x + mSample_y * img.w];

                // Generate mask, 8 pixels packed per byte most significant digit
                // leftmost pixel. Pixel is one byte (image indexed, 256 colored)
                mask = new byte[len / 8];
                for (int i = 0; i < len; i++)
                {
                    if (pixels[i] != pixels[0])
                    {
                        mask[i / 8] |= (byte)(1 << (7 - (i % 8)));
                    }
                }
            }

            Sdl.SDL_WM_SetIcon(icon, mask);
        }


        /**
        * Initializes the video subsystem, camera, gui, and opengl
        */
        public override void initialize()
        {
            // Initialize video subsystem
            initializeSDLSystem();

            //Initialize the camera
            camera = new Camera(this, new Vector2(0, 0), 300);
            camera.velocity = new Vector2(0, 0);
            camera.position = new Vector2(0, 0);

            //Initialize the gui
            if (gui != null) {
                engine.inputComponent.setContext(typeof(InputComponent.GUIBindings), gui);
                gui.initialize();
            }

            //Initialize OpenGL
            initializeOpenGL();
        }

        /*
         * Loads the gui's content
         */
        public override void loadContent()
        {
            gui.loadContent();
        }

        private bool sdl_initialized = false;
        protected void initializeSDLSystem()
        {
            if (!sdl_initialized)
            {
                if (Sdl.SDL_InitSubSystem(Sdl.SDL_INIT_VIDEO) == -1)
                    throw new Exception("Could not initialize SDL Video: " + Sdl.SDL_GetError());
            }
        }

        /*
         * Creates a window, initializes OpenGl settings, and updates the current height and width of the window.
         */
        private void initializeOpenGL()
        {
            //Center the window
            Sdl.SDL_putenv("SDL_VIDEO_CENTERED=center");

            //Create the window
			Debug.WriteLine("SDL_SetVideoMode Called");
            vidSurface = Sdl.SDL_SetVideoMode(((fullscreen) ? 1 : windowWidth), ((fullscreen) ? 0 : windowHeight), 32, Sdl.SDL_OPENGL | Sdl.SDL_DOUBLEBUF | Sdl.SDL_HWSURFACE | ((fullscreen) ? Sdl.SDL_FULLSCREEN : 0));
            if (vidSurface == null) throw new Exception("Error, could not set video mode.");

            //Marshal in surface to obtain info
            Sdl.SDL_Surface managedSurface = (Sdl.SDL_Surface)Marshal.PtrToStructure(vidSurface, typeof(Sdl.SDL_Surface));
            pixFmt = managedSurface.format;

            //Current Dimensions of the window.
            this.width = (short)managedSurface.w;
            this.height = (short)managedSurface.h;
            camera.setView();

            //Settings
            Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_DOUBLEBUFFER, 1);
            Gl.glEnable(Gl.GL_TEXTURE_2D);
            Gl.glClearColor(0, 0, 0, 1);
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT);
            Gl.glViewport(0, 0, width, height);
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            Gl.glOrtho(0.0f, width, height, 0.0f, -1.0f, 1.0f);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
            Gl.glEnable(Gl.GL_BLEND);
            Gl.glDisable(Gl.GL_DEPTH_TEST);
            Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
        }

        /**
         * Toggles fullscreen, reinitializes opengl, and flushes textures
         */
        public void toggleFullScreen()
        {
            fullscreen = !fullscreen;

            //Initialize window
            Sdl.SDL_FreeSurface(vidSurface);
            initializeOpenGL();
            //Still has bugs with unregistered textures and square view (not as square anymore, but still blank on sides)

            //Flush textures
            engine.resourceComponent.flush<Texture2D>();
            Animation.flushAnimations(); //Work around for animations when toggling fullscreen.
        }

        /**
        * Clears the screen
        */
        protected virtual void clear()
        {
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT);
        }

        /**
         * Sets the initial GUI.
         *
         * @param gui The GUI to set
         */
        public void setInitialGUI(GUI gui)
        {
            if (this.gui != null) throw new Exception("Cannot set initial GUI when there is already a GUI.");
            
            this.gui = gui;
        }

        /**
         * Switches the currently active GUI. ONLY USE THIS FUNCTION AFTER Initialize() has been called.
         * 
         * @param gui The GUI to switch to
         */
        public void switchGUI(GUI gui)
        {
            engine.inputComponent.setContext(typeof(InputComponent.GUIBindings), gui);
            this.gui = gui;
        }

        /**
        * Set up a fade to the current tint color, lasting "duration" milliseconds, and firing delegate "done" afterward
        *
        * @param duration the duration of the fade in ms
        * @param done the action to perform when done fading
        */
        public void fadeTint(int duration, Action done = null) //Should take a tint, not use a global tint
        {
            fadebegin = Sdl.SDL_GetTicks();
            fadeend = fadebegin + duration;
            fadedone = done;
        }

        //Sets the view to the camera
        private void viewCamera()
        {
            RectangleF viewRect = camera.viewRect;
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
            Gl.glTranslatef(-viewRect.topLeft.x * camera.scale, -viewRect.topLeft.y * camera.scale, 0);
            Gl.glScalef(camera.scale, camera.scale, 1f);
        }

        //Loads the identity
        private void loadIdentity()
        {
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
        }

        /**
        * Updates the graphics component.
        * Updates fade
        */
        protected virtual void update()
        {
            //Update fade
            if (fadeend != 0)
            {
                int curTime = Sdl.SDL_GetTicks();

                //Alpha is the proportion of fade time elapsed to total fade time
                tint.a = (float)(curTime - fadebegin) / (float)(fadeend - fadebegin);

                //Handle fade end
                if (curTime >= fadeend)
                {
                    fadeend = 0;  //End fading   0 should be a constant
                    if (fadedone != null) fadedone();
                }
            }
        }

        //////////////////////////////////////////////////////////////
        // DRAW ROUTINE
        //////////////////////////////////////////////////////////////

        /**
        * Draws the world, editor, and gui
        */
        public virtual void draw()
        {
            ///Update
            update();
            clear();

            //Draw world
            if (engine.world != null)
            {
                //Sort actors
                engine.world.sortActors();

                //Set view to camera's
                viewCamera();

                //Draw the world
                drawTiles(engine.world);
                drawSprites(engine.world);
                loadIdentity();

                //Draw tint
                if (tint.a != 0) drawRect(0, 0, camera.screenWidth, camera.screenHeight, tint);
            }

            //Draw editor
            if (engine.editorComponent.isActive) drawEditorOverlays();

            //Draw GUI
            if (gui != null) gui.draw();

            Sdl.SDL_GL_SwapBuffers();
        }

        /**
        * Draws a world's tiles
        *
        * @param world the world to draw
        */
        protected virtual void drawTiles(World world)
        {
            RectangleF viewRect = camera.viewRect;

            for (int x = 0; x < world.width; x++)
            {
                for (int y = 0; y < world.height; y++)
                {
                    Tile t = world.tileArray[x, y];
                    if (!RectangleF.intersects(t.imageRect, viewRect)) continue;

                    drawTile(t, viewRect);

                    if (engine.editorComponent.isActive)
                    {
                        //Nonstandard overlay
                        if (engine.editorComponent.showOverlay && t.tileData.isNonstandard())
                            drawRect(t.x + 3, t.y + 3, Tile.size - 5, Tile.size - 5, new Color(1, 0, 1, .3f));

                        //Flags
                        if (engine.editorComponent.showOverlay && t.solidity)
                            drawTex(engine.editorComponent.editorGui.solid, t.x, t.y, Color.WHITE);
                        if (engine.editorComponent.showOverlay && t.opacityFlip)
                            drawTex(engine.editorComponent.editorGui.opaque, t.x + Tile.size / 2, t.y + Tile.size / 2, Color.WHITE);

                        //Grid
                        if (engine.editorComponent.showGrid && t.texture.key != "")
                        {
                            if (t.imageWidth > Tile.size || t.imageHeight > Tile.size)
                                drawTex(engine.editorComponent.editorGui.grid, t.x, t.y, new Color(0, 1, 1));
                            else
                                drawTex(engine.editorComponent.editorGui.grid, t.x, t.y, new Color(1f, 0, .9f));
                        }
                    }
                }
            }
        }

        /*
         * Draws a single tile, incrementally if needed
         * 
         * @param tile the tile to draw
         * @param viewRect the view to consider
         */
        protected virtual void drawTile(Tile tile, RectangleF viewRect)
        {
            if (tile.texture.key != "")
            {
                //Get the size of the tile in increments of Tile.size
                int tilesWide = (int)((float)tile.imageWidth / Tile.size);
                int tilesHigh = (int)((float)tile.imageHeight / Tile.size);
                int fTilesWide = tilesWide;
                int fTilesHigh = tilesHigh;
                if (tile.xIndex + fTilesWide > tile.world.width) fTilesWide -= ((tile.xIndex + fTilesWide) - tile.world.width);
                if (tile.yIndex + fTilesHigh > tile.world.height) fTilesHigh -= ((tile.yIndex + fTilesHigh) - tile.world.height);

                //Draw the single tile by increments of Tile.size, in order to draw larger-than-a-tile tiles
                for (int x = 0; x < fTilesWide; x++)
                {
                    for (int y = 0; y < fTilesHigh; y++)
                    {
                        //Merge the lighting of the current tile and the incremental tile
                        Tile t = tile.world.tileArray[tile.xIndex + x, tile.yIndex + y];
                        if (t != tile && t.texture.key != "") continue;
                        if (t.color.a < 0.0001f) continue;

                        //Final tint
                        Color tint = t.getFinalColor();//glowOutput; //getfinalcolor

                        //Get the specific bounds for this increment
                        RectangleF bounds = new RectangleF((float)x * Tile.size / tile.imageWidth, (float)y * Tile.size / tile.imageHeight, (float)Tile.size / tile.imageWidth, (float)Tile.size / tile.imageHeight);

                        Color tint1 = Color.Avg((t.left == null) ? tint : t.left.getFinalColor(), (t.up == null) ? tint : t.up.getFinalColor());
                        Color tint2 = Color.Avg((t.right == null) ? tint : t.right.getFinalColor(), (t.up == null) ? tint : t.up.getFinalColor());
                        Color tint3 = Color.Avg((t.left == null) ? tint : t.left.getFinalColor(), (t.down == null) ? tint : t.down.getFinalColor());
                        Color tint4 = Color.Avg((t.right == null) ? tint : t.right.getFinalColor(), (t.down == null) ? tint : t.down.getFinalColor());

                        drawTex(tile.texture, t.x, t.y, Tile.size, Tile.size, Color.Avg(tint, tint1), Color.Avg(tint, tint2), Color.Avg(tint, tint3), Color.Avg(tint, tint4), bounds);
                    }
                }
            }
        }

        /**
        * Draws a world's sprites
        *
        * @param world the world to draw
        */
        protected virtual void drawSprites(World world)
        {
            foreach (Actor a in world.actors)
            {
                //Get actor's texture
                Handle tex = a.sprite;
                if (tex == null) continue;
                Texture2D texture = tex.getResource<Texture2D>();
                if (texture == null) return;

                //Get the tile actor is on
                Vector2 spritePos = a.position + a.world2model;
                Vector2 aMiddle = new Vector2(texture.width / 2, texture.height / 2);
                Tile t = world.getTileAt(spritePos + aMiddle);

                //If the tile actor is on isn't visible, don't draw actor
                if (t == null || t.color.a < 0.00001f) t = world.getTileAt(spritePos);
                if (t == null || t.color.a + world.ambientLight < 0.00001f) continue;

                //Merge the actor's lighting with the tile
                a.color += (t.getFinalColor() + (a.color * -1f)) * a.colorChangeRate;
                a.color += world.ambientLight;
                Color actorTint = a.tint * a.color;

                //Default drawing
                if (a.defaultDraw)
                {
                    drawTex(tex, (int)(spritePos.x + a.xoffset), (int)(spritePos.y + a.yoffset), actorTint);
                }

                //Custom drawing (if any)
                if (a.customDraw != null)
                {
                    a.customDraw(a);
                }

                //Editor erasor selector draw
                if (engine.editorComponent.isActive && engine.editorComponent.eraseTool.active)
                {
                    engine.graphicsComponent.drawRect((int)(a.position.x - 2), (int)(a.position.y + (a.diameter / 4)), 4, 4, new Color(1f, 0, 1f));
                }
            }
        }

        //Draws any overlays the editor might require
        protected virtual void drawEditorOverlays()
        {
            //Border
            World world = engine.world;
            drawBorder(new Color(0, 1, 0), 2f, new Vector2[] 
                { 
                    camera.world2Screen(new Vector2(0, 0)), 
                    camera.world2Screen(new Vector2(world.width * Tile.size, 0)), 
                    camera.world2Screen(new Vector2(world.width*Tile.size, world.height*Tile.size)), 
                    camera.world2Screen(new Vector2(0, world.height*Tile.size)), 
                    camera.world2Screen(new Vector2(0, 0)),
                });

            //Selection tiles
            if (engine.editorComponent.activeTool is SelectionTool)
            {
                engine.editorComponent.selectionTool.drawTiles();
            }

            //Selection
            if (engine.editorComponent.selectionTool.active
                || (engine.editorComponent.fillTool.active && !engine.editorComponent.fillTool.selectionBox.isDown))
            {
                engine.editorComponent.selectionTool.drawSelection();
            }

            //Floating pic
            if (engine.editorComponent.activeTool != null
                && engine.editorComponent.activeTool.floatingPic != null)
            {
                engine.editorComponent.activeTool.floatingPic.draw();
            }
        }

        //////////////////////////////////////////////////////////////
        // RUDIMENTARY DRAW FUNCTIONS
        //////////////////////////////////////////////////////////////

        //Draws text to the screen
        public void drawText(string text, int x, int y, Handle font, Color color, int ptSize = 12)
        {
            // Get the font handle at the desired point size
            IntPtr handle;
            try
            {
                handle = font.getResource<Font>().getFontHandle(ptSize);
            }
            catch (Exception e)
            {
                Trace.WriteLine("GraphicsComponent.drawText(string,Font,Color,int): Could not draw font: " + e.Message);
                return;
            }

            // Render the text to a SDL surface
            IntPtr surf = SdlTtf.TTF_RenderUNICODE_Blended(handle, text, color.sdlColor);
            if (surf == IntPtr.Zero)
            {
                Trace.WriteLine("GraphicsComponent.drawText(string,Font,Color,int): Could not render font: " + SdlTtf.TTF_GetError());
            }

            // Draw the Texture2D
            Texture2D tempTex = new Texture2D(surf);
            drawTex(tempTex, x, y, Color.WHITE);
            Sdl.SDL_FreeSurface(surf);
        }

        //Draws a texture
        public void drawTex(Handle tex, int x, int y, Color tint, RectangleF bounds = null)
        { drawTex(tex, x, y, tint, tint, tint, tint, bounds); }
        internal void drawTex(Texture2D tex, int x, int y, Color tint, RectangleF bounds = null)
        { drawTex(tex, x, y, tint, tint, tint, tint, bounds); }

        //Draws a texture, given specific corner colors
        public void drawTex(Handle tex, int x, int y, Color topLeft, Color topRight, Color bottomLeft, Color bottomRight, RectangleF bounds = null)
        {
            if (tex == null) return;
            Texture2D texture = tex.getResource<Texture2D>();
            if (texture == null) return;
            drawTex(texture, x, y, texture.width, texture.height, topLeft, topRight, bottomLeft, bottomRight, bounds);
        }
        internal void drawTex(Texture2D tex, int x, int y, Color topLeft, Color topRight, Color bottomLeft, Color bottomRight, RectangleF bounds = null)
        { drawTex(tex, x, y, tex.width, tex.height, topLeft, topRight, bottomLeft, bottomRight, bounds); }

        //Draws a texture, given a specific draw size
        public void drawTex(Handle tex, int x, int y, int w, int h, Color tint, RectangleF bounds = null)
        { drawTex(tex, x, y, w, h, tint, tint, tint, tint, bounds); }
        internal void drawTex(Texture2D tex, int x, int y, int w, int h, Color tint, RectangleF bounds = null)
        { drawTex(tex, x, y, w, h, tint, tint, tint, tint, bounds); }

        //Draws a texture, given a specific draw size and corner colors
        public void drawTex(Handle tex, int x, int y, int w, int h, Color topLeft, Color topRight, Color bottomLeft, Color bottomRight, RectangleF bounds = null)
        {
            if (tex == null) return;
            drawTex(tex.getResource<Texture2D>(), x, y, w, h, topLeft, topRight, bottomLeft, bottomRight, bounds);
        }
        internal void drawTex(Texture2D tex, int x, int y, int w, int h, Color topLeft, Color topRight, Color bottomLeft, Color bottomRight, RectangleF bounds = null)
        {
            if (tex == null) return;

            //Check if texture already bound
            if (tex != lastTex) {
                Gl.glBindTexture(Gl.GL_TEXTURE_2D, tex.handle);
                lastTex = tex;
            }

            //Get final bounds
            RectangleF finalBounds = new RectangleF(new Vector2(0,0), new Vector2(1,1));
            if (bounds != null) finalBounds = bounds;
            
            //Draw rect
            Gl.glBegin(Gl.GL_QUADS);
                //Top-left vertex
                Gl.glColor4f(topLeft.r, topLeft.g, topLeft.b, topLeft.a);
                Gl.glTexCoord2f(finalBounds.left, finalBounds.top);
                Gl.glVertex3f((float)Math.Round((double)x), (float)Math.Round((double)y), 0);

                //Top-right vertex
                Gl.glColor4f(topRight.r, topRight.g, topRight.b, topRight.a);
                Gl.glTexCoord2f(finalBounds.right, finalBounds.top);
                Gl.glVertex3f((float)Math.Round((double)x+w), (float)Math.Round((double)y), 0);

                //Bottom-right vertex
                Gl.glColor4f(bottomRight.r, bottomRight.g, bottomRight.b, bottomRight.a);
                Gl.glTexCoord2f(finalBounds.right, finalBounds.bottom);
                Gl.glVertex3f((float)Math.Round((double)x+w), (float)Math.Round((double)y+h), 0);

                //Bottom-left vertex
                Gl.glColor4f(bottomLeft.r, bottomLeft.g, bottomLeft.b, bottomLeft.a);
                Gl.glTexCoord2f(finalBounds.left, finalBounds.bottom);
                Gl.glVertex3f((float)Math.Round((double)x), (float)Math.Round((double)y+h), 0);
            Gl.glEnd();
        }

        //Draws a rectangle, given ltrb notation in the form of two vectors
        public void drawRect(Vector2 topLeft, Vector2 bottomRight, Color col)
        {
            Gl.glDisable(Gl.GL_TEXTURE_2D);

            Gl.glBegin(Gl.GL_QUADS);
            //Top-left vertex
            Gl.glColor4f(col.r, col.g, col.b, col.a);
            Gl.glVertex3f(topLeft.x, topLeft.y, 0);
            Gl.glVertex3f(bottomRight.x, topLeft.y, 0);
            Gl.glVertex3f(bottomRight.x, bottomRight.y, 0);
            Gl.glVertex3f(topLeft.x, bottomRight.y, 0);
            Gl.glEnd();

            Gl.glEnable(Gl.GL_TEXTURE_2D);
        }

        //Draws a rectangle, given position-size notation
        public void drawRect(int x, int y, int w, int h, Color col)
        {
            Gl.glDisable(Gl.GL_TEXTURE_2D);

            Gl.glBegin(Gl.GL_QUADS);
            //Top-left vertex
            Gl.glColor4f(col.r, col.g, col.b, col.a);
            Gl.glVertex3f(x, y, 0);
            Gl.glVertex3f(x + w, y, 0);
            Gl.glVertex3f(x + w, y + h, 0);
            Gl.glVertex3f(x, y + h, 0);
            Gl.glEnd();

            Gl.glEnable(Gl.GL_TEXTURE_2D);
        }

        //Draws the border of a shape, given an array of its corners
        public void drawBorder(Color col, float width = 1f, params Vector2[] vtx)
        {
            Gl.glDisable(Gl.GL_TEXTURE_2D);

            Gl.glLineWidth(width);
            Gl.glBegin(Gl.GL_LINE_LOOP);
            Gl.glColor4f(col.r, col.g, col.b, col.a);
            foreach (Vector2 v in vtx)
            {
                Gl.glVertex3f(v.x, v.y, 0);
            }
            Gl.glEnd();
            Gl.glLineWidth(1f);

            Gl.glEnable(Gl.GL_TEXTURE_2D);
        }

        //Draws a set of lines, given pairs of coordinates in an array
        public void drawLines(Color col, float width = 1f, params Vector2[] vtx)
        {
            Gl.glDisable(Gl.GL_TEXTURE_2D);

            Gl.glBegin(Gl.GL_LINES);
                Gl.glLineWidth(width);
                Gl.glColor4f(col.r, col.g, col.b, col.a);
                foreach (Vector2 v in vtx)
                {
                    Gl.glVertex3f(v.x, v.y, 0);
                }
            Gl.glEnd();

            Gl.glEnable(Gl.GL_TEXTURE_2D);
        }
    }
}