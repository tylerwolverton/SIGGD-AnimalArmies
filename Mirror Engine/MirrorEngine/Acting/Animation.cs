/**
 * @file Animation.cs
 * @author SIGGD, PURDUE ACM <siggd.purdue@gmail.com>
 * @version 2.0
 */

using System;
using System.Collections.Generic;

namespace Engine
{
    /**
    * Contains a set of sprites and cycles through them. 
    * Provides functionality for performing actions on certain frames
    */
    public class Animation
    {

        /**
        * Goes through every animation created and destroys all of their frames. 
        * They are then reloaded. It is a solution for something to do with how edge condition handles are handled with 'resourceComponent.flush<Texture2D>();'
        * Pretty Hackish 
        */
        private static List<Handle[]> AnimationList = new List<Handle[]>(); //List of every animation created
        public static void flushAnimations()
        {
            foreach (Handle[] handles in AnimationList)
            {
                foreach (Handle h in handles)
                {
                    h.destroy();
                }
            }
        }


        private string animDir;     //The location of the sprites
        private Handle[] frames;    //The array of sprites
        public bool loop;           //Whether the animation will loop
        public int delay;           //Given delay value between iterations of animation
        public int curDelay;        //Counter to determine when delay expires
        public bool finished;       //Whether the animation is done animating
        public int curFrame;        //The index of the current sprite
        public bool isNewFrame;     //Whether the frame has changed since the last tick
        
        public int xoffset { get; private set; } //The standard x offset for all the sprites
        public int yoffset { get; private set; } //The standard y offset for all the sprites

        public float ticksPerFrame { get; set; }    //The framerate
        private float ticksSinceLast = 0;           //The number of ticks since the last frameswap

        //Actions 
        private List<FrameAct> frameActs;   //A list of frameactions tied to specific frames
        private List<PredAct> predActs;     //A list of predactions tied to series of frame indices
        private int curFrameAct;            //The index of the most recently triggered frame action
        
        // Action types
        public delegate void Action(int frame); //Delegate for an action

        /**
        * Constructor. Loads the frame handles
        */
        public Animation(ResourceComponent rc, string animDir, int xoffset = 0, int yoffset = 0, float ticksPerFrame = 6, bool loop = true, int delay = 0)
        {
            if (animDir.Equals(""))
            {
                throw new Exception("Animation directory not provided.");
            }

            this.animDir = animDir;
            this.xoffset = xoffset;
            this.yoffset = yoffset;
            this.ticksPerFrame = ticksPerFrame;
            this.loop = loop;
            this.delay = delay;
            
            frames = rc.discoverHandles(animDir).ToArray();
            AnimationList.Add(frames);
        }

        //Destroys all the sprites of the animation
        public void unload()
        {
            foreach (Handle h in frames)
            {
                h.destroy();
            }
        }

        //Get current sprite
        public Handle getCurTex()
        {
            return frames[curFrame];
        }

        //Runs the animation and calls any necessary actions
        public void run()
        {
            if (finished) return;

            if (ticksSinceLast >= ticksPerFrame)
            {
                if (curFrame >= frames.Length - 1)
                {
                    if (loop)
                    {
                        // Account for delay between iterations of animation
                        if (curDelay <= 0)
                        {
                            curFrame = 0;
                            curFrameAct = 0;
                            curDelay = delay;
                            isNewFrame = true;
                        }
                        else
                        {
                            curDelay--;
                        }
                    }
                    else
                    {
                        finished = true;
                    }
                }
                else
                {
                    curFrame++;
                    isNewFrame = true;
                }

                ticksSinceLast -= ticksPerFrame;
            }

            if (isNewFrame)
            {
                // Consider Frame Actions
                if (frameActs != null)
                {
                    int len = frameActs.Count;
                    while (curFrameAct < len && frameActs[curFrameAct].frame < curFrame)
                    {  // Seek to find the next frame act
                        curFrameAct++;
                    }

                    while (curFrameAct < len && frameActs[curFrameAct].frame == curFrame)
                    {
                        frameActs[curFrameAct].run(curFrame); // Try it
                        curFrameAct++;
                    }

                    if (curFrameAct >= len)
                        curFrameAct = 0;
                }

                // Consider Predicate Actions
                if (predActs != null)
                {
                    foreach (PredAct pAct in predActs)
                    {
                        pAct.run(curFrame);  // Try all predicate actions
                    }
                }

                isNewFrame = false;
            }

            ticksSinceLast++;
        }

        //Resets the animation
        public void reset()
        {
            curFrame = 0;
            ticksSinceLast = 0;
            isNewFrame = true;
            curFrameAct = 0;
            finished = false;
        }

        /**
        * Removes all actions
        */
        public void clearFrameActs()
        {
            if (frameActs != null) frameActs.Clear();
            if (predActs != null) predActs.Clear();
        }

        /**
        * Adds a frame action to the animation
        *
        * @param frame the frame to call the action
        * @param action the action to call
        */
        public void addFrameAct(int frame, Action action)
        {
            FrameAct fAct = null;

            if (frame < 0 || frame > frames.Length - 1)
                throw new Exception("addFrameAct: Frame out of bounds.");

            if (action == null)
                throw new Exception("addFrameAct: Null action");

            if (frameActs == null)
            {
                frameActs = new List<FrameAct>();
            }
            else
            {
                fAct = frameActs.Find((a) => (a.frame == frame));
            }

            if (fAct == null)
            {
                fAct = new FrameAct(frame);
                fAct.action += action;
                frameActs.Add(fAct);
            }
            else
            {
                fAct.action += action;
            }
        }

        /**
        * Add an action to the first frame
        */
        public void addBeginAct(Action action)
        {
            addFrameAct(0, action);
        }

        /**
        * Add an action to the last frame
        */
        public void addEndAct(Action action)
        {
            addFrameAct(frames.Length - 1, action);
        }

        /**
        * Add a predicate action to the animation
        *
        * @param pred the series of indices to call the action on
        * @param action the action to call
        */
        public void addPredAct(Predicate<int> pred, Action action)
        {
            PredAct pAct = null;

            if (pred == null)
                throw new Exception("addPredAct: Null predicate");

            if (action == null)
                throw new Exception("addPredAct: Null action");

            if (predActs == null)
            {
                predActs = new List<PredAct>();
            }
            else
            {
                pAct = predActs.Find((a) => (a.pred.Equals(pred)));
            }

            if (pAct == null)
            {
                pAct = new PredAct(pred);
                pAct.action += action;
                predActs.Add(pAct);
            }
            else
            {
                pAct.action += action;
            }
        }

        //An action tied to a specific frame
        private class FrameAct
        {
            public readonly int frame;  //The index to call the action on
            public event Action action; //The action to call

            //Creates the frameact
            public FrameAct(int frame)
            {
                this.frame = frame;
            }

            //Determines if the action needs to be called and calls it
            public void run(int frame)
            {
                if (this.frame == frame) action(frame);
            }
        }

        //An action with a predicate
        private class PredAct
        {
            public readonly Predicate<int> pred; //The indices to call the action on
            public event Action action;          //The action to call

            //Creates a predact
            public PredAct(Predicate<int> pred)
            {
                this.pred = pred;
            }

            //Determines if the action needs to be called and calls it
            public void run(int frame)
            {
                if (pred(frame)) action(frame);
            }
        }
    }
}