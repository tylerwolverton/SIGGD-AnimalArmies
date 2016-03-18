/**
 * @file GUIRadioButton.cs
 * @author SIGGD, Purdue ACM <siggd.purdue@gmail.com>
 * @version 2.0
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Engine
{
    //GUI Component - Radio Button! Works and looks like one.
    //The pressed and unpressed images are customizable.
    public class GUIRadioButton : GUIButton
    {
        public delegate void setDelegate(bool isDown);
        public event setDelegate setEvent;

        const string RADIOUP = "GUI/000_EngineGUI/024_radio.png";       //Default unpressed image
        const string RADIODOWN = "GUI/000_EngineGUI/025_radio.png";     //Default pressed image

        Handle radioUpImage;
        Handle radioDownImage;

        bool isDown = false;

        /*  Constructor
         * 
         * @param gui The game gui
         * @param isDown The default state for this radio button
         * @param uncheckedImg A handle for the image that will be drawn if the button is unpressed (isDown = false).
         * @param checkedImg A handle for the image that will be drawn if the button is pressed (isDown = true).
         */
        public GUIRadioButton(GUI gui, bool isDown, string text, Handle uncheckedImg = null, Handle checkedImg = null)
            : base(gui, uncheckedImg, checkedImg, text)
        {
            ResourceComponent rc = gui.graphics.engine.resourceComponent;

            radioUpImage = uncheckedImg;
            radioDownImage = checkedImg;

            //Sets default radio buttons if none are specified
            if (radioUpImage == null)
                radioUpImage = rc.get(Path.GetFullPath(Path.Combine(gui.rootDirectory, RADIOUP)));
            if (radioDownImage == null)
                radioDownImage = rc.get(Path.GetFullPath(Path.Combine(gui.rootDirectory, RADIODOWN)));
            
            this.isDown = isDown;
            //Will make the picture display correctly based on the default pressed state
            refresh();

            // Size the text and image
            Vector2 textSize = Vector2.Zero;
            Vector2 imgSize = Vector2.Zero;
            if (text != null)
            {
                if (this.font == null)
                {
                    this.font = rc.get(gui.defaultFontPath);
                }
                textSize = Font.calcTextSize(font, text, fontSize);
            }
            if (texture != null)
            {
                Texture2D bgTex = texture.getResource<Texture2D>();
                imgSize = new Vector2(bgTex.width, bgTex.height);
            }

            size = new Vector2(imgSize.x + textSize.x, Math.Max(imgSize.y, textSize.y));
            stretchImage = false;
            textOffset = new Vector2(imgSize.x + 2, 0);
            this.pos = Vector2.Zero;
        }

        /* Sets the pressed state
         * @param isDown The desired pressed state
         */
        public void set(bool isDown)
        {
            this.isDown = isDown;
            refresh();
        }

        //When the mouse is clicked, fire the event for when the button is pushed
        public override void onMouseClick(Vector2 pos, MouseKeyBinding.MouseButton button)
        {
            if(setEvent != null) setEvent(true);
        }

        //When the mouse is pressed, determine which picture to set
        public override void onMouseDown(Vector2 pos, MouseKeyBinding.MouseButton button)
        {
            if (isDown) texture = radioUpImage;
            else        texture = radioDownImage;
        }

        //Display the picture that displays the correct pressed state
        public override void onMouseUp(Vector2 pos, MouseKeyBinding.MouseButton button)
        {
            refresh();
        }

        private void refresh()
        {
            if (isDown) texture = radioDownImage;
            else        texture = radioUpImage;
        }
    }
}
