using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Engine
{
    public class NewMapTool : Tool
    {

        GUITextBox widthEntry;
        GUITextBox heightEntry;
        GUIButton confirmButton;
        GUIButton cancelButton;

        public NewMapTool(EditorComponent editor) : base(editor)
        {
            ResourceComponent rc = editor.engine.resourceComponent;
            toolButton.unpressedImg = rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/033_newMap.png")));
            toolButton.pressedImg = rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/034_newMap.png")));
            editor.editorGui.addToolButton(toolButton, false);

            createDialog();
            cancelButton.mouseClickEvent += cancelAction;
            confirmButton.mouseClickEvent += confirmAction;
        }

        void createDialog()
        {
            ResourceComponent rc = editor.engine.resourceComponent;
            toolDialog = new GUIControl(editor.editorGui);

            GUILabel background = new GUILabel(editor.editorGui, editor.editorGui.leftBG, "Create a new map");
            background.size = new Vector2(editor.editorGui.graphics.width * .40f, editor.editorGui.graphics.height * .20f);
            toolDialog.pos = new Vector2(toolButton.pos.x + 20, toolButton.pos.y - background.size.y + 12);
            toolDialog.add(background);

            GUILabel widthPrompt = new GUILabel(editor.editorGui, text: "Width:");
            widthPrompt.pos = new Vector2(5, 20);
            toolDialog.add(widthPrompt);

            widthEntry = new GUITextBox(editor.editorGui);
            widthEntry.minWidth = 40f;
            widthEntry.maxWidth = 40f;
            widthEntry.pos = new Vector2(5, 40);
            widthEntry.size = new Vector2(widthEntry.minWidth, widthEntry.size.y);
            toolDialog.add(widthEntry);

            GUILabel heightPrompt = new GUILabel(editor.editorGui, text: "Height:");
            heightPrompt.pos = new Vector2(widthEntry.maxWidth + 20, 20);
            toolDialog.add(heightPrompt);

            heightEntry = new GUITextBox(editor.editorGui);
            heightEntry.minWidth = 40f;
            heightEntry.maxWidth = 40f;
            heightEntry.size = new Vector2(heightEntry.minWidth, widthEntry.size.y);
            heightEntry.pos = new Vector2(widthEntry.maxWidth + 20, 40);
            toolDialog.add(heightEntry);

            confirmButton = new GUIButton(editor.editorGui, rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/019_buttBack.png"))), rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/018_buttBack.png"))), "OK");
            confirmButton.pos = new Vector2(5, 60);
            toolDialog.add(confirmButton);

            cancelButton = new GUIButton(editor.editorGui, rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/019_buttBack.png"))), rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/018_buttBack.png"))), "CANCEL");
            cancelButton.pos = new Vector2(30, 60);
            toolDialog.add(cancelButton);
        }

        public override void activate()
        {
            if (editor.editorGui.items.Contains(toolDialog))
                editor.editorGui.remove(toolDialog);
            else
                editor.editorGui.refreshDialog(toolDialog);

            if (editor.engine.world != null)
            {
                widthEntry.text = editor.engine.world.width.ToString();
                heightEntry.text = editor.engine.world.height.ToString();
            }

            if (widthEntry.text.Equals("0") || heightEntry.text.Equals("0"))
            {
                widthEntry.text = MirrorEngine.DEFAULTMAPWIDTH.ToString();
                heightEntry.text = MirrorEngine.DEFAULTMAPHEIGHT.ToString();
            }
        }

        public override void deactivate()
        {
            base.deactivate();
            editor.editorGui.remove(toolDialog);
        }

        void cancelAction(Vector2 cancelPos, MouseKeyBinding.MouseButton mouseButton)
        {
            toolButton.resetImg();
            editor.editorGui.remove(toolDialog);
        }

        //Loads a new map
        void confirmAction(Vector2 confirmPos, MouseKeyBinding.MouseButton mouseButton)
        {
            
            int width = 0;
            int height = 0;
            try
            {
                width = int.Parse(widthEntry.text);
                height = int.Parse(heightEntry.text);
            }
            catch (Exception) { Trace.WriteLine("Bad input."); return; }

            editor.engine.newWorld(width, height);

            toolButton.resetImg();
            editor.editorGui.remove(toolDialog);
        }
    }
}