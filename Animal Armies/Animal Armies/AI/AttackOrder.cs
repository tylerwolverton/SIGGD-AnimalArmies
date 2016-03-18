using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.AI
{
    public class AttackOrder : Order
    {
		 
        private AnimalActor enemy;

        public AttackOrder(Platoon platoon, AnimalActor enemy) : base(platoon)
        {
            this.enemy = enemy;
        }

        public override void execute()
        {
            // Find centroid of our units
            GameTile center = context.getCenterTile();

            Engine.Actor a = null;
            if (enemy == null)
            {
                double dist = double.MaxValue;
                for (int f = 0; f < context.world.actors.Count(); f++)
                {
                    Engine.Actor x = context.world.actors.ElementAt(f);
                    if (x is AnimalActor && ((AnimalActor)x).team != this.context.team && ((GameTile)x.curTile).manhattan((GameTile)context.units.ElementAt(0).curTile) < dist)
                    {
                        dist = ((GameTile)x.curTile).manhattan((GameTile)context.units.ElementAt(0).curTile);
                        a = x;
                    }
                }
            }
            else
            {
                a = (Engine.Actor)enemy;
            }

            if (a == null) return;
            GameTile victim = (GameTile)a.curTile;
            for (int i = 0; i < context.units.Count; i++)
            {
                GameTile moveTile = null;
                if (context.world.getActorOnTile(victim.right) == null)
                {
                    moveTile = (GameTile)victim.right;
                }
                else if (context.world.getActorOnTile(victim.up) == null)
                {
                    moveTile = (GameTile)victim.up;
                }
                else if (context.world.getActorOnTile(victim.left) == null)
                {
                    moveTile = (GameTile)victim.left;
                }
                else if (context.world.getActorOnTile(victim.down) == null)
                {
                    moveTile = (GameTile)victim.down;
                }
                if (moveTile != null)
                {
                    moveUnit(context.units.ElementAt(i), moveTile);
                    context.units.ElementAt(i).attackTile(victim);

                    Console.WriteLine("platoon unit {0} at ({1}, {2}) moving to ({3}, {4}) and attacking ({5}, {6})", i, context.units.ElementAt(i).curTile.xIndex, context.units.ElementAt(i).curTile.yIndex, moveTile.xIndex, moveTile.yIndex, victim.xIndex, victim.yIndex);
                }
                else
                {
                    Console.WriteLine("platoon unit {0} at ({1}, {2}) couldn't move to attack ({3}, {4})", i, context.units.ElementAt(i).curTile.xIndex, context.units.ElementAt(i).curTile.yIndex, victim.xIndex, victim.yIndex);
                }
            }
            
            /*
            foreach (AnimalActor actor in platoon.units)
            {
                GameTile best = null;
                double best_dist = double.PositiveInfinity;
                foreach (GameTile target in actor.findPaths())
                {
                    // Check that the target tile is empty
                    AnimalActor actor_at_target = platoon.world.getAnimalOnTile(target);
                    if (actor_at_target != null)
                    {
                        continue;
                    }

                    // Take the manhattan distance from the target tile to the centroid
                    double dist = Math.Abs(target.x - centroid_x) + Math.Abs(target.y - centroid_y);
                    if (dist < best_dist)
                    {
                        best = target;
                        best_dist = dist;
                    }

                }

                actor.moveTile(best);
            }
            */
        }
    }
}
