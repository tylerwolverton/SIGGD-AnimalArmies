using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;

namespace Game
{

	class HumanPlayer : Player
	{
		SinglePressBinding click, enter, rightClick;
		AnimalActor selectedAnimalActor = null;
		Boolean mouseButtonReleased = true, enterButtonReleased = true, selectLeftReleased = true, selectRightReleased = true;

		int currentAutoselect = 0;
		// Declare tile sets for move and attack range
		IEnumerable<Tile> moveRange;
		IEnumerable<Tile> attackRange;

		protected LinkedList<AnimalActor> movedUnits;
        private int currentUnitCount;

		LinkedList<AnimalActor> storage;
		public Boolean stashed=false;
		public Boolean expand = false;
		float moveColMax, moveColMin, atkColMax, atkColMin;

		Color moveColor = new Color(0, 0, 1);
		Color atkColor = new Color(1, 0, 0);
        Color selectedColor = new Color(1, 1, 1);

		bool moveColHitMax = true;
		bool atkColHitMax = true;		

		public HumanPlayer(GameWorld world, team_t team)
			: base(world, team)
		{
			click = world.click;
			enter = world.enter;
            rightClick = world.rightClick;
			movedUnits = new LinkedList<AnimalActor>();
		}

		public override void startTurn()
		{
			movedUnits = new LinkedList<AnimalActor>();
            currentUnitCount = units.Count;

			if (currentUnitCount > 0)
			{
				world.cameraManager.moveCamera(units.First().position);
			}

            foreach (AnimalActor actor in units)
            {
                updateStatusEffects(actor);
            }

            System.Console.WriteLine("Number of units for player " + team + ": " + units.Count + "\n");
            selectedAnimalActor = null;
            //world.cameraManager.loadView(team, true);
		}

        private void finishTurn()
        {
            world.cameraManager.saveView(team);
            selectedAnimalActor = null;
            enterButtonReleased = false;
            world.endTurn();
        }

        /*
         * Evan's Voovoo
         * Change color of ambient light on tiles incrementally
         */
        private void CalculateTileFlash()
        {
            moveColMax = 0.45f;
            moveColMin = 0.0f;

            atkColMax = 0.45f;
            atkColMin = 0.0f;

            if (moveColHitMax)
            {
                moveColor.g -= 0.01f;

                if (moveColor.g <= moveColMin)
                {
                    moveColHitMax = false;
                }
            }
            else
            {
                moveColor.g += 0.01f;

                if (moveColor.g >= moveColMax)
                {
                    moveColHitMax = true;
                }
            }

            if (atkColHitMax)
            {
                atkColor.g -= 0.01f;
                atkColor.b -= 0.01f;

                if (atkColor.g <= atkColMin)
                {
                    atkColHitMax = false;
                }
            }
            else
            {
                atkColor.g += 0.01f;
                atkColor.b += 0.01f;

                if (atkColor.g >= atkColMax)
                {
                    atkColHitMax = true;
                }
            }
        }


		public override void Update()
		{
			//if (!stashed && world.collapse.isPressed)
			//{
			//	collapseUnits();
			//	stashed = true;
			//}
			//if (stashed && world.uncollapse.isPressed)
			//{
			//	expandUnits();

			//	stashed = false;

			//}
            CalculateTileFlash();
            //System.Console.WriteLine("Number of units in thing " + units.Count + " movedUnits" + movedUnits.Count +"\n");
            // End current turn if every animal has moved
            if (movedUnits.Count == currentUnitCount)
            {
                finishTurn();
				return;
            }

			if (selectedAnimalActor != null)
			{
                if (moveRange == null && selectedAnimalActor.canMove)
                {
                    moveRange = selectedAnimalActor.findPaths();
                }

				IEnumerator<Tile> adjacent = null;
				if (selectedAnimalActor.canMove)
				{
					adjacent = moveRange.GetEnumerator();
					while (adjacent.MoveNext())
                        if (adjacent.Current != null)
                            // Change color of ambient light on tiles
							adjacent.Current.color = moveColor;

                    // Don't light up tile that actor is on so player can tell which unit is selected
                    world.getTileAt(selectedAnimalActor.position).color = selectedColor;
				}


				if (selectedAnimalActor.canAct && !selectedAnimalActor.canMove)
				{
					adjacent = attackRange.GetEnumerator();
					while (adjacent.MoveNext())
						if (adjacent.Current != null)
                            // Change color of ambient light on tiles
                            adjacent.Current.color = atkColor;
                    
                    // Don't light up tile that actor is on so player can tell which unit is selected
                    world.getTileAt(selectedAnimalActor.position).color = selectedColor;
				}
			}
            // Reset the move and attack color values
			else
			{
				moveColor = new Color(0, 0, 1);
				atkColor = new Color(1, 0, 0);
			}

			// Handle end turn command
			if (enter.isPressed)
			{
				if (enterButtonReleased)
				{
                    finishTurn();
                    return;
				}
			}
			else
				enterButtonReleased = true;

            if (rightClick.isPressed)
            {
                // Animal has moved, return to the old position and cancel attack phase
                if (selectedAnimalActor != null)
                {
                    if (selectedAnimalActor.canMove == false)
                    {
                        selectedAnimalActor.position = selectedAnimalActor.oldPosition;
                        selectedAnimalActor.canMove = true;
                    }

                    selectedAnimalActor.anim.ticksPerFrame *= 2;
                }
                selectedAnimalActor = null;
                moveRange = null;
            }

			// Handle unit selection through clicks
			if (click.isPressed)
			{
				if (mouseButtonReleased)
				{
              	    mouseButtonReleased = false;

                    // Get position of the mouseclick in pixels
					Vector2 position = world.engine.graphicsComponent.camera.screen2World(world.engine.inputComponent.getMousePosition());
                    // Convert position to a multiple of 32
					position = new Vector2(position.x - (position.x % 32), position.y - (position.y % 32));
                    
					Tile clickedTile= world.getTileAt(position);
                    AnimalActor animalOnTile = world.getAnimalOnTile(clickedTile);

					if (selectedAnimalActor == null)
					{
                        if (animalOnTile != null && animalOnTile.canMove == true)
                        {
                            selectedAnimalActor = animalOnTile;
                            selectedAnimalActor.anim.ticksPerFrame /= 2;
                        }
					}

					else if (moveRange != null && moveRange.Contains(world.getTileAt(position)) && (animalOnTile == null || animalOnTile == selectedAnimalActor) && selectedAnimalActor.canMove)
					{
                        selectedAnimalActor.moveTile(world.getTileAt(position));

						attackRange = selectedAnimalActor.findAttackTiles();
						moveRange = null;
						//selected = null;
					}
                    else if (!selectedAnimalActor.canMove && attackRange.Contains(world.getTileAt(position)))
                    {
                        // If an animal is on the attacked tile, attack it (don't allow an animal to attack itself)
                        if (animalOnTile != null && clickedTile != world.getTileAt(selectedAnimalActor.position))
                        {
                           selectedAnimalActor.attackTile(clickedTile); 
                        }
                        // Update all of the attacking animal's variables and add to moved unit list
                        selectedAnimalActor.canAct = false;
                        movedUnits.AddFirst(selectedAnimalActor);
                        selectedAnimalActor.anim.ticksPerFrame *= 2;
                        selectedAnimalActor.changeActorSprite(true);
                        attackRange = null;
                        selectedAnimalActor.oldPosition = selectedAnimalActor.position;
                        selectedAnimalActor = null;
                    }
				}
			}
			else
			{
				mouseButtonReleased = true;
			}

			if (world.selectLeft.isPressed && (selectedAnimalActor==null || selectedAnimalActor.canMove==true))
			{
				if (selectLeftReleased)
				{
					currentAutoselect = (currentAutoselect + 1) % units.Count;
					while (movedUnits.Contains(units.ElementAt(currentAutoselect)))
					{
						currentAutoselect = (currentAutoselect + 1) % units.Count;
					}

                    moveRange = null;
                    if (selectedAnimalActor != null)
                    {
                        selectedAnimalActor.anim.ticksPerFrame *= 2;
                    }
                    selectedAnimalActor = units.ElementAt(currentAutoselect);
                    selectedAnimalActor.anim.ticksPerFrame /= 2;
					selectLeftReleased = false;
				}
			}
			else
			{
				selectLeftReleased = true;
			}

			if (world.selectRight.isPressed && (selectedAnimalActor == null || selectedAnimalActor.canMove == true))
			{
				if (selectRightReleased)
				{
					currentAutoselect = (currentAutoselect - 1);
					if (currentAutoselect < 0)
						currentAutoselect = units.Count-1;
					while (movedUnits.Contains(units.ElementAt(currentAutoselect)))
					{
						currentAutoselect = (currentAutoselect - 1);
						if (currentAutoselect<0)
							currentAutoselect=units.Count-1;
					}
					
					moveRange = null;
                    if (selectedAnimalActor != null)
                    {
                        selectedAnimalActor.anim.ticksPerFrame *= 2;
                    }
                    selectedAnimalActor = units.ElementAt(currentAutoselect);
                    selectedAnimalActor.anim.ticksPerFrame /= 2;
					selectRightReleased = false;
				}
			}
			else
			{
				selectRightReleased = true;
			}
		}

        /* TODO:
         * What do these functions do?  Why do they exist?
         * It would be very helpful if whoever wrote them also wrote comments.
         */
	    public void collapseUnits()
	    {
		    IEnumerator<AnimalActor> enu = units.GetEnumerator();
		    enu.MoveNext();
		    AnimalActor first= enu.Current;
		    storage = new LinkedList<AnimalActor>();
		    while (enu.MoveNext())
		    {
			    storage.AddFirst(enu.Current);
			
		    }
		    enu = storage.GetEnumerator();
		    while (enu.MoveNext())
		    {
			    enu.Current.die();
			    FlyingSprite sprite = new FlyingSprite(world, enu.Current.position, new Vector2(0, 0),1 , new Vector2(1,1), Vector2.Zero, 0, first.position, null);
			    sprite.anim = enu.Current.anim;
			    world.addActor(sprite);
		    }

	    }

	    public void expandUnits()
	    {
	        IEnumerator<AnimalActor> enu = storage.GetEnumerator();
		    Tile current = world.getTileAt(units.First.Value.position);
		    LinkedList<Tile> tiles = new LinkedList<Tile>();
		    tiles.AddFirst(current);
		    AnimalActor currentActor=null;
		    while(tiles.Count!=0 && (currentActor!=null || enu.MoveNext()))
		    {
			    if (currentActor == null)
				    currentActor = enu.Current;
			    current= tiles.First.Value;
			    tiles.RemoveFirst();
			
			    IEnumerator<Tile> tileEnu= current.adjacent.GetEnumerator();
			    while (tileEnu.MoveNext())
			    {
				    if (tileEnu.Current!=null)
				    tiles.AddLast(tileEnu.Current);
			    }
				     AnimalActor a = world.getAnimalOnTile(current);
				     if (a == null && current!=null)
				     {
						     enu.Current.position = new Vector2(current.x, current.y);
						     FlyingSprite sprite = new FlyingSprite(world, units.First.Value.position, new Vector2(0, 0), 1, new Vector2(1, 1), Vector2.Zero, 0, enu.Current.position, enu.Current);
						 
					         sprite.anim=enu.Current.anim;
						     world.addActor(enu.Current);
						     enu.Current.removeMe = false;
						     enu.Current.changeTeam(team);
						     world.addActor(sprite);
						     //enu.Current.customDraw = AnimalActor.drawInvisible;
						     currentActor = null;
				     }
			     }
	        }
	}
}
