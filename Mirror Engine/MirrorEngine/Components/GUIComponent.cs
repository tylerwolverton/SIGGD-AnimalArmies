#if false
/**
 * @file
 * @author PURDUE ACM, SIGGD <siggd.purdue@gmail.com>
 * @version 1.0
 * 
 * @section LICENSE
 * 
 * @section DESCRIPTION
 */

using System;
using System.Collections.Generic;
using System.IO;
using Engine.GUI;
using Engine.Input;
using Engine.Textures;

namespace Engine
{
    /// brief class desc here
    /**
    * classdeschere
    */
    public class GUIComponent : Component
    {

        public readonly GraphicsComponent graphics; ///< 
        public readonly ResourceComponent resources; ///<

        public GUIItem focus { get; set; }  ///< Which GUI Item is currently in focus?

        public SpriteFont font; ///<
        public TextureSet engineTextures { get; protected set; }   ///< The set of textures designated by Mirror
        public TextureSet guiTextures { get; protected set; } ///< The set of textures added by derived projects

        ///The inputs accepted by the GUI at all times
        public enum StaticBindings { CLICK, RIGHTCLICK, KEYPRESS, }
        public InputBinding[] staticBindings;

        
        private GUIContainer current_; // The container currently in focus
        
        ///
        public GUIContainer current
        {
            get
            {
                return current_;
            }
            set
            {
                InputComponent input = graphics.engine.inputComponent;

                // Entering the gui
                if (value != null && !input.hasSaved())
                {
                    input.save();

                    if (value.bindings != null)
                    {
                        input.replace(value.bindings);
                    }
                    input.append(staticBindings);
                }
                else if (value != null)
                {  // Changing the Gui
                    if (value.bindings != null)
                    {  // 
                        input.replace(value.bindings);
                        input.append(staticBindings);

                    }

                    // If not null, we have already appended our static bindings (since we're already in the gui)
                }
                else
                {   // Exiting the GUI
                    input.restore();
                }

                current_ = value;
            }
        }

        public float fullscale; ///<
        public float notfullscale; ///<

        ///
        public InputBinding this[StaticBindings bind]
        {
            get
            {
                return staticBindings[(int)bind];
            }

            set
            {
                staticBindings[(int)bind] = value;
            }
        }

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        public GUIComponent(GraphicsComponent theGraphics)
            : base(theGraphics.engine)
        {

            this.graphics = theGraphics;
            this.resources = engine.resourceComponent;

            staticBindings = new InputBinding[Enum.GetValues(typeof(StaticBindings)).Length];
        }

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        public override void Initialize()
        {

            font = resources.contentManager.Load<SpriteFont>("Fonts/Courier New");
         
            //Load the textures used by the GUI
            engineTextures = resources.getTextureSet(Path.Combine("000_GUI"));//This is now correct          

            //Set up the inputs used by the GUI.
            this[StaticBindings.CLICK] = new SinglePressBinding(engine.inputComponent,
                    null, null, InputComponent.MouseButton.LEFT);
            (this[StaticBindings.CLICK] as SinglePressBinding).downEvent += onClick;
            (this[StaticBindings.CLICK] as SinglePressBinding).upEvent += releaseClick;

            this[StaticBindings.RIGHTCLICK] = new SinglePressBinding(engine.inputComponent,
                   null, null, InputComponent.MouseButton.RIGHT);

            (this[StaticBindings.RIGHTCLICK] as SinglePressBinding).downEvent += onRightClick;
            (this[StaticBindings.RIGHTCLICK] as SinglePressBinding).upEvent += releaseRightClick;

            this[StaticBindings.KEYPRESS] = new TextBinding(engine.inputComponent);
            (this[StaticBindings.KEYPRESS] as TextBinding).charEntered += onKeypress;
        }

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        public virtual void DrawMenu(SpriteBatch spriteBatch)
        {

            if (current == null)
                return;

            current.draw(spriteBatch, new Vector2(0, 0));
        }

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        public virtual void DrawCursor(SpriteBatch spriteBatch)
        {
            MouseState ms = engine.inputComponent.currentMouseState;
            spriteBatch.Draw(engineTextures[0], new Vector2((float)ms.X, (float)ms.Y), Color.White);
        }

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        public override void Draw()
        {

            SpriteBatch spriteBatch = graphics.spriteBatch;

            Matrix scale = Matrix.CreateScale(graphics.camera.scale - graphics.camera.scaleChange);

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, scale);
            SamplerState samplerstate = new SamplerState();
            samplerstate.Filter = TextureFilter.Point;
            spriteBatch.GraphicsDevice.SamplerStates[0] = samplerstate;
            DrawMenu(spriteBatch);
            spriteBatch.End();

            spriteBatch.Begin();
            DrawCursor(spriteBatch);
            spriteBatch.End();
        }

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        protected void releaseClick()
        {
            MouseState mPos = engine.inputComponent.currentMouseState;
            if (current != null)
            {
                float scale = graphics.camera.scale - graphics.camera.scaleChange;
                current.releaseClick(new Vector2(mPos.X / scale, mPos.Y / scale) - current.location);
            }
        }

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        protected void onClick()
        {
            MouseState mPos = engine.inputComponent.currentMouseState;
            if (current != null)
            {
                float scale = graphics.camera.scale - graphics.camera.scaleChange;
                current.handleClick(new Vector2(mPos.X / scale, mPos.Y / scale) - current.location);
            }
        }

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        protected void releaseRightClick()
        {
            MouseState mPos = engine.inputComponent.currentMouseState;
            if (current != null)
            {
                float scale = graphics.camera.scale - graphics.camera.scaleChange;
                current.releaseRightClick(new Vector2(mPos.X / scale, mPos.Y / scale) - current.location);
            }
        }

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        protected void onRightClick()
        {
            MouseState mPos = engine.inputComponent.currentMouseState;
            if (current != null)
            {
                float scale = graphics.camera.scale - graphics.camera.scaleChange;
                current.handleRightClick(new Vector2(mPos.X / scale, mPos.Y / scale) - current.location);
            }
        }

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        protected void onKeypress(char c)
        {
            if (focus != null)
            {
                focus.handleKeyPress(c);
            }
            else if (current != null)
            {
                current.handleKeyPress(c);
            }
        }
    }
}
#endif
