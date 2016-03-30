using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.AI
{
    public class RetreatOrder  :   Order
    { 
         public RetreatOrder(Platoon platoon, GameTile target)
            : base(platoon)
        {
        }
         public override void execute()
         { 
             foreach (AnimalActor aiActor in platoon.units)
             {
                 GameTile best = null;
                 double best_dist = double.NegativeInfinity;
                 foreach (GameTile target in aiActor.findPaths())
                 {
                     // Check that the target tile is empty
                     AnimalActor actor_at_target = platoon.world.getAnimalOnTile(target);
                     if (actor_at_target != null)
                     {
                         continue;
                     }

                     double dist = 0;
                     foreach (Engine.Actor enemyActor in platoon.world.actors)
                     {
                         // Ignore the player in the same team
                         if (enemyActor.color == aiActor.color) continue;

                         // 
                         dist += Math.Log10( Math.Abs(enemyActor.curTile.x - target.x) + Math.Abs(enemyActor.curTile.y - target.y) );
                     }
                     if (dist > best_dist) best = target; 
                 }
                 moveUnit(aiActor, best);
             }
             
            
         }
    }
}
