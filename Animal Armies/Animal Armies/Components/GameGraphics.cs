using Engine;

using Game.GUI;

namespace Game
{
	public class GameGraphics : GraphicsComponent
    {
        const int WIDTH = 1000;
        const int HEIGHT = 600;
        GUILabel TitleLabel;
		GUILabel MenuLabel;
		GUILabel ExitLabel;
        GUILabel StartLabel;
		GUILabel ControlsLabel;
		GUILabel CreditsLabel;
		GUILabel Controls;
		GUILabel BackLabel;

		PlayerSelection PlayerSelect;

        public new Game engine
        {
            get
            {
                return base.engine as Game;
            }
        }

        public GameGraphics(Game theEngine)
            : base(theEngine, WIDTH , HEIGHT, false)
        {

        }

        public override void initialize()
        {
            base.initialize();

            camera.position = new Vector2(WIDTH/2, HEIGHT/2);
            camera.scale = 2;
            camera.scaleChange = 0;
			if (engine.currentWorldName == null || engine.currentWorldName.Equals("Maps/Menu.map"))
			{
				menuLoad();
			}
        }

        public void loadTeamSelect(Vector2 pos, MouseKeyBinding.MouseButton mouseButton)
        {
            menuUnload();

            PlayerSelect = new PlayerSelection(engine, gui);

            PlayerSelect.LoadGuiElements();

            StartLabel = new GUILabel(gui, new Handle(engine.resourceComponent, "Menu/Start.png"));
            StartLabel.mouseClickEvent += startGame;
            StartLabel.pos = new Vector2(width / 2 - 64, height - 75);
            gui.add(StartLabel);            
        }
        
        public void startGame(Vector2 pos, MouseKeyBinding.MouseButton mouseButton)
        {
            if( PlayerSelect.StartGame())
            {
                engine.setWorld("Maps/newWorld.map");

                gui.remove(StartLabel);
            }

        }

		public void menuLoad()
        {
            TitleLabel = new GUILabel(gui, new Handle(engine.resourceComponent, "Menu/Title.png"));
            TitleLabel.pos = new Vector2(width / 2 - 192, height / 2 - 256);
            gui.add(TitleLabel);
			MenuLabel = new GUILabel(gui, new Handle(engine.resourceComponent, "Menu/Start.png"));
            MenuLabel.mouseClickEvent += loadTeamSelect;
			MenuLabel.pos = new Vector2(200, height / 2 + 150);
			gui.add(MenuLabel);
			ControlsLabel = new GUILabel(gui, new Handle(engine.resourceComponent, "Menu/ControlsButton.png"));
			ControlsLabel.mouseClickEvent += showControls;
			ControlsLabel.pos = new Vector2(425, height / 2 + 150);
			gui.add(ControlsLabel);
			ExitLabel = new GUILabel(gui, new Handle(engine.resourceComponent, "Menu/Quit.png"));
			ExitLabel.mouseClickEvent += exitGame;
			ExitLabel.pos = new Vector2(650, height / 2 + 150);
			gui.add(ExitLabel);

			CreditsLabel = new GUILabel(gui, new Handle(engine.resourceComponent, "Menu/Credits.png"));
			CreditsLabel.pos = new Vector2(350, height / 2 + 200);
			gui.add(CreditsLabel);
		}

		public void exitGame(Vector2 pos, MouseKeyBinding.MouseButton mouseButton)
		{
			engine.quit = true;
		}

		private void addBackButton()
		{
			BackLabel = new GUILabel(gui, new Handle(engine.resourceComponent, "Menu/BackLabel.png"));
			BackLabel.mouseClickEvent += backToMenu;
			BackLabel.pos = new Vector2(200, height / 2 + 150);
			gui.add(BackLabel);
		}

		public void backToMenu(Vector2 pos, MouseKeyBinding.MouseButton mouseButton)
		{
			menuLoad();
			gui.remove(BackLabel);
			gui.remove(Controls);
		}

		public void showControls(Vector2 pos, MouseKeyBinding.MouseButton mouseButton)
		{
			menuUnload();
			addBackButton();

			Controls = new GUILabel(gui, new Handle(engine.resourceComponent, "Menu/Controls.png"));
			Controls.pos = new Vector2(width / 2 - 192, height / 2 - 256);
			gui.add(Controls);
		}
		
		public void menuUnload()
        {
            gui.remove(TitleLabel);
			gui.remove(MenuLabel);
			gui.remove(ControlsLabel);
			gui.remove(CreditsLabel);
			gui.remove(ExitLabel);
		}

        protected override void update()
        {
            base.update();
            if (engine.paused)
            {
                return;
            }

            GameWorld gameWorld = engine.world as GameWorld;


			//If camera to left of game world, snap to left side, if to right, snap to right.

			float hWidth = (camera.viewRect.right - camera.viewRect.left);
			float hHeight = (camera.viewRect.bottom - camera.viewRect.top);

			if (camera.viewRect.left < 0)
			{
				camera.viewRect = new RectangleF(0, camera.viewRect.top, hWidth, hHeight);
			}
			else if (camera.viewRect.right > gameWorld.width * Tile.size)
			{
				camera.viewRect = new RectangleF(camera.viewRect.left - (camera.viewRect.right - gameWorld.width * Tile.size), camera.viewRect.top, hWidth, hHeight);
			}
			//Likewise with top and bottom
			if (camera.viewRect.top < 0)
			{
				camera.viewRect = new RectangleF(camera.viewRect.left, 0, hWidth, hHeight);
			}
			else if (camera.viewRect.bottom > gameWorld.height * Tile.size)
			{
				camera.viewRect = new RectangleF(camera.viewRect.left, camera.viewRect.top - (camera.viewRect.bottom - gameWorld.height * Tile.size), hWidth, hHeight);
			}
        }

        protected override void drawTile(Tile tile, RectangleF viewRect)
        {
            if (tile.texture.key != "")
            {
                //Get the size of the tile in increments of Tile.size
                int tilesWide = (int)((float)tile.imageWidth / Tile.size);
                int tilesHigh = (int)((float)tile.imageHeight / Tile.size);
                int fTilesWide = tilesWide;
                int fTilesHigh = tilesHigh;
                if (tile.xIndex + fTilesWide > tile.world.width) fTilesWide -= ((tile.xIndex + fTilesWide) - tile.world.width);
                if (tile.yIndex + fTilesHigh > tile.world.height) fTilesHigh -= ((tile.yIndex + fTilesHigh) - tile.world.height);

                //Draw the single tile by increments of Tile.size, in order to draw larger-than-a-tile tiles
                for (int x = 0; x < fTilesWide; x++)
                {
                    for (int y = 0; y < fTilesHigh; y++)
                    {
                        //Merge the lighting of the current tile and the incremental tile
                        Tile t = tile.world.tileArray[tile.xIndex + x, tile.yIndex + y];
                        if (t != tile && t.texture.key != "") continue;
                        if (t.color.a < 0.0001f) continue;

                        //Final tint
                        Color tint = t.getFinalColor();//glowOutput; //getfinalcolor

                        //Get the specific bounds for this increment
                        RectangleF bounds = new RectangleF((float)x * Tile.size / tile.imageWidth, (float)y * Tile.size / tile.imageHeight, (float)Tile.size / tile.imageWidth, (float)Tile.size / tile.imageHeight);

                        Color tint1 = tint;// = Color.Avg((t.left == null) ? tint : t.left.getFinalColor(), (t.up == null) ? tint : t.up.getFinalColor());
                        Color tint2 = tint;// = Color.Avg((t.right == null) ? tint : t.right.getFinalColor(), (t.up == null) ? tint : t.up.getFinalColor());
                        Color tint3 = tint;// = Color.Avg((t.left == null) ? tint : t.left.getFinalColor(), (t.down == null) ? tint : t.down.getFinalColor());
                        Color tint4 = tint;// = Color.Avg((t.right == null) ? tint : t.right.getFinalColor(), (t.down == null) ? tint : t.down.getFinalColor());

                        drawTex(tile.texture, t.x, t.y, Tile.size, Tile.size, Color.Avg(tint, tint1), Color.Avg(tint, tint2), Color.Avg(tint, tint3), Color.Avg(tint, tint4), bounds);
                    }
                }
            }
        }
    }
}