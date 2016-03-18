/**
 * @file
 * @author PURDUE ACM, SIGGD <siggd.purdue@gmail.com>
 * @version 1.0
 * 
 * @section LICENSE
 * 
 * @section DESCRIPTION
 * Defines the loading and retrieval of a set of AudioSamples.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace Engine
{

    ///A set of AudioSamples
    /**
    * Groups a set of AudioSamples to be loaded and also played by the AudioComponent.
    */
    public class AudioSet
    {

        private readonly ResourceComponent resourceComponent;
        private readonly string audioSetPath; ///< Path to the directory of Waves

        public List<AudioSample> soundEffects; ///< Contains the Wave files to be played

        /**
        * Constructor stores the path to the Wave files
        * 
        * @param audioSetPath path to the directory of Waves
        */
        public AudioSet(ResourceComponent resourceComponent, string audioSetPath)
        {

            this.resourceComponent = resourceComponent;
            this.audioSetPath = audioSetPath;
            soundEffects = new List<AudioSample>();
        }

        /**
        * Loads each wave in the directory
        */
        public void LoadContent()
        {

            string tsPath = Path.Combine(resourceComponent.rootDirectory, audioSetPath);
            IEnumerable<string> soundEffectNames;
            try
            {
                soundEffectNames = Directory.EnumerateFiles(tsPath);
            }
            catch (IOException e) {
                Trace.WriteLine("Failed to load audioset at " + tsPath + ": " + e.Message);
                return;
            }

            foreach (string s in soundEffectNames)
            {
                try {
                    soundEffects.Add(new AudioSample(s));
                }
                catch (Exception e) {
                    Trace.WriteLine("Error, AudioSet: " + tsPath + ", Number: " + soundEffects.Count + ": " + e.Message);
                    soundEffects.Add(null);
                }
            }
        }

        /**
        * @return number of AudioSamples
        */
        public int Count()
        {
            return soundEffects.Count;
        }

        /**
        * @return indexth AudioSample
        */
        public AudioSample this[int index]
        {
            get
            {
                if (index > soundEffects.Count - 1)
                {
                    index = soundEffects.Count - 1;
                }

                return soundEffects[index];
            }
        }
    }
}