using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Tao.Sdl;

namespace Engine
{

    ///< A handle to a sound effect.
    public class AudioSample : ILoadable
    {

        public IntPtr handle { get; private set; }

        public AudioSample()
        {
        }

        /*
         * Loads in the wave file, then stores the handle.
         * 
         * @param path The path to the wave file.
         */
        public AudioSample(string path)
        {

            if ((handle = SdlMixer.Mix_LoadWAV(path)) == IntPtr.Zero)
            {
                throw new Exception("Could not load sound file '" + path + "'");
            }
        }

        public void load(ResourceComponent rc, string path)
        {
            
            handle = SdlMixer.Mix_LoadWAV(path);

            if (handle == IntPtr.Zero)
            {
                throw new Exception("Could not load sound file '" + path + "'");
            }
        }

        ///< Frees the memory used by SdlMixer.
        public void unload() //not being called yet
        {
            SdlMixer.Mix_FreeChunk(handle);
        }
    }
}
