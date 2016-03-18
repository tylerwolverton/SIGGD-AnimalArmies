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
using System.Linq;
using Engine;
using System.IO;

namespace Engine
{
    /// brief classdeschere
    /**
    * classdeschere
    */
    public class FillTool : Tool
    {
        Mapfile.TileData fillCriteria;

        GUICheckBox textureCritBox;
        GUICheckBox solidityCritBox;
        GUICheckBox opacityCritBox;
        public GUICheckBox selectionBox;

        GUIRadioControl opControl;
        public enum Operator
        {
            AND,
            OR,
        }

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        public FillTool(EditorComponent editor)
            : base(editor)
        {
            ResourceComponent rc = editor.engine.resourceComponent;
            toolButton.unpressedImg = rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/005_fill.png")));
            toolButton.pressedImg = rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/006_fill_inv.png")));
            editor.editorGui.addToolButton(toolButton, true);

            createDialog();
            
            isDragging = false;
            fillCriteria.setToIgnore();
            fillCriteria.texture = "";

            textureCritBox.toggleEvent += (isDown) => 
            {
                if (isDown) fillCriteria.texture = "";
                else fillCriteria.texture = Mapfile.TileData.IGNORESTRING;
            };

            solidityCritBox.toggleEvent += (isDown) =>
            {
                if (isDown) fillCriteria.solidity = 0;
                else fillCriteria.solidity = Mapfile.TileData.IGNOREBYTE;
            };

            opacityCritBox.toggleEvent += (isDown) =>
            {
                if (isDown) fillCriteria.opacityFlip = 0;
                else fillCriteria.opacityFlip = Mapfile.TileData.IGNOREBYTE;
            };
        }

        void createDialog()
        {
            toolDialog = new GUIControl(editor.editorGui);

            //Right menu
            GUILabel background = new GUILabel(editor.editorGui, editor.editorGui.leftBG, text: "Fill Tool");
            background.size = new Vector2(EditorGUI.RIGHTBOUNDARY, editor.engine.graphicsComponent.height - EditorGUI.BOTTOMBOUNDARY);
            toolDialog.pos = new Vector2(editor.engine.graphicsComponent.width - EditorGUI.RIGHTBOUNDARY, 0);
            toolDialog.add(background);

            GUILabel critLabel = new GUILabel(editor.editorGui, text: "Criteria:");
            critLabel.pos = new Vector2(0, 25);
            toolDialog.add(critLabel);

            textureCritBox = new GUICheckBox(editor.editorGui, true, "Same texture");
            textureCritBox.pos = new Vector2(0, 45);
            toolDialog.add(textureCritBox);

            solidityCritBox = new GUICheckBox(editor.editorGui, false, "Same solidity");
            solidityCritBox.pos = new Vector2(0, 65);
            toolDialog.add(solidityCritBox);

            opacityCritBox = new GUICheckBox(editor.editorGui, false, "Same opacityFlip");
            opacityCritBox.pos = new Vector2(0, 85);
            toolDialog.add(opacityCritBox);

            GUILabel opLabel = new GUILabel(editor.editorGui, text: "Operator:");
            opLabel.pos = new Vector2(0, 110);
            toolDialog.add(opLabel);

            opControl = new GUIRadioControl(editor.editorGui, (int)Operator.AND);
            opControl.addRadioButton(null, "AND");
            opControl.addRadioButton(null, "OR");
            opControl.pos = new Vector2(0, 130);
            toolDialog.add(opControl);

            GUILabel selectionLabel = new GUILabel(editor.editorGui, text: "Selection:");
            selectionLabel.pos = new Vector2(0, opControl.pos.y + opControl.size.y + 35);
            toolDialog.add(selectionLabel);

            selectionBox = new GUICheckBox(editor.editorGui, true, "Ignore");
            selectionBox.pos = new Vector2(0, selectionLabel.pos.y + 15);
            toolDialog.add(selectionBox);
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
            Vector2 screenPos = editor.engine.inputComponent.getMousePosition();
            Vector2 worldPos = editor.engine.graphicsComponent.camera.screen2World(screenPos);

            Tile victim = editor.engine.world.getTileAt(worldPos);
            if (victim != null)
            {
                FillToolAction action = fill(victim);
                if (action != null)
                {
                    toolAction.Push(action);
                    undos.Clear();
                }
            }

            isDragging = false;
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
            foreach (TextureData texdat in (action as FillToolAction).changed)
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
            //FillToolAction data = (FillToolAction)action;
            //fill(data.texture, data.startTile);
        }

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        public FillToolAction fill(Tile startTile)
        {
            Mapfile.TileData ignoreTile = new Mapfile.TileData();
            ignoreTile.setToIgnore();
            if (ignoreTile.Equals(fillCriteria)) return null;

            LinkedList<TextureData> changed = new LinkedList<TextureData>(); // for undo

            Mapfile.TileData std = editor.engine.world.file.worldTileData[0, startTile.xIndex, startTile.yIndex];
            Mapfile.TileData tempCrit = fillCriteria;
            bool inSelection = false;
            if(!selectionBox.isDown && editor.selectionTool.selection.contains(new Vector2(startTile.xIndex, startTile.yIndex))) inSelection = true;

            tempCrit.overWriteData(std);

            //update actions
            Queue<Tile> applyToMe = new Queue<Tile>();
            List<Tile> appliedToMe = new List<Tile>();

            applyToMe.Enqueue(startTile);
            changed.AddLast(new TextureData(startTile.xIndex, startTile.yIndex));

            //Update start tile and its tiledata
            editor.engine.world.file.worldTileData[0, startTile.xIndex, startTile.yIndex].overWriteData(editor.currentTile);
            startTile.overWriteFromTileData(editor.currentTile);

            while (applyToMe.Count() > 0)
            {
                Tile mid = applyToMe.Dequeue();
                foreach (Tile tile in mid.adjacent)
                {
                    if (tile != null && !appliedToMe.Contains(tile))
                    {
                        Mapfile.TileData td = editor.engine.world.file.worldTileData[0, tile.xIndex, tile.yIndex];

                        bool apply = false;

                        //Constrain to opted criteria
                        if (opControl.pressed == (int)Operator.AND)
                        {
                            apply = td.Equals(tempCrit);
                        }
                        else if(opControl.pressed == (int) Operator.OR)
                        {
                            if (fillCriteria.texture != Mapfile.TileData.IGNORESTRING && td.texture == tempCrit.texture) apply = true;
                            if (fillCriteria.solidity != Mapfile.TileData.IGNOREBYTE && td.solidity == tempCrit.solidity) apply = true;
                            if (fillCriteria.opacityFlip != Mapfile.TileData.IGNOREBYTE && td.opacityFlip == tempCrit.opacityFlip) apply = true;
                        }

                        //Constrain to selection border if opted
                        if (!selectionBox.isDown)
                        {
                            if (inSelection ^ editor.selectionTool.selection.contains(new Vector2(tile.xIndex, tile.yIndex))) apply = false;
                        }

                        if (apply)
                        {
                            //Update actions
                            applyToMe.Enqueue(tile);
                            changed.AddLast(new TextureData(tile.xIndex, tile.yIndex));

                            //Update current tile and tiledata
                            editor.engine.world.file.worldTileData[0, tile.xIndex, tile.yIndex].overWriteData(editor.currentTile);
                            tile.overWriteFromTileData(editor.currentTile);
                        }
                    }
                    appliedToMe.Add(tile);
                }
            }
            return new FillToolAction(changed, "", startTile, this);
        }

        /// brief classdeschere
        /**
        * classdeschere
        */
        public class FillToolAction : ToolAction
        {

            public LinkedList<TextureData> changed; ///< original vals of changed tiles
            public string texture; ///< data necessary to recreate action once undone
            public Tile startTile; ///< duplicate info of first item on stack, but makes redo easier, mod redo if want to remove.

            /**
            * desc here
            *
            * @param paramsdeschere
            *
            * @return returndeschere
            */
            public FillToolAction(LinkedList<TextureData> changed, string texture, Tile startTile, Tool parent)
                : base(parent)
            {
                this.changed = changed;
                this.texture = texture;
                this.startTile = startTile;
            }
        }
    }
}
