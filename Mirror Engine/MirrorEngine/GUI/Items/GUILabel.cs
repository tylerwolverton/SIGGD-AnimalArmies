/**
 * @file GUILabel.cs
 * @author SIGGD, Purdue ACM <siggd.purdue@gmail.com>
 * @version 2.0
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Tao.Sdl;

namespace Engine
{
    /**
     * Represents an element in the GUI with text, and image, or both
     */
    public class GUILabel : GUIItem
    {
        public Color tintTex = Color.WHITE; //The color of the label

        private Vector2 _pos = Vector2.Zero;
        public override Vector2 pos // Position, in screen coordinates
        {
            get {
                return _pos;
            }

            set {
                _pos = value;
            }
        }

        private Vector2 size_ = Vector2.Zero;   // The set size, if any
        public override Vector2 size {          // The size, autocalculated if not set
            get
            {
                // Return the set size if one has been set
                if (size_ != Vector2.Zero)
                    return size_;
                
                Vector2 textSize = Vector2.Zero;
                Vector2 imgSize = Vector2.Zero;

                ResourceComponent rc = gui.graphics.engine.resourceComponent;
                // Size the text
                if (text != null) {
                    if (font == null)
                    {
                        font = rc.get(gui.defaultFontPath);
                    }
                    textSize = Font.calcTextSize(font, text, fontSize);
                }

                // Size the image
                if (texture != null) {
                    Texture2D bgTex = texture.getResource<Texture2D>();
                    imgSize = new Vector2(bgTex.width, bgTex.height);
                }

                // Return the largest of the sizes
                return Vector2.max(textSize, imgSize);
            }

            set
            {
                size_ = value;
            }
        }

        private string _text;
        public string text      //Text to display
        {
            get
            {
                return _text;
            }
            set
            {
                if (_text != value) upToDate = false;
                _text = value;
            }
        }
        
        public Color bgColor;        // Background color, if any
        public Color textColor;      // Text color
        public Vector2 textOffset = Vector2.Zero;   // Position of the text relative to pos
        
        private Handle _font;
        public Handle font
        {
            get
            {
                return _font;
            }
            set
            {
                if (_font != null && _font.key != value.key) upToDate = false;
                _font = value;
            }
        }

        private int _fontsize;
        public int fontSize     //Pointsize of the font
        {
            get
            {
                return _fontsize;
            }
            set
            {
                if (_fontsize != value) upToDate = false;
                _fontsize = value;
            }
        }

        private bool upToDate = false;
        private Texture2D _textTex;
        internal Texture2D textTex
        {
            get
            {
                //If text texture hasn't been created, then create it
                //hasn't provided support for changing the text during runtime
                if (_textTex == null || !upToDate)
                {
                    // Get the font handle at the desired point size
                    IntPtr handle;
                    try
                    {
                        handle = font.getResource<Font>().getFontHandle(fontSize);
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine("GraphicsComponent.drawText(string,Font,Color,int): Could not draw font: " + e.Message);
                        return null;
                    }

                    // Render the text to a SDL surface
                    IntPtr surf = SdlTtf.TTF_RenderUNICODE_Blended(handle, text, Color.WHITE.sdlColor);
                    if (surf == IntPtr.Zero)
                    {
                        Trace.WriteLine("GraphicsComponent.drawText(string,Font,Color,int): Could not render font: " + SdlTtf.TTF_GetError());
                    }

                    //Draw the Texture2D
                    _textTex = new Texture2D(surf);
                    Sdl.SDL_FreeSurface(surf);
                    upToDate = true;
                }

                return _textTex;
            }

            set
            {
                _textTex = value;
            }
        }


        //public Vector2 bgImageSize;
        //public Animation bgAnim;

        public bool stretchImage = true; //Stretch the image to fit the size

        /* Constructs the GUILabel.
         *
         * @param gui The gui to draw on
         * @param texture The image for the label
         * @param text The text to draw
         * @param font A handle to the font to use
         * @param fontSize The font size, in points
         */
        public GUILabel(GUI gui, Handle texture = null, string text = null, Handle font = null, int fontSize = 12)
            : base(gui)
        {

            this.texture = texture;
            this.bgColor = new Color(0, 0, 0, 0);
            this.text = text;
            this.font = font;
            this.textColor = Color.WHITE;
            this.fontSize = fontSize;
            this.textOffset = new Vector2(0, 0);   // Default initalization

            if (this.font == null)
            {
                this.font = gui.graphics.engine.resourceComponent.get(gui.defaultFontPath);
            }
        }

        /**
         * Draw the GUIItem on the screen
         */
        public override void draw()
        {
            drawBackground();
            drawImage();
            drawText();
        }

        protected virtual void drawBackground()
        {
            // Draw background color
            if (bgColor.a != 0) {
                gui.graphics.drawRect((int)pos.x, (int)pos.y, (int)size.x, (int)size.y, bgColor);
            }
        }

        protected virtual void drawImage()
        {
            // Draw background image
            if (texture == null) return;

            Texture2D tex = texture.getResource<Texture2D>();
            if (texture != null)
            {
                gui.graphics.drawTex(texture, (int)pos.x, (int)pos.y, stretchImage ? (int)size.x : tex.width, stretchImage ? (int)size.y : tex.height, tintTex);
            }
        }

        protected virtual void drawText()
        {
            ResourceComponent rc = gui.graphics.engine.resourceComponent;
            // Draw text
            if (text != null && text.Length != 0 && font != null) {
                Vector2 adjPos = pos + textOffset;
                gui.graphics.drawTex(textTex, (int)adjPos.x, (int)adjPos.y, textTex.width, textTex.height, textColor);
            }
        }
    }
}
