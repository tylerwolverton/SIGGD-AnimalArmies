using Engine;

namespace Game.GUI
{
	public class PauseMenu
	{
		Game engine;
		Engine.GUI gui;
		GameWorld world;

		GUILabel BackgroundLabel;
		GUILabel ResumeLabel;
		GUILabel EndTurnLabel;
		GUILabel QuitLabel;

		public PauseMenu(Game p_engine, Engine.GUI p_gui, GameWorld p_world)
		{
			engine = p_engine;
			gui = p_gui;
			world = p_world;
		}

		public void Initialize()
		{
			BackgroundLabel = new GUILabel(gui, new Handle(engine.resourceComponent, "Menu/PauseMenu/Background.png"));
			BackgroundLabel.pos = new Vector2(300, 200);

			ResumeLabel = new GUILabel(gui, new Handle(engine.resourceComponent, "Menu/PauseMenu/Resume.png"));
			ResumeLabel.mouseClickEvent += ResumeGame;
			ResumeLabel.pos = new Vector2(350, 400);

			EndTurnLabel = new GUILabel(gui, new Handle(engine.resourceComponent, "Menu/PauseMenu/EndTurn.png"));
			EndTurnLabel.mouseClickEvent += EndTurn;
			EndTurnLabel.pos = new Vector2(500, 400);

			QuitLabel = new GUILabel(gui, new Handle(engine.resourceComponent, "Menu/PauseMenu/Quit.png"));
			QuitLabel.mouseClickEvent += ExitGame;
			QuitLabel.pos = new Vector2(650, 400);
		}

		public void ShowPauseMenu()
		{
			gui.add(BackgroundLabel);
			gui.add(ResumeLabel);
			gui.add(EndTurnLabel);
			gui.add(QuitLabel);
		}

		public void HidePauseMenu()
		{
			gui.remove(BackgroundLabel);
			gui.remove(ResumeLabel);
			gui.remove(EndTurnLabel);
			gui.remove(QuitLabel);
		}

		public void ResumeGame(Vector2 pos, MouseKeyBinding.MouseButton mouseButton)
		{
			HidePauseMenu();
		}

		public void EndTurn(Vector2 pos, MouseKeyBinding.MouseButton mouseButton)
		{
			HidePauseMenu();
			world.endTurn();
		}

		public void ExitGame(Vector2 pos, MouseKeyBinding.MouseButton mouseButton)
		{
			engine.quit = true;
		}
	}
}
