using System;
using System.Collections.Generic;
using Engine;

namespace Game
{
    public class GameActorFactory : ActorFactory
    {
        private GameWorld world;

        public GameActorFactory() { }

        public GameActorFactory(GameWorld world)
        {
            this.world = world;

            names = new Dictionary<string, int>()
            {
			   {"Rat", 0},
			   {"TRex", 1},
			   {"Turtle",2},
			   {"Red Base",3},
			   {"Blue Base",4},
               {"Yellow Base",5},
			   {"Purple Base",6},
               {"Cursor",7},
			   {"Explosion",8},
               {"Robin",9},
			   {"Llama",10},
               {"Blob",11},
               {"Penguin", 12}
            };
        }

        public override Actor createActor(int id, Vector2 position, Vector2 velocity = null, double color = -1)
        {
            ResourceComponent rc = this.world.engine.resourceComponent;
            GameActor a = null;
            if ((position.x >= 0 && position.x < world.width * Tile.size && position.y >= 0 && position.y < world.height * Tile.size))
            {
                switch (id)
                {
                    case 0:
                        a = new Rat(world, position, new Vector2(0, 0), 2, new Vector2(2, 2), new Vector2(0, 0), 0);
						return a;
					case 1:
						a = new TRex(world, position, new Vector2(0, 0), 2, new Vector2(2, 2), new Vector2(0, 0), 0);
						return a;
					case 2:
						a = new Turtle(world, position, new Vector2(0, 0), 2, new Vector2(2, 2), new Vector2(0, 0), 0);
						return a;
					case 3:
						a = new Base(world, position, new Vector2(0, 0), 2, new Vector2(2, 2), new Vector2(0, 0), 0,team_t.Red, Color.RED);
						return a;
					case 4:
						a = new Base(world, position, new Vector2(0, 0), 2, new Vector2(2, 2), new Vector2(0, 0), 0, team_t.Blue, Color.BLUE);
						return a;
                    case 5:
                        a = new Base(world, position, new Vector2(0, 0), 2, new Vector2(2, 2), new Vector2(0, 0), 0, team_t.Yellow, (new Color(255,255,0)));
                        return a;
                    case 6:
                        a = new Base(world, position, new Vector2(0, 0), 2, new Vector2(2, 2), new Vector2(0, 0), 0, team_t.Purple, (new Color(255, 0, 255)));
                        return a;
                    case 8:
                        a = new Explosion(world, position, new Vector2(0, 0), 2, new Vector2(2, 2), new Vector2(0, 0), 0);
                        return a;
                    case 9:
                        a = new Robin(world, position, new Vector2(0, 0), 2, new Vector2(2, 2), new Vector2(0, 0), 0);
                        return a;
                    case 10:
                        a = new Llama(world, position, new Vector2(0, 0), 2, new Vector2(2, 2), new Vector2(0, 0), 0);
                        return a;
                    case 11:
                        a = new Blob(world, position, new Vector2(0, 0), 2, new Vector2(2, 2), new Vector2(0, 0), 0);
                        return a;
                    case 12:
                        a = new Penguin(world, position, new Vector2(0, 0), 2, new Vector2(2, 2), new Vector2(0, 0), 0);
                        return a;
                }
            }

            return a;
        }

		public AnimalActor createRandomAnimalActor(Vector2 position, Vector2 velocity = null, double color = -1)
		{
			ResourceComponent rc = this.world.engine.resourceComponent;
            AnimalActor a = null;
			int numberOfAnimals = 6;
			if ((position.x >= 0 && position.x < world.width * Tile.size && position.y >= 0 && position.y < world.height * Tile.size))
			{
				int id = MirrorEngine.randGen.Next(numberOfAnimals);
				switch (id)
				{
					case 0:
						a = new Rat(world, position, new Vector2(0, 0), 2, new Vector2(2, 2), new Vector2(0, 0), 0);
						return a;
					case 1:
						a = new TRex(world, position, new Vector2(0, 0), 2, new Vector2(2, 2), new Vector2(0, 0), 0);
						return a;
					case 2:
						a = new Turtle(world, position, new Vector2(0, 0), 2, new Vector2(2, 2), new Vector2(0, 0), 0);
						return a;
                    case 3:
                        a = new Robin(world, position, new Vector2(0, 0), 2, new Vector2(2, 2), new Vector2(0, 0), 0);
                        return a;
                    case 4:
                        a = new Llama(world, position, new Vector2(0, 0), 2, new Vector2(2, 2), new Vector2(0, 0), 0);
                        return a;
                    case 5:
                        a = new Penguin(world, position, new Vector2(0, 0), 2, new Vector2(2, 2), new Vector2(0, 0), 0);
                        return a;
				}
			}
				return a;

		}
    }
}
