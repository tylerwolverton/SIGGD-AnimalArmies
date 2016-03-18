/**
 * An order context contains all the information needed
 * to carry out an order, including a list of units that
 * the order owns, a reference to the game world, and
 * various helper methods.
 * 
 * Previously known as "platoon".
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;

namespace Game.AI
{
    public class OrderContext
    {
        public GameWorld world;
        public List<AnimalActor> units;
        public readonly team_t team;


        private bool _unitListChanged;
        public bool unitListChanged
        {
            get
            {
                bool result = _unitListChanged;
                _unitListChanged = false;
                return result;
            }
        }

        public OrderContext(GameWorld world, team_t color)
        {
            this.world = world;
            this.team = color;

            this.units = new List<AnimalActor>();
        }

        public void addUnit(AnimalActor unit) 
        {
            if (!units.Contains(unit)) 
            {
                //Console.WriteLine("Adding unit at " + unit.position);
                units.Add(unit);
                _unitListChanged = true;
            }
        }

        public void pruneUnits()
        {
            for (int i = 0; i < units.Count; i++)
            {
                if (units.ElementAt(i).removeMe)
                {
                    //Console.WriteLine("Removing unit at " + units.ElementAt(i).position);
                    units.RemoveAt(i);
                    i--;
                    _unitListChanged = true;
                }
            }
        }

        // Find the center of all dese unitz
        public Engine.Vector2 getCenter()
        {
            float centroid_x = 0.0F;
            float centroid_y = 0.0F;

            foreach (AnimalActor actor in units)
            {
                centroid_x += actor.position.x;
                centroid_y += actor.position.y;
            }
            centroid_x = centroid_x / units.Count;
            centroid_y = centroid_y / units.Count;

            return new Engine.Vector2(centroid_x, centroid_y);
        }

        public GameTile getCenterTile()
        {
            return (GameTile)world.getTileAt(getCenter());
        }

        /**
         * Get a list of all enemy units in the entire world,
         * or if zone is non-null, only get enemies within the zone.
         */
        public List<AnimalActor> getEnemies(RectangleF zone = null)
        {
            List<AnimalActor> enemies = new List<AnimalActor>();
            foreach (LinkedList<AnimalActor> unitList in world.teams)
            {
                // Ignore teams that are us
                if (unitList.Count == 0 || unitList.First.Value.team == this.team)
                    continue;

                foreach (AnimalActor animal in unitList)
                {
                    // Ignore enemies not in the zone (if applicable)
                    if (zone != null && !zone.contains(animal.position))
                        continue;

                    if (!enemies.Contains(animal))
                        enemies.Add(animal);
                }
            }
            return enemies;
        }
    }
}
