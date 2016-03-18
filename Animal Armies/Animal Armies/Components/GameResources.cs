using System;
using System.Collections.Generic;
using Engine;

namespace Game
{
    public class GameResources : ResourceComponent
    {
        public new Game engine
        {
            get
            {
                return base.engine as Game;
            }
        }

        public GameResources(MirrorEngine theEngine)
            : base(theEngine)
        {
        }
    }
}
