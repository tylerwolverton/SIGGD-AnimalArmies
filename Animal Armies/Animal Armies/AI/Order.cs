using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Engine;

namespace Game.AI
{
    public abstract class Order
    {
        protected Platoon platoon;

        public Order(Platoon platoon)
        {
            this.platoon = platoon;
        }

        public abstract void execute();

        private static int CAM_SLEEP_TIME = 8;

        // Move the camera with a nice jump for the user to see
        protected bool moveUnit(AnimalActor unit, GameTile target)
        {
            if (!unit.canMoveTo(target))
            {
                return false;
            }

            platoon.world.cameraManager.moveCamera(unit.position, true);
            Thread.Sleep(CAM_SLEEP_TIME);
            //for (int i = 0; i < 30; i++)
            //{
            //    platoon.world.engine.graphicsComponent.draw();
            //    Thread.Sleep(CAM_SLEEP_TIME);
            //    platoon.world.Update();
            //}

            unit.moveTile(target);
            platoon.world.engine.graphicsComponent.draw();
            // Sleep to let the user adjust
            Thread.Sleep(CAM_SLEEP_TIME);
            //for (int i = 0; i < 30; i++)
            //{
            //    platoon.world.engine.graphicsComponent.draw();
            //    Thread.Sleep(CAM_SLEEP_TIME);
            //    platoon.world.Update();
            //}
            return true;
        }

        public void addUnit(AnimalActor unit)
        {
            platoon.addUnit(unit);
        }
    }

    enum order_t
    {
        cluster,
        attack,
        surround,
        defend,
        move,
        retreat
    };
	 

}
