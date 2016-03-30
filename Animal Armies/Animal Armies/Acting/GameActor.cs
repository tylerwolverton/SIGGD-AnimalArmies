using System;
using System.Collections.Generic;
using Engine;

namespace Game
{
    public class GameActor : Actor, ILife
    {
        // Actor fields
        public Vector2 speed;

        // Masking Variables
        public int maskingCategory;
        public List<int> collidableActors = new List<int>();
        
        private Life _life;
        public Life life { get { return _life; } set { _life = value; } }
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

        public new GameBehavior myBehavior
        {
            get
            {
                return base.myBehavior as GameBehavior;
            }
            set
            {
                base.myBehavior = value;
            }
        }

        public GameActor(GameWorld world, Vector2 position, Vector2 velocity,float diameter,  Vector2 size, Vector2 world2model, int imgIndex, int maxlife)
            : base(world, position, velocity,diameter,size, world2model) 
        {
            life = new Life(this, maxlife);
			
		}

        // Remove collision from the game
        public override void collide(Actor a)
        {
            //Do nothing
        }

		public override void Update()
		{
			base.Update();
		}

    }
}
