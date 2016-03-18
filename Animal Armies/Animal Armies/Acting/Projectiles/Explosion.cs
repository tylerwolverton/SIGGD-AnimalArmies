using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;
using Game;


namespace Game
{
    public class Explosion:GameActor
    {
        public Explosion(GameWorld world, Vector2 position, Vector2 velocity, float diameter, Vector2 size, Vector2 world2model, int imgIndex)
            : base(world, new Vector2(position.x, position.y), velocity, diameter, size, world2model, imgIndex, 1)
        {
            // Give explosion an animation
            this.anim = new Animation(world.engine.resourceComponent, "Sprites/008_explosion/", 0, 0, 2, false);
            
            // Create a new animation action to remove explosion from game
            Animation.Action destroy = (frame) =>
            {
                removeMe = true;
            };

            // Trigger destroy function after last frame of animation
            this.anim.addEndAct(destroy);
        }

        public override void Update()
        {
            base.Update();
        }


    }
}
