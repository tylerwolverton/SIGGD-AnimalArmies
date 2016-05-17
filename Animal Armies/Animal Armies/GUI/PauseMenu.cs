using Engine;

namespace Game.GUI
{
	public class PauseMenu
	{
		Game engine;
		Engine.GUI gui;

		GUILabel BackgroundLabel;
		GUILabel ResumeLabel;
		GUILabel QuitLabel;

		public PauseMenu(Game p_engine, Engine.GUI p_gui)
		{
			engine = p_engine;
			gui = p_gui;
		}

		public void Initialize()
		{
			BackgroundLabel = new GUILabel(gui, new Handle(engine.resourceComponent, "Menu/PauseMenu/Background.png"));
			BackgroundLabel.pos = new Vector2(350, 200);
			//gui.add(BackgroundLabel);

			ResumeLabel = new GUILabel(gui, new Handle(engine.resourceComponent, "Menu/PauseMenu/Resume.png"));
			ResumeLabel.mouseClickEvent += ResumeGame;
			ResumeLabel.pos = new Vector2(365, 300);
			//gui.add(ResumeLabel);

			QuitLabel = new GUILabel(gui, new Handle(engine.resourceComponent, "Menu/PauseMenu/Quit.png"));
			QuitLabel.mouseClickEvent += ExitGame;
			QuitLabel.pos = new Vector2(510, 300);
			//gui.add(QuitLabel);

			//HidePauseMenu();
		}

		public void ShowPauseMenu()
		{
			gui.add(BackgroundLabel);
			gui.add(ResumeLabel);
			gui.add(QuitLabel);
			//BackgroundLabel.visible = true;
			//ResumeLabel.visible = true;
			//QuitLabel.visible = true;
		}

		public void HidePauseMenu()
		{
			gui.remove(BackgroundLabel);
			gui.remove(ResumeLabel);
			gui.remove(QuitLabel);
			//BackgroundLabel.visible = false;
			//ResumeLabel.visible = false;
			//QuitLabel.visible = false;
		}

		public void ResumeGame(Vector2 pos, MouseKeyBinding.MouseButton mouseButton)
		{
			HidePauseMenu();
		}

		public void ExitGame(Vector2 pos, MouseKeyBinding.MouseButton mouseButton)
		{
			engine.quit = true;
		}
	}
}
