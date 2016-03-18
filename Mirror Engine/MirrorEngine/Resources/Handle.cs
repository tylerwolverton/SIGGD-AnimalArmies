/**
 * @file Handle.cs
 * @author SIGGD, Purdue ACM <siggd.purdue@gmail.com>
 * @version 2.0
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Engine
{
    //Loads and contains a resource
    public class Handle
    {
        private ResourceComponent rc;
        private string fullPath;       //The full path to the resource (eg: C:/Downloads/Game/GameContent/font/font.ttf)
        private string _key;           //Internal storage for the resource key
        public string key              //The key for this resource. A relative path from the content folder (eg: fonts/font.ttf)
        {
            get
            {
                if (_key == null) return "";
                else return _key;
            }
            private set
            {
                _key = value;
            }
        }

        internal ILoadable resource;   //Resource that this handle represents
        
        /* Handle constructor
         * 
         * Initialize the handle. Sets up the internal data, but does not actually perform loading from disk.
         * 
         * @param rc The engine resource component object
         * @param s The relative path to the resource (see key above)
         * 
         */ 
        public Handle(ResourceComponent rc, string s)
        {
            this.rc = rc;
            //In order to load the resource from disk, we need the full path to it.
            this.fullPath = ResourceComponent.getFullPathFromKey(s);
            if (s.Length < 1)
            {
                key = "";
                return;
            }
            //Makes sure the key is well-formed to be a handle-key
            this.key = ResourceComponent.getKeyFromPath(s);
        }

        //Get one generic resource in handle that implements ILoadable and load it in handle
        public T getResource<T>() where T : class, ILoadable, new()
        {
            if (key == "") return null;

            //If the resource hasn't been loaded yet, load it
            if (resource == null)
            {
                resource = new T();
                resource.load(rc, fullPath);
                rc.addResource(this);
            }

            //Tell the resource component that we used this handle so that it doesn't get prematurely bumped from the cache
            rc.updateLRU(this);
            return (T)resource;
        }

        //Compares class T to the class of the resource this handle represents
        public bool isType<T>() where T : class, ILoadable, new()
        {
            if (resource == null) return false;
            return resource is T;
        }

        //destroy the handle
        public void destroy()
        {
            if (resource != null)
            {
                resource.unload();
                resource = null;
            }
        }

        //Returns the key for the handle
        public override string ToString()
        {
            if (key == null) return "";
            return key;
        }

        /* Compares object obj to this Handle for equality. If this obj is of class Handle and obj's resource equals this Handle's, then they are equal
         * 
         * @param obj The object to compare this Handle o
         * 
         * @return True if equal - False otherwise
         */
        public override bool Equals(object obj)
        {
            if (obj is Handle)
            {
                Handle tempObj = (Handle)obj;
                if (tempObj.resource == this.resource) return true;
            }

            return false;
        }
    }
}
