using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.GUI
{
    public class GameOverScreen
    {
        private Game engine;

        private Dictionary<team_t, GUILabel> HandleDict;

        public GameOverScreen(Game engine)
        {
            this.engine = engine;

            HandleDict = new Dictionary<team_t, GUILabel>();
            
            HandleDict.Add(team_t.Purple, new GUILabel(engine.graphicsComponent.gui, new Handle(engine.resourceComponent, "Menu/GameOver/PurpleWon.png")));
            HandleDict.Add(team_t.Yellow, new GUILabel(engine.graphicsComponent.gui, new Handle(engine.resourceComponent, "Menu/GameOver/YellowWon.png")));
            HandleDict.Add(team_t.Red, new GUILabel(engine.graphicsComponent.gui, new Handle(engine.resourceComponent, "Menu/GameOver/RedWon.png")));
            HandleDict.Add(team_t.Blue, new GUILabel(engine.graphicsComponent.gui, new Handle(engine.resourceComponent, "Menu/GameOver/BlueWon.png")));
        }

        public void ShowWinner(team_t teamColor)
        {
            HandleDict[teamColor].pos = new Vector2(0,0);
            engine.graphicsComponent.gui.add(HandleDict[teamColor]);
        }
    }
}
