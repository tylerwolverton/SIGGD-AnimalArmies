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
    * Since XNA doesn't have floating point rectangles for some obscene reason
    */
    public class RectangleF
    {

        public Vector2 topLeft { get; private set; } ///<
        public Vector2 bottomRight { get; private set; } ///<

        public Vector2 topRight
        {
            get
            {
                return new Vector2(right, top);
            }
        }

        public Vector2 bottomLeft
        {
            get
            {
                return new Vector2(left, bottom);
            }
        }

        ///
        public float left
        {
            get
            {
                return topLeft.x;
            }
        }

        ///
        public float top
        {
            get
            {
                return topLeft.y;
            }
        }

        ///
        public float right
        {
            get
            {
                return bottomRight.x;
            }
        }

        ///
        public float bottom
        {
            get
            {
                return bottomRight.y;
            }
        }

        public Vector2 center
        {
            get
            {
                return new Vector2((left + right) / 2, (top + bottom) / 2);
            }
        }

        public float width
        {
            get
            {
                return Math.Abs(right - left);
            }
        }

        public float height
        {
            get
            {
                return Math.Abs(bottom - top);
            }
        }

        public Vector2 size
        {
            get
            {
                return new Vector2(width, height);
            }
        }

        /**
        * desc here
        * 
        * @param paramsdeschere
        * 
        * @return returndeschere
        */
        public RectangleF(Vector2 topLeft, Vector2 bottomRight)
        {

            this.topLeft = topLeft;
            this.bottomRight = bottomRight;
        }

        /**
        * desc here
        * 
        * @param paramsdeschere
        * 
        * @return returndeschere
        */
        public RectangleF(float x, float y, float width, float height)
        {

            if (width < 0 || height < 0) throw new Exception("Invalid rectangle: Negative width/height");

            this.topLeft = new Vector2(x, y);
            this.bottomRight = new Vector2(x + width, y + height);
        }

        /**
        * desc here
        * 
        * @param paramsdeschere
        * 
        * @return returndeschere
        */
        public static bool intersects(RectangleF a, RectangleF b)
        {

            // The idea behind this bit of code is to add up all the possible ways they could NOT intersect, then negate it.
            return !(a.left > b.right
                      || a.right < b.left
                      || a.top > b.bottom
                      || a.bottom < b.top);
        }

        public bool contains(Vector2 a)
        {
            return (a.x >= this.left && a.x <= this.right && a.y >= this.top && a.y <= this.bottom);
        }

        public override String ToString()
        {
            return topLeft.ToString() + " --> " + bottomRight.ToString();
        }
        /**
        * desc here
        * 
        * @param paramsdeschere
        * 
        * @return returndeschere
        */
        public void translate(Vector2 trans)
        {
            topLeft += trans;
            bottomRight += trans;
        }

        public void normalize()
        {
            Vector2 tl = new Vector2(Math.Min(left, right), Math.Min(top, bottom));
            Vector2 br = new Vector2(Math.Max(left, right), Math.Max(top, bottom));
            topLeft = tl;
            bottomRight = br;
        }
    }
}