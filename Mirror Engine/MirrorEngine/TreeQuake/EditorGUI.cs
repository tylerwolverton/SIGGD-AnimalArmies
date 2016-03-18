using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace Engine
{
    public class EditorGUI : GUI
    {

        public EditorComponent editor;

        //gui
        private GUILabel toolMenu;
        private GUILabel statusMenu;
        private GUILabel tileLabel;
        private GUILabel actorLabel;
        public Handle font;           
        public Handle grid;
        public Handle solid;
        public Handle solidX;
        public Handle opaque;
        
        public Handle opacityX;

        public GUIControl toolButtonsDialog;
        public const int LEFTBOUNDARY = 16;
        public const int RIGHTBOUNDARY = 100;
        public const int TOPBOUNDARY = 100;
        public const int BOTTOMBOUNDARY = 80;

        public Handle leftBG;
        public Handle rightBG;

        //Button positioning
        int topPosition;
        int bottomPosition;
        const int BUTTONHEIGHT = 16;

        public EditorGUI(GraphicsComponent graphics)
            : base(graphics)
        {
            editor = graphics.engine.editorComponent;
            topPosition = 0;
            bottomPosition = graphics.windowHeight - BUTTONHEIGHT;

            toolButtonsDialog = new GUIControl(editor.editorGui);

            font = editor.engine.resourceComponent.get(defaultFontPath);
            grid = editor.engine.resourceComponent.get(Path.GetFullPath(Path.Combine(rootDirectory, "GUI\\000_EngineGUI\\013_grid.png")));
            solid = editor.engine.resourceComponent.get(Path.GetFullPath(Path.Combine(rootDirectory, "GUI\\000_EngineGUI\\000_solid.png")));
            opaque = editor.engine.resourceComponent.get(Path.GetFullPath(Path.Combine(rootDirectory, "GUI\\000_EngineGUI\\037_opaque.png")));
            solidX = editor.engine.resourceComponent.get(Path.GetFullPath(Path.Combine(this.rootDirectory, "GUI/000_EngineGUI/solidX.png")));
            opacityX = editor.engine.resourceComponent.get(Path.GetFullPath(Path.Combine(this.rootDirectory, "GUI/000_EngineGUI/opacityX.png")));
        }

        public override void initialize()
        {
            base.initialize();

            leftBG = editor.engine.resourceComponent.get(Path.GetFullPath(Path.Combine(rootDirectory, "GUI/000_EngineGUI/011_uiLeft.png")));
            rightBG = editor.engine.resourceComponent.get(Path.GetFullPath(Path.Combine(rootDirectory, "GUI/000_EngineGUI/012_uiRight.png")));

            //Tool menu
            toolMenu = new GUILabel(this, leftBG);
            toolMenu.size = new Vector2(16, editor.engine.graphicsComponent.height);
            toolMenu.pos = new Vector2(0, 0);
            add(toolMenu);

            //Status menu
            statusMenu = new GUILabel(this, leftBG, "Treequake");
            statusMenu.size = new Vector2(RIGHTBOUNDARY, BOTTOMBOUNDARY);
            statusMenu.pos = new Vector2(graphics.width - RIGHTBOUNDARY, graphics.height  - BOTTOMBOUNDARY);
            add(statusMenu);

            //Tile label
            tileLabel = new GUILabel(this, text: "Tile:");
            tileLabel.pos = statusMenu.pos + new Vector2(0, 20);
            add(tileLabel);

            //Actor label
            actorLabel = new GUILabel(this, text: "Actor:");
            actorLabel.pos = tileLabel.pos + new Vector2(50, 0);
            add(actorLabel);
        }

        public virtual void addToolButton(GUIButton button, bool atTop)
        {
            if (button != null)
            {
                button.pos = new Vector2(0, atTop ? topPosition : bottomPosition);
                toolButtonsDialog.add(button);
                add(button);
            }

            if (atTop) topPosition += BUTTONHEIGHT;
            else bottomPosition -= BUTTONHEIGHT;
        }

        public void resetAllBut(GUIItem exceptionalItem)
        {
            
            foreach (GUIItem item in toolButtonsDialog.items)
            {
                if (item is GUIButton && (item as GUIButton) != exceptionalItem)
                {
                    (item as GUIButton).resetImg();
                }
            }
        }

        public override void draw()
        {
            base.draw();

            //Draw currentTile 
            //texture
            int tX = (int)tileLabel.pos.x;
            int tY = (int)tileLabel.pos.y + 15;
            if (editor.currentTile.texture != Mapfile.TileData.IGNORESTRING)
            {
                if (editor.currentTile.texture == "") graphics.drawText("nul", tX, tY, font, Color.WHITE,12*2);
                else graphics.drawTex(editor.engine.resourceComponent.get(editor.currentTile.texture), tX, tY, Tile.size * 2, Tile.size * 2, Color.WHITE);
            }

            //Nonstandard overlay
            if (editor.currentTile.isNonstandard())
            {
                graphics.drawRect(tX+2, tY+2, Tile.size*2 - 4, Tile.size*2 - 4, new Color(1, 0, 1, .3f));
            }

            //Solidity
            if (editor.currentTile.solidity != Mapfile.TileData.IGNOREBYTE)
            {
                if (editor.currentTile.solidity == 1) graphics.drawTex(solid, tX, tY, 8 * 2, 8 * 2, Color.WHITE);
                else graphics.drawTex(solidX, tX, tY, 8 * 2, 8 * 2, Color.WHITE);
            }

            //OpacityFlip
            if (editor.currentTile.opacityFlip != Mapfile.TileData.IGNOREBYTE)
            {
                if (editor.currentTile.opacityFlip == 1) graphics.drawTex(opaque, tX + (Tile.size*2)/2, tY + (Tile.size*2)/2, 8 * 2, 8 * 2, Color.WHITE);
                else graphics.drawTex(opacityX, tX + (Tile.size*2)/2, tY + (Tile.size*2)/2, 8 * 2, 8 * 2, Color.WHITE);
            }

            //Draw currentActor
            if(editor.actorTool.thumbs.items.Count > 0)
                graphics.drawTex(editor.actorTool.thumbs.items[editor.actorTool.currentActorIndex].texture, (int)actorLabel.pos.x, (int)actorLabel.pos.y + 15, Tile.size * 2, Tile.size * 2, Color.WHITE);
        }
    }
}
