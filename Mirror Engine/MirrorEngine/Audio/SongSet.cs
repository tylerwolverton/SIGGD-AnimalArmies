/**
 * @file
 * @author PURDUE ACM, SIGGD <siggd.purdue@gmail.com>
 * @version 1.0
 * 
 * @section LICENSE
 * 
 * @section DESCRIPTION
 * Defines the loading and retrieval of a set of songs.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace Engine
{
    /// A set of songs
    /**
    * Groups a set of Musics to be loaded and also played by the AudioComponent.
    */
    public class SongSet: ILoadable
    {

        private readonly ResourceComponent resourceComponent;
        private readonly string songSetPath; ///< Path to the directory of Oggs

        private List<SongSample> music; ///< Contains the ogg files to be played

        /**
        * Constructor stores the path to the ogg files
        *
        * @param musicSetPath the path to the directory of ogg files
        */
        public SongSet(ResourceComponent resourceComponent, string songSetPath)
        {

            this.resourceComponent = resourceComponent;
            this.songSetPath = songSetPath;
            music = new List<SongSample>();
        }

        /**
        * Loads each ogg in the directory
        */
        public void load(String p = "")
        {

            string tsPath = Path.Combine(resourceComponent.rootDirectory, songSetPath);
            IEnumerable<string> musicNames;
            try
            {
                musicNames = Directory.EnumerateFiles(tsPath);
            }
            catch (IOException e) {
                Trace.WriteLine("Failed to load music set at " + tsPath + ": " + e.Message);
                return;
            }

            foreach (string s in musicNames)
            {
                try {
                    music.Add(new SongSample(s));
                }
                catch (Exception e) {
                    Trace.WriteLine("Error, MusicSet: " + tsPath + ", Number: " + music.Count + ": " + e.Message);
                    music.Add(null);
                }
            }
        }

        /**
        * @return number of Musics
        */
        public int Count()
        {
            return music.Count;
        }

        /**
        * @return indexth Music
        */
        public SongSample this[int index]
        {
            get
            {
                if (index > music.Count - 1)
                {
                    index = music.Count - 1;
                }

                return music[index];
            }
        }
    }
}