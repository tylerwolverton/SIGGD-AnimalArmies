/**
 * @file
 * @author PURDUE ACM, SIGGD <siggd.purdue@gmail.com>
 * @version 1.0
 * 
 * @section LICENSE
 * 
 * @section DESCRIPTION
 */

using System;
using System.Collections.Generic;

namespace Engine
{
    /// brief class desc here
    /**
    * classdeschere
    */
    public class Camera
    {

        private readonly GraphicsComponent graphics; //

        private float _scale = 1; //Window dimensions
        public float scale
        {
            get { return _scale; }
            set
            {
                scaleChange += value - _scale;
                _scale = value;
            }
        }

        public int screenWidth { get; set; } ///<
        public int screenHeight { get; set; } ///<
        public float scaleChange = 0; ///<
        
        public Vector2 position { get; set; } ///< Camera location
        public Vector2 velocity { get; set; } ///<

        public int destHeight { get; set; }  ///< The ideal height of the view rectangle in game pixels

        ///
        public RectangleF viewRect
        {
            get
            {
                return new RectangleF(position.x - screenWidth / 2 / scale, position
                    .y - screenHeight / 2 / scale, screenWidth / scale, screenHeight / scale);
            }
            set
            {
                position = new Vector2(value.topLeft.x + screenWidth / 2 / scale, value.topLeft.y + screenHeight / 2 / scale);
            }
        }
        
        /**
        * desc here
        * 
        * @param paramsdeschere
        * 
        * @return returndeschere
        */
        public Camera(GraphicsComponent graphics, Vector2 position, int destHeight)
        {
            this.graphics = graphics;
            this.position = position;
            this.destHeight = destHeight;
        }

        /**
        * desc here
        * 
        * @param paramsdeschere
        * 
        * @return returndeschere
        */
        public void setView()
        {
            this.screenWidth = graphics.width;
            this.screenHeight = graphics.height;
            this.scale = screenHeight / destHeight;
        }

        public void resetScale()
        {
            this.scale = 1;
        }

        public void performScale(float modifier)
        {
            this.scale *= modifier;
        }

        public void centerToWorld()
        {
            position = graphics.engine.world.getCenter();
        }

        public void centerToMouse()
        {
            position = screen2World(graphics.engine.inputComponent.getMousePosition());
        }

        /**
        * Converts from world coords to screen coords
        *
        * @param w World
        *
        * @return Screen
        */
        public Vector2 world2Screen(Vector2 w)
        {
            return new Vector2((w.x - viewRect.topLeft.x) * scale, (w.y - viewRect.topLeft.y) * scale);
        }

        /**
        * Converts from screen coords to world coords
        *
        * @param s Screen
        *
        * @return World
        */
        public Vector2 screen2World(Vector2 s)
        {
            return new Vector2(s.x / scale + viewRect.topLeft.x, s.y / scale + viewRect.topLeft.y);
        }

        /**
        * desc here
        * 
        * @param paramsdeschere
        * 
        * @return returndeschere
        */
        public void Update(int elapsedTime)
        {
            position += velocity * (((float)elapsedTime)/1000);
        }
    }
}