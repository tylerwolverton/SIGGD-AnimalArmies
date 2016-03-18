using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public interface ILoadable
    {
        //load resource component from given path "s"
        void load(ResourceComponent rc, string s = "");

        //unload resource component
        void unload();
    }
}
