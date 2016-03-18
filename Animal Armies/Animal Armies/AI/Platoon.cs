using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.AI
{
    public class Platoon
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

        Order task;

        public Platoon(GameWorld world, team_t color)
        {
            this.world = world;
            this.team = color;

            this.units = new List<AnimalActor>();
        }

        public void addUnit(AnimalActor unit) 
        {
            if (!units.Contains(unit)) 
            {
                Console.WriteLine("Adding unit at " + unit.position);
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
                    Console.WriteLine("Removing unit at " + units.ElementAt(i).position);
                    units.RemoveAt(i);
                    i--;
                    _unitListChanged = true;
                }
            }
        }

        public void setOrder(Order task)
        {
            this.task = task;
        }

        public void takeTurn()
        {
            task.execute();
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

    }
}
