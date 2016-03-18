using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tao.Sdl;
using Tao.OpenGl;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Engine
{
    public class Texture2D : ILoadable
    {
        public int handle;
        public int texture_format { get; private set; }
        public int width { get; private set; }
        public int height { get; private set; }

        public Texture2D()
        {
        }

        public Texture2D(string path)
        {
            IntPtr surf = SdlImage.IMG_Load(path);
            if (surf == IntPtr.Zero) {
                throw new Exception("Error: failed to load resource: " + Sdl.SDL_GetError());
            }

            initFromSurface(surf);
        }

        public void load(ResourceComponent rc, string path)
        {
            IntPtr surf = SdlImage.IMG_Load(path);
            if (surf == IntPtr.Zero)
            {
                throw new Exception("Error: failed to load resource at " + path + ": " + Sdl.SDL_GetError());
            }

            initFromSurface(surf);
        }

        public Texture2D(IntPtr surf)
        {
            initFromSurface(surf);
        }

        public void initFromSurface(IntPtr surf)
        {
            // Marshal in info
            Sdl.SDL_Surface managedSurf = (Sdl.SDL_Surface)Marshal.PtrToStructure(surf, typeof(Sdl.SDL_Surface));
            width = managedSurf.w;
            height = managedSurf.h;

            IntPtr format = managedSurf.format;
            Sdl.SDL_PixelFormat managedFormat = (Sdl.SDL_PixelFormat)Marshal.PtrToStructure(format, typeof(Sdl.SDL_PixelFormat));

            if (managedFormat.BytesPerPixel == 4) {
                if (managedFormat.Rmask == 0x000000ff)
                    texture_format = Gl.GL_RGBA;
                else
                    texture_format = Gl.GL_BGRA;
            } else if (managedFormat.BytesPerPixel == 3) {
                if (managedFormat.Rmask == 0x000000ff)
                    texture_format = Gl.GL_RGB;
                else
                    texture_format = Gl.GL_BGR;
            } else {
                throw new Exception("Only true color textures supported.");
            }

            Gl.glGenTextures(1, out handle);
            Gl.glBindTexture( Gl.GL_TEXTURE_2D, handle);
            // Nearest neighbor scaling
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_NEAREST);

            // Load texture data into video memory
            Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, managedFormat.BytesPerPixel, width, height, 0, texture_format, Gl.GL_UNSIGNED_BYTE, managedSurf.pixels);
        }

        public void unload()
        {
            Gl.glDeleteTextures(1, ref handle);
        }
    }
}
