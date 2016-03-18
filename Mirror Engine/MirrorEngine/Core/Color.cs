using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tao.Sdl;

namespace Engine
{

    //Represents an RGB(A) color
    public struct Color
    {
        public static readonly Color WHITE = new Color(1.0f, 1.0f, 1.0f);
        public static readonly Color BLACK = new Color(0f, 0f, 0f);
        public static readonly Color RED = new Color(1.0f, 0, 0);
        public static readonly Color GREEN = new Color(0, 1.0f, 0);
        public static readonly Color BLUE = new Color(0, 0, 1.0f);

        public float r;
        public float g;
        public float b;
        public float a; //Alpha controls the opacity of the color, or the intensity of light in certain contexts.

        //Gets the color as an SDL_Color
        public Sdl.SDL_Color sdlColor
        {
            get
            {
                Sdl.SDL_Color col;
                col.r = (byte)(r * 255);
                col.g = (byte)(g * 255);
                col.b = (byte)(b * 255);
                col.unused = (byte)(a * 255);
                return col;
            }
        }

        //RGB(A) Constructor
        public Color(float R, float G, float B, float A = 1.0f)
        {
            this.r = R;
            this.g = G;
            this.b = B;
            this.a = A;
        }

        //Grayscale constructor
        public Color(float C, float A = 1.0f)
        {
            r = g = b = C;
            this.a = A;
        }

        //Gets the average value of the hues
        public float getBrightness()
        {
            return (r + g + b) / 3;
        }

        //Limits all values between 0 and 1, and normalizes the other hue values if needed
        public Color normalize(int value = 1)
        {
            //Normalize to r = 1 if r > 1
            if (r > value)
            {
                g = value * (g / r);
                b = value * (b / r);
                r = value;
            }

            //Normalize to g = 1 if g > 1
            if (g > value)
            {
                b = value * (b / g);
                r = value * (r / g);
                g = value;
            }

            //Normalize to b = 1 if b > 1
            if (b > value)
            {
                r = value * (r / b);
                g = value * (g / b);
                b = value;
            }

            //Clamp all values to 0, and clamp a to 1
            if (a > 1) a = 1;
            if (a < 0) a = 0;
            if (r < 0) r = 0;
            if (g < 0) g = 0;
            if (b < 0) b = 0;

            return this;
        }

        //Map color to SDL int
        public int MapColor(IntPtr pixFmt)
        {
            if (a == 1.0f)
            {
                return Sdl.SDL_MapRGB(pixFmt, (byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
            }
            else
            {
                return Sdl.SDL_MapRGBA(pixFmt, (byte)(r * 255), (byte)(g * 255), (byte)(b * 255), (byte)(a * 255));
            }
        }

        //Color addition, involves alpha
        public static Color operator +(Color c1, Color c2)
        {
            return new Color(c1.r + c2.r, c1.g + c2.g, c1.b + c2.b, c1.a + c2.a);
        }

        //Color float addition, does not involve alpha
        public static Color operator +(Color c, float f)
        {
            return new Color(c.r + f, c.g + f, c.b + f, c.a);
        }

        //Color dot product, involves alpha
        public static Color operator *(Color c1, Color c2)
        {
            return new Color((c1.r * c2.r), (c1.g * c2.g), (c1.b * c2.b), (c1.a * c2.a));
        }

        //Color scaling, does not involve alpha
        public static Color operator *(Color c, float f)
        {
            return new Color(c.r * f, c.g * f, c.b * f, c.a);
        }

        //Gets the average of the two colors
        public static Color Avg(Color c1, Color c2)
        {
            return (c1 + c2) * 0.5f;
        }

        //Color comparison
        public static bool operator ==(Color c1, Color c2)
        {
            return c1.r == c2.r && c1.g == c2.g && c1.b == c2.b && c1.a == c2.a;
        }

        //Color non-equivalence comparison
        public static bool operator !=(Color c1, Color c2)
        {
            return !(c1 == c2);
        }
    }
}
