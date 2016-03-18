using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;


namespace Game
{
	class FlyingBehavior: GameBehavior
	{
		FlyingSprite sprite;
		AnimalActor carry;
		IEnumerator<Tile> path;
		int speed = 8;
		 public FlyingBehavior(GameWorld world, GameActor actor, AnimalActor carry)
            : base(world, actor)
        {
            sprite = (FlyingSprite)actor;
			this.carry = carry;
			if (sprite.plannedPath != null)
			{
				path = sprite.plannedPath.GetEnumerator();
				path.MoveNext();
				sprite.to = new Vector2(path.Current.x, path.Current.y);
			}

        }

		 public override void run()
		 {

				 Vector2 pos = sprite.position;
				 if (pos.x > sprite.to.x)
					 pos.x -= speed;
				 if (pos.x < sprite.to.x)
					 pos.x += speed;
				 if (pos.y > sprite.to.y)
					 pos.y -= speed;
				 if (pos.y < sprite.to.y)
					 pos.y += speed;
				 sprite.velocity = Vector2.Zero;
				 if (Math.Abs(pos.y - sprite.to.y) < 2 && Math.Abs(pos.x - sprite.to.x) < 2)
				 {
					 if (path == null || !path.MoveNext())
					 {
						 sprite.removeMe = true;
						 if (carry != null)
							 carry.customDraw = AnimalActor.drawWithHealthBar;
					 }
					 else
					 {
						 sprite.to = new Vector2(path.Current.x, path.Current.y);

					 }
				
				 }
			 


		 }
	}
}
