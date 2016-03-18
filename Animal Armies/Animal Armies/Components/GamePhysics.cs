using System;
using System.Collections.Generic;
using Engine;
using System.Diagnostics;

namespace Game
{
    public class GamePhysics : PhysicsComponent
    {
        public new Game engine
        {
            get
            {
                return base.engine as Game;
            }
        }

        public override void Update()
        {
            base.Update();
        }

        public GamePhysics(Game theEngine)
            : base(theEngine)
        {
        }
    }
}
