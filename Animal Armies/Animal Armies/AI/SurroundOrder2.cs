using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;

namespace Game.AI 
{
    public class SurroundOrder2 : Order
    {
        private AnimalActor target;

        private RectangleF zone;

        /*
         * Surround, either whatever we can find or a particular unit
         */
        public SurroundOrder2(Platoon platoon, AnimalActor target = null)
            : base(platoon)
        {
            this.target = target;
            this.zone = null;
        }

        /*
         * Surround whatever we can find within a square w/ radius radius centered on center.
         */
        public SurroundOrder2(Platoon platoon, GameTile center, int radius)
            : base(platoon)
        {
            this.target = null;
            radius *= GameTile.size;
            zone = new RectangleF(center.x - radius, center.y - radius, 2 * radius, 2 * radius);
        }

        public override void execute()
        {
            //Find what tiles we can reach by who
            Dictionary<GameTile, List<AnimalActor>> teamTiles = getTeamTiles();

            //Find all enemies, their adjacent tiles, and who can reach those tiles
            Dictionary<AnimalActor, Dictionary<GameTile, List<AnimalActor>>> targetList = getTargetInfo(teamTiles);

            //Find enemy to attack
            attack(targetList);

			foreach (AnimalActor actor in platoon.units)
			{
				// Don't bother with dead units or those that have already moved.
				if (actor.removeMe || !actor.canMove || !actor.canAct)
					continue;

				moveRemainingUnit(actor);
			}
				
        }

        private Dictionary<GameTile, List<AnimalActor>> getTeamTiles()
        {
            //Iterate through each of our units, make a map of tiles : a list of who can reach the tiles
            Dictionary<GameTile, List<AnimalActor>> teamTiles = new Dictionary<GameTile, List<AnimalActor>>();
            foreach (AnimalActor actor in platoon.units)
            {
                // Don't bother with dead units or those that have already moved.
                if (actor.removeMe || !actor.canMove || !actor.canAct)
                    continue;

                foreach (GameTile tile in actor.findPaths())
                {
                    if (!teamTiles.ContainsKey(tile))
                        teamTiles.Add(tile, new List<AnimalActor>());
                    teamTiles[tile].Add(actor);
                }
            }
            return teamTiles;
        }

        /*
         * For each enemy, build a list of tiles adjacent to that enemy, and which of our units can reach each of those tiles
         */
        private Dictionary<AnimalActor, Dictionary<GameTile, List<AnimalActor>>> getTargetInfo(Dictionary<GameTile, List<AnimalActor>> teamTiles)
        {
            Dictionary<AnimalActor, Dictionary<GameTile, List<AnimalActor>>> targetList = new Dictionary<AnimalActor, Dictionary<GameTile, List<AnimalActor>>>();

            // If we have a designated target, only focus on it
            if (this.target != null)
            {
                targetList.Add(target, buildAdjList(target, teamTiles));
                return targetList;
            }
            
            // Otherwise, compute targeting info for all enemies
            foreach (LinkedList<AnimalActor> team in platoon.world.teams)
            {
                // Ignore teams that are us
                if (team.Count == 0 || team.First.Value.team == platoon.team)
                    continue;

                foreach (AnimalActor animal in team)
                {
                    // Ignore enemies not in our zone (if applicable)
                    if (zone != null && !zone.contains(animal.position))
                        continue;

                    if (!targetList.ContainsKey(animal))
                        targetList.Add(animal, buildAdjList(animal, teamTiles));
                }
            }
            return targetList;
        }

        /*
         * Given an enemy unit and our list of teamTiles, build a list of tiles adjacent to that enemy and which of our units can move there
         */
        private Dictionary<GameTile, List<AnimalActor>> buildAdjList(AnimalActor target, Dictionary<GameTile, List<AnimalActor>> teamTiles)
        {
            Dictionary<GameTile, List<AnimalActor>> adjTileMap = new Dictionary<GameTile, List<AnimalActor>>();
            foreach (GameTile tile in target.curTile.adjacent)
            {
                if (tile == null) continue;
                if (teamTiles.ContainsKey(tile))
                    adjTileMap.Add(tile, teamTiles[tile]);
            }
            return adjTileMap;
        }

        private void attack(Dictionary<AnimalActor, Dictionary<GameTile, List<AnimalActor>>> targetList)
        {
            AnimalActor maxEnemy = null;
            List<GameTile> maxTiles = null;
            int maxAtackNum = 0;
            List<AnimalActor> maxArrangement = null;

            List<AnimalActor> enemies = new List<AnimalActor>(targetList.Keys);
            for (int i = 0; i < enemies.Count; i++)
            {
                List<List<AnimalActor>> arrangements;
                List<GameTile> adjTiles = new List<GameTile>(targetList[enemies[i]].Keys);
                List<List<AnimalActor>> adjTileUnits = new List<List<AnimalActor>>();
                foreach (GameTile tile in adjTiles)
                {
                    adjTileUnits.Add(targetList[enemies[i]][tile]);
                }

                
                arrangements = recurse(adjTileUnits, null, null);
                if (arrangements == null)
                    continue;
                foreach (List<AnimalActor> arrangement in arrangements)
                {
                    int uniqueCount = countUnique(arrangement);
                    if (uniqueCount > maxAtackNum)
                    {
                        maxEnemy = enemies[i];
                        maxTiles = adjTiles;
                        maxArrangement = arrangement;
                        maxAtackNum = uniqueCount;
                    }
                }
            }

            if (maxArrangement == null) return;

            //Attack the max enemy
            List<AnimalActor> moved = new List<AnimalActor>();
            for (int i = 0; i < maxArrangement.Count; i++ )
            {
                if (!moved.Contains(maxArrangement[i]))
                {
                    moveUnit(maxArrangement[i], maxTiles[i]);
                    maxArrangement[i].attackTile(maxEnemy.curTile);
                    moved.Add(maxArrangement[i]);
                }
            }
        }

        private void moveRemainingUnit(AnimalActor actor)
        {
			AnimalActor target = null;
			int minTeamSize = 999;

			foreach (var team in TeamDictionary.TeamDict.Values)
			{
				if (team.IsActive
					&& team.Color != platoon.units.First().team
					&& team.ActorList.Count() < minTeamSize)
				{
					minTeamSize = team.ActorList.Count();
					target = team.ActorList.First();
				}
			}

			if (target == null)
			{
				return;
			}
			
            var possibleMoves = actor.findPaths();
            var lastPos = actor.curTile;

            foreach(var tile in possibleMoves)
            {
                if((target.curTile as GameTile).euclidian(actor.curTile as GameTile) <
					(tile as GameTile).euclidian(actor.curTile as GameTile))
                {
                    if (moveUnit(actor, tile as GameTile))
                        break;
                }
            }

            foreach (var tile in possibleMoves)
            {
				if (actor.curTile != lastPos)
				{
					break;
                }
				moveUnit(actor, tile as GameTile);
            }
        }

        private int countUnique(List<AnimalActor> arrangement)
        {
            int count = 0;
            List<AnimalActor> newList = new List<AnimalActor>();
            foreach (AnimalActor animal in arrangement)
            {
                if (!newList.Contains(animal))
                {
                    count += 1;
                    newList.Add(animal);
                }
            }
            return count;
        }

        private List<List<AnimalActor>> recurse(List<List<AnimalActor>> adjTileUnits, List<AnimalActor> attackers, List<List<AnimalActor>> arrangements)
        {
            if (adjTileUnits.Count == 0) return null;

            if (arrangements == null)
                arrangements = new List<List<AnimalActor>>();

            List<AnimalActor> firstList = adjTileUnits[0];
            List<List<AnimalActor>> otherLists = new List<List<AnimalActor>>();
            foreach (List<AnimalActor> list in adjTileUnits)
            {
                if (list != firstList)
                {
                    otherLists.Add(list);
                }
            }

            foreach (AnimalActor animal in firstList)
            {
                if (attackers == null)                
                    attackers = new List<AnimalActor>();
                attackers.Add(animal);

                if (otherLists.Count != 0)
                {
                    recurse(otherLists, attackers, arrangements);
                }
                else
                {
                    //We've picked a unit for each tile
                    List<AnimalActor> attackersCopy = new List<AnimalActor>(attackers);
                    arrangements.Add(attackersCopy);
                    //return attackers;
                }
                attackers.RemoveAt(attackers.Count - 1);
            }
            return arrangements;
        }
    }
}