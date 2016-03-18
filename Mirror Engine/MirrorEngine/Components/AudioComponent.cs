/**
 * @file AudioComponent.cs
 * @author SIGGD, Purdue ACM <siggd.purdue@gmail.com>
 * @version 2.0
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Tao.Sdl;

namespace Engine
{
    // Provides controls for all audio
    public class AudioComponent : Component
    {
        public Handle currentSong { get; protected set; }   // Currently playing song (null if none)
        private SdlMixer.ChannelFinishedDelegate cfinished; // Global reference to the finished delegate                                
        
        //Channel control
        protected LinkedList<int> openChannels;         // Freelist of available channels
        protected int numChannels = 0;                  // Number of channels to be used in the future, actual number of channels in use may be twice numChannels.
        private bool readyToDeallocate = false;         // Determines whether there are extra channels ready to finish
        protected const int MAXCHANNELS = 128;          // Maximum number of allowed channels
        protected const int DEFAULTNUMCHANNELS = 12;    // Initial and lowest number of allowed channels

        //Master volume
        protected int _masterVolume = 128;
        public int masterVolume
        {
            get
            {
                return _masterVolume;
            }
            set
            {
                if (value < 0) value = 0;
                if (value > 128) value = 128;

                SdlMixer.Mix_Volume(-1, value);
                _masterVolume = value;
            }
        }

        //Music Volume
        protected int _musicVolume = 128;
        public int musicVolume
        {
            get
            {
                return _musicVolume;
            }
            set
            {
                if (value < 0) value = 0;
                if (value > 128) value = 128;

                SdlMixer.Mix_VolumeMusic(value);
                _musicVolume = value;
            }
        }

        /**
        * Constructor initializes SDL Audio
        * allocates DEFAULTNUMCHANNELS channels
        * and sets the SDL ChannelFinished delegate
        *
        * @param theEngine The Engine.
        */
        public AudioComponent(MirrorEngine engine)
            : base(engine)
        {
            if (Sdl.SDL_InitSubSystem(Sdl.SDL_INIT_AUDIO) != 0) {
                throw new Exception("Could not init SDL Audio: " + Sdl.SDL_GetError());
            }

            if (SdlMixer.Mix_OpenAudio(44100, (short)Sdl.AUDIO_S16SYS, 2, 2048) == -1) {
                throw new Exception("Could not init SDL_Mixer: " + SdlMixer.Mix_GetError());
            }

            //Channel Setup
            openChannels = new LinkedList<int>();
            addChannels(DEFAULTNUMCHANNELS);
            cfinished = freeChannel;
            SdlMixer.Mix_ChannelFinished(cfinished);
        }


        /**
        * Pauses all playing channels as well as the currentSong.
        */
        public void pauseAudioEngine()
        {
            for(int i = 0; i < (numChannels + (readyToDeallocate ? 1 : 0) * numChannels); i++)
            {
                SdlMixer.Mix_Pause(i);
            }

            SdlMixer.Mix_PauseMusic();
        }

        /**
        * Resumes all paused channels as well as the currentSong, if paused.
        */
        public void resumeAudioEngine() 
        {
            for (int i = 0; i < (numChannels + (readyToDeallocate ? 1 : 0) * numChannels); i++)
            {
                SdlMixer.Mix_Resume(i);
            }

            SdlMixer.Mix_ResumeMusic();
        }

        /**
        * Stops all channels as well as the currentSong.
        */
        public void stopAudioEngine()
        {
            for (int i = 0; i < (numChannels + (readyToDeallocate ? 1 : 0) * numChannels); i++)
            {
                SdlMixer.Mix_HaltChannel(i);
            }

            SdlMixer.Mix_HaltMusic();
        }


        /**
        * Plays a Handle to a wav file.
        * Allocates more channels when needed, and deallocates extra channels.
        *
        * @param sound the AudioSample to play
        * 
        * @param loop whether or not to loop the sound.
        */
        public void playSound(bool loop, Handle sound)
        {
            IntPtr soundHandle = sound.getResource<AudioSample>().handle;

            int i;
            if ((i = requestChannel()) == -1)
            {
                if (numChannels < MAXCHANNELS)
                {
                    addChannels(numChannels * 2);
                    if((i = requestChannel())== -1 ) return;
                    SdlMixer.Mix_PlayChannel(i, soundHandle, (loop) ? -1 : 0);
                }
            }
            else
            {
                SdlMixer.Mix_PlayChannel(i, soundHandle, (loop) ? -1 : 0);

                if(numChannels > DEFAULTNUMCHANNELS && openChannels.Count > (numChannels * 3/4))
                {
                    removeExtraChannels();
                }
            }

            if (readyToDeallocate) deallocateChannels();
        }

        /**
        * Plays a Handle to an ogg file. Any previously playing music will be stopped
        *
        * @param song The song to play. Providing no song will play the currentSong.
        * 
        * @param loop whether or not to loop the music.
        */
        public void playSong(bool loop, Handle song = null)
        {
            if (song == null) song = currentSong;
            if (song == null) return;

            SongSample sample = song.getResource<SongSample>();
            int retValue = SdlMixer.Mix_PlayMusic(sample.handle, (loop) ? -1 : 1);

            if (retValue == -1) {
                Trace.WriteLine("Music::play(): Could not play music: " + SdlMixer.Mix_GetError());
            } else {
                currentSong = song;
            }
        }

        /**
         * Fades a Handle to an ogg file in.
         * 
         * @param song The song to fade in. Providing no argument will fade the currentSong in.
         * 
         * @param loop whether or not to loop music
         * 
         * @param ms duration of fade in ms
         */ 
        public void fadeSongIn(bool loop, int ms, Handle song = null)
        {
            if (song == null) song = currentSong;
            if (song == null) return;

            SdlMixer.Mix_FadeInMusic(currentSong.getResource<SongSample>().handle, loop ? -1 : 1, ms);
        }

        /*
         * Fades the currentSong out
         * 
         * @param ms duration of fade in ms
         */ 
        public void fadeSongOut(int ms)
        {
            SdlMixer.Mix_FadeOutMusic(ms);
        }

        /**
        * Stops a sound, given the channel.
        *
        * @param channel the channel to stop.
        */
        private void stopChannel(int channel)
        {
            SdlMixer.Mix_HaltChannel(channel);
        }

        /**
        * Sets the volume of a type of sound.
        *
        * @param sound the AudioSample to set
        * 
        * @param volume from 0 to 128
        */
        public void setChunkVolume(Handle sound, int volume)
        {
            if (volume < 0) volume = 0;
            if (volume > 128) volume = 128;

            SdlMixer.Mix_VolumeChunk(sound.getResource<AudioSample>().handle, volume);
        }


        /*
         * Requests an open channel.
         * 
         * @return returns an available channel of zero or greater, or -1 if no channel is available.
         */ 
        private int requestChannel()
        {
            if (openChannels.Count == 0) 
                return -1;

            int i = openChannels.First.Value;
            openChannels.RemoveFirst();

            return i;
        }

        /*
         * Adds a channel back into the freelist openChannels
         * 
         * @param i the channel to add
         */ 
        private void freeChannel(int i)
        {
            if (i < numChannels) openChannels.AddLast(i);
        }

        /*
         * Used to add any channels not already allocated up to x
         * 
         * @param x the highest number of channels to allow
         */ 
        private void addChannels(int x)
        {
            if (x > MAXCHANNELS) x = MAXCHANNELS;

            SdlMixer.Mix_AllocateChannels(x);

            for (int i = numChannels; i < x; i++)
            {
                openChannels.AddLast(i);
                SdlMixer.Mix_Volume(i, _masterVolume);
            }

            numChannels = x;
            readyToDeallocate = false;
        }

        /*
         * Removes extra channels from the freelist, 
         *  and sets readyToDeallocate in order to later remove them from SDL
         */ 
        private void removeExtraChannels()
        {
            int newSize = numChannels * 1/2;
            if (newSize < DEFAULTNUMCHANNELS) newSize = DEFAULTNUMCHANNELS;

            for (int i = numChannels; i >= newSize; i--)
            {
                openChannels.Remove(i);
            }

            numChannels = newSize;
            readyToDeallocate = true;
        }

        /*
         * Deallocates extra channels from SDL
         */ 
        private void deallocateChannels()
        {
            for (int i = numChannels; i < numChannels * 2; i++)
            {
                if (SdlMixer.Mix_Playing(i) == 1) return;
            }

            SdlMixer.Mix_AllocateChannels(numChannels);
            readyToDeallocate = false;
        }
    }
}
