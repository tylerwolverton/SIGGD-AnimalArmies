/**
 * @file GUIButton.cs
 * @author SIGGD, Purdue ACM <siggd.purdue@gmail.com>
 * @version 2.0
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    /** 
     * This class implements a generic button, with customizable pressed and unpressed images.
     */
    public class GUIButton : GUILabel
    {
        private Handle _unpressedImg;
        public Handle unpressedImg
        {
            get {
                return _unpressedImg;
            }
            set {
                if (texture == _unpressedImg) {
                    texture = value;
                }
                _unpressedImg = value;
            }
        }

        private Handle _pressedImg;
        public Handle pressedImg
        {
            get {
                return _pressedImg;
            }
            set {
                if (texture == _pressedImg)
                {
                    texture = value;
                }
                _pressedImg = value;
            }
        }
        /* Constructs the button.
         * @param gui The gui on which to draw
         * @param unpressedImg A Handle for the image for the unpressed state
         * @param pressedImg A Handle for the image for the pressed state (defaults to the unpressed image if unspecified)
         * @param text The text to include on top of the image
         */
        public GUIButton(GUI gui, Handle unpressedImg, Handle pressedImg = null, string text = "")
            : base(gui)
        {
            this.unpressedImg = unpressedImg;
            this.pressedImg = pressedImg;
            if (this.pressedImg == null) this.pressedImg = unpressedImg;

            this.texture = unpressedImg;
            this.text = text;
            this.pos = Vector2.Zero;
        }

        //Sets the image back to the unpressed image
        public void resetImg()
        {
            texture = _unpressedImg;
        }

        //Called when the mouse is pressed down. Changes the image on the button
        public override void onMouseDown(Vector2 pos, MouseKeyBinding.MouseButton button)
        {
            if (texture == _unpressedImg)
            {
                texture = _pressedImg;
            } else {
                texture = _unpressedImg;
            }
        }

        //Called when the mouse is released
        public override void onMouseUp(Vector2 pos, MouseKeyBinding.MouseButton button)
        {
            base.onMouseUp(pos, button);
            texture = _unpressedImg;
        }
    }
}
