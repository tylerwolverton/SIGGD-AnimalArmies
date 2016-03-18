using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.AI
{
    public class SurroundOrder : Order
    {
        public SurroundOrder(Platoon platoon) : base(platoon) { }

        public override void execute()
        {
            findTarget();
        }
        public void findTarget()
        {
            //Iterate through each of our units, make a map of tiles : a list of who can reach the tiles
            Dictionary<GameTile, List<AnimalActor>> teamTiles = new Dictionary<GameTile, List<AnimalActor>>();
            foreach (AnimalActor actor in context.units)
            {
                foreach (GameTile tile in actor.findPaths())
                {
                    if (!teamTiles.ContainsKey(tile))
                        teamTiles.Add(tile, new List<AnimalActor>());
                    teamTiles[tile].Add(actor);
                }
            }
            Dictionary<AnimalActor, Dictionary<GameTile, List<AnimalActor>>> targetList = new Dictionary<AnimalActor, Dictionary<GameTile, List<AnimalActor>>>();
            foreach (LinkedList<AnimalActor> team in context.world.teams)
            {
                foreach (AnimalActor animal in team)
                {
                    if (animal.team != context.team)
                    {
                        Dictionary<GameTile, List<AnimalActor>> adj = new Dictionary<GameTile, List<AnimalActor>>();
                        foreach (GameTile tile in animal.curTile.adjacent)
                        {
                            if(tile != null && teamTiles.ContainsKey(tile)) 
                            {
                                adj.Add(tile, teamTiles[tile]);
                            }
                        } 
                        if (!targetList.ContainsKey(animal))
                            targetList.Add(animal, adj);
                    }
                }
            }
            int[] reachable = { 0, 0, 0, 0 };
            List<AnimalActor> enemies = new List<AnimalActor>(targetList.Keys);
            for (int i = 0; i < enemies.Count; i++)
            {
                List<GameTile> gameTiles = new List<GameTile>(targetList[enemies[i]].Keys);
                List<List<AnimalActor>> adjTileUnits = new List<List<AnimalActor>>();
                foreach (GameTile tile in gameTiles)
                {
                    adjTileUnits.Add(targetList[enemies[i]][tile]);
                }
                if (adjTileUnits.Count == 4)
                {
                    for (int w = 0; w < adjTileUnits[0].Count; w++)
                    {
                        for (int x = 0; x < adjTileUnits[1].Count; x++)
                        {
                            for (int y = 0; y < adjTileUnits[2].Count; y++)
                            {
                                for (int z = 0; z < adjTileUnits[3].Count; z++)
                                {
                                    if (adjTileUnits[0][w] != adjTileUnits[1][x] && adjTileUnits[0][w] != adjTileUnits[2][y] && adjTileUnits[0][w] != adjTileUnits[3][z] && adjTileUnits[1][x] != adjTileUnits[2][y] && adjTileUnits[1][x] != adjTileUnits[3][z] && adjTileUnits[2][y] != adjTileUnits[3][z])
                                    {
                                        moveUnit(adjTileUnits[0][w], gameTiles[0]);
                                        moveUnit(adjTileUnits[1][x], gameTiles[1]);
                                        moveUnit(adjTileUnits[2][y], gameTiles[2]);
                                        moveUnit(adjTileUnits[3][z], gameTiles[3]);
                                        adjTileUnits[0][w].attackTile(enemies[i].curTile);
                                        adjTileUnits[1][x].attackTile(enemies[i].curTile);
                                        adjTileUnits[2][y].attackTile(enemies[i].curTile);
                                        adjTileUnits[3][z].attackTile(enemies[i].curTile);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}


