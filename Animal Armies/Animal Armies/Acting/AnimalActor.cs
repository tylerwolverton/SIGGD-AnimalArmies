using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;
using Game;
using System.Diagnostics;


namespace Game
{
    public enum tileDefense
    {
        grass = 0,
        water = 0,
        forest = 1,
        mountain = 2,
        hills = 1
    }

    public class AnimalActor : GameActor
    {
        //TODO: Getters and Setters for these? Nope
        public int moveRange = 4; //I set moveRange to be set in the findPaths by passing findPaths an int
        public int attackRange = 1;

        public int attackDamage = 10;
        public int defense = 5;
        public int baseAttack;
        public int baseDefense;
        public double criticalRange = .9;

        public const int maxLevel = 3;
        public int expPoints = 0;
        public int level = 0;
        public int[] expLevel = {0, 4, 9, 15};

        public team_t team;

        public Vector2 oldPosition = new Vector2(0,0);
        public bool canMove = false;
        public bool canAct = false;
        protected TileCost movementType = TileCost.Land;
		
		public int spawnCost = 1;

        public Color teamColor;

        // Status variables
        public bool isPoisoned = false;
        public int poisonCount = 0;
        public int poisonAttack;
        public int poisonDefense;

        private List<Tile> pathsFromCurrentTile = null;

		int[,] pathCost = null;
        /*
         * Construct a new animal actor
         */
        public AnimalActor(GameWorld world, Vector2 position, Vector2 velocity, float diameter, Vector2 size, Vector2 world2model, int imgIndex, int maxlife = 10)
            : base(world, new Vector2(position.x - (position.x % 32), position.y - (position.y % 32)), velocity, diameter, size, world2model, imgIndex, maxlife)
        {
            myBehavior = new AnimalBehavior(world, this);
            position = new Vector2(position.x - (position.x % 32), position.y - (position.y % 32));
            // keep track of old position for cancellation of actions
            oldPosition = position;
			life.deathEvent += new Life.DeathEventHandler(die);

            //This is to draw a health bar on the animal actors
            defaultDraw = false;
            customDraw = drawWithHealthBar;
        }

        /*************************************** Damage and Death Functions ***************************************/

        /**
         * Death Function, removes it from the teams and currentActors, set it to be cleaned up.
         * 
         */
        public void die()
        {
            TeamDictionary.TeamDict[team].ActorList.Remove(this);
            world.currentActors.Remove(this);
            removeMe = true;
        }

        /*
         * Apply any extra effects to the actor from the attacking animal  
         */
        public void applyStatusEffect(AnimalActor attacker)
        {
            if (attacker.actorName == "Rat")
            {
                this.attackDamage = this.poisonAttack;
                this.defense = this.poisonDefense;
                
                this.poisonCount = 4;
                this.isPoisoned = true;
            }
        }

        private void checkEndOfGame()
        {
            int teamCount = 0;
            //
            foreach (LinkedList<AnimalActor> team in world.teams)
            {
                if(team.Count() > 0)
                {
                    teamCount++;
                }
            }

            if (teamCount <= 1)
            {
                world.engine.quit = true;
            }
        }

        /*************************************** Movement Functions ***************************************/

        /**
         * 
         * 
         */
        public List<Tile> findPaths()
        {
            // Cache for lazy evaluation
            if (pathsFromCurrentTile != null)
            {
                return pathsFromCurrentTile;
            }
		
            pathsFromCurrentTile = new List<Tile>();
            Tile center = world.getTileAt(position);

            // Assuming animal can move moveRange in any direction, search space is a square of size moverange*2+1
            int[,] moveCost = new int[moveRange * 2 + 1, moveRange * 2 + 1];

            for (int i = 0; i < moveRange * 2 + 1; i++)
            {
                for (int j = 0; j < moveRange * 2 + 1; j++)
                {
                    moveCost[i, j] = -1;
                }
            }

            // Store whether tile has been visited
            Boolean[,] visited = new Boolean[moveRange * 2 + 1, moveRange * 2 + 1];

            for (int i = 0; i < moveRange * 2 + 1; i++)
            {
                for (int j = 0; j < moveRange * 2 + 1; j++)
                {
                    visited[i, j] = false;
                }
            }

            // Real tile index-offset= array index.
            int offsetx = center.xIndex - (moveRange);
            int offsetY = center.yIndex - (moveRange);

            // Set the origin tile's cost to 0
            moveCost[moveRange, moveRange] = 0;

            // 
            Vector2 currentTileIndex;
            while (true)
            {
                currentTileIndex = new Vector2(-1, -1);

                // Find minimum cost, reachable non-visited tile.
                for (int i = 0; i < moveRange * 2 + 1; i++)
                {
                    for (int j = 0; j < moveRange * 2 + 1; j++)
                    {
                        if (visited[i, j] || moveCost[i, j] == -1) // If visited or not-reachable, continue.
                        {
                            continue;
                        }
                        // If current not yet set, match first not visited.
                        if (currentTileIndex.x == -1)
                        {
                            currentTileIndex = new Vector2(i, j);
                        }
                        // Find tile with minimum move cost.
                        else if (moveCost[(int)currentTileIndex.x, (int)currentTileIndex.y] > moveCost[i, j])
                        {
                            currentTileIndex = new Vector2(i, j);
                        }
                    }
                }
                // If current was not set in loop, no paths left to evaluate.
                if (currentTileIndex.x == -1)
                {
                    break;
                }

                int currentCost = moveCost[(int)currentTileIndex.x, (int)currentTileIndex.y];
                for (int x = 1; x >= -1; x--)
                    for (int y = 1; y >= -1; y--)
                    {
                        // Only move to adjacent tiles.
                        if (Math.Abs(x) == Math.Abs(y))
                        {
                            continue;
                        }
                        // If within movable square process adjacent tile.
                        if (currentTileIndex.x + x <= moveRange * 2 && currentTileIndex.y + y <= moveRange * 2 && currentTileIndex.x + x >= 0 && currentTileIndex.y + y >= 0)
                        {
                            Tile checking = world.getTile((int)currentTileIndex.x + offsetx + x, (int)currentTileIndex.y + offsetY + y);
                            if (checking == null)
                                continue;
                            int tileCost = movementType[(checking as GameTile).type];
                            
                            // Don't allow animal to move past an enemy unit
                            GameActor act = world.getActorOnTile(checking);
                            if (act != null && act is AnimalActor && ((AnimalActor)act).team != this.team)
                            {
                                tileCost = 999;
                            }

                            // If not visited and cheaper from this path or if not previously reachable set cost as cost of path through current.
                            if (!visited[(int)currentTileIndex.x + x, (int)currentTileIndex.y + y]
                                && (moveCost[(int)currentTileIndex.x + x, (int)currentTileIndex.y + y] > currentCost + tileCost
                                || moveCost[(int)currentTileIndex.x + x, (int)currentTileIndex.y + y] == -1))
                            {

                                moveCost[(int)currentTileIndex.x + x, (int)currentTileIndex.y + y] = currentCost + tileCost;
                            }
                        }

                    }
                if (currentCost <= moveRange)
                {
                    pathsFromCurrentTile.Add(world.getTile((int)currentTileIndex.x + offsetx, (int)currentTileIndex.y + offsetY));
                }
                visited[(int)currentTileIndex.x, (int)currentTileIndex.y] = true;
            }
			pathCost = moveCost;
            return pathsFromCurrentTile;
        }




		public Boolean canMoveTo(Tile target)
        {
            if (!canMove || target == null)
            {
                return false;
            }

            if (((GameTile)target).manhattan((GameTile)this.curTile) > this.moveRange)
            {
                return false;
            }

            if (!findPaths().Contains(target))
            {
                return false;
            }

            GameActor targetActor = world.getActorOnTile(target);
            if (targetActor != null && targetActor != this && targetActor is AnimalActor)
            {
                return false;
            }
            return true;
        }
        
        /*
         *  Searches for available tile to spawn and actor 
         */
        public GameTile FindOpenTile(AnimalActor actor, GameTile initialTile)
        {
            if (world.getActorOnTile(initialTile) == null && actor.movementType[initialTile.type] != 999)
            {
                return initialTile;
            }
            
            GameTile possibleTile;
            int yOffset = 0;

            // Increase the radius of the search square
            for (int radius = 1; radius < Math.Max(world.width, world.height); radius++)
            {
                // Check each tile in the top row of the square
                if (initialTile.yIndex - radius > 0)
                {
                    for (int i = -radius; i < radius; i++)
                    {
                        if (initialTile.xIndex + i > 0)
                        {
                            if (initialTile.xIndex + i > world.width)
                            {
                                break;
                            }

                            possibleTile = (world.getTile(initialTile.xIndex + i, initialTile.yIndex - radius) as GameTile);
                            if (world.getActorOnTile(possibleTile) == null && actor.movementType[possibleTile.type] != 999)
                            {
                                return possibleTile;
                            }
                        }
                    }
                }
                else
                {
                    yOffset = -(initialTile.yIndex - radius);
                }

                // Check each tile in the sides of the square
                for (int sideY = -radius + yOffset; sideY < radius; sideY++)
                {
                    if (initialTile.xIndex - radius > 0)
                    {
                        possibleTile = (world.getTile(initialTile.xIndex - radius, initialTile.yIndex + sideY) as GameTile);
                        if (possibleTile != null && world.getActorOnTile(possibleTile) == null && actor.movementType[possibleTile.type] != 999)
                        {
                            return possibleTile;
                        }
                    }

                    if (initialTile.xIndex + radius < world.width)
                    {
                        possibleTile = (world.getTile(initialTile.xIndex + radius, initialTile.yIndex + sideY) as GameTile);
                        if (world.getActorOnTile(possibleTile) == null && actor.movementType[possibleTile.type] != 999)
                        {
                            return possibleTile;
                        }
                    }
                }

                // Check each tile in the bottom row of the square
                if (initialTile.yIndex + radius < world.height)
                {
                    for (int i = -radius; i < radius; i++)
                    {
                        if (initialTile.xIndex + i < world.width)
                        {
                            break;
                        }

                        possibleTile = (world.getTile(initialTile.xIndex + i, initialTile.yIndex + radius) as GameTile);
                        if (world.getActorOnTile(possibleTile) == null && actor.movementType[possibleTile.type] != 999)
                        {
                            return possibleTile;
                        }
                    }
                }

            }
            return null;
        }

        /**
         * Move to the target tile, if there is another animal there do not move there.
         */
        public Boolean moveTile(Tile target)
        {
            if (canMoveTo(target)) {
				Tile currentTile = world.getTileAt(position);
                
                canMove = false;
                pathsFromCurrentTile = null;
				

				// Real tile index-offset= array index in move costs array.

				int offsetx = currentTile.xIndex - (moveRange);
				int offsety = currentTile.yIndex - (moveRange);

				int costX = target.xIndex - offsetx;
				int costY = target.yIndex - offsety;

				//Starting backwards from target tile, find cheapest path back to current tile using previously calculated movement cost array.

				List<Tile> movePath = new List<Tile>();
				movePath.Add(target);
				Tile currentVisit= target;
				while (currentVisit != currentTile)
				{
					int low=int.MaxValue;
					int lowX = -1;
					int lowY = -1;
					if (costX - 1 > -1 && pathCost[costX-1, costY ] != -1)
					{
						low = pathCost[costX - 1, costY];
						lowX = costX - 1;
						lowY = costY;
					}
					if (costY - 1 > -1)
					{
						if (pathCost[costX, costY - 1] < low && pathCost[costX, costY -1] != -1 )
						{
							low = pathCost[costX, costY - 1];
							lowX = costX;
							lowY = costY - 1;
						}

					}

					if (costX + 1 < pathCost.GetLength(0))
					{
						if (pathCost[costX + 1, costY] < low && pathCost[costX+1, costY] != -1)
						{
							low = pathCost[costX + 1, costY];
							lowX = costX + 1;
							lowY = costY;
						}

					}

					if (costY + 1 < pathCost.GetLength(1))
					{
						if (pathCost[costX, costY + 1] < low && pathCost[costX, costY + 1]!=-1)
						{
							low = pathCost[costX, costY + 1];
							lowX = costX;
							lowY = costY + 1;
						}

					}
					currentVisit=world.getTile(lowX + offsetx, lowY + offsety);
					
					movePath.Add(currentVisit);
					costX = lowX;
					costY = lowY;


				}
				movePath.Reverse();
				
				FlyingSprite sprite = new FlyingSprite(world, this.position, new Vector2(0, 0), 1, new Vector2(1, 1), Vector2.Zero, 0, new Vector2(target.x,target.y), this,movePath);
				this.position = new Vector2(target.x, target.y);
				sprite.anim = this.anim;
						
				world.addActor(sprite);
				this.customDraw = AnimalActor.drawInvisible;
				

			   

                return true;
            }
            return false;
        }

        /*************************************** Attack Functions ***************************************/

        /**
        * Finds attack squares: 
        * if melee only return adjacent tiles  
        * if ranged... TODO.
        */
        public List<Tile> findAttackTiles()
        {
			List<Tile> tilesInRange = new List<Tile>();
			Tile currentTile = world.getTileAt(position);
			for (int i = -attackRange; i <= attackRange; i++)
				for (int j = -attackRange; j <= attackRange; j++)
				{
					if ((Math.Abs(i) + Math.Abs(j) <= attackRange) && world.getTile(currentTile.xIndex + i, currentTile.yIndex + j) != null)
						tilesInRange.Add(world.getTile(currentTile.xIndex + i, currentTile.yIndex + j));
				}
			return tilesInRange;
        }

        /**
        * Called when another animalActor fights this animal actor. We are defending.
        * 
        * Damage Calculation, each AnimalActor attack each other in a fight.
        * 
        * Damage is attack multiplied by the percent of health we have left of the attacker; 
        *           then we subtract the defense of the defender from the damage.
        *                  
        *          
        * TODO: Ranged Attacks?
        * TODO: Do Checks for what we can attack here.
        */
        private void fight(AnimalActor attacker)
        {
            // Get a random number to check if the attack is a critical hit
            double criticalValue = MirrorEngine.randGen.NextDouble();
            double modifiedAttack = attacker.attackDamage;

            // Check random value against
            if (criticalValue >= attacker.criticalRange)
            {
                modifiedAttack = attacker.attackDamage * 2;
                applyStatusEffect(attacker);
            }

            // Apply damage to defender
            GameTile currentTile = (GameTile)world.getTileAt(this.position.x, this.position.y);
            Life.damage(this, (int)Math.Max((modifiedAttack - (this.defense + currentTile.defense)), 0));

            // Add explosion to show damage
            GameActor explosion = new Explosion(world, new Vector2(position.x, position.y + 2), new Vector2(0, 0), 1, new Vector2(2, 2), new Vector2(0, 0), 0);
            world.addActor(explosion);

            if (life.dead)
            {
                attacker.expPoints += this.spawnCost;
                
                checkLevelUp(attacker, this);
                checkEndOfGame();
                die();
                return;
            }

            // Apply retaliation damage to attacker if defender can hit attacker
            GameTile AttackerTile = (GameTile)world.getTileAt(attacker.position.x, attacker.position.y);

			if (this.findAttackTiles().Contains(AttackerTile))
			{

                Life.damage(attacker, (int)Math.Max((attackDamage - (attacker.defense + AttackerTile.defense)), 0));

                if (attacker.life.dead)
                {
                    // Add explosion to show damage
                    this.expPoints += attacker.spawnCost;
                    
                    GameActor attackerExplosion = new Explosion(world, new Vector2(attacker.position.x, attacker.position.y + 2), new Vector2(0, 0), 1, new Vector2(2, 2), new Vector2(0, 0), 0);
                    world.addActor(attackerExplosion);
                    
                    checkLevelUp(this, attacker);
                    checkEndOfGame();
                    attacker.die();
                    return;
                }
            }
        }

		

        public Boolean canAttackActor(AnimalActor target)
        {
            if (!canAct || target == null)
            {
                return false;
            }
            return true;
        }

        /**
         * Attacks the target tile, if there is an actor on that tile fight it.
         */
        public Boolean attackTile(Tile target)
        {

            AnimalActor animalTarget = world.getAnimalOnTile(target);
			if (canAttackActor(animalTarget))
			{
				//If adjacent tiles contain the target tile
				if (findAttackTiles().Contains(target))
				{

					if (this.attackRange > 1)
					{
						FlyingSprite sprite = new FlyingSprite(world, this.position, new Vector2(0, 0), 1, new Vector2(1, 1), Vector2.Zero, 0, new Vector2(target.x, target.y));
						sprite.anim = new Animation(world.engine.resourceComponent, "Sprites/011_blob/");
						world.addActor(sprite);
						animalTarget.fight(this);
					}
					else if (this.movementType.Equals(animalTarget.movementType) || this.movementType.Equals(TileCost.Air))
					{
						List<Tile> hittingAnim = new List<Tile>();
						hittingAnim.Add(target);
						hittingAnim.Add(curTile);

						FlyingSprite sprite = new FlyingSprite(world, this.position, new Vector2(0, 0), 1, new Vector2(1, 1), Vector2.Zero, 0, new Vector2(curTile.x,curTile.y), this, hittingAnim);
						
						sprite.anim = this.anim;

						world.addActor(sprite);
						this.customDraw = AnimalActor.drawInvisible;

						animalTarget.fight(this);
					}

					canAct = false;
					return true;
				}
			}
            return false;
        }

        /*************************************** Stat Functions ***************************************/

        /**
         * Returns a stat in the range from .5 to 1.5 of the original stat
         */
        public int randomizeStat(int stat)
        {
            if (MirrorEngine.randGen.Next(100) < 50)
            {
                return stat - MirrorEngine.randGen.Next(stat / 2);
            }
            else
            {
                return stat + MirrorEngine.randGen.Next(stat / 2);
            }
        }

        /**
         * Level up an actor if experience threshold met. Add up to 2 stat points to attack and defense.
         */
        public void checkLevelUp(AnimalActor killer, AnimalActor victim)
        {
            if (killer.level == maxLevel)
            {
                return;
            }

            if (killer.expPoints >= expLevel[killer.level + 1])
            {
                killer.level++;
                killer.attackDamage += MirrorEngine.randGen.Next(3);
                killer.defense += MirrorEngine.randGen.Next(3);

                ProduceOffspring(killer);
            }
        }

        private void ProduceOffspring(AnimalActor parent)
        {
            GameTile spawnLocation = FindOpenTile(parent, (world.getTileAt(parent.position) as GameTile));
            AnimalActor offspring = (AnimalActor)world.actorFactory.createActor(world.actorFactory.getActorId(parent.actorName), new Vector2(spawnLocation.x, spawnLocation.y));
            offspring.teamColor = parent.teamColor;
            offspring.changeTeam(parent.team);
            offspring.changeActorSprite(true);
            world.addActor(offspring);
        }

        /*************************************** Graphics Functions ***************************************/

        //Custom draw method for animal actors with health bars.
        public static void drawWithHealthBar(Actor a)
        {
            //Readability vars...
            AnimalActor aa = (AnimalActor)a;
            Handle tex = aa.sprite;
            Vector2 spritePos = aa.position + aa.world2model;
            Color actorTint = aa.tint * aa.color;
            GraphicsComponent gcomp = aa.world.engine.graphicsComponent;

            //Draw the animal's sprite
            gcomp.drawTex(tex, (int)(spritePos.x + aa.xoffset), (int)(spritePos.y + aa.yoffset), actorTint);

            //Draw the health bar
            int hBarW = 32; //TODO: Grab the width of the sprite programmatically
            int hBarH = hBarW / 8;
            //Draw the border of the health bar
            gcomp.drawRect((int)(spritePos.x + aa.xoffset), (int)(spritePos.y + aa.yoffset), hBarW, hBarH, Color.BLACK);
            //Draw the health
            hBarW = (int)(hBarW * (aa.life.health / aa.life.maxlife));
            gcomp.drawRect((int)(spritePos.x + aa.xoffset + 1), (int)(spritePos.y + aa.yoffset + 1), hBarW - 2, hBarH - 2, aa.teamColor);
        }

		public static void drawInvisible(Actor a)
		{



		}


        /**
         * Changes the Animal Actors team, overloaded later.
         * 
         */
        public virtual void changeTeam(team_t newTeam)
        {
            team = newTeam;
            world.addToTeam(this, team);
        }

        /**
         * Changes the sprite of an actor. 
         * Pass true to the function to grey out an actor at the end of its turn
         **/
        public virtual void changeActorSprite(bool isGrey) { }

        /*************************************** Tile Access Functions ***************************************/

        /**
        * Tile Cost
        * Define new tileCost with: new TileCost(grassCost, forestCost, mountainCost, waterCost);
        * or use one of the defaults of Land, Sea or Air.
        */
        public struct TileCost
        {
            //We can access this with movementCost.grass etc.
            public int grass, forest, mountain, water, hills;

            public TileCost(int grass, int forest, int mountain, int water, int hills)
            {
                this.grass = grass;
                this.forest = forest;
                this.mountain = mountain;
                this.water = water;
                this.hills = hills;
            }

            //So we can access this using a string. movementCost["grass"] etc.
            public int this[string tileType]
            {
                get
                {
                    switch (tileType)
                    {
                        case "grass":
                            return grass;
                        case "forest":
                            return forest;
                        case "mountain":
                            return mountain;
                        case "water":
                            return water;
                        case "hills":
                            return hills;
                        default:
                            return 999; //We probably don't want the player moving 
                    }
                }
                set { }
            }

            //Actor movement type defaults.
            public static TileCost Land = new TileCost(1, 2, 3, 999, 2);
            public static TileCost Sea = new TileCost(999, 999, 999, 1, 999);
            public static TileCost Air = new TileCost(1, 1, 1, 1, 1);
        }
    }
}
