using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;
namespace Game
{
	public class AnimalBehavior : GameBehavior
	{
		
		public AnimalBehavior(GameWorld world, GameActor actor)
			: base(world, actor)
		{
			this.actor = actor;
		}
		public override void run()
		{

			//actor.position = new Vector2(actor.position.X - (actor.position.X % 16), actor.position.Y - (actor.position.Y % 16));
		}
	}
}
