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
    public class TextBinding : InputBinding
    {
        
        public delegate void CharEvent(char c); ///< Delegate type for character typed
        public event CharEvent charEntered; ///< Events
        public event CharEvent keyLifted;

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        public TextBinding(InputComponent input)
            : base(input)
        {
        }

        public override void onEvent(InputEvent e)
        {
           
            KeyEvent kevt = e as KeyEvent;
            if (kevt == null)
                return;

            if (kevt.down)
            {
                if (charEntered == null)
                    return;

                if (kevt.id >= Sdl.SDLK_a && kevt.id <= Sdl.SDLK_z)
                {
                    bool shift = (kevt.mods & Sdl.KMOD_CAPS) == Sdl.KMOD_CAPS || (kevt.mods & Sdl.KMOD_SHIFT) == Sdl.KMOD_SHIFT;
                    charEntered((char)((shift ? 'A' : 'a') + (kevt.id - Sdl.SDLK_a)));
                }
                else if (kevt.id >= Sdl.SDLK_0 && kevt.id <= Sdl.SDLK_9)
                {
                    charEntered((char)('0' + (kevt.id - Sdl.SDLK_0)));
                }
                else if (kevt.id == Sdl.SDLK_SPACE)
                {
                    charEntered(' ');
                }
                else if (kevt.id == Sdl.SDLK_BACKSPACE)
                {
                    charEntered('\b');
                }
            }
            else
            {
                if (keyLifted == null) return;

                if (kevt.id >= Sdl.SDLK_a && kevt.id <= Sdl.SDLK_z)
                {
                    bool shift = (kevt.mods & Sdl.KMOD_CAPS) == Sdl.KMOD_CAPS || (kevt.mods & Sdl.KMOD_SHIFT) == Sdl.KMOD_SHIFT;
                    keyLifted((char)((shift ? 'A' : 'a') + (kevt.id - Sdl.SDLK_a)));
                }
                else if (kevt.id >= Sdl.SDLK_0 && kevt.id <= Sdl.SDLK_9)
                {
                    keyLifted((char)('0' + (kevt.id - Sdl.SDLK_0)));
                }
                else if (kevt.id == Sdl.SDLK_SPACE)
                {
                    keyLifted(' ');
                }
                else if (kevt.id == Sdl.SDLK_BACKSPACE)
                {
                    keyLifted('\b');
                }
            }
        }

        public override InputBinding clone()
        {
            return new TextBinding(input);
        }
    }
}
