/**
 * @file MirrorEngine.cs
 * @author SIGGD, Purdue ACM <siggd.purdue@gmail.com>
 * @version 2.0
 */

using System;
using System.Collections.Generic;
using System.IO;
using Tao.Sdl;
using System.Diagnostics;

namespace Engine
{
    /*
     * The Mirror Engine
     * Manages all of the components that make up the engine, and runs the game loop
     * Provides mechanisms to manage the current world
     */
    public abstract class MirrorEngine
    {
        public static Random randGen = new Random(); //Engine-wide random number generator

        public const int DEFAULTMAPWIDTH = 60;  //The standard width of a new map
        public const int DEFAULTMAPHEIGHT = 40; //The standard height of a new map

        public const int MINTICK = 17;  //Length of one "tick" of the engine, ie. the minimum for how long the engine can take to go through its paces each time
        public bool paused = false;     //Determines whether the engine should run the game-specific components
        private int lastTickTime = 0;   //Time when last tick occurred
        private int lastRendTime = 0;   //Time when last render occurred

        public ResourceComponent resourceComponent { get; protected set; }
        public InputComponent inputComponent { get; protected set; }
        public PhysicsComponent physicsComponent { get; protected set; }
        public GraphicsComponent graphicsComponent { get; protected set; }
        public AudioComponent audioComponent { get; protected set; }
        public EditorComponent editorComponent { get; protected set; }

        public World world { get; protected set; } //The currently running world.
        public string currentWorldName { get; protected set; } //Map-key of the currently running world
        public string gameTitle; //The title of the window

		public Boolean quit = false;
        /**
        * Constructor. Constructs all engine components.
        */
        public MirrorEngine(string gameTitle = "Mirror Engine")
        {
            resourceComponent = new ResourceComponent(this);
            inputComponent = new InputComponent(this);
            physicsComponent = new PhysicsComponent(this);
            graphicsComponent = new GraphicsComponent(this);
            audioComponent = new AudioComponent(this);
            editorComponent = new EditorComponent(this);

            this.gameTitle = gameTitle;
        }

        /**
        * Initializes all engine components after construction. 
        */
        protected virtual void initialize()
        {
            if (resourceComponent != null) resourceComponent.initialize();
            if (inputComponent != null) inputComponent.initialize();
            if (physicsComponent != null) physicsComponent.initialize();
            if (graphicsComponent != null) graphicsComponent.initialize();
            if (audioComponent != null) audioComponent.initialize();
            if (editorComponent != null) editorComponent.initialize();
        }

        /**
        * Loads all content after initialization.
        * Sets the Window's icon to "Content/GameIcon.bmp", if available
        * Sets the Window's title to the gameTitle
        */
        protected virtual void loadContent()
        {
            if (resourceComponent != null) resourceComponent.loadContent();
            if (inputComponent != null) inputComponent.loadContent();
            if (physicsComponent != null) physicsComponent.loadContent();
            if (graphicsComponent != null) graphicsComponent.loadContent();
            if (audioComponent != null) audioComponent.loadContent();
            if (editorComponent != null) editorComponent.loadContent();

            if (gameTitle != "")
            {
                //Set the title of the game window
                Sdl.SDL_WM_SetCaption(gameTitle, gameTitle);

            }
        }

        /**
        * Unloads all content.
        */
        protected virtual void unloadContent()
        {
            if (resourceComponent != null) resourceComponent.unloadContent();
            if (inputComponent != null) inputComponent.unloadContent();
            if (physicsComponent != null) physicsComponent.unloadContent();
            if (graphicsComponent != null) graphicsComponent.unloadContent();
            if (audioComponent != null) audioComponent.unloadContent();
            if (editorComponent != null) editorComponent.unloadContent();
        }

        /*
         * Global run method.
         * Initializes the engine and game, runs the game loop, and unloads all content afterword
         * 
         * @return return value for main()
         */
        public int run()
        {
            // Initialize the engine
            if (Sdl.SDL_Init(0) == -1) throw new Exception("Could not initialize SDL: " + Sdl.SDL_GetError());

            graphicsComponent.SetIcon(ResourceComponent.getFullPathFromKey("GameIcon.bmp"), true); // should be called before initialize() -- see method

            initialize();
            loadContent();
            

            //Jump to Treequake if no world
            if (world == null)
            {
                newWorld(DEFAULTMAPWIDTH, DEFAULTMAPHEIGHT);

                editorComponent.swap();
                editorComponent.selectTool(editorComponent.newMapTool);
                editorComponent.selectTool(editorComponent.editorSettingsTool);
            }

            //Game loop
            while (true)
            {
                //Handle Events and Quit when appropriate
                if (inputComponent.pollEvents() || quit) break;

                //Update the world if there is time
                bool isRunningSlowly = false;
                int curTime = Sdl.SDL_GetTicks();
                if (curTime - lastTickTime > MINTICK - lastRendTime)
                {
                    //Debug.WriteLine("World: " + (curTime - lastTickTime));
                    isRunningSlowly = curTime - lastTickTime > MINTICK * 3;

                    if (!paused)
                    {
                        world.Update();
                        physicsComponent.Update();
                    }

                    if (editorComponent.isActive)
                    {
                        editorComponent.Update();
                    }

                    lastTickTime = curTime;
                }

                //Draw the screen if not running slow
                int beginRendTime = Sdl.SDL_GetTicks();
                if (!isRunningSlowly) graphicsComponent.draw();
                int endRendTime = Sdl.SDL_GetTicks();
                lastRendTime = endRendTime - beginRendTime;
            }

            //End the engine
            unloadContent();
            Sdl.SDL_Quit();
            return 0;
        }

        /**
        * Sets the currently running world
        * 
        * @param filePath the map-key to load
        * @param mapFile the mapFile to load, overrides the filePath
        */
        public virtual void setWorld(string filePath, Mapfile mapFile = null)
        {
            string worldName = ResourceComponent.getKeyFromPath(filePath);
            if (worldName != currentWorldName)
            {
                resourceComponent.discardScriptStates();
            }

            if (!resourceComponent.loadWorld(filePath, mapFile)) return;
            world = resourceComponent.getWorld(worldName);

            world.firstUpdate = true;
            currentWorldName = worldName;
        }

        /*
         * Resets the current world
         */
        public virtual void resetWorld()
        {
            Mapfile tempFile = world.file;
            resourceComponent.killWorld(world);
            resourceComponent.loadWorld(currentWorldName, tempFile);
            setWorld(currentWorldName);
        }

        /*
         * Constructs a fresh world and sets the current world it the new world
         */
        public virtual void newWorld(int width, int height)
        {
            int newID = 0;
            while(resourceComponent.worlds.ContainsKey("Maps/NewWorld" + newID.ToString())) newID++;
            setWorld("Maps/NewWorld" + newID.ToString(), new Mapfile(width, height, "Maps/NewWorld" + newID.ToString()));
        }

        /*
         * Constructs a game world
         * 
         * @param path the path to load the world from
         */
        public World constructGameWorld(string path)
        {
            return constructGameWorld(new Mapfile(path));
        }

        /*
         * Constructs a game world
         * 
         * @param file the Mapfile to load the world from
         */
        public abstract World constructGameWorld(Mapfile file);
    }
}