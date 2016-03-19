using System;
using System.Collections.Generic;
using System.Linq;
using Engine;
using System.Threading;
using System.Diagnostics;
using Game.AI;

namespace Game
{
    public class GameWorld : World
    {
		public SinglePressBinding click, enter, rightClick, selectLeft, selectRight, collapse, uncollapse;
        public static int numTeams = 5; 
        public int currentTeam = 0;
        public int unitsMoved; // number of units moved this turn
        public LinkedList<AnimalActor>[] teams = new LinkedList<AnimalActor>[numTeams];
        public team_t[] teamList = new team_t[] {team_t.None,team_t.Red, team_t.Blue, team_t.Purple, team_t.Yellow }; // Define team colors
		public String[] teamBanner = new String[] { "", "GUI\\002_TeamBoxes\\Blue_team.png", "GUI\\002_TeamBoxes\\Purple_team.png", "GUI\\002_TeamBoxes\\Red_team.png","GUI\\002_TeamBoxes\\Yellow_team.png"};
        public List<AnimalActor> currentActors;
        public team_t currentColor;
		public AI.ComputerPlayer AI;
        public GameTile[,] tiles;

        public GameTile highlightTile;
        public Actor highLightPlayer;
        public GUITextBox locationBox;
        public GUITextBox playerInfoBox;

		public GUILabel teamBox;

		public int teamBoxCooldown=0;
		public Player[] players = new Player[numTeams];

        public CameraManager cameraManager;

        public new Game engine
        {
            get
            {
                return base.engine as Game;
            }
        }

        public GameWorld(Game theEngine, Mapfile map) :base(theEngine, map)
        {

            cameraManager = new CameraManager(engine, numTeams);
			// calls the base constructor
        }

        protected override void constructGameActorFactory()
        {
            actorFactory = new GameActorFactory(this);
        }

        public override void setWorldBehavior(Handle script = null)
        {
            if (script == null || script.key == "") return;
            myBehavior = new GameBehavior(this, null, script: script);
        }

        public override Behavior constructTileBehavior(Tile tile, Handle script = null)
        {
            if (tile == null || script == null || script.key == "") return null;
            return new GameBehavior(this, null, tile, script);
        }

        /*************************************** Team Functions ***************************************/ 

		public void addToTeam(AnimalActor recruit, team_t newTeam)
		{
			teams[(int)newTeam].AddLast(recruit);
		}

        public void readyTeam(team_t team)
        {
            unitsMoved = 0;
            IEnumerator<AnimalActor> i = teams[currentTeam].GetEnumerator();
            currentActors= new List<AnimalActor>();
			
            while (i.MoveNext())
            {
                currentActors.Add(i.Current);

                i.Current.canMove = true;
                i.Current.canAct = true;
            }
        }

        public void unreadyTeam(team_t team)
        {
            unitsMoved = 0;
            IEnumerator<AnimalActor> i = teams[currentTeam].GetEnumerator();
            while (i.MoveNext())
            {
                i.Current.canMove = false;
                i.Current.canAct = false;
                i.Current.changeActorSprite(false);
            }
        }

        // Switch focus to the next team in the array
        public void endTurn()
        {
            // increment current team and ready it while unreadying the old team

            unreadyTeam(currentColor);
            // set currentTeam to the next team in the list (accounting for wraparound with a mod)

            currentTeam = (currentTeam + 1) % (numTeams);
            currentColor = teamList[currentTeam];

			if (currentTeam != 0)
			{
				teamBox.texture = new Handle(engine.resourceComponent, teamBanner[currentTeam]);
				teamBoxCooldown = 120;
			}

			
			teamBox.pos = new Vector2(engine.graphicsComponent.width / 2 - teamBox.size.x / 2, 0);

            readyTeam(currentColor);
            players[currentTeam].startTurn();
        }
        
        /*************************************** Initialization Functions ***************************************/ 

        public override void initialize()
        {
            base.initialize();
			click = (SinglePressBinding)engine.inputComponent[GameInput.ExampleBindings.CLICK];
			enter = (SinglePressBinding)engine.inputComponent[GameInput.ExampleBindings.ENTER];
            rightClick = (SinglePressBinding)engine.inputComponent[GameInput.ExampleBindings.RIGHTCLICK];
			selectLeft= (SinglePressBinding)engine.inputComponent[GameInput.ExampleBindings.SELECTLEFT];
			selectRight= (SinglePressBinding)engine.inputComponent[GameInput.ExampleBindings.SELECTRIGHT];
			collapse = (SinglePressBinding)engine.inputComponent[GameInput.ExampleBindings.COLLAPSE];
			uncollapse = (SinglePressBinding)engine.inputComponent[GameInput.ExampleBindings.UNCOLLAPSE];
        }

		protected override void initTiles(Mapfile.TileData[, ,] tileData)
		{
			//Get dimensions
			width = tileData.GetLength(1);
			height = tileData.GetLength(2);
			tileArray = new GameTile[width, height];

            WorldGenerator worldGen = new WorldGenerator(MirrorEngine.randGen.Next(999), 7);

            //Fill tile array
            Boolean generate = true;
            if (generate)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        tileArray[x, y] = worldGen.getTile(x, y, this, ref file.worldTileData[0, x, y]);
                    }
                }
            }
            else
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        tileArray[x, y] = new GameTile(this, x, y, file.worldTileData[0, x, y]);

                        (tileArray[x, y] as GameTile).type = worldGen.typeLookUp((tileArray[x, y] as GameTile).texture.key);
                    }
                }
            }
		}

        public void initializeInfoBox()
        {
            Engine.GUI tempGUI = engine.graphicsComponent.gui;
            locationBox = new GUITextBox(tempGUI, "");
            locationBox.size = new Vector2(30, 30);
            locationBox.textOffset = new Vector2(locationBox.right, locationBox.bot / 2);
            locationBox.bgColor.a= 0.30f;
            playerInfoBox = new GUITextBox(tempGUI, "");
            tempGUI.add(locationBox);
            tempGUI.add(playerInfoBox);

			teamBox = new GUILabel(tempGUI);
			tempGUI.add(teamBox);
        }

        /**
         * Updates the GameWorld 
         */
        public override void Update()
        {
			
            // <GraphicsThings>
			base.Update();
			if (!engine.currentWorldName.Equals("Maps/Menu.map"))
			{
				UpdateCamera();
				UpdateInfoBox();
				// </GraphicsThings>

				if (currentActors.Count == 0)
					endTurn();

				players[currentTeam].Update();
			}
            
        }

        private void UpdateCamera()
        {
            /*
            * Allow the camera to be moved by wasd and the arrow keys.
            * 
            * Here's my (possibly flawed) understanding of how the input works:
            * The inputComponent builds a map of key bindings.  On key events from SDL, the
            * inputComponent looks at the map and executes the action defined in the 
            * key binding for the event's key.  In this case, the action updates/maintains a 
            * position within the key binding itself, so while key events update the position
            * you still need to write code elsewhere to propagate that new information into the
            * engine.  To move the camera, we simply add the key binding's position to that of the camera's.
            * TL;DR: Read it if you want to avoid cursing the universe while trying to add similar functionality.
            */
            Vector2 cameraMoveDirection = new Vector2((engine.inputComponent[InputComponent.GUIBindings.HORIZONTALAXIS] as AxisBinding).position, -1 * (engine.inputComponent[InputComponent.GUIBindings.VERTICALAXIS] as AxisBinding).position);
			if (cameraMoveDirection.x == 0 && cameraMoveDirection.y == 0)
			{

				Vector2 position = engine.inputComponent.getMousePosition();
				int width = engine.graphicsComponent.camera.screenWidth;
				int height = engine.graphicsComponent.camera.screenHeight;

				if (position.x > width * .95)
					cameraMoveDirection.x = 1;
				if (position.x < width * .05)
					cameraMoveDirection.x = -1;
				if (position.y > height * .95)
					cameraMoveDirection.y = 1;
				if (position.y < width * .05)
					cameraMoveDirection.y = -1;
			}
				cameraManager.panCamera(cameraMoveDirection * 5);
        }

        private void UpdateInfoBox()
        {
			if (teamBoxCooldown == 0)
			{
				teamBox.texture = null;
			}
			else
				teamBoxCooldown--;

            GUI tempGUI = engine.graphicsComponent.gui;//setting a temporary gui again just to set a GUI
            highlightTile = (GameTile)getTileAt(engine.graphicsComponent.camera.screen2World(engine.inputComponent.getMousePosition()));//the tile that the mouse is hovering over
			
            if (highlightTile != null)
            {
                locationBox.texture = highlightTile.texture;
                highLightPlayer = (this.getAnimalOnTile(highlightTile) as AnimalActor);//the animal that the mouse is hovering over
                locationBox.text = "| " + highlightTile + " ";
                locationBox.text += "| Terrain Def: " + highlightTile.defense;

                //updates the Player Information box
                if (highLightPlayer != null)
                {
                    //draws the image of the tile when it has one
                    //the text shows the Name and Health of the actor 
                    playerInfoBox.text = "|" + (highLightPlayer as Actor).actorName;
                    playerInfoBox.text += "| Health= " + (highLightPlayer as AnimalActor).life.health + " ";
                    playerInfoBox.text += "| Team= " + (highLightPlayer as AnimalActor).team.ToString() + " ";
                    playerInfoBox.text += "| Attack= " + (highLightPlayer as AnimalActor).attackDamage.ToString() + " ";
                    playerInfoBox.text += "| Defense= " + (highLightPlayer as AnimalActor).defense.ToString() + " ";
                    playerInfoBox.text += "| Level= " + (highLightPlayer as AnimalActor).level.ToString() + " ";
                    if ((highLightPlayer as AnimalActor).level != AnimalActor.maxLevel)
                    {
                        playerInfoBox.text += "| Exp= " + (highLightPlayer as AnimalActor).expPoints.ToString() + "/" + (highLightPlayer as AnimalActor).expLevel[(highLightPlayer as AnimalActor).level + 1].ToString() + " |";
                    }
                    else
                    {
                        playerInfoBox.text += "| Exp= " + (highLightPlayer as AnimalActor).expLevel[(highLightPlayer as AnimalActor).level] + "/" + (highLightPlayer as AnimalActor).expLevel[(highLightPlayer as AnimalActor).level].ToString() + " |";
                    }
                    //makes the playerinfobox the same color as its team and makes it transparent
                    playerInfoBox.bgColor = new Color(highLightPlayer.color.r, highLightPlayer.color.g, highLightPlayer.color.b, 0.3f);
                    //constantly updates location of the Playerbox so that 
                    //it doesnt overlap the texture location box 
                    playerInfoBox.pos = new Vector2(locationBox.right + 1, 0f);
                }
                else
                {
                    locationBox.texture = null;
                    playerInfoBox.text = "";
                }
            }
        }

        // Note: the following two methods are NOT THE SAME!!!  There is a reason there are two of them.
        public GameActor getActorOnTile(Tile target)
        {
            if (target == null)
                return null;
            IEnumerator<Actor> a = getActorsInRect(new RectangleF(target.x - 8, target.y - 8, 16, 16)).GetEnumerator();
            while (a.MoveNext())
                if (a.Current is GameActor)
                    return (GameActor)a.Current;

            return null;
        }

		public AnimalActor getAnimalOnTile(Tile target)
		{
			if (target == null)
				return null;
			IEnumerator<Actor> a = getActorsInRect(new RectangleF(target.x-8, target.y-8, 16, 16)).GetEnumerator();
			while (a.MoveNext())
				if (a.Current is AnimalActor)
					return (AnimalActor)a.Current;

			return null;
		}

		protected override void start()
        {
            Handle song = engine.resourceComponent.get("Music/Menu.ogg");
            //engine.audioComponent.playSong(true, song);
       
            // Number of Human Players (eventually have this passed in from main menu)
            // One higher than number you want due to team_None existing
			if (!engine.currentWorldName.Equals("Maps/Menu.map"))
			{
                initializeInfoBox();

				int numHumans = 3;

				engine.inputComponent.normalize();
				for (int i = 0; i < numTeams; i++)
				{
					teams[i] = new LinkedList<AnimalActor>();

					if (i < numHumans)
						players[i] = new HumanPlayer(this, teams[i], (team_t)i);
					else
						players[i] = new ComputerPlayer(this, teams[i], (team_t)i);
				}

				currentColor = teamList[currentTeam];

				readyTeam(currentColor);

				endTurn();
			}
        }
    }
}
