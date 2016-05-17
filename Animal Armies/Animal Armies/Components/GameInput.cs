using Tao.Sdl;
using Engine;

namespace Game
{
	public class GameInput : InputComponent
    {
        public new Game engine
        {
            get
            {
                return base.engine as Game;
            }
        }

        public GameInput(Game theEngine)
            : base(theEngine)
        {
            addBindings(typeof(ExampleBindings));
        }

        public enum ExampleBindings
        {
			CLICK,
            RIGHT,
            LEFT,
            UP,
            DOWN,
            ENTER,
            MENU,
            RIGHTCLICK,
			SELECTLEFT,
			SELECTRIGHT,
			COLLAPSE,
			UNCOLLAPSE
        }
     
        public override void initialize()
        {
            base.initialize();
			this[ExampleBindings.CLICK] = new SinglePressBinding(this,
			   new ButtonEvent[] { new MouseButtonEvent(MouseKeyBinding.MouseButton.LEFT), new JoyButtonEvent(0, 0) });
            this[ExampleBindings.RIGHT] = new SinglePressBinding(this,
                 new ButtonEvent[] { new KeyEvent(Sdl.SDLK_d), new JoyButtonEvent(0, 0) });
            this[ExampleBindings.LEFT] = new SinglePressBinding(this,
                 new ButtonEvent[] { new KeyEvent(Sdl.SDLK_a), new JoyButtonEvent(0, 0) });
            this[ExampleBindings.UP] = new SinglePressBinding(this,
                new ButtonEvent[] { new KeyEvent(Sdl.SDLK_w), new JoyButtonEvent(0, 0) });
            this[ExampleBindings.DOWN] = new SinglePressBinding(this,
                new ButtonEvent[] { new KeyEvent(Sdl.SDLK_s), new JoyButtonEvent(0, 0) });
            this[ExampleBindings.ENTER] = new SinglePressBinding(this,
                new ButtonEvent[] { new KeyEvent(Sdl.SDLK_RETURN), new JoyButtonEvent(0, 0) });
            this[ExampleBindings.MENU] = new SinglePressBinding(this,
                 new ButtonEvent[] { new KeyEvent(Sdl.SDLK_ESCAPE), new JoyButtonEvent(0, 0) });
            this[ExampleBindings.RIGHTCLICK] = new SinglePressBinding(this,
               new ButtonEvent[] { new MouseButtonEvent(MouseKeyBinding.MouseButton.RIGHT), new JoyButtonEvent(0, 0) });
			this[ExampleBindings.SELECTLEFT] = new SinglePressBinding(this,
				new ButtonEvent[] { new KeyEvent(Sdl.SDLK_q), new JoyButtonEvent(0, 0) });
			this[ExampleBindings.SELECTRIGHT] = new SinglePressBinding(this,
				new ButtonEvent[] { new KeyEvent(Sdl.SDLK_e), new JoyButtonEvent(0, 0) });
			
			this[ExampleBindings.COLLAPSE] = new SinglePressBinding(this,
			   new ButtonEvent[] { new KeyEvent(Sdl.SDLK_c), new JoyButtonEvent(0, 0) });
			this[ExampleBindings.UNCOLLAPSE] = new SinglePressBinding(this,
			   new ButtonEvent[] { new KeyEvent(Sdl.SDLK_v), new JoyButtonEvent(0, 0) });


		}

        //hacked in function to avoid "offcenter" problems when losing window focus.
        public override void normalize()
        {
            (this[ExampleBindings.CLICK] as SinglePressBinding).normalize();
        }
    }
}