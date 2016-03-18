using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Engine
{
    public class SelectionTool : Tool
    {
        private Vector2 origin;
        public RectangleF selection;

        public Vector2 copiedSize;
        public Tile[,] copiedTiles;
        public Mapfile.TileData[,] copiedTileData;

        public enum Mode
        {
            select,
            paste,
        }
        public Mode mode = Mode.select;

        GUIRadioControl modeControl;
        GUIButton copyButton;
        GUIButton clearButton;
        GUIButton pasteButton;

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        public SelectionTool(EditorComponent editor)
            : base(editor)
        {
            ResourceComponent rc = editor.engine.resourceComponent;
            toolButton.unpressedImg = rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/selection.png")));
            toolButton.pressedImg = rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/selectioninv.png")));
            editor.editorGui.addToolButton(toolButton, true);

            createDialog();

            copyButton.mouseClickEvent += copyAction;
            pasteButton.mouseClickEvent += pasteAction;
            clearButton.mouseClickEvent += clearAction;

            selection = new RectangleF(-1, -1, 0, 0);
        }

        void createDialog()
        {
            toolDialog = new GUIControl(editor.editorGui);

            ResourceComponent rc = editor.engine.resourceComponent;

            //Right menu
            GUILabel background = new GUILabel(editor.editorGui, editor.editorGui.leftBG, text: "Selection Tool");
            background.size = new Vector2(EditorGUI.RIGHTBOUNDARY, editor.engine.graphicsComponent.height - EditorGUI.BOTTOMBOUNDARY);
            toolDialog.pos = new Vector2(editor.engine.graphicsComponent.width - EditorGUI.RIGHTBOUNDARY, 0);
            toolDialog.add(background);

            GUILabel modeLabel = new GUILabel(editor.editorGui, text: "Mode:");
            modeLabel.pos = new Vector2(0, 25);
            toolDialog.add(modeLabel);

            modeControl = new GUIRadioControl(editor.editorGui);
            modeControl.addRadioButton((isdown) => { if (isdown) mode = Mode.select; }, "Select");
            modeControl.addRadioButton((isdown) => 
            {
                if (isdown)
                {
                    //Update mode
                    if (copiedTiles == null) 
                    { 
                        modeControl.pressed = (int)mode; 
                    }
                    else
                    {
                        mode = Mode.paste;

                        //Update selection for copying
                        isDragging = true;
                        Vector2 pos = editor.engine.graphicsComponent.camera.world2Screen(selection.topLeft * Tile.size);
                        moveAction(pos);
                        isDragging = false;
                    }
                }
            }, "Paste");
            modeControl.pos = new Vector2(0, 45);
            toolDialog.add(modeControl);

            clearButton = new GUIButton(editor.editorGui, rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/019_buttBack.png"))), rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/018_buttBack.png"))), "Erase");
            clearButton.pos = new Vector2(0, 105);
            toolDialog.add(clearButton);

            GUILabel selectLabel = new GUILabel(editor.editorGui, text: "Select Mode:");
            selectLabel.pos = new Vector2(0, 145);
            toolDialog.add(selectLabel);

            copyButton = new GUIButton(editor.editorGui, rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/019_buttBack.png"))), rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/018_buttBack.png"))), "Copy");
            copyButton.pos = new Vector2(0, 165);
            toolDialog.add(copyButton);

            GUILabel pasteLabel = new GUILabel(editor.editorGui, text: "Paste Mode:");
            pasteLabel.pos = new Vector2(0, 205);
            toolDialog.add(pasteLabel);

            pasteButton = new GUIButton(editor.editorGui, rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/019_buttBack.png"))), rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/018_buttBack.png"))), "Paste");
            pasteButton.pos = new Vector2(0, 225);
            toolDialog.add(pasteButton);
        }

        public override void activate()
        {
            base.activate();
            modeControl.items[(int)Mode.select].handleMouseUp(modeControl.items[(int)Mode.select].pos, MouseKeyBinding.MouseButton.LEFT);
        }

        public void copyAction(Vector2 pos, MouseKeyBinding.MouseButton button)
        {
            if (mode == Mode.paste) return;
            if (selection.topLeft == new Vector2(-1, -1)) return;

            if (selection.size == Vector2.Zero)
            {
                editor.currentValues = editor.engine.world.file.worldTileData[0, (int)selection.left, (int)selection.top];
                editor.currentValues.leftSlope = 1f;
                editor.currentValues.rightSlope = 1f;
                editor.currentValues.normal = 1;
                editor.currentTile = editor.currentValues;
                editor.pencilTool.updateStatus();
                return;
            }

            copiedSize = selection.size;
            copiedTileData = new Mapfile.TileData[(int)selection.width+1, (int)selection.height+1];
            copiedTiles = new Tile[(int)selection.width + 1, (int)selection.height + 1];
            Mapfile.TileData defaultTileData = new Mapfile.TileData("");

            for (int x = 0; x <= (int) selection.width; x++)
            {
                for (int y = 0; y <= (int) selection.height; y++)
                {
                    copiedTileData[x, y] = editor.engine.world.file.worldTileData[0, (int)selection.left + x, (int) selection.top + y];
                    if (copiedTileData[x, y].Equals(defaultTileData))
                    {
                        copiedTileData[x, y].setToIgnore();
                        copiedTiles[x, y] = new Tile(editor.engine.world, 0, 0, defaultTileData);
                    }
                    else
                    {
                        copiedTiles[x, y] = new Tile(editor.engine.world, (int)selection.left + x, (int)selection.top + y, copiedTileData[x, y]);
                    }
                }
            }

            mode = Mode.paste;
            modeControl.pressed = (int)mode;
        }

        public void pasteAction(Vector2 pos, MouseKeyBinding.MouseButton button)
        {
            if (mode != Mode.paste) return;

            for (int x = 0; x <= (int)selection.width; x++)
            {
                for (int y = 0; y <= (int)selection.height; y++)
                {
                    editor.engine.world.file.worldTileData[0, (int) selection.left + x, (int) selection.top + y].overWriteData(copiedTileData[x, y]);
                    editor.engine.world.tileArray[(int)selection.left + x, (int)selection.top + y].overWriteFromTileData(copiedTileData[x, y]);
                }
            }
        }

        public void clearAction(Vector2 pos, MouseKeyBinding.MouseButton button)
        {
            for (int x = (int)selection.left; x <= (int)selection.right; x++)
            {
                for (int y = (int)selection.top; y <= (int)selection.bottom; y++)
                {
                    editor.engine.world.file.worldTileData[0, x, y] = new Mapfile.TileData("");
                    editor.engine.world.tileArray[x, y] = new Tile(editor.engine.world, x, y, new Mapfile.TileData(""));
                }
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
            //set selection
            World world = editor.engine.world;
            Vector2 screenPos = editor.engine.inputComponent.getMousePosition();
            Vector2 worldPos = editor.engine.graphicsComponent.camera.screen2World(screenPos);
            
            int xIndex = (int) (worldPos.x / Tile.size);
            int yIndex = (int) (worldPos.y / Tile.size);
            
            if(xIndex < 0) xIndex = 0;
            if(yIndex < 0) yIndex = 0;
            if(xIndex > world.width -1) xIndex = world.width - 1;
            if (yIndex > world.height - 1) yIndex = world.height - 1;

            origin = new Vector2(xIndex, yIndex);
            Vector2 secondary = origin;

            if (mode == Mode.paste)
            {
                xIndex = (int)(origin.x + copiedSize.x);
                yIndex = (int)(origin.y + copiedSize.y);

                if (xIndex < 0) xIndex = 0;
                if (yIndex < 0) yIndex = 0;
                if (xIndex > world.width - 1) xIndex = world.width - 1;
                if (yIndex > world.height - 1) yIndex = world.height - 1;

                secondary = new Vector2(xIndex, yIndex);
            }


            selection = new RectangleF(origin, secondary);
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
                //update selection
                World world = editor.engine.world;
                //Vector2 screenPos = new Vector2(editor.engine.inputComponent.mouseX, editor.engine.inputComponent.mouseY);
                Vector2 worldPos = editor.engine.graphicsComponent.camera.screen2World(position);

                int xIndex = (int)(worldPos.x / Tile.size);
                int yIndex = (int)(worldPos.y / Tile.size);

                if (xIndex < 0) xIndex = 0;
                if (yIndex < 0) yIndex = 0;
                if (xIndex > world.width - 1) xIndex = world.width - 1;
                if (yIndex > world.height - 1) yIndex = world.height - 1;

                Vector2 secondary = new Vector2(xIndex, yIndex);
                if (mode == Mode.paste)
                {
                    origin = secondary;
                    xIndex = (int)(origin.x + copiedSize.x);
                    yIndex = (int)(origin.y + copiedSize.y);

                    if (xIndex < 0) xIndex = 0;
                    if (yIndex < 0) yIndex = 0;
                    if (xIndex > world.width - 1) xIndex = world.width - 1;
                    if (yIndex > world.height - 1) yIndex = world.height - 1;

                    secondary = new Vector2(xIndex, yIndex);
                }

                selection = new RectangleF(origin, secondary);
                selection.normalize();
            }
        }

        public void drawTiles()
        {
            if (mode != Mode.paste) return;

            GraphicsComponent gc = editor.engine.graphicsComponent;
            RectangleF viewRect = gc.camera.viewRect;

            Mapfile.TileData ignoreTile = new Mapfile.TileData("");
            ignoreTile.setToIgnore();
            
            for (int x = 0; x <= (int)selection.width; x++)
            {
                for (int y = 0; y <= (int)selection.height; y++)
                {
                    if (ignoreTile.Equals(copiedTileData[x,y])) continue; //Not commutative
                    Tile t = new Tile(editor.engine.world, (int)selection.left + x, (int)selection.top + y, copiedTiles[x, y].tileData);

                    Vector2 pos = gc.camera.world2Screen(new Vector2(t.x, t.y));
                    Vector2 specPos = gc.camera.world2Screen(new Vector2(t.x + Tile.size / 2, t.y + Tile.size / 2));
                    int flagSize = (int) (editor.editorGui.solid.getResource<Texture2D>().width * gc.camera.scale);

                    //texture
                    if (t.texture.key == "") gc.drawText("nul", (int)pos.x, (int)pos.y, editor.editorGui.font, Color.WHITE, (int)(12 * gc.camera.scale));
                    else gc.drawTex(editor.engine.resourceComponent.get(copiedTileData[x, y].texture), (int)pos.x, (int)pos.y, (int)(t.imageWidth * gc.camera.scale), (int)(t.imageHeight * gc.camera.scale), new Color(1, 1, 1, .4f));

                    //Nonstandard overlay
                    if (t.tileData.isNonstandard())
                    {
                        editor.editorGui.graphics.drawRect((int)(pos.x + (2*gc.camera.scale)), (int)(pos.y + (2*gc.camera.scale)), (int)((Tile.size - 4)*gc.camera.scale), (int)((Tile.size - 4)*gc.camera.scale), new Color(1, 0, 1, .3f));
                    }

                    //solidity
                    if (t.solidity) gc.drawTex(editor.editorGui.solid, (int)pos.x, (int)pos.y, flagSize, flagSize, new Color(1, 1, 1, .8f));
                    else            gc.drawTex(editor.editorGui.solidX, (int)pos.x, (int)pos.y, flagSize, flagSize, new Color(1, 1, 1, .8f));
                    
                    //opacity
                    if (t.opacityFlip) gc.drawTex(editor.editorGui.opaque, (int)(specPos.x), (int)(specPos.y), flagSize, flagSize, new Color(1,1,1,.8f));
                    else               gc.drawTex(editor.editorGui.opacityX,(int)(specPos.x), (int)(specPos.y), flagSize, flagSize, new Color(1,1,1,.8f));
                }
            }
        }

        public void drawSelection()
        {
            if (selection.topLeft == new Vector2(-1,-1)) return;

            Camera camera = editor.engine.graphicsComponent.camera;
            editor.engine.graphicsComponent.drawBorder(new Color(0, 0, 1), vtx: new Vector2[] 
                    {
                        camera.world2Screen(selection.topLeft * Tile.size),
                        camera.world2Screen((selection.topRight + new Vector2(1,0)) * Tile.size),
                        camera.world2Screen((selection.bottomRight + new Vector2(1,1)) * Tile.size),
                        camera.world2Screen((selection.bottomLeft + new Vector2(0,1)) * Tile.size),
                        camera.world2Screen(selection.topLeft * Tile.size),
                    });

            if(mode != Mode.paste && !(editor.fillTool.active && !editor.fillTool.selectionBox.isDown))
                editor.engine.graphicsComponent.drawRect(camera.world2Screen(selection.topLeft * Tile.size), camera.world2Screen((selection.bottomRight + new Vector2(1, 1)) * Tile.size), new Color(0, 0, 1, .2f));
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
        }

        /// brief classdeschere
        /**
        * classdeschere
        */
        public class SelectionToolAction : ToolAction
        {
            /**
            * desc here
            *
            * @param paramsdeschere
            *
            * @return returndeschere
            */
            public SelectionToolAction(LinkedList<TextureData> changed, byte[] texture, Tool parent)
                : base(parent)
            {
            }
        }
    }
}

