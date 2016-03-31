using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;

namespace Game.GUI
{
    public static class SelectHandleNames
    {
        public static string humanResString = "Menu/TeamSelect/Human.png";
        public static string computerResString = "Menu/TeamSelect/Computer.png";
        public static string noneResString = "Menu/TeamSelect/None.png";
    }

    public class TeamState
    {
        public GUILabel GuiLabel;
        public Team TeamVal;
        private Game gameEngine;

        public TeamState(Game engine, GUILabel guiLabel, team_t color, string bannerPath, player_type_t type = player_type_t.None)
        {
            gameEngine = engine;
            GuiLabel = guiLabel;
            GuiLabel.mouseClickEvent += updateType;
            TeamVal = new Team(color, type, bannerPath);
        }

        public void updateType(Vector2 pos, MouseKeyBinding.MouseButton mouseButton)
        {
            switch (TeamVal.PlayerType)
            {
                case player_type_t.Human:
                    TeamVal.PlayerType = player_type_t.Computer;
                    GuiLabel.texture = new Handle(gameEngine.resourceComponent, SelectHandleNames.computerResString);
                    break;
                case player_type_t.Computer:
                    TeamVal.PlayerType = player_type_t.None;
                    GuiLabel.texture = new Handle(gameEngine.resourceComponent, SelectHandleNames.noneResString);
                    break;
                case player_type_t.None:
                    TeamVal.PlayerType = player_type_t.Human;
                    GuiLabel.texture = new Handle(gameEngine.resourceComponent, SelectHandleNames.humanResString);
                    break;
            }
        }
    }

    public class PlayerSelection
    {
        Game engine;
        Engine.GUI gui;

        List<GUILabel> backgrounds;
        List<TeamState> teamStates;

        Handle humanHandle, computerHandle, noneHandle;

        public PlayerSelection(Game engine, Engine.GUI gui)
        {
            this.engine = engine;
            this.gui = gui;

            humanHandle = new Handle(engine.resourceComponent, SelectHandleNames.humanResString);
            computerHandle = new Handle(engine.resourceComponent, SelectHandleNames.computerResString);
            noneHandle = new Handle(engine.resourceComponent, SelectHandleNames.noneResString);
        }

        public GUILabel CreateGUILabel(Engine.GUI gui, Handle handle, Vector2 pos)
        {
            var GuiLabel = new GUILabel(gui, handle);
            GuiLabel.pos = pos;
            gui.add(GuiLabel);

            return GuiLabel;
        }

        public void LoadGuiElements()
        {

            backgrounds = new List<GUILabel> { CreateGUILabel(gui, new Handle(engine.resourceComponent, "Menu/TeamSelect/Purple.png"), new Vector2(0, 100)),
                                               CreateGUILabel(gui, new Handle(engine.resourceComponent, "Menu/TeamSelect/Yellow.png"), new Vector2(250, 100)),
                                               CreateGUILabel(gui, new Handle(engine.resourceComponent, "Menu/TeamSelect/Red.png"), new Vector2(500, 100)),
                                               CreateGUILabel(gui, new Handle(engine.resourceComponent, "Menu/TeamSelect/Blue.png"), new Vector2(750, 100)) };

            teamStates = new List<TeamState> { new TeamState(engine, CreateGUILabel(gui, humanHandle, new Vector2(75, 200)), team_t.Purple, "GUI\\002_TeamBoxes\\Purple_Team.png", player_type_t.Human),
                                               new TeamState(engine, CreateGUILabel(gui, computerHandle, new Vector2(325, 200)), team_t.Yellow, "GUI\\002_TeamBoxes\\Yellow_Team.png", player_type_t.Computer),
                                               new TeamState(engine, CreateGUILabel(gui, noneHandle, new Vector2(575, 200)), team_t.Red, "GUI\\002_TeamBoxes\\Red_Team.png"),
                                               new TeamState(engine, CreateGUILabel(gui, noneHandle, new Vector2(825, 200)), team_t.Blue, "GUI\\002_TeamBoxes\\Blue_Team.png") };
        }

        private void UnloadGuiElements()
        {
            foreach(var bg in backgrounds)
            {
                gui.remove(bg);
            }

            foreach(var team in teamStates)
            {
                gui.remove(team.GuiLabel);
            }
        }

        public bool StartGame()
        {
            int noneTeams = 0;
            foreach(var team in teamStates)
            {
                if (team.TeamVal.PlayerType == player_type_t.None)
                    noneTeams++;
            }

            // Need at least 2 teams
            if (noneTeams > 2) return false;

            foreach (var team in teamStates)
            {
                TeamDictionary.TeamDict.Add(team.TeamVal.Color, team.TeamVal);
            }

            UnloadGuiElements();

            return true;
        }
    }
}
