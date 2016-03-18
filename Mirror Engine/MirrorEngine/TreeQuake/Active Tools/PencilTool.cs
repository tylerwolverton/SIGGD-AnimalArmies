/**
 * @file
 * @author PURDUE ACM, SIGGD <siggd.purdue@gmail.com>
 * @version 1.0
 * 
 * @section LICENSE
 * 
 * @section DESCRIPTION
 */

using System;
using System.Collections.Generic;
using Engine;
using System.Windows.Forms;
using System.IO;

namespace Engine
{
    /// brief classdeschere
    public class PencilTool : Tool
    {
        GUICheckBox textureOverwriteBox;
        ScrollingImageTable thumbs;

        GUICheckBox solidityOverwriteBox;
        GUICheckBox solidityCheckBox;

        GUICheckBox opacityOverwriteBox;
        GUICheckBox opacityFlipCheckBox;

        GUICheckBox behaviorOverwriteBox;
        GUIButton behaviorSetButton;
        GUIButton behaviorRemoveButton;
        OpenFileDialog openDlg;

        GUIButton addButton;

        int numAdded = 0;

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        public PencilTool(EditorComponent editor)
            : base(editor)
        {
            ResourceComponent rc = editor.engine.resourceComponent;
            toolButton.unpressedImg = rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/003_pencil.png")));
            toolButton.pressedImg = rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/004_pencil_inv.png")));
            editor.editorGui.addToolButton(toolButton, true);

            openDlg = new OpenFileDialog();
            openDlg.InitialDirectory = Path.GetFullPath(Path.Combine(ResourceComponent.DEVELOPROOTPREFIX + ResourceComponent.DEFAULTROOTDIRECTORY, "Scripts"));
            openDlg.Filter = "Script (*.py)|*.py";

            createDialog();
            discoverThumbnails();

            textureOverwriteBox.toggleEvent += (isDown) =>
            {
                if (isDown) editor.currentTile.texture = editor.currentValues.texture;
                else editor.currentTile.texture = Mapfile.TileData.IGNORESTRING;
            };

            solidityOverwriteBox.toggleEvent += (isDown) =>
            {
                if (isDown) editor.currentTile.solidity = editor.currentValues.solidity;
                else editor.currentTile.solidity = Mapfile.TileData.IGNOREBYTE;
            };

            solidityCheckBox.toggleEvent += (isDown) => 
            {
                editor.currentValues.solidity = (byte)(isDown ? 1 : 0);
                if (!solidityOverwriteBox.isDown) solidityOverwriteBox.toggle();
                editor.currentTile.solidity = editor.currentValues.solidity;
            };

            opacityOverwriteBox.toggleEvent += (isDown) =>
            {
                if (isDown) editor.currentTile.opacityFlip = editor.currentValues.opacityFlip;
                else editor.currentTile.opacityFlip = Mapfile.TileData.IGNOREBYTE;
            };

            opacityFlipCheckBox.toggleEvent += (isDown) =>
            {
                editor.currentValues.opacityFlip = (byte)(isDown ? 1 : 0);
                if (!opacityOverwriteBox.isDown) opacityOverwriteBox.toggle();
                editor.currentTile.opacityFlip = editor.currentValues.opacityFlip;
            };

            behaviorOverwriteBox.toggleEvent += (isDown) =>
            {
                if (isDown) editor.currentTile.behavior = editor.currentValues.behavior;
                else editor.currentTile.behavior = Mapfile.TileData.IGNORESTRING;
            };

            behaviorSetButton.mouseClickEvent += (pos, button) => { setTileBehavior(); };
            behaviorRemoveButton.mouseClickEvent += (pos, button) => 
            { 
                editor.currentValues.behavior = "";
                if (editor.currentTile.behavior != Mapfile.TileData.IGNORESTRING) editor.currentTile.behavior = "";
            };

            addButton.mouseClickEvent += (pos, button) => { this.addNewTile(); };
        }

        void createDialog()
        {
            ResourceComponent rc = editor.engine.resourceComponent;
            toolDialog = new GUIControl(editor.editorGui);

            //Right menu
            GUILabel background = new GUILabel(editor.editorGui, editor.editorGui.leftBG, text: "Pencil Tool");
            background.size = new Vector2(EditorGUI.RIGHTBOUNDARY, editor.engine.graphicsComponent.height - EditorGUI.BOTTOMBOUNDARY);
            toolDialog.pos = new Vector2(editor.engine.graphicsComponent.width - EditorGUI.RIGHTBOUNDARY, 0);
            toolDialog.add(background);

            GUILabel textureLabel = new GUILabel(editor.editorGui, text: "Texture");
            textureLabel.pos = new Vector2(0, 25);
            toolDialog.add(textureLabel);

            textureOverwriteBox = new GUICheckBox(editor.editorGui, editor.currentTile.texture != Mapfile.TileData.IGNORESTRING, "Overwrite");
            textureOverwriteBox.pos = new Vector2(0, 40);
            toolDialog.add(textureOverwriteBox);

            int numCols = EditorGUI.RIGHTBOUNDARY / Tile.size;
            int numRows = (editor.engine.graphicsComponent.height / Tile.size) / 3;
            thumbs = new ScrollingImageTable(editor.editorGui, numRows, numCols, Tile.size, Tile.size, ScrollingImageTable.ScrollDirection.VERTICAL, new Vector2(0, 55));
            thumbs.padding = 2;
            toolDialog.add(thumbs);

            GUILabel solidityLabel = new GUILabel(editor.editorGui, text: "Solidity");
            solidityLabel.pos = new Vector2(0, thumbs.size.y);
            toolDialog.add(solidityLabel);

            solidityOverwriteBox = new GUICheckBox(editor.editorGui, editor.currentTile.solidity != Mapfile.TileData.IGNOREBYTE, "Overwrite");
            solidityOverwriteBox.pos = new Vector2(0, thumbs.size.y + 15);
            toolDialog.add(solidityOverwriteBox);

            solidityCheckBox = new GUICheckBox(editor.editorGui, editor.currentValues.solidity == 1, "Is solid");
            solidityCheckBox.pos = new Vector2(0, thumbs.size.y + 30);
            toolDialog.add(solidityCheckBox);

            GUILabel opacityLabel = new GUILabel(editor.editorGui, text: "Opacity");
            opacityLabel.pos = new Vector2(0, thumbs.size.y + 50);
            toolDialog.add(opacityLabel);

            opacityOverwriteBox = new GUICheckBox(editor.editorGui, editor.currentTile.opacityFlip != Mapfile.TileData.IGNOREBYTE, "Overwrite");
            opacityOverwriteBox.pos = new Vector2(0, thumbs.size.y + 65);
            toolDialog.add(opacityOverwriteBox);

            opacityFlipCheckBox = new GUICheckBox(editor.editorGui, editor.currentValues.opacityFlip == 1, "Is inverted");
            opacityFlipCheckBox.pos = new Vector2(0, thumbs.size.y + 80);
            toolDialog.add(opacityFlipCheckBox);

            GUILabel behaviorLabel = new GUILabel(editor.editorGui, text: "Behavior");
            behaviorLabel.pos = new Vector2(0, thumbs.size.y + 100);
            toolDialog.add(behaviorLabel);

            behaviorOverwriteBox = new GUICheckBox(editor.editorGui, editor.currentTile.behavior != Mapfile.TileData.IGNORESTRING, "Overwrite");
            behaviorOverwriteBox.pos = new Vector2(0, thumbs.size.y + 115);
            toolDialog.add(behaviorOverwriteBox);

            behaviorSetButton = new GUIButton(editor.editorGui, rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/019_buttBack.png"))), rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/018_buttBack.png"))), "Set");
            behaviorSetButton.pos = new Vector2(0, thumbs.size.y + 135);
            toolDialog.add(behaviorSetButton);

            behaviorRemoveButton = new GUIButton(editor.editorGui, rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/019_buttBack.png"))), rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/018_buttBack.png"))), "Remove");
            behaviorRemoveButton.pos = new Vector2(25, thumbs.size.y + 135);
            toolDialog.add(behaviorRemoveButton);
            
            addButton = new GUIButton(editor.editorGui, rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/019_buttBack.png"))), rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/018_buttBack.png"))), "Add custom tile");
            addButton.pos = new Vector2(0, background.size.y - 25);
            toolDialog.add(addButton);
        }

        void discoverThumbnails()
        {
            String directory = "Tiles/Tiles";
            ResourceComponent rc = editor.engine.resourceComponent;
            List<Handle> tempSet = rc.discoverHandles(directory);

            //Null Tile
            GUIButton nullTile = new GUIButton(editor.editorGui, null, null, text: "nul");
            nullTile.size = new Vector2(Tile.size, Tile.size);
            nullTile.mouseDownEvent += (pos, button) =>
            {
                editor.currentValues.texture = "";
                if(!textureOverwriteBox.isDown) textureOverwriteBox.toggle();
                editor.currentTile.texture = editor.currentValues.texture;
            };
            thumbs.add(nullTile);

            //Dynamic tile discovery
            if (tempSet == null) return;
            for (int i = 0; i < tempSet.Count; i++)
            {
                GUIButton temp = new GUIButton(editor.editorGui, tempSet[i], tempSet[i], "");
                temp.size = new Vector2(Tile.size, Tile.size);
                string texturePath = tempSet[i].key;
                temp.mouseDownEvent += (pos, button) =>
                    {
                        editor.currentValues.texture = texturePath;
                        if (!textureOverwriteBox.isDown) textureOverwriteBox.toggle();
                        editor.currentTile.texture = editor.currentValues.texture;
                    };
                thumbs.add(temp);
            }
            thumbs.performLayout();
        }

        public void addNewTile()
        {
            //Return if unoriginal
            Mapfile.TileData ignoreButTex = new Mapfile.TileData("");
            ignoreButTex.setToIgnore();
            ignoreButTex.texture = editor.currentValues.texture;
            if (ignoreButTex.Equals(editor.currentTile)) return;

            //Get tile data
            Mapfile.TileData tempData = editor.currentTile;
            numAdded++;

            //Create button
            GUIButton tempButton = new GUIButton(editor.editorGui, (tempData.texture == Mapfile.TileData.IGNORESTRING) ? null : editor.engine.resourceComponent.get(tempData.texture), text: ((tempData.texture == "") ? "n" : "") + numAdded.ToString());
            tempButton.size = new Vector2(Tile.size, Tile.size);
            tempButton.mouseClickEvent += (mpos, mbutton) =>
            {
                editor.currentTile = tempData;
                editor.currentValues.overWriteData(editor.currentTile);
                updateStatus();
            };

            //Add button
            thumbs.add(tempButton);
            toolDialog.add(tempButton);
            editor.editorGui.add(tempButton); //should upwardly recursive add
            thumbs.performLayout();
        }

        public void updateStatus()
        {
            //Update values
            if (editor.currentValues.solidity == 1 ^ solidityCheckBox.isDown) solidityCheckBox.toggle();
            if (editor.currentValues.opacityFlip == 1 ^ opacityFlipCheckBox.isDown) opacityFlipCheckBox.toggle();

            //Update overwrites
            if (editor.currentTile.texture == Mapfile.TileData.IGNORESTRING ^ !textureOverwriteBox.isDown) textureOverwriteBox.toggle();
            if (editor.currentTile.solidity == Mapfile.TileData.IGNOREBYTE ^ !solidityOverwriteBox.isDown) solidityOverwriteBox.toggle();
            if (editor.currentTile.opacityFlip == Mapfile.TileData.IGNOREBYTE ^ !opacityOverwriteBox.isDown) opacityOverwriteBox.toggle();
        }

        private void setTileBehavior()
        {
            string openFile = "";
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
                editor.currentValues.behavior = key;
                editor.currentTile.behavior = key;
                if (!behaviorOverwriteBox.isDown) behaviorOverwriteBox.toggle();
            }
        }

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        public override void downAction(MouseKeyBinding.MouseButton button)
        {

            doPencil();
            isDragging = true;
        }

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        public override void moveAction(Vector2 position)
        {
            if (isDragging && editor.isActive)
            {
                doPencil();
            }
        }

        public void doPencil()
        {
            Vector2 screenPos = editor.engine.inputComponent.getMousePosition();
            Vector2 worldPos = editor.engine.graphicsComponent.camera.screen2World(screenPos);

            Tile victim = editor.engine.world.getTileAt(worldPos);
            if (victim != null)
            {
                Mapfile.TileData td = editor.engine.world.file.worldTileData[0, victim.xIndex, victim.yIndex];
                if (td.Equals(editor.currentTile)) return;

                //Update Tiles and MapFile data
                editor.engine.world.file.worldTileData[0, victim.xIndex, victim.yIndex].overWriteData(editor.currentTile);
                victim.overWriteFromTileData(editor.currentTile);

                //Update actions
                LinkedList<TextureData> changed = new LinkedList<TextureData>();
                changed.AddLast(new TextureData(victim.xIndex, victim.yIndex));
                undos.Clear();
                toolAction.Push(new PencilToolAction(changed, new byte[] { 0 }, this));
            }
        }

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        public override void undoAction(ToolAction action)
        {
            foreach (TextureData chg in (action as PencilToolAction).changed)
            {
            }
        }

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        public override void redoAction(ToolAction action)
        {
            foreach (TextureData chg in (action as PencilToolAction).changed)
            {
            }
        }

        /// brief classdeschere
        /**
        * classdeschere
        */
        public class PencilToolAction : ToolAction
        {

            public LinkedList<TextureData> changed; ///< original vals of changed tiles
            public byte[] texture; ///< data necessary to recreate action once undone

            /**
            * desc here
            *
            * @param paramsdeschere
            *
            * @return returndeschere
            */
            public PencilToolAction(LinkedList<TextureData> changed, byte[] texture, Tool parent)
                : base(parent)
            {
                this.changed = changed;
                this.texture = texture;
            }
        }
    }
}
