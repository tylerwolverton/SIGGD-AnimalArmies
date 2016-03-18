/**
 * @file GUICheckBox.cs
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
    /*
     * A CheckBox. Just a button with a toggle event.
     */
    public class GUICheckBox : GUIButton
    {
        public delegate void toggleDelegate(bool isDown);   //The delegate to register functions to
        public event toggleDelegate toggleEvent;            //The event that triggers the delegate

        string CHECKUP = "GUI/000_EngineGUI/022_box.png";   //Default unpressed image location
        string CHECKDOWN = "GUI/000_EngineGUI/023_box.png"; //Default checked image location

        Handle checkUpImage;
        Handle checkDownImage;

        //bool _isDown = false;
        public bool isDown //Whether or not the button is checked
        {
            get;
            private set;
        }

        /* Constructs the checkbox.
         * 
         * @param gui The gui to draw on
         * @param isDown The initial checked state for the button
         * @param text The text to display on the button
         * @param uncheckedImg The image to display when the button is unchecked.
         * @param checkedImg The image to display when the button is checked.
         */
        public GUICheckBox(GUI gui, bool isDown, string text, Handle uncheckedImg = null, Handle checkedImg = null)
            : base(gui, uncheckedImg, checkedImg, text)
        {
            
            ResourceComponent rc = gui.graphics.engine.resourceComponent;

            checkUpImage = uncheckedImg;
            checkDownImage = checkedImg;

            //If no images are specified in the constructor, we'll use the default ones
            if (checkUpImage == null)
                checkUpImage = rc.get(Path.GetFullPath(Path.Combine(gui.rootDirectory, CHECKUP)));
            if (checkDownImage == null)
                checkDownImage = rc.get(Path.GetFullPath(Path.Combine(gui.rootDirectory, CHECKDOWN)));

            this.isDown = isDown;
                refresh();

            // Size the text and image
            Vector2 textSize = Vector2.Zero;
            Vector2 imgSize = Vector2.Zero;
            if (text != null)
            {
                if (font == null)
                {
                    font = rc.get(gui.defaultFontPath);
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

        //Toggles the state of the button, updating the state and redrawing the button.
        public void toggle()
        {
            isDown = !isDown;
            if(toggleEvent != null) toggleEvent(isDown);
            refresh();
        }

        //Called when the button is clicked
        public override void onMouseClick(Vector2 pos, MouseKeyBinding.MouseButton button)
        {
            toggle();
        }

        //Called when the mousebutton is pressed down
        public override void onMouseDown(Vector2 pos, MouseKeyBinding.MouseButton button)
        {
            if (isDown) texture = checkUpImage;
            else        texture = checkDownImage;
        }

        //Called when the mousebutton is let up
        public override void onMouseUp(Vector2 pos, MouseKeyBinding.MouseButton button)
        {
            refresh();
        }

        //Updates the button image
        private void refresh()
        {
            if (isDown)   texture = checkDownImage;
            else          texture = checkUpImage;
        }
    }
}
