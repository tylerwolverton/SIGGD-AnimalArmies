using System;
using System.Collections.Generic;
using Engine;

namespace Game
{
    public class GameAudio : AudioComponent
    {
        public new Game engine
        {
            get
            {
                return base.engine as Game;
            }
        }

        public GameAudio(Game theEngine)
            : base(theEngine)
        {
        }
    }
}
