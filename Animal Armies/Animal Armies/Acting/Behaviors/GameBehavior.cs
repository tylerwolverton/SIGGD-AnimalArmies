using System;
using System.Collections.Generic;
using Engine;

namespace Game
{
    public class GameBehavior : Behavior
    {
        public new GameWorld world
        {
            get
            {
                return base.world as GameWorld;
            }
            set
            {
                base.world = value;
            }
        }

        public new GameActor actor
        {
            get
            {
                return base.actor as GameActor;
            }
            set
            {
                base.actor = value;
            }
        }
        
        public GameBehavior(GameWorld world, GameActor actor, Tile tile = null, Handle script = null) : base(world, actor, tile, script: script) { }
    }
}
