/**
 * @file Component.cs
 * @author SIGGD, Purdue ACM <siggd.purdue@gmail.com>
 * @version 2.0
 */

namespace Engine
{
    //Component serves as a base class for any--and all--components that will be plugged into the engine
    public class Component
    {
        public bool isActive = true; //Whether the component is currently running
        public MirrorEngine engine { get; private set; } //Reference to the engine

        /**
        * Constructor
        *
        * @param engine the engine
        */
        public Component(MirrorEngine engine)
        {
            this.engine = engine;
        }

        /**
        * Initialization routine to be implemented by sub components if needed
        */
        public virtual void initialize()
        {
        }

        /**
        * Content loading routine to be implemented by sub components if needed
        */
        public virtual void loadContent()
        {
        }

        /**
        * Content unloading routine to be implemented by sub components if needed
        */
        public virtual void unloadContent()
        {
        }
    }
}