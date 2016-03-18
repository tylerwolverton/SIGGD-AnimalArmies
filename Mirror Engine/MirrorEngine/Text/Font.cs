using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tao.Sdl;

namespace Engine
{
    /**
     * This class represents a Font resource tied to the SDL_ttf library. Supported formats are TTF and OTF.
     * A font in SDL_ttf is bound to one particular point size; to change the point size of a font, the whole font must be reloaded by disk.
     * This class provides caching for different sizes of the same font. Once a particular font size has been loaded, it remains in memory until the Font object is disposed of.
     */
    public class Font : ILoadable, IDisposable
    {
        string path;  ///< Path to the font
        Dictionary<int, IntPtr> handles;   ///< Dictionary mapping point sizes to parsed font handles

        public Font()
        {
        }

        /**
         * Construct a new Font from a file.
         * 
         * @param path The path to the font file 
         */
        public Font(string path)
        {
            this.path = path;

            // Initialize font handle dictionary
            handles = new Dictionary<int, IntPtr>();

            // Attempt to parse the font with the initial font size (to immediately detect if the font is invalid)
            getFontHandle(12);
        }

        public void load(string path)
        {
            this.path = path;

            // Initialize font handle dictionary
            handles = new Dictionary<int, IntPtr>();

            // Attempt to parse the font with the initial font size (to immediately detect if the font is invalid)
            getFontHandle(12);
        }

        /**
         * Obtain a pointer to the SDL_TTF font handle for a particular point size. Caching occurs here.
         *
         * @param size The point size to retrieve
         * @return A pointer to the font's handle
         */
        public IntPtr getFontHandle(int size) {
            // Load the font if it is not already present
            if (!handles.ContainsKey(size)) {
                IntPtr handle = SdlTtf.TTF_OpenFont(path, size);
                if (handle == IntPtr.Zero) {
                    throw new Exception("Could not open font " + path + " at point size " + size);
                }
                handles[size] = handle;
            }

            // Return the font handle
            return handles[size];
        }

        /**
         * Calculates the size that a string would occupy if it were renderered.
         * 
         * @param text The text to size
         * @param size The point size of the font
         * @return A vector containing the width and height of the given text at the given point size
         */
        public Vector2 calcTextSize(string text, int size)
        {
            int w, h;
            // Calculate the size (C# uses UNICODE)
            if ( SdlTtf.TTF_SizeUNICODE(getFontHandle(size), text, out w, out h) == -1)
                throw new Exception("Text sizing failed: " + SdlTtf.TTF_GetError());

            // Return the size
            return new Vector2(w, h);
        }

        /**
         * Frees all native resources used.
         */
        public void Dispose()
        {
            // Free the loaded font handles
            foreach (var kv in handles) {
                SdlTtf.TTF_CloseFont(kv.Value);
            }

            // Clear out the font handle array
            handles.Clear();
        }
    }
}
