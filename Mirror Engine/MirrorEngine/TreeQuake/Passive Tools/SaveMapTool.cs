using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Engine
{
    public class SaveMapTool : Tool
    {

        SaveFileDialog saveDlg;
        String saveFile;

        public SaveMapTool(EditorComponent editor) : base(editor)
        {
            ResourceComponent rc = editor.engine.resourceComponent;
            toolButton.unpressedImg = rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/016_save (1).png")));
            toolButton.pressedImg = rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/017_save (2).png")));
            editor.editorGui.addToolButton(toolButton, false);

            saveDlg = new SaveFileDialog();
            saveDlg.InitialDirectory = Path.GetFullPath(Path.Combine(ResourceComponent.DEVELOPROOTPREFIX + ResourceComponent.DEFAULTROOTDIRECTORY, "Maps"));
            saveDlg.Filter = "Map files (*.map)|*.map";
            
            saveFile = "";
        }

        public override void activate()
        {
            toolButton.resetImg();

            saveDlg.FileName = Path.GetFileNameWithoutExtension(editor.engine.world.file.filePath);
            DialogResult res;
            try
            {
                res = saveDlg.ShowDialog();

                if (res == DialogResult.OK)
                {
                    saveFile = Path.GetFileName(saveDlg.FileName);
                }
            }
            catch (Exception e) { }

            if (!saveFile.Equals(""))
            {
                editor.engine.world.file.worldName = Path.Combine("Maps", saveFile);
                editor.engine.world.file.filePath = Path.GetFullPath(Path.Combine(ResourceComponent.DEVELOPROOTPREFIX + ResourceComponent.DEFAULTROOTDIRECTORY, "Maps", saveFile));
                editor.engine.world.file.save();
            }
        }
    }
}
