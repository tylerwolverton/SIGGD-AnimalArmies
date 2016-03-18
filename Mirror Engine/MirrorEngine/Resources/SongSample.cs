using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Tao.Sdl;

namespace Engine
{

    ///< A handle to a song.
    public class SongSample : ILoadable
    {

        public IntPtr handle { get; private set; }

        public SongSample()
        {
        }
        /*
         * Loads in the ogg file, then stores the handle.
         * 
         * @param path The path to the ogg file.
         */ 
        public SongSample(string path)
        {

            if ((handle = SdlMixer.Mix_LoadMUS(path)) == IntPtr.Zero) 
            {
                throw new Exception("Could not load music file '" + path + "'");
            }
        }

        public void load(ResourceComponent rc, string path)
        {
            if ((handle = SdlMixer.Mix_LoadMUS(path)) == IntPtr.Zero)
            {
                throw new Exception("Could not load music file '" + path + "'");
            }
        }

        ///< Frees the memory used by SdlMixer.
        public void unload()//not being called yet.
        {
            SdlMixer.Mix_FreeMusic(handle);
        }
    }
}
