using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;

using Cluster = System.Tuple<Engine.Vector2, System.Collections.Generic.List<Engine.Actor>, System.Double>;

namespace Game.AI
{
    class DivideConquerOrder : Order
    {
        public DivideConquerOrder(OrderContext context)
            : base(context)
        {

        }

        public override void execute()
        {
            List<Cluster> clusters;
            clusters = KMeans.getClusters(context.getEnemies().ConvertAll(x => (Actor)x));
            
            foreach (Cluster c in clusters)
            {
                Console.WriteLine("Found cluster with " + c.Item2.Count + " elements at " + c.Item1);
                foreach (Actor a in c.Item2)
                {
                    Console.WriteLine("\t" + a.curTile + " " + ((AnimalActor)a).team);
                }
            }
        }
    }
}
