using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game
{
    static class TeamDictionary
    {
        public static Dictionary<team_t, Team> TeamDict;

        static TeamDictionary()
        {
            TeamDict = new Dictionary<team_t, Team>();

            //TeamDictionary.TeamDict.Add(team_t.Purple, new Team(team_t.Purple, player_type_t.None, "GUI\\002_TeamBoxes\\Purple_team.png"));
            //TeamDictionary.TeamDict.Add(team_t.Yellow, new Team(team_t.Yellow, player_type_t.None, "GUI\\002_TeamBoxes\\Yellow_team.png"));
            //TeamDictionary.TeamDict.Add(team_t.Blue, new Team(team_t.Blue, player_type_t.None, "GUI\\002_TeamBoxes\\Blue_team.png"));
            //TeamDictionary.TeamDict.Add(team_t.Red, new Team(team_t.Red, player_type_t.None, "GUI\\002_TeamBoxes\\Red_team.png"));
        }
    }
}
