/**
 * @file ResourceComponent.cs
 * @author SIGGD, Purdue ACM <siggd.purdue@gmail.com>
 * @version 2.0
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using Tao.Sdl;
using System.Reflection;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace Engine
{
    //Manages resources and worlds, provides access to Handles.
    public class ResourceComponent : Component
    {
        //Directory
        public const string DEVELOPROOTPREFIX = "../../";               //Root prefix for content to be added/changed during runtime
        public const string DEFAULTROOTDIRECTORY = "Content";           //Default root directory for all content
        public static string rootDirectory { get; protected set; }      //The current root directory being using. Determined at runtime (handles are relative to this)

        //Cache
        protected const int SIZE_OF_CACHE = 150;                        //The size limit of the cache
        private Handle[] cache;                                         //The array cache of Handles
        private LinkedList<int> freeList = new LinkedList<int>();       //A List of available positions in the cache
        private Dictionary<string, int> cacheLookup;                    //A string to position dictionary for cache lookup
        private LinkedList<Handle> lruList = new LinkedList<Handle>();  //A list that keeps track of the least recently used Handle

        //Scripting
        public ScriptRuntime scriptRuntime { get; private set; }                //The environment for running scripts
        private Dictionary<ScriptScope, string> scriptScopes;                   //Dictionary of ScriptScopes to their handle-keys, used for scope recovery
        private Dictionary<string, Dictionary<string, dynamic>> scriptStates;   //Stores the state of all unique scripts for later recovery
        public ScriptEngine scriptEngine {                                      //The engine for running scripts
            get
            {
                return scriptRuntime.GetEngine("py");
            }
        }

        //Worlds
        protected const string mapDirectory = "Maps";   //Directory containing Maps
        internal Dictionary<string, World> worlds;      //Dictionary of world-keys to Worlds

        /**
        * Constructor. Sets the rootDirectory based on the current build.
        * Initializes the cache, the scripting engine, freetype, and the world dictionary.
        *
        * @param engine the engine
        */
        public ResourceComponent(MirrorEngine engine)
            : base(engine)
        {
            // Set root directory
            rootDirectory = DEFAULTROOTDIRECTORY;
#if ENGINEDEVELOP
            rootDirectory = DEVELOPROOTPREFIX + rootDirectory;
#endif

            //Initialize cache
            cache = new Handle[SIZE_OF_CACHE];
            cacheLookup = new Dictionary<string, int>();
            for (int i = 0; i < SIZE_OF_CACHE; i++)
            {
                freeList.AddLast(i);
            }

            // Initialize Scripting Engine
            Dictionary<string, object> opts = new Dictionary<string, object>();
            opts["Debug"] = true;
            ScriptEngine eng = Python.CreateEngine(opts);
            scriptRuntime = eng.Runtime;
            // Inject executing C# assembly
            scriptRuntime.LoadAssembly(Assembly.GetExecutingAssembly());
            // Inject game C# assembly
            scriptRuntime.LoadAssembly(Assembly.GetEntryAssembly());

            //Initialize script scopes dictionary
            scriptScopes = new Dictionary<ScriptScope, string>();

            //Initialize Freetype
            if (SdlTtf.TTF_Init() == -1) throw new Exception("Could not initialize SDL_ttf");

            //Intialize world Dictionary
            worlds = new Dictionary<string, World>();
        }

        /*
         * Initializes the resource component. 
         * Initializes the scriptEngine.
         */
        public override void initialize()
        {
            //Get path to working scripts
            string scriptsPath = Path.Combine(DEVELOPROOTPREFIX, DEFAULTROOTDIRECTORY, "Scripts");

            //Add path
            ICollection<String> paths = scriptEngine.GetSearchPaths();
            paths.Add(Path.GetFullPath(scriptsPath));
            scriptEngine.SetSearchPaths(paths);
        }

        /*
         * Converts a full/relative path or handle-key into a handle-key. 
         * Useful for guaranteeing that a string is a key to a Handle.
         */
        public static string getKeyFromPath(string fullPath)
        {
            if (!fullPath.Contains(DEFAULTROOTDIRECTORY)) return fullPath;
            return fullPath.Substring(fullPath.IndexOf(DEFAULTROOTDIRECTORY) + DEFAULTROOTDIRECTORY.Length + 1);
        }

        /*
         * Converts a handle-key into a relative path if it is not already rooted. 
         * Uses ResourceComponent.rootDirectory to create relative path. 
         * Useful for guaranteeing that a string is a full path without disrupting any existing full path.
         */
        public static string getFullPathFromKey(string key)
        {
            if (Path.IsPathRooted(key)) return key;
            return Path.GetFullPath(Path.Combine(rootDirectory, key));
        }

        /*
         * Returns a fresh scope for a script to use. 
         * Requires the script's key for unique identification behind-the-scenes.
         */
        public ScriptScope createScope(string scriptKey)
        {
            ScriptScope scope = scriptRuntime.CreateScope();
            scriptScopes.Add(scope, scriptKey);
            return scope;
        }

        /*
         * Returns the World corresponding to the given key if it has been created. Otherwise, returns null.
         */
        public World getWorld(string worldName)
        {
            if (!worlds.ContainsKey(worldName)) return null;
            return worlds[worldName];
        }

        /*
         * If the given key hasn't been loaded, then loads the given key. 
         * If a Mapfile has been provided, then it loads the world from the map, not from disk. 
         * 
         * @return whether the load was successful.
         */
        public bool loadWorld(string filePath, Mapfile map = null)
        {
            //Get the world's name
            string worldName = getKeyFromPath(filePath);

            //Load world if it hasn't been already
            if (!worlds.ContainsKey(worldName) || worlds[worldName] == null)
            {
                World w;

                if (map == null) //Load from disk
                {
                    //Check if file exists
                    if (!File.Exists(getFullPathFromKey(filePath)))
                    {
                        Trace.WriteLine(filePath + " does not exist.");
                        return false;
                    }

                    w = engine.constructGameWorld(filePath);
                }
                else //Load from Mapfile
                {
                    w = engine.constructGameWorld(map);
                }

                //Load the world
                w.initialize();
                w.loadContent();

                //Add the world
                worlds[worldName] = w;
            }

            return true;
        }

        /*
         * Adds the given World to the set of worlds. Useful for manually loading worlds.
         */
        public virtual void addWorld(World world)
        {
            worlds[world.worldName] = world;
        }

        /*
         * Removes the given World from the set of worlds.
         */
        public virtual void killWorld(World world)
        {
            worlds[world.worldName] = null;
            flushScriptScopes();
        }

        /**
         * Called whenever a handle needs to be added to the cache.
         * 
         * @param h The handle that is being added
         */
        internal void addResource(Handle h)
        {
            //Don't add resource if it is already in the cache
            if (cacheLookup.ContainsKey(h.key)) return;

            //Determine if cache is full
            LinkedListNode<int> node = freeList.First;
            if (node == null)
            {
                //Make room in cache
                removeResource();
            }

            //Get available position from freelist and add the handle to it
            node = freeList.First;
            int findVal = node.Value;
            cache[findVal] = h;

            //Update the freelist and cachelookup
            freeList.RemoveFirst();
            cacheLookup.Add(h.key, findVal);

            //Add to LRU list in the first position
            LinkedListNode<Handle> lruNode = new LinkedListNode<Handle>(h);
            lruList.AddFirst(lruNode);
        }

        /**
         * Gets the Handle corresponding to the given key.
         * 
         * @param key the key to the resource. Can be a full path, in order to override the default directory.
         * 
         * @return An existing handle, or a new handle if none exists
         */
        public Handle get(String key)
        {
            //Override for requesting a dead Handle
            if (key == "") return new Handle(this, "");

            //Check if is in the cache already
            string trueKey = getKeyFromPath(key);
            if (cacheLookup.ContainsKey(trueKey)) 
                return cache[cacheLookup[trueKey]];

            //Create a new handle
            key = getFullPathFromKey(key);
            Handle newHandle = new Handle(this, key);
            return newHandle;
        }

        /**
         * Gets the listof handles of every file in the directory
         * 
         * @param directory the directory to search, relative to the root directory
         */
        public List<Handle> discoverHandles(String directory)
        {
            List<Handle> handles = new List<Handle>();
            String contentPath = Path.Combine(rootDirectory, directory);

            //Get all paths
            IEnumerable<string> handlePaths = null;
            try
            {
                handlePaths = Directory.EnumerateFiles(contentPath);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                return null;
            }

            //Get handles for each path
            foreach (string s in handlePaths)
            {
                handles.Add(new Handle(this, Path.GetFullPath(s)));
            }

            //Return null if none found
            if (handles.Count == 0) return null;

            return handles;
        }

        /**
         * Called whenever a handle is used in order to update the lru list
         *
         * @param h the handle to update
         */
        internal void updateLRU(Handle h)
        {
            if (!lruList.Contains(h)) return;

            LinkedListNode<Handle> node = lruList.Find(h);
            lruList.Remove(node);
            lruList.AddFirst(node);
        }

        /**
         * Removes the least recently used handle
         */
        private void removeResource()
        {
            Handle eject = lruList.Last.Value;  //The node to eject from the cache
            removeResource(eject);
        }

        /**
         * Removes the given handle from the cache,
         * and adds its position back to the freelist.
         */
        internal void removeResource(Handle h)
        {
            //Get index of cache to remove resource from
            int indexToRemove = cacheLookup[h.key];

            //Remove handle from the lookup, lrulist and cache, and destroy it.
            cacheLookup.Remove(h.key);
            lruList.Remove(h);
            cache[indexToRemove] = null;
            h.destroy();

            //add index back to freelist
            freeList.AddLast(indexToRemove);
        }

        /*
         * Flushes all Handles from the cache
         */ 
        public void flushAll()
        {
            while (lruList.Count > 0)
            {
                removeResource();
            }
        }

        /*
         * Flushes all Handles of type T from the cache
         */ 
        public void flush<T>() where T : class, ILoadable, new()
        {
            for(int i = 0; i < cache.Length; i++)
            {
                if (cache[i] != null && cache[i].isType<T>()) removeResource(cache[i]);
            }
        }

        /*
         * Flushes all ScriptScopes from the scriptScopes dictionary
         */ 
        internal void flushScriptScopes()
        {
            scriptScopes.Clear();
        }

        /*
         * Discards all stored script states
         */ 
        internal void discardScriptStates()
        {
            scriptStates = null;
        }

        /*
         * Stores the states of all unique scripts for later recovery
         */ 
        internal void salvageScripts()
        {
            //Create script states dictionary and a temporary black list
            scriptStates = new Dictionary<string,Dictionary<string,dynamic>>();
            List<string> blackList = new List<string>();

            //Salvage each script
            foreach (KeyValuePair<ScriptScope, string> pair in scriptScopes)
            {
                if (pair.Key != null)
                {
                    //Get the script's values
                    ScriptScope scriptScope = pair.Key;
                    string scriptKey = pair.Value;

                    //Check if the script is unique, otherwise, blacklist it and continue
                    if (scriptStates.ContainsKey(scriptKey))
                    {
                        scriptStates.Remove(scriptKey);
                        blackList.Add(scriptKey);
                        continue;
                    }

                    //Create temporary storage for Python state
                    IronPython.Runtime.List stateToSalvage;

                    //Allow script to prepare itself for salvaging
                    dynamic salvageState;
                    bool successC = scriptScope.TryGetVariable("salvageState", out salvageState);
                    try
                    {
                        if (successC && salvageState != null) salvageState();
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(Script.getRuntimeError(e, scriptKey));
                    }
                    
                    //Obtain desired variables to salvage
                    bool successA = scriptScope.TryGetVariable<IronPython.Runtime.List>("stateToSalvage", out stateToSalvage);
                    if (!successA) continue;

                    //Create a dictionary of script variable names to dynamic values
                    Dictionary<string, dynamic> scriptDictionary = new Dictionary<string, dynamic>();

                    //Save desired variables
                    foreach (string s in stateToSalvage)
                    {
                        dynamic value = null;
                        bool successB = scriptScope.TryGetVariable(s, out value);
                        if (successB && value != null) scriptDictionary.Add(s, value);
                    }

                    //Add the script to the dictionary if it isn't empty
                    if (scriptDictionary.Count > 0)
                    {
                        scriptStates.Add(scriptKey, scriptDictionary);
                    }
                }
            }
        }

        /*
         * Updates existing scripts based on salvaged script states
         */
        internal void recoverScripts()
        {
            if (scriptStates == null) return;
            
            //For each salvaged script
            foreach (KeyValuePair<ScriptScope, string> pair in scriptScopes)
            {
                if(pair.Key != null && scriptStates.ContainsKey(pair.Value))
                {
                    //Get the script's values
                    ScriptScope scriptScope = pair.Key;
                    string scriptKey = pair.Value;
                    Dictionary<string, dynamic> scriptDictionary = scriptStates[scriptKey];

                    //Create temporary storage for Python state
                    IronPython.Runtime.List stateToSalvage;
                    
                    //Obtain desired variables to salvage
                    bool successA = scriptScope.TryGetVariable<IronPython.Runtime.List>("stateToSalvage", out stateToSalvage);
                    if (!successA) continue;

                    //Update state 
                    foreach(KeyValuePair<string, dynamic> scriptState in scriptDictionary)
                    {
                        if(stateToSalvage.Contains(scriptState.Key))
                            scriptScope.SetVariable(scriptState.Key, scriptState.Value);
                    }

                    //Allow script to recover itself from salvaging
                    dynamic recoverState;
                    bool successC = scriptScope.TryGetVariable("recoverState", out recoverState);
                    try
                    {
                        if (successC && recoverState != null) recoverState();
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(Script.getRuntimeError(e, scriptKey));
                    }
                }
            }

            scriptStates = null;
        }
    }
}