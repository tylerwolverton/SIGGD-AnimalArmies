using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public abstract class InputEvent
    {
        public abstract bool matches(InputEvent e);
    }

    public abstract class AxisEvent : InputEvent
    {
        private readonly float m_val;  
        private readonly float m_prevVal;  

        public float val {
            get {
                return (flip) ? -m_val : m_val;
            }
        }

        public float prevVal {
            get {
                return (flip) ? -m_prevVal : m_prevVal;
            }
        }

        public bool flip;           ///< True if should flip the axis's value

        public AxisEvent(bool flip = false, float val = 0, float prevVal = 0)
        {
            m_val = val;
            m_prevVal = prevVal;
            this.flip = flip;
        }
    }

    public abstract class ButtonEvent : InputEvent
    {
        public bool down;

        public ButtonEvent(bool down = false)
        {
            this.down = down;
        }
    }

    public class KeyEvent : ButtonEvent
    {
        public int id;
        public int mods;

        public KeyEvent(int id, int mods = 0, bool down = false)
            : base(down)
        {
            this.id = id;
            this.mods = mods;
        }

        public override bool matches(InputEvent e)
        {
            KeyEvent evt = e as KeyEvent;
            if (evt == null) {
                return false;
            }
            return this.id == evt.id && (evt.mods & this.mods) == this.mods;
        }
    }

    public class MouseButtonEvent : ButtonEvent
    {
        public MouseKeyBinding.MouseButton id;

        public MouseButtonEvent(MouseKeyBinding.MouseButton id, bool down = false)
            : base(down)
        {
            this.id = id;
        }

        public override bool matches(InputEvent e)
        {
            MouseButtonEvent evt = e as MouseButtonEvent;
            if (evt == null) {
                return false;
            }
            return this.id == evt.id;
        }
    }

    public class JoyButtonEvent : ButtonEvent
    {
        public int deviceId;
        public int id;

        public JoyButtonEvent(int deviceId, int id, bool down = false)
            : base(down)
        {
            this.deviceId = deviceId;
            this.id = id;
        }

        public override bool matches(InputEvent e)
        {
            JoyButtonEvent evt = e as JoyButtonEvent;
            if (evt == null) {
                return false;
            }
            return this.deviceId == evt.deviceId && this.id == evt.id;
        }
    }

    public class MouseAxisEvent : AxisEvent
    {
        public InputComponent.MouseAxis id;

        public MouseAxisEvent(InputComponent.MouseAxis id, bool flip = false, float val = 0, float prevVal = 0)
            : base(flip, val, prevVal)
        {
            this.id = id;
        }

        public override bool matches(InputEvent e)
        {
            MouseAxisEvent evt = e as MouseAxisEvent;
            if (evt == null) {
                return false;
            }
            return this.id == evt.id;
        }
    }

    public class JoyAxisEvent : AxisEvent
    {
        public int deviceId;
        public int id;

        public JoyAxisEvent(int deviceId, int id, bool flip = false, float val = 0, float prevVal = 0)
            : base(flip, val, prevVal)
        {
            this.deviceId = deviceId;
            this.id = id;
        }

        public override bool matches(InputEvent e)
        {
            JoyAxisEvent evt = e as JoyAxisEvent;
            if (evt == null) {
                return false;
            }
            return this.deviceId == evt.deviceId && this.id == evt.id;
        }
    }

    public class JoyHatEvent : AxisEvent
    {
        public int deviceId;
        public int id;

        public JoyHatEvent(int deviceId, int id, bool flip = false, float val = 0, float prevVal = 0)
            : base(flip, val, prevVal)
        {
            this.deviceId = deviceId;
            this.id = id;
        }

        public override bool matches(InputEvent e)
        {
            JoyHatEvent evt = e as JoyHatEvent;
            if (evt == null) {
                return false;
            }
            return this.deviceId == evt.deviceId && this.id == evt.id;
        }
    }
}