using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.AI
{
    class HoldPositionOrder : Order
    {
        GameTile position; 
        bool tileCentric;   // True if the platoon is centered on a particular tile, false if it's centered on the group of units
        Engine.Vector2 posVec;
        int attackRadius;

        ClusterOrder cluster;

        public HoldPositionOrder(Platoon platoon, GameTile position, int attackRadius)
            : base(platoon)
        {
            if (position == null && (platoon.units == null || platoon.units.Count == 0))
            {
                this.position = (GameTile)platoon.world.getTileAt(100, 100);
            }
            else if (position == null)
            {
                // We'll hold the position that the units currently occupy
                this.position = (GameTile) platoon.world.getTileAt(platoon.getCenter());
                this.tileCentric = false;
            }
            else
            {
                this.position = position;
                this.tileCentric = true;
            }
            posVec = new Engine.Vector2(this.position.x, this.position.y);
            this.attackRadius = attackRadius;

            this.cluster = new ClusterOrder(this.context, this.position);
        }


        public override void execute()
        {
            Console.WriteLine("Executing hold position order");

            if (context.unitListChanged)
            {
                updatePosition();
            }
            
            AnimalActor closestEnemy = findClosestEnemy();

            if (closestEnemy != null)
            {
                // Code to surround and attack that enemy here.  Will probably use Anubhaw and Andrew's Surround Order.
                AttackOrder attack = new AttackOrder(context, closestEnemy);
                attack.execute();
            }

            // Cluster any remaining unmoved units
            cluster.execute();
        }

        private void updatePosition()
        {
            // Update our position
            if (!this.tileCentric && context.units.Count > 0)
            {
                this.position = (GameTile)context.world.getTileAt(context.getCenter());
                posVec = new Engine.Vector2(this.position.x, this.position.y);
                cluster.setCenter(position);
            }
        }

        private AnimalActor findClosestEnemy()
        {
            AnimalActor closestEnemy = null;
            double closestEnemyDist = double.PositiveInfinity;

            // We prioritize units by their proximity to the position
            foreach (Engine.Actor act in context.world.getActorsInCone(posVec, attackRadius * 32, new Engine.Vector2(0, 0), 360))
            {
                if (!(act is AnimalActor))
                {
                    continue;
                }

                AnimalActor target = (AnimalActor)act;
                Console.WriteLine("Found animal actor at (" + target.position.x + "," + target.position.y + "), team is " + target.team);
                if (target.team == context.team)
                {
                    continue;
                }

                
                // Figure out if this enemey is closer to the the tile.
                double dist = position.manhattan((GameTile)target.curTile);
                Console.WriteLine("Found enemy at (" + target.position.x + "," + target.position.y + "), distance from center is " + dist);
                if (dist < closestEnemyDist)
                {
                    closestEnemyDist = dist;
                    closestEnemy = target;
                }
            }
            return closestEnemy;
        }


    }
}
