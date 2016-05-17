using System;
using System.Collections.Generic;
using System.Linq;
using Engine;
using Game.AI;
using Game.GUI;

namespace Game
{
    public enum team_t { Blue, Purple, Red, Yellow };
    public enum player_type_t { None, Human, Computer };

    public class Team
    {
        public bool IsActive { get; set; }
        public team_t Color { get; set; }
        public player_type_t PlayerType { get; set; }
        public string BannerPath { get; set; }
        public LinkedList<AnimalActor> ActorList { get; set; }
        public Vector2 CameraPosition { get; set; }

        public Team(team_t teamColor, player_type_t playerType, string bannerPath)
        {
            IsActive = false;
            Color = teamColor;
            PlayerType = playerType;
            BannerPath = bannerPath;
            ActorList = new LinkedList<AnimalActor>();
            CameraPosition = new Vector2(0, 0);
        }
    }

    public class GameWorld : World
    {
		public SinglePressBinding click, enter, rightClick, selectLeft, selectRight, pauseMenuKey, collapse, uncollapse;
        public int currentTeam;
        private int numTeams;
        public int unitsMoved; // number of units moved this turn
        public List<LinkedList<AnimalActor>> teams;
        public List<AnimalActor> currentActors;
        public team_t currentColor;
		//public AI.ComputerPlayer AI;
        public GameTile[,] tiles;

        public GameTile highlightTile;
        public Actor highLightPlayer;

		private PauseMenu pauseMenu;
		bool pauseMenuActive = false;
		int pauseMenuCooldown = 0;

		public GUILabel playerInfoLabel;
		public GUILabel playerInfoOutlineLabel;
		public GUITextBox healthInfoBox;
		public GUITextBox attackInfoBox;
		public GUITextBox levelInfoBox;

		public GUILabel teamBox;

		public int teamBoxCooldown;
		public List<Player> playerList = new List<Player>();

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
            teams  = new List<LinkedList<AnimalActor>>();
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
            TeamDictionary.TeamDict[newTeam].ActorList.AddLast(recruit);
		}

        public void readyTeam(team_t team)
        {
            teamBox.texture = new Handle(engine.resourceComponent, TeamDictionary.TeamDict[team].BannerPath);
            teamBox.pos = new Vector2(engine.graphicsComponent.width / 2 - teamBox.size.x / 2, 0);
            teamBoxCooldown = 120;

            unitsMoved = 0;
            currentActors = new List<AnimalActor>();
			
            foreach (var actor in TeamDictionary.TeamDict[team].ActorList)
            {
                currentActors.Add(actor);

                actor.canMove = true;
                actor.canAct = true;
            }
		}

        public void unreadyTeam(team_t team)
        {
            unitsMoved = 0;
            
            foreach (var actor in TeamDictionary.TeamDict[team].ActorList)
            {
                actor.canMove = false;
                actor.canAct = false;
                actor.changeActorSprite(false);
            }
        }

        // Switch focus to the next team in the array
        public void endTurn()
        {
            // increment current team and ready it while unreadying the old team
            unreadyTeam(currentColor);

            // set currentTeam to the next team in the list (accounting for wraparound with a mod
            currentTeam = (currentTeam + 1) % (numTeams);
            currentColor = playerList[currentTeam].team;

            //if (currentTeam != 0)
            //{
            //    teamBox.texture = new Handle(engine.resourceComponent, TeamDictionary.TeamDict[currentColor].BannerPath);
            //    teamBoxCooldown = 120;
            //}
			
			//teamBox.pos = new Vector2(engine.graphicsComponent.width / 2 - teamBox.size.x / 2, 0);

            readyTeam(currentColor);
            playerList[currentTeam].startTurn();
        }
        
        /*************************************** Initialization Functions ***************************************/ 

        public override void initialize()
        {
            base.initialize();
			click = (SinglePressBinding)engine.inputComponent[GameInput.ExampleBindings.CLICK];
			enter = (SinglePressBinding)engine.inputComponent[GameInput.ExampleBindings.ENTER];
            rightClick = (SinglePressBinding)engine.inputComponent[GameInput.ExampleBindings.RIGHTCLICK];
			selectLeft= (SinglePressBinding)engine.inputComponent[GameInput.ExampleBindings.SELECTLEFT];
			selectRight = (SinglePressBinding)engine.inputComponent[GameInput.ExampleBindings.SELECTRIGHT];
			pauseMenuKey = (SinglePressBinding)engine.inputComponent[GameInput.ExampleBindings.MENU];
			collapse = (SinglePressBinding)engine.inputComponent[GameInput.ExampleBindings.COLLAPSE];
			uncollapse = (SinglePressBinding)engine.inputComponent[GameInput.ExampleBindings.UNCOLLAPSE];

			pauseMenu = new PauseMenu(engine, engine.graphicsComponent.gui);
			pauseMenu.Initialize();
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

			playerInfoLabel = new GUILabel(tempGUI, new Handle(engine.resourceComponent, "GUI\\PlayerInfoBg.png"));
			playerInfoOutlineLabel = new GUILabel(tempGUI, new Handle(engine.resourceComponent, "GUI\\PlayerInfoOutline.png"));
			healthInfoBox = new GUITextBox(tempGUI, "");
		    attackInfoBox = new GUITextBox(tempGUI, "");
			levelInfoBox = new GUITextBox(tempGUI, "");

			tempGUI.add(playerInfoOutlineLabel);
			tempGUI.add(playerInfoLabel);
			tempGUI.add(healthInfoBox);
			tempGUI.add(attackInfoBox);
			tempGUI.add(levelInfoBox);

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
				CheckPause();
				// </GraphicsThings>

				if (currentActors.Count == 0)
					endTurn();

				playerList[currentTeam].Update();
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

            Engine.GUI tempGUI = engine.graphicsComponent.gui;//setting a temporary gui again just to set a GUI
            highlightTile = (GameTile)getTileAt(engine.graphicsComponent.camera.screen2World(engine.inputComponent.getMousePosition()));//the tile that the mouse is hovering over
			
            if (highlightTile != null)
            {
                //locationBox.texture = highlightTile.texture;
                highLightPlayer = (this.getAnimalOnTile(highlightTile) as AnimalActor);

				//updates the Player Information box
				if (highLightPlayer != null)
				{
					//draws the image of the tile when it has one
					//the text shows the Name and Health of the actor 
					healthInfoBox.text = " Health: " + (highLightPlayer as AnimalActor).life.health + " ";
					attackInfoBox.text = " Attack: " + (highLightPlayer as AnimalActor).attackDamage.ToString() + " ";
					attackInfoBox.text += "| Defense: " + (highLightPlayer as AnimalActor).defense.ToString() + " ";
					levelInfoBox.text = " Level: " + (highLightPlayer as AnimalActor).level.ToString() + " ";

					if ((highLightPlayer as AnimalActor).level != AnimalActor.maxLevel)
					{
						levelInfoBox.text += "| Exp: " + (highLightPlayer as AnimalActor).expPoints.ToString() + "/" + (highLightPlayer as AnimalActor).expLevel[(highLightPlayer as AnimalActor).level + 1].ToString();
					}
					else
					{
						levelInfoBox.text += "| Exp: " + (highLightPlayer as AnimalActor).expLevel[(highLightPlayer as AnimalActor).level] + "/" + (highLightPlayer as AnimalActor).expLevel[(highLightPlayer as AnimalActor).level].ToString();
					}

					var mousePos = engine.inputComponent.getMousePosition();
					if (mousePos.x < engine.graphicsComponent.camera.screenWidth - 165)
					{
						playerInfoOutlineLabel.pos = mousePos + new Vector2(13, -2);
						playerInfoLabel.pos = mousePos + new Vector2(15, 0);
						healthInfoBox.pos = mousePos + new Vector2(15, 0);
						attackInfoBox.pos = mousePos + new Vector2(15, 15);
						levelInfoBox.pos = mousePos + new Vector2(15, 30);
					}
					else
					{
						playerInfoOutlineLabel.pos = mousePos + new Vector2(148, -2);
						playerInfoLabel.pos = mousePos - new Vector2(150, 0);
						healthInfoBox.pos = mousePos - new Vector2(150, 0);
						attackInfoBox.pos = mousePos + new Vector2(-150, 15);
						levelInfoBox.pos = mousePos + new Vector2(-150, 30);
					}

					playerInfoOutlineLabel.visible = true;
					playerInfoLabel.visible = true;
					healthInfoBox.visible = true;
					attackInfoBox.visible = true;
					levelInfoBox.visible = true;
				}
				else
				{
					playerInfoOutlineLabel.visible = false;
					playerInfoLabel.visible = false;
					healthInfoBox.visible = false;
					attackInfoBox.visible = false;
					levelInfoBox.visible = false;
					healthInfoBox.text = "";
					attackInfoBox.text = "";
					levelInfoBox.text = "";
				}
            }
        }

		private void CheckPause()
		{
			if (pauseMenuCooldown <= 0)
			{
				if (pauseMenuKey.isPressed
					&& !pauseMenuActive)
				{
					pauseMenu.ShowPauseMenu();
					pauseMenuActive = true;
					pauseMenuCooldown = 10;
				}
				else if (pauseMenuKey.isPressed
						&& pauseMenuActive)
				{
					pauseMenu.HidePauseMenu();
					pauseMenuActive = false;
					pauseMenuCooldown = 10;
				}
			}
			else
			{
				pauseMenuCooldown--;
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
       
			if (!engine.currentWorldName.Equals("Maps/Menu.map"))
			{
                initializeInfoBox();

				engine.inputComponent.normalize();
                foreach (var teamEntry in TeamDictionary.TeamDict.Values)
                {
                    if (teamEntry.PlayerType == player_type_t.Human)
                    {
                        TeamDictionary.TeamDict[teamEntry.Color].IsActive = true;
                        playerList.Add(new HumanPlayer(this, teamEntry.Color));
                        teams.Add(TeamDictionary.TeamDict[teamEntry.Color].ActorList);
                        numTeams++;
                    }
                    else if (teamEntry.PlayerType == player_type_t.Computer)
                    {
                        TeamDictionary.TeamDict[teamEntry.Color].IsActive = true;
                        playerList.Add(new ComputerPlayer(this, teamEntry.Color));
                        teams.Add(TeamDictionary.TeamDict[teamEntry.Color].ActorList);
                        numTeams++;
                    }
                }
                cameraManager = new CameraManager(engine, numTeams);
				
                currentColor = playerList.First().team;
				readyTeam(currentColor);

				endTurn();
			}
        }
    }
}
