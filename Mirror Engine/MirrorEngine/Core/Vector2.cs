/**
 * @file Vector2.cs
 * @author SIGGD, Purdue ACM <siggd.purdue@gmail.com>
 * @version 2.0
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    //A 2D vector and accompanying mathematical operations
    public class Vector2
    {
        public float x;
        public float y;

        public static readonly Vector2 Zero = new Vector2(0, 0); //The zero vector

        //Constructor
        public Vector2(float X, float Y)
        {
            this.x = X;
            this.y = Y;
        }

        // Clone constructor
        public Vector2(Vector2 template)
        {
            this.x = template.x;
            this.y = template.y;
        }

        //Gets the square of the length of the vector
        public float getLengthSquared()
        {
            return x * x + y * y;
        }

        //Gets the length of the vector
        public float getLength()
        {
            return (float)Math.Sqrt(getLengthSquared());
        }

        //Normalizes the vector
        public void Normalize()
        {
            float scale = getLength();
            if (scale == 0) return;
            x /= scale;
            y /= scale;
        }

        //Gets the string representation of this vector
        public override string ToString()
        {
            return "<" + this.x + ", " + this.y + ">";
        }

        //Vector addition
        public static Vector2 operator+(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.x + v2.x, v1.y + v2.y);
        }

        //Vector negation
        public static Vector2 operator -(Vector2 v)
        {
            return new Vector2(-v.x, -v.y);
        }

        //Vector subtraction
        public static Vector2 operator-(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.x - v2.x, v1.y - v2.y);
        }

        //Vector comparison
        public static bool operator==(Vector2 v1, Vector2 v2)
        {
            if (Object.Equals(v1, null))
            {
                if (Object.Equals(v2, null)) return true;
                else return false;
            }
            else if (Object.Equals(v2, null)) return false;

            return v1.x == v2.x && v1.y == v2.y;
        }

        //Vector non-equivalence comparison
        public static bool operator!=(Vector2 v1, Vector2 v2)
        {
            return !(v1 == v2);
        }

        //Normalizes the given vector
        public static Vector2 Normalize(Vector2 v)
        {
            Vector2 ret = new Vector2(v.x, v.y);
            ret.Normalize();
            return ret;
        }

        //Gets the vector made up of the max values of each component of each vector
        public static Vector2 max(Vector2 v1, Vector2 v2)
        {
            return new Vector2(Math.Max(v1.x, v2.x), Math.Max(v1.y, v2.y));
        }

	    /**
	     * Rotates this vector about a given axis.
	     *
	     * @param rotator The axis to rotate about.
	     * @param degrees The degrees of rotation
	     */
	    public static Vector2 rotate(Vector2 rotator, float degrees)
	    {
            Vector2 temp = new Vector2(0, 0);
		    degrees = (float)(-degrees * Math.PI / 180.0f);
            temp.x = (float)((Math.Cos(degrees) * rotator.x) - (Math.Sin(degrees) * rotator.y));
            temp.y = (float)((Math.Cos(degrees) * rotator.y) + (Math.Sin(degrees) * rotator.x));

            return temp;
	    }

        //Vector scaling: v*f
        public static Vector2 operator *(Vector2 v, float f)
        {
            return new Vector2(v.x * f, v.y * f);
        }

        //Vector scaling: f*v
        public static Vector2 operator *(float f, Vector2 v)
        {
            return new Vector2(v.x * f, v.y * f);
        }

        //Vector scaling: v/f
        public static Vector2 operator /(Vector2 v, float f)
        {
            return new Vector2(v.x / f, v.y / f);
        }

        //Vector scaling v*d
        public static Vector2 operator *(Vector2 v, double d)
        {
            return new Vector2((float)(v.x * d), (float)(v.y * d));
        }

        //Vector scaling d*v
        public static Vector2 operator *(double d, Vector2 v)
        {
            return new Vector2((float)(v.x * d), (float)(v.y * d));
        }

        //Vector scaling v/d
        public static Vector2 operator /(Vector2 v, double d)
        {
            return new Vector2((float)(v.x / d), (float)(v.y / d));
        }
    }
}
