using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Engine
{
    public class ResetTool : Tool
    {
        public ResetTool(EditorComponent editor)
            : base(editor)
        {
            ResourceComponent rc = editor.engine.resourceComponent;
            toolButton.unpressedImg = rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/014_prince.png")));
            toolButton.pressedImg = rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/015_prince.png")));
            editor.editorGui.addToolButton(toolButton, false);
        }

        public override void activate()
        {
            if (editor.isActive)
            {
                editor.engine.resourceComponent.salvageScripts();
                editor.engine.resourceComponent.flush<Script>();
                editor.engine.resetWorld();
                editor.engine.resourceComponent.recoverScripts();
                editor.swap();
            }
        }
    }
}
