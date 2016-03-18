/**
 * @file
 * @author PURDUE ACM, SIGGD <siggd.purdue@gmail.com>
 * @version 1.0
 * 
 * @section LICENSE
 * 
 * @section DESCRIPTION
 * Binding for an analog input, from an origin
 */

using System;
using System.Collections.Generic;

namespace Engine
{
    /// brief class desc here
    /**
    * classdeschere
    */
    public class AxisBinding : InputBinding
    {

        private const float THRESHOLD = .7f;

        public delegate void AxisChangedEvent(int i);
        public event AxisChangedEvent valChanged;

        public AxisEvent[] axes { get; private set; } ///<
        public ButtonEvent[] pos { get; private set; } ///<
        public ButtonEvent[] neg { get; private set; } ///<

        public float position {
            get
            {
                return Math.Max(-1f, Math.Min(rawPos, 1f));
            }
        } ///<

        private float rawPos;
        private int prevAxisPos = 0;

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        public AxisBinding(InputComponent input, AxisEvent[] axes, ButtonEvent[] pos, ButtonEvent[] neg)
            : base(input)
        {
            this.axes = axes;
            this.pos = pos;
            this.neg = neg;
        }

        public override void onEvent(InputEvent e)
        {
            if (axes != null) {
                foreach (AxisEvent axis in axes) {
                    if (axis.matches(e)) {
                        AxisEvent evt = e as AxisEvent;
                        evt.flip = axis.flip;
                        rawPos -= evt.prevVal;
                        rawPos += evt.val;
                        
                        evt.flip = false;
                    }
                }
            }

            if (pos != null) {
                foreach (ButtonEvent button in pos) {
                    if (button.matches(e)) {
                        ButtonEvent evt = e as ButtonEvent;
                        if (evt.down) {
                            rawPos += 1;
                        } else {
                            rawPos -= 1;
                        }
                    }
                }
            }

            if (neg != null) {
                foreach (ButtonEvent button in neg) {
                    if (button.matches(e)) {
                        ButtonEvent evt = e as ButtonEvent;
                        if (evt.down) {
                            rawPos -= 1;
                        } else {
                            rawPos += 1;
                        }
                    }
                }
            }

            int r = 0;
            if (rawPos != 0f && rawPos != 1f && rawPos != -1f)
            {
                if (rawPos < -THRESHOLD) r = -1;
                else if (rawPos > THRESHOLD) r = 1;
            }
            else r = (int)rawPos;

            if (r != prevAxisPos)
            {
                if(valChanged != null) valChanged(r);
                prevAxisPos = r;
            }
        }

        public override InputBinding clone()
        {
            return new AxisBinding(input, axes, pos, neg);
        }
    }
}