/**
 * @file GUIRadioControl.cs
 * @author SIGGD, Purdue ACM <siggd.purdue@gmail.com>
 * @version 2.0
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    //A collection of radio buttons. Allows only 1 radio button in the group to be pressed down
    public class GUIRadioControl : GUIControl
    {

        private int _pressed = 0;
        public int pressed          //Which button is pressed
        {
            get
            {
                return _pressed;
            }
            set
            {
                if (_pressed == value) return;
                setButton(_pressed, false);
                setButton(value, true);
                _pressed = value;
            }
        }

        /* Constructs the GUIRadioControl.
         * 
         * @param gui The gui to draw on
         * @param downButton The index of the button that should be initially pressed down
         */ 
        public GUIRadioControl(GUI gui, int downButton = 0)
            : base(gui)
        {
            _pressed = downButton;
        }

        /* Adds a GUIRadioButton the the control.
         * 
         * @param set The function to call when the new button is pushed.
         * @param text The text to display with the button
         * 
         * @return The GUIRadioButton that was added to this control.
         */
        public GUIRadioButton addRadioButton(GUIRadioButton.setDelegate set, string text)
        {
            GUIRadioButton nextButton = new GUIRadioButton(gui, items.Count == pressed ? true : false, text);
            int id = items.Count;
            nextButton.setEvent += (isDown) => { pressed = id;};
            nextButton.setEvent += set;

            if (items.Count == 0) nextButton.pos = new Vector2(0, 0);
            else nextButton.pos = items[items.Count - 1].pos + new Vector2(0, items[items.Count - 1].size.y + 5);
            
            items.Add(nextButton);
            return nextButton;
        }

        //Fires the radio button's down event
        private void setButton(int button, bool isDown)
        {
            try
            {
                (items[button] as GUIRadioButton).set(isDown);
            }
            catch (Exception e) { }
        }
        
        //Overrides manually adding items to this control.
        public override void add(GUIItem item){}
    }
}
