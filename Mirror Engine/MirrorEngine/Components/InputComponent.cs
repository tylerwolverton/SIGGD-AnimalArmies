/**
 * @file InputComponent.cs
 * @author SIGGD, Purdue ACM <siggd.purdue@gmail.com>
 * @version 2.0
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Tao.Sdl;

namespace Engine
{
    //Polls for input and fires events for any existing bindings. Manages input contexts.
    public class InputComponent : Component
    {
        private Dictionary<Tuple<Type, object>, InputBinding[]> bindings;   //All bindings that are active
        private Dictionary<Type, object> currentContexts;                   //The contexts that are active

        //Bindings for the current GUI
        public enum GUIBindings
        {
            MOUSEBUTTON,
            TEXT,
            HORIZONTALAXIS,
            VERTICALAXIS,
        }

        //Bindings for Treequake
        public enum EditorBindings
        {
            SWAP,
            ACTOR,
            PENCIL,
            FILL,
            PLUS,
            MINUS,
            CENTER,
            XMOVE,
            YMOVE,
            MOUSEBUTTON,
        }

        //Generic bindings for the engine during gameplay
        //Swap should probably be in EngineBindings
        public enum GameStateBindings
        {
            SWAP,
        }

        //Generic bindings for the engine
        //Should probably contain escape as well
        public enum EngineBindings
        {
            FULLSCREEN,
        }

        //Defines X and Y constants for the mouse position
        public enum MouseAxis { X, Y }

        protected int mouseX;  //Current X position of mouse, relative to application
        protected int mouseY;  //Current Y position of mouse, relative to application
        
        //Gets the position of the mouse
        public Vector2 getMousePosition() 
        {
            return new Vector2(mouseX, mouseY);
        }

        //Defines a joystick
        public class JoyInfo
        {
            public int numAxes { get; private set; }
            public int numButtons { get; private set; }
            public int numHats { get; private set; }

            public float[] axisVals;
            public int[] hatVals;

            private IntPtr joy;

            public JoyInfo(IntPtr joy)
            {
                this.joy = joy;
                this.numAxes = Sdl.SDL_JoystickNumAxes(joy);
                this.numButtons = Sdl.SDL_JoystickNumButtons(joy);
                this.numHats = Sdl.SDL_JoystickNumHats(joy);
                this.axisVals = new float[numAxes];
                this.hatVals = new int[numHats];
            }
        }
        public JoyInfo[] joys { get; private set; } //The active joysticks

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        public InputComponent(MirrorEngine theEngine)
            : base(theEngine)
        {
            // Set up joysticks
            Sdl.SDL_InitSubSystem(Sdl.SDL_INIT_JOYSTICK);

            // Open all joysticks
            joys = new JoyInfo[Sdl.SDL_NumJoysticks()];
            for (int i = 0; i < joys.Length; i++) 
            {
                joys[i] = new JoyInfo(Sdl.SDL_JoystickOpen(i));
            }

            // Enable joystick events
            Sdl.SDL_JoystickEventState(Sdl.SDL_ENABLE);

            bindings = new Dictionary<Tuple<Type, object>, InputBinding[]>();
            currentContexts = new Dictionary<Type, object>();
        }

        public override void initialize()
        {
            addBindings(typeof(GUIBindings));
            addBindings(typeof(EditorBindings));
            addBindings(typeof(GameStateBindings));
            addBindings(typeof(EngineBindings));

            // GUI Bindings
            this[GUIBindings.MOUSEBUTTON] = new MouseKeyBinding(this);
            this[GUIBindings.TEXT] = new TextBinding(this);
            this[GUIBindings.HORIZONTALAXIS] = new AxisBinding(this,
                new AxisEvent[] { new JoyAxisEvent(0,0) }, new ButtonEvent[] { new KeyEvent(Sdl.SDLK_RIGHT), new KeyEvent(Sdl.SDLK_d) }, new ButtonEvent[] { new KeyEvent(Sdl.SDLK_LEFT), new KeyEvent(Sdl.SDLK_a) });
            this[GUIBindings.VERTICALAXIS] = new AxisBinding(this,
                new AxisEvent[] {new JoyAxisEvent(0,0) }, new ButtonEvent[] { new KeyEvent(Sdl.SDLK_UP), new KeyEvent(Sdl.SDLK_w) }, new ButtonEvent[] { new KeyEvent(Sdl.SDLK_DOWN), new KeyEvent(Sdl.SDLK_s) });
            this[EngineBindings.FULLSCREEN] = new SinglePressBinding(engine.inputComponent, new ButtonEvent[] { new KeyEvent(Sdl.SDLK_F2) });
            this[GameStateBindings.SWAP] = new SinglePressBinding(engine.inputComponent, new ButtonEvent[] { new KeyEvent(Sdl.SDLK_F1), new JoyButtonEvent(0, 2) });
            this[EditorBindings.SWAP] = new SinglePressBinding(engine.inputComponent, new ButtonEvent[] { new KeyEvent(Sdl.SDLK_F1), new JoyButtonEvent(0, 2) });
            this[EditorBindings.ACTOR] = new SinglePressBinding(this, new ButtonEvent[] { new KeyEvent(Sdl.SDLK_1) });
            this[EditorBindings.PENCIL] = new SinglePressBinding(this, new ButtonEvent[] { new KeyEvent(Sdl.SDLK_2) });
            this[EditorBindings.FILL] = new SinglePressBinding(this, new ButtonEvent[] { new KeyEvent(Sdl.SDLK_3) });
            this[EditorBindings.PLUS] = new SinglePressBinding(this, new ButtonEvent[] { new KeyEvent(Sdl.SDLK_KP_PLUS), new KeyEvent(Sdl.SDLK_EQUALS) });
            this[EditorBindings.MINUS] = new SinglePressBinding(this, new ButtonEvent[] { new KeyEvent(Sdl.SDLK_KP_MINUS), new KeyEvent(Sdl.SDLK_MINUS) });
            this[EditorBindings.CENTER] = new SinglePressBinding(this, new ButtonEvent[] { new KeyEvent(Sdl.SDLK_KP_ENTER), new KeyEvent(Sdl.SDLK_RETURN) });
            this[EditorBindings.XMOVE] = new AxisBinding(this,
                new AxisEvent[] { new JoyAxisEvent(0, 0) }, new ButtonEvent[] { new KeyEvent(Sdl.SDLK_d) }, new ButtonEvent[] { new KeyEvent(Sdl.SDLK_a) });
            this[EditorBindings.YMOVE] = new AxisBinding(this,
                new AxisEvent[] { new JoyAxisEvent(0, 1, true) }, new ButtonEvent[] { new KeyEvent(Sdl.SDLK_w) }, new ButtonEvent[] { new KeyEvent(Sdl.SDLK_s) });
            this[EditorBindings.MOUSEBUTTON] = new MouseKeyBinding(this);
        }

        //Receive events
        public void keyEvent(int id, int mods, bool down)
        {
            fireEvent(new KeyEvent(id, mods, down));
        }

        public void mouseAxisEvent(MouseAxis id, int val)
        {
            float prevVal = 0;
            if (id == MouseAxis.X) {
                prevVal = mouseX;
                mouseX = val;
            } else {
                prevVal = mouseY;
                mouseY = val;
            }

            fireEvent(new MouseAxisEvent(id, false, val, prevVal));
        }

        public void mouseButtonEvent(MouseKeyBinding.MouseButton id, bool down)
        {
            fireEvent(new MouseButtonEvent(id, down));
        }

        public void joyButtonEvent(int deviceId, int id, bool down)
        {
            fireEvent(new JoyButtonEvent(deviceId, id, down));
        }

        public void joyAxisEvent(int deviceId, int id, int value)
        {
            float prevVal = joys[deviceId].axisVals[id];
            float val = (value + .5f) / (32767.5f);  // Convert to float between -1 and 1
            joys[deviceId].axisVals[id] = val;
            fireEvent(new JoyAxisEvent(deviceId, id, false, val, prevVal));
        }

        public void joyHatEvent(int deviceId, int id, int val)
        {
            int prevVal = joys[deviceId].hatVals[id];
            joys[deviceId].hatVals[id] = val;
            fireEvent(new JoyHatEvent(deviceId, id, false, val, prevVal));
        }

        //Dispatch events. Override in derived classes to add more bindings
        protected virtual void fireEvent(InputEvent evt)
        {
            if (bindings == null) return;
            
            KeyValuePair<Type, object>[] contexts = currentContexts.ToArray<KeyValuePair<Type, object>>();
            foreach (var ctx in contexts) 
            {
                foreach (InputBinding b in bindings[new Tuple<Type,object>(ctx.Key, ctx.Value)]) 
                {
                    if (b != null) b.onEvent(evt);
                }
            }
        }

        /**
         * Initialize a binding array from an enumeration (if not already set), and adds them to the current context (null if no context)
         * 
         * @param e The type of the enumeration
         */
        protected void addBindings(Type e)
        {
            if (!currentContexts.ContainsKey(e)) 
            {
                currentContexts.Add(e, null);
            }

            var t = new Tuple<Type, object>(e, currentContexts[e]);
            if (!bindings.ContainsKey(t)) 
            {
                bindings[t] = new InputBinding[Enum.GetValues(e).Length];
            }
        }

        /**
         * Remove a binding array from an enumeration
         * 
         * @param e The type of the enumeration
         * @param ctx The context of the binding
         */
        protected void removeBindings(Type e, object ctx = null)
        {
            bindings.Remove(new Tuple<Type, object>(e, ctx));
        }

        public InputBinding this[Enum bind]
        {
            get
            {
                return bindings[new Tuple<Type, object>(bind.GetType(), currentContexts[bind.GetType()])][(int)(object)bind];
            }

            set
            {
                bindings[new Tuple<Type, object>(bind.GetType(), currentContexts[bind.GetType()])][(int)(object)bind] = value;
            }
        }

        public bool pollEvents()
        {
            // Handle events
            Sdl.SDL_Event evt;
            MouseKeyBinding.MouseButton button = MouseKeyBinding.MouseButton.LEFT;
            if (Sdl.SDL_PollEvent(out evt) != 0)
            {
                switch (evt.type)
                {
                    case Sdl.SDL_KEYDOWN:
                        keyEvent(evt.key.keysym.sym, evt.key.keysym.mod, true);
                        break;

                    case Sdl.SDL_KEYUP:
                        keyEvent(evt.key.keysym.sym, evt.key.keysym.mod, false);
                        break;

                    case Sdl.SDL_MOUSEMOTION:
                        mouseAxisEvent(InputComponent.MouseAxis.X, evt.motion.x);
                        mouseAxisEvent(InputComponent.MouseAxis.Y, evt.motion.y);
                        break;

                    case Sdl.SDL_MOUSEBUTTONDOWN:
                        switch (evt.button.button)
                        {
                            case Sdl.SDL_BUTTON_LEFT:
                                button = MouseKeyBinding.MouseButton.LEFT;
                                break;
                            case Sdl.SDL_BUTTON_MIDDLE:
                                button = MouseKeyBinding.MouseButton.MIDDLE;
                                break;
                            case Sdl.SDL_BUTTON_RIGHT:
                                button = MouseKeyBinding.MouseButton.RIGHT;
                                break;
                        }
                        mouseButtonEvent(button, true);
                        break;

                    case Sdl.SDL_MOUSEBUTTONUP:
                        switch (evt.button.button)
                        {
                            case Sdl.SDL_BUTTON_LEFT:
                                button = MouseKeyBinding.MouseButton.LEFT;
                                break;
                            case Sdl.SDL_BUTTON_MIDDLE:
                                button = MouseKeyBinding.MouseButton.MIDDLE;
                                break;
                            case Sdl.SDL_BUTTON_RIGHT:
                                button = MouseKeyBinding.MouseButton.RIGHT;
                                break;
                        }
                        mouseButtonEvent(button, false);
                        break;

                    case Sdl.SDL_JOYBUTTONDOWN:
                        joyButtonEvent(evt.jbutton.which, evt.jbutton.button, true);
                        break;

                    case Sdl.SDL_JOYBUTTONUP:
                        joyButtonEvent(evt.jbutton.which, evt.jbutton.button, false);
                        break;

                    case Sdl.SDL_JOYAXISMOTION:
                        joyAxisEvent(evt.jaxis.which, evt.jaxis.axis, evt.jaxis.val);
                        break;

                    case Sdl.SDL_JOYHATMOTION:
                        joyHatEvent(evt.jhat.which, evt.jhat.hat, evt.jhat.val);
                        break;

                    case Sdl.SDL_QUIT:
                        return true;
                }
            }

            return false;
        }

        /**
         * Set current context for bindings. Creates context if it doesn't yet exist
         *
         * @param t The type of the bidings
         * @param context The context to use.
         */
        public void setContext(Type t, object context)
        {
            var k = new Tuple<Type, object>(t, context);
            var nullk = new Tuple<Type, object>(t, null);

            // Guarantee valid type
            if (!bindings.ContainsKey(nullk)) 
            {
                throw new Exception("InputComponent: Enum type does not exist in null context:" + t);
            }

            currentContexts[t] = context;

            // If bindings haven't been created for this context, create them
            if (!bindings.ContainsKey(k)) 
            {
                // Create empty bindings array
                addBindings(t);

                // Copy bindings for this type from the null context, where they are guaranteed to exist
                for (int i = 0; i < bindings[nullk].Length; i++) 
                {
                    bindings[k][i] = bindings[nullk][i].clone();
                }
            } 
        }

        public object getContext(Type t)
        {
            return currentContexts[t];
        }

        public virtual void normalize()
        {
        }
    }
}