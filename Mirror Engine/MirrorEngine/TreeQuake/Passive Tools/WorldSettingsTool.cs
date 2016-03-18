using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Engine
{
    public class WorldSettingsTool : Tool
    {
        OpenFileDialog openDlg;
        string openFile = "";

        GUIButton closeButton;
        GUIButton selectBehaviorButton;
        GUIButton removeBehaviorButton;

        GUITextBox widthBox;
        GUITextBox heightBox;
        GUIButton resizeButton;

        public WorldSettingsTool(EditorComponent editor)
            : base(editor)
        {
            ResourceComponent rc = editor.engine.resourceComponent;
            toolButton.unpressedImg = rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/021_cogs (2).png")));
            toolButton.pressedImg = rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/020_cogs (1).png")));
            editor.editorGui.addToolButton(toolButton, true);

            createDialog();

            closeButton.mouseClickEvent += closeAction;
            selectBehaviorButton.mouseClickEvent += selectBehaviorAction;
            removeBehaviorButton.mouseClickEvent += removeBehaviorAction;
            resizeButton.mouseClickEvent += (pos, button) => { resize(); };

            openDlg = new OpenFileDialog();
            openDlg.InitialDirectory = Path.GetFullPath(Path.Combine(ResourceComponent.DEVELOPROOTPREFIX + ResourceComponent.DEFAULTROOTDIRECTORY, "Scripts"));
            openDlg.Filter = "Script (*.py)|*.py";
        }

        void createDialog()
        {
            ResourceComponent rc = editor.engine.resourceComponent;
            toolDialog = new GUIControl(editor.editorGui);

            GUILabel background = new GUILabel(editor.editorGui, editor.editorGui.leftBG, "World options");
            background.size = new Vector2((editor.editorGui.graphics.width - EditorGUI.RIGHTBOUNDARY) - EditorGUI.LEFTBOUNDARY - 10, EditorGUI.TOPBOUNDARY);
            toolDialog.pos = new Vector2(toolButton.pos.x + 20, 0);
            toolDialog.add(background);

            selectBehaviorButton = new GUIButton(editor.editorGui, rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/019_buttBack.png"))), rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/018_buttBack.png"))), "Select Behavior");
            selectBehaviorButton.pos = new Vector2(5, 20);
            toolDialog.add(selectBehaviorButton);

            removeBehaviorButton = new GUIButton(editor.editorGui, rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/019_buttBack.png"))), rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/018_buttBack.png"))), "Remove Behavior");
            removeBehaviorButton.pos = new Vector2(105, 20);
            toolDialog.add(removeBehaviorButton);

            GUILabel widthLabel = new GUILabel(editor.editorGui, text: "Width");
            widthLabel.pos = new Vector2(4, 50);
            toolDialog.add(widthLabel);

            widthBox = new GUITextBox(editor.editorGui, "0");
            widthBox.pos = new Vector2(5, 70);
            widthBox.minWidth = 50;
            widthBox.onText('\b');
            toolDialog.add(widthBox);

            GUILabel heightLabel = new GUILabel(editor.editorGui, text: "Height");
            heightLabel.pos = new Vector2(60, 50);
            toolDialog.add(heightLabel);

            heightBox = new GUITextBox(editor.editorGui, "0");
            heightBox.pos = new Vector2(60, 70);
            heightBox.minWidth = 50;
            heightBox.onText('\b');
            toolDialog.add(heightBox);

            resizeButton = new GUIButton(editor.editorGui, rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/019_buttBack.png"))), rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/018_buttBack.png"))), "Resize");
            resizeButton.pos = new Vector2(115, 70);
            toolDialog.add(resizeButton);

            closeButton = new GUIButton(editor.editorGui, rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/000_solid.png"))), rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/000_solid.png"))));
            closeButton.pos = new Vector2(background.size.x - 12, 4);
            closeButton.size = new Vector2(8, 8);
            toolDialog.add(closeButton);
        }

        public override void activate()
        {
            if (editor.editorGui.items.Contains(toolDialog))
                editor.editorGui.remove(toolDialog);
            else
                editor.editorGui.refreshDialog(toolDialog);
        }

        public override void deactivate()
        {
            base.deactivate();
            editor.editorGui.remove(toolDialog);
        }

        void closeAction(Vector2 closePos, MouseKeyBinding.MouseButton mouseButton)
        {
            toolButton.resetImg();
            editor.editorGui.remove(toolDialog);
        }

        public void selectBehaviorAction(Vector2 pos, MouseKeyBinding.MouseButton button)
        {
            openFile = "";
            DialogResult res;
            try
            {
                res = openDlg.ShowDialog();

                if (res == DialogResult.OK)
                {
                    openFile = openDlg.FileName;
                }
            }
            catch (Exception e) { }

            if (!openFile.Equals(""))
            {
                string key = ResourceComponent.getKeyFromPath(openFile);
                editor.engine.world.setWorldBehavior(editor.engine.resourceComponent.get(key));
                editor.engine.world.file.worldBehaviorKey = key;
            }
        }

        public void removeBehaviorAction(Vector2 pos, MouseKeyBinding.MouseButton button)
        {
            editor.engine.world.file.worldBehaviorKey = "";
            editor.engine.world.myBehavior = null;
        }

        private void resize()
        {
            if (widthBox.text.Length == 0 || heightBox.text.Length == 0) return;

            //Get dimensions
            int width;
            int height;
            try
            {
                width = Convert.ToInt32(widthBox.text);
                height = Convert.ToInt32(heightBox.text);
            }
            catch (Exception e) { return; }

            World tempWorld = editor.engine.world;
            Mapfile tempFile = tempWorld.file;
            Mapfile.TileData emptyTileData = new Mapfile.TileData("");

            //Create new arrays
            Tile[,] newTileArray = new Tile[width, height];
            Mapfile.TileData[, ,] newTileData = new Mapfile.TileData[1, width, height];

            //Transfer tiles
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (x < tempWorld.width && y < tempWorld.height)
                    {
                        newTileArray[x, y] = tempWorld.tileArray[x, y];
                        newTileData[0, x, y] = tempFile.worldTileData[0, x, y];
                    }
                    else
                    {
                        newTileData[0, x, y] = emptyTileData;
                        newTileArray[x, y] = new Tile(tempWorld, x, y, emptyTileData);
                    }
                }
            }

            //Erase actors
            List<Actor> actorsToErase = new List<Actor>();
            foreach (Actor a in tempWorld.actors)
            {
                if (tempWorld.getTileAt(a.position.x, a.position.y) == null)
                {
                    actorsToErase.Add(a);
                }
            }
            foreach (Actor a in actorsToErase)
            {
                Mapfile.ActorData w = new Mapfile.ActorData();
                w.id = (byte)editor.engine.world.actorFactory.names[a.actorName];
                w.x = a.spawn.x;
                w.y = a.spawn.y;
                w.z = 0;
                editor.engine.world.file.worldActorData.Remove(w);
                editor.engine.world.actors.Remove(a);
            }

            //Overwrite
            tempFile.worldTileData = newTileData;
            tempWorld.tileArray = newTileArray;
            tempWorld.width = width;
            tempWorld.height = height;
        }
    }
}