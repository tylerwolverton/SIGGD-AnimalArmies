using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;

namespace Game
{
    public class CameraManager
    {
        private GraphicsComponent gc;
        private Camera cam;

        // Minimum distance the camera will jump, in pixels
        private static int CAM_JUMP_MIN = 5 * GameTile.size;

        // Each player can save one camera position
        private Vector2[] player_positions;

        public CameraManager(Game engine, int num_teams)
        {
            gc = engine.graphicsComponent;
            cam = engine.graphicsComponent.camera;
            player_positions = new Vector2[num_teams];
        }

        /**
         * Set the camera position absolutely
         * 
         * @param pos Pixel to be at the center of the screen
         * @param draw Should we redraw the screen with the new camera pos?
         * 
         * @return True if the camera moved, false otherwise
         */
        public bool moveCamera(Vector2 pos, bool draw = false)
        {
            if (Math.Abs(pos.x - cam.position.x) + Math.Abs(pos.y - cam.position.y) > CAM_JUMP_MIN)
            {
                cam.position = new Vector2(pos);
                if (draw)
                {
                    gc.draw();
                }
                return true;
            }
            return false;
        }

        /**
         * Move the camera relative to its current position
         * 
         * @param dir The offset to add to the camera's current position
         * @param draw Should we redraw the screen with the new camera pos?
         * 
         * @return True if the camera moved, false otherwise
         */
        public bool panCamera(Vector2 dir, bool draw = false)
        {
            cam.position += dir;
            if (draw)
            {
                gc.draw();
            }
            return true;
        }

        /**
         * Get the camera's current position, in pixels from the top left
         */
        public Vector2 getPosition()
        {
            Vector2 ret = new Vector2(cam.position);
            return ret;
        }

        /**
         * Save the current camera position.  Each team gets one slot.
         * 
         * @param team Current player's team
         * 
         * @return same as getPosition()
         */
        public Vector2 saveView(team_t team)
        {
            player_positions[(int)team] = new Vector2(cam.position);
            return player_positions[(int)team];
        }

        /**
         * Load a saved camera position.  Each team gets one slot.
         * 
         * @param team Current player's team
         * 
         * @return True if the camera moved, false otherwise
         */
        public bool loadView(team_t team, bool draw = false)
        {
            if (player_positions[(int)team] == null)
            {
                return false;
            }
            return moveCamera(player_positions[(int)team], draw);
        }
    }


}
