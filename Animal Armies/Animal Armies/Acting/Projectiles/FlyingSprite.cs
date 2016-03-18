using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;

namespace Game
{
	class FlyingSprite: GameActor
	{
	public Vector2 from, to;
	public List<Tile> plannedPath;
	public FlyingSprite(GameWorld world, Vector2 position, Vector2 velocity, float diameter, Vector2 size, Vector2 world2model, int imgIndex, Vector2 to, AnimalActor carry=null, List<Tile> plannedPath=null)
            : base(world, new Vector2(position.x - (position.x % 32), position.y - (position.y % 32)), velocity, diameter, size, world2model, imgIndex, 10)
        {
			this.plannedPath = plannedPath;
			this.to=to;
			this.myBehavior= new FlyingBehavior(world,this, carry);
			
	    }
		

	}
}
