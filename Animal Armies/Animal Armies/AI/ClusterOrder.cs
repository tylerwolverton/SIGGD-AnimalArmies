using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.AI
{
    public class ClusterOrder : Order
    { 
        GameTile center;
        int turn_count = 0;

        public ClusterOrder(Platoon platoon, GameTile target)
            : base(platoon)
        {
            setCenter(target);
        }

        // If center is null, set the center to the average position of the units
        public void setCenter(GameTile center) {
            // If we don't know where we should cluster to, just pull all the units in.
            if (center == null)
            {
                this.center = context.getCenterTile();
            }
            else 
            {
                this.center = center;
            }
            System.Console.WriteLine("Set cluster center to " + this.center);
        }

        public override void execute()
        {
            turn_count++;
            System.Console.WriteLine("Executing cluster order");
            System.Console.WriteLine("Center is at " + center + ", should probably be at " + context.getCenterTile());

            // Cluster!  Move units toward the center of the centroid.
            // TODO: There's a bug that lets actors step on each other (or something).
            foreach (AnimalActor actor in context.units)
            {
                if (!actor.canMove)
                {
                    continue;
                }

                // Preference for inaction - if we can't find a tile better than the current, don't move
                GameTile best = (GameTile)actor.curTile;
                double best_dist = center.euclidian(best);

                foreach (GameTile target in actor.findPaths())
                {
                    // Check that the target tile is empty
                    GameActor actor_at_target = context.world.getActorOnTile(target);
                    if (actor_at_target != null)
                    {
                        continue;
                    }

                    double dist = target.euclidian(center);
                    if (dist < best_dist)
                    {
                        best = target;
                        best_dist = dist;
                    }

                }

                if (best != (GameTile)actor.curTile)
                {
                    System.Console.WriteLine("Center at " + center + "; Moving actor at " + actor.curTile + " to " + best);
                    moveUnit(actor, best);
                }
                
                System.Console.WriteLine("Unit at " + actor.curTile);
            }
        }
    }
}
