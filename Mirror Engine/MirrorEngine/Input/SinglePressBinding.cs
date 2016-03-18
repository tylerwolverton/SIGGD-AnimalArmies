/**
 * @file
 * @author PURDUE ACM, SIGGD <siggd.purdue@gmail.com>
 * @version 1.0
 * 
 * @section LICENSE
 * 
 * @section DESCRIPTION
 * Binding for a single press/depress cycle, with associated state
 */

using System;
using System.Collections.Generic;
using Tao.Sdl;

namespace Engine
{
    /// brief class desc here
    /**
    * classdeschere
    */
    public class SinglePressBinding : InputBinding
    {
        public ButtonEvent[] buttons;

        public delegate void PressEvent(); ///< Delegate type

        public PressEvent downEvent = null; ///<
        public PressEvent upEvent = null; ///<

        public bool isPressed {
            get {
                return downCount > 0;
            }
        }

        private int downCount;   ///< Number of bound buttons currently pressed

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        public SinglePressBinding(InputComponent input, ButtonEvent[] buttons)
            : base(input)
        {
            this.buttons = buttons;
        }

        public override void onEvent(InputEvent e)
        {
            if (buttons == null) {
                return;
            }

            foreach (ButtonEvent evt in buttons) {
                if (evt.matches(e)) {
                    ButtonEvent recvEvt = e as ButtonEvent;
                    if (recvEvt.down) {
                        downCount++;

                        if (downCount == 1 && downEvent != null) {  // Was up, now down
                            downEvent();
                        }
                    } else {
                        downCount--;

                        if (downCount == 0 && upEvent != null) {  // Was down, now up
                            upEvent();
                        }
                    }
                }
            }
        }

        public override InputBinding clone()
        {
            return new SinglePressBinding(input, buttons);
        }

        //hacked in function to avoid "offcenter" problems when losing window focus.
        public void normalize()
        {
            downCount = 0;
        }
    }
}