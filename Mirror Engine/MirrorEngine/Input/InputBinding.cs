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

namespace Engine
{
    /// brief class desc here
    /**
    * classdeschere
    */
    public abstract class InputBinding
    {

        protected InputComponent input; ///<

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        public InputBinding(InputComponent input)
        {
            this.input = input;
        }

        public abstract void onEvent(InputEvent e);

        /**
         * Generates a duplicate of this binding, bound to the same event, but with no actions bount to it.
         * Implementing this function is necessary to create new input contexts from old ones.
         */
        public abstract InputBinding clone();
    }
}
