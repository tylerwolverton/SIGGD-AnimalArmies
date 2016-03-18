using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Engine
{
    public class LoadMapTool : Tool
    {

        OpenFileDialog openDlg;
        String openFile;

        public LoadMapTool(EditorComponent editor) : base(editor) 
        {
            ResourceComponent rc = editor.engine.resourceComponent;
            toolButton.unpressedImg = rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/035_loadMap.png")));
            toolButton.pressedImg = rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/036_loadMap.png")));
            editor.editorGui.addToolButton(toolButton, false);

            openDlg = new OpenFileDialog();
            openDlg.InitialDirectory = Path.GetFullPath(Path.Combine(ResourceComponent.DEVELOPROOTPREFIX + ResourceComponent.DEFAULTROOTDIRECTORY, "Maps"));
            openDlg.Filter = "Mapfile (*.map)|*.map";
            
            openFile = "";
        }

        public override void activate()
        {
            toolButton.resetImg();

            openFile = "";
            DialogResult res;
            try
            {
                res = openDlg.ShowDialog();

                if (res == DialogResult.OK)
                {
                    openFile = Path.GetFileName(openDlg.FileName);
                }
            }
            catch (Exception e) { }

            if (!openFile.Equals(""))
            {
                string worldName = Path.Combine("Maps", Path.GetFileName(openFile));

                if (!editor.engine.resourceComponent.worlds.ContainsKey(worldName))
                {
                    worldName = Path.GetFullPath(Path.Combine(ResourceComponent.DEVELOPROOTPREFIX, ResourceComponent.DEFAULTROOTDIRECTORY, worldName));
                }
                
                editor.engine.setWorld(worldName);
            }
        }
    }
}
