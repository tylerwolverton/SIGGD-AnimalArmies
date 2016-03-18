/**
 * @file Behavior.cs
 * @author SIGGD PURDUE ACM <siggd.purdue@gmail.com>
 * @version 2.0
 */

using System;
using System.Collections.Generic;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System.Diagnostics;
using Microsoft.CSharp.RuntimeBinder;

namespace Engine
{
    //Defines what an Actor does; can be implemented in C# or Python
    public class Behavior
    {
        public World world; //The World associated with this Behavior
        public Actor actor { get; set; } //The Actor associated with this Behavior, if any
        public Tile tile; //The tile associated with this Behavior, if any
        
        public string scriptKey; //The handle key to the script, if any
        private ScriptScope scope; //The scope of the script, if any
        
        /**
        * Constructs a Behavior given the world, and an optional actor, tile, and script
        * Calls the construct method of the script, if present
        *
        * @param script the Handle of the script, if Python is desired
        */
        public Behavior(World world, Actor actor = null, Tile tile = null, Handle script = null)
        {
            this.world = world;
            this.actor = actor;
            this.tile = tile;

            if (script == null) return;

            scriptKey = script.key;

            // Run the code
            scope = script.getResource<Script>().createScope();
            if (scope == null) return;

            dynamic constructor = getVariable("construct");
            try
            {
                if (constructor != null) constructor(world, actor, tile);
            }
            catch (Exception e)
            {
                Trace.WriteLine(Script.getRuntimeError(e, actor.actorName));
                removeVariable("construct");
            }
        }

        //Virtual method that updates the Behavior. Calls the run method in the script if present
        public virtual void run()
        {
            if (scope == null) return;

            dynamic run = getVariable("run");
            try
            {
                if (run != null) run();
            }
            catch (Exception e)
            {
                if (actor != null) { Trace.WriteLine(Script.getRuntimeError(e, actor.actorName)); }
                else { Trace.WriteLine(Script.getRuntimeError(e, "unknown")); }
                
                removeVariable("run");
            }
        }

        //Signals the destruction of the script. Calls the destroy method in the script if present
        public virtual void destroy()
        {
            if (scope == null) return;

            dynamic destroy = getVariable("destroy");
            try
            {
                if (destroy != null) destroy();
            }
            catch (Exception e)
            {
                Trace.WriteLine(Script.getRuntimeError(e, actor.actorName));
                removeVariable("destroy");
            }
        }

        //Attempts to call a method in the script and pass it a param list of objects
        public void callMethod(string name, params object[] values)
        {
            if (scope == null || name == null) return;

            dynamic method = getVariable(name);
            try
            {
                if (method != null) method(values);
            }
            catch (Exception e)
            {
                Trace.WriteLine(Script.getRuntimeError(e, name));
                removeVariable(name);
            }
        }

        //Attempts to get a variable in the script
        public dynamic getVariable(string name)
        {
            if(scope == null || name == null) return null;

            if(!scope.ContainsVariable(name))
            {
#if ENGINEDEBUG
                Trace.WriteLine(actor.actorName + "'s behavior does not contain " + name);
#endif
                return null;
            }

            dynamic val;
            bool success = scope.TryGetVariable(name, out val);

            if (success == false)
            {
                Trace.WriteLine("Failed to retrieve " + name + " from + " + actor.actorName + "'s behavior");
            }

            return val;
        }

        //Attempts to set the variable in the script to the object value
        public void setVariable(string name, object value)
        {
            if (scope == null || name == null) return;

            if (!scope.ContainsVariable(name))
            {
                Trace.WriteLine(actor.actorName + "'s behavior does not contain " + name);
                return;
            }

            scope.SetVariable(name, value);
        }

        /*
         * Attempts to remove a function or variable from the script. 
         * Used for removing corrupt functions.
         */
        internal void removeVariable(string name)
        {
            if (scope == null || name == null) return;
            scope.RemoveVariable(name);
        }
    }
}