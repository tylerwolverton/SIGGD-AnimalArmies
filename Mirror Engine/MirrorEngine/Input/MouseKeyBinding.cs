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
using Tao.Sdl;

namespace Engine
{
    /// brief class desc here
    /**
    * Binding for text input
    */
    public class MouseKeyBinding : InputBinding
    {
        public enum MouseButton { LEFT, MIDDLE, RIGHT }
        public delegate void MouseKeyEvent(MouseButton m); ///< Delegate type for mouse button pressed
        public event MouseKeyEvent mouseKeyDown; ///< Events
        public event MouseKeyEvent mouseKeyUp;

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        public MouseKeyBinding(InputComponent input)
            : base(input)
        {
        }

        public override void onEvent(InputEvent e)
        {

            MouseButtonEvent mevt = e as MouseButtonEvent;
            if (mevt == null)
                return;

            if (mevt.down) {
                if (mouseKeyDown == null)
                    return;
                mouseKeyDown(mevt.id);
            }

            if (!(mevt.down))
            {
                if (mouseKeyUp == null)
                    return;
                mouseKeyUp(mevt.id);
            }
        }

        public override InputBinding clone()
        {
            return new MouseKeyBinding(input);
        }
    }
}
