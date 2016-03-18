using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;
namespace Game
{
    public class BaseBehavior : GameBehavior
    {
        team_t team;
        Color teamColor;
        Boolean transformReady = true;
        public int teamValue = 1;
        private const Boolean SPAWN_RANDOM_ACTORS = true;
        public BaseBehavior(GameWorld world, GameActor actor, team_t team, Color teamColor)
            : base(world, actor)
        {
            this.actor = actor;
            this.team = team;
            this.teamColor = teamColor;
        }
        public override void run()
        {
            if (transformReady)
            {


                Tile current = world.getTileAt(actor.position);
                for (int i = -2; i <= 2; i++)
                {
                    for (int j = -2; j <= 2; j++)
                    {
                        Tile checkTile = world.getTile(current.xIndex + i, current.yIndex + j);

                        if (teamValue >= 0 && SPAWN_RANDOM_ACTORS)
                        { 
                            AnimalActor newActor = ((GameActorFactory)world.actorFactory).createRandomAnimalActor(new Vector2(checkTile.x, checkTile.y));
                            GameTile realTile = (GameTile)newActor.FindOpenTile(newActor, checkTile as GameTile);

                            if (realTile != null)
                            {
                                // Move animal to valid position
                                newActor.position.x = realTile.x;
                                newActor.position.y = realTile.y;

                                // Update old position so if a move is cancelled the animal doesn't go back to the invalid position
                                newActor.oldPosition = newActor.position;

                                // Avoid animals flying off of map when moved
                                newActor.velocity = Vector2.Zero;
                                
                                if (newActor != null)
                                {
                                    world.addActor(newActor);
                                    newActor.teamColor = teamColor;
                                    newActor.changeTeam(team);
                                    teamValue -= newActor.spawnCost;
                                }
                            }
                        }
                    }
                }
                transformReady = false;

                actor.removeMe = true;
            }

        }

    }
}
