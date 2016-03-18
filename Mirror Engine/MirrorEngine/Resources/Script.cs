using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System.Diagnostics;

namespace Engine
{
    public class Script : ILoadable
    {

        private static ResourceComponent rc;
        public CompiledCode script { private set; get; }
        public string scriptKey;

        public Script()
        {
        }

        public Script(String path)
        {
        }

        public ScriptScope createScope()
        {
            ScriptScope scope = rc.createScope(scriptKey);

            try
            {
                script.Execute(scope);
            }
            catch (Exception e)
            {
                string error = e.Message;
                error = error.Substring(0, error.IndexOf("File \"") + 6) + error.Substring(error.IndexOf(ResourceComponent.DEFAULTROOTDIRECTORY) + ResourceComponent.DEFAULTROOTDIRECTORY.Length + 1);
                Trace.WriteLine(error);
                scope = null;
            }

            return scope;
        }

        public void load(ResourceComponent resourceComponent, string path)
        {
            rc = resourceComponent;
            this.scriptKey = ResourceComponent.getKeyFromPath(path);

            ScriptEngine engine = rc.scriptEngine;

            try
            {
                script = engine.CreateScriptSourceFromFile(path).Compile();
            }
            catch (SyntaxErrorException e)
            {
                ExceptionOperations eo;
                eo = engine.GetService<ExceptionOperations>();
                string error = eo.FormatException(e);

                throw new Exception("\nPython syntax error:\n" + error);
            }
        }

        public void unload()
        {
        }

        public static string getRuntimeError(Exception e, string scriptKey)
        {
            ExceptionOperations eo;
            eo = rc.scriptEngine.GetService<ExceptionOperations>();
            string error = eo.FormatException(e);

            if (e is Microsoft.Scripting.ArgumentTypeException)
            {
                error = "  File \"Scripts\\" + scriptKey + ".py\"\n" + error;
            }

            error = "\nPython runtime error:\n" + error + "\n";

            error = error.Substring(0, error.IndexOf("Traceback")) +
                    error.Substring(error.IndexOf("last):") + 8);

            if (!(e is Microsoft.Scripting.ArgumentTypeException))
            {
                error = error.Substring(0, error.IndexOf("File \"") + 6) +
                        error.Substring(error.IndexOf(ResourceComponent.DEFAULTROOTDIRECTORY) +
                                                      ResourceComponent.DEFAULTROOTDIRECTORY.Length + 1);
            }

            return error;
        }
    }
}
