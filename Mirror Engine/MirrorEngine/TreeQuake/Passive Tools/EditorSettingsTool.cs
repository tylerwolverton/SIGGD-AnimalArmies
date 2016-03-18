using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Engine
{
    public class EditorSettingsTool : Tool
    {
        GUIButton closeButton;

        public GUICheckBox pauseCheckBox;
        GUICheckBox gridCheckBox;
        GUICheckBox ambienceCheckBox;
        GUICheckBox overlayCheckBox;

        GUIButton minusButton;
        GUIButton plusButton;
        GUIButton defaultButton;
        GUIButton centerButton;

        public EditorSettingsTool(EditorComponent editor)
            : base(editor)
        {
            ResourceComponent rc = editor.engine.resourceComponent;
            toolButton.unpressedImg = rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/021_cogs (2).png")));
            toolButton.pressedImg = rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/020_cogs (1).png")));
            editor.editorGui.addToolButton(toolButton, false);

            createDialog();

            closeButton.mouseClickEvent += closeAction;

            pauseCheckBox.toggleEvent += pauseToggle;
            gridCheckBox.toggleEvent += gridToggle;
            ambienceCheckBox.toggleEvent += ambienceToggle;
            overlayCheckBox.toggleEvent += overlayToggle;
            
            centerButton.mouseClickEvent += (pos, button) => { editor.center(); };
            defaultButton.mouseClickEvent += (pos, button) => { editor.zoomMouse = EditorComponent.ZOOMDEFAULT; };
            minusButton.mouseClickEvent += (pos, button) => { editor.zoomCenter--; };
            plusButton.mouseClickEvent += (pos, button) => { editor.zoomCenter++; };
        }

        void createDialog()
        {
            ResourceComponent rc = editor.engine.resourceComponent;
            toolDialog = new GUIControl(editor.editorGui);

            GUILabel background = new GUILabel(editor.editorGui, editor.editorGui.leftBG, "Treequake options");
            background.size = new Vector2((editor.editorGui.graphics.width - EditorGUI.RIGHTBOUNDARY) - EditorGUI.LEFTBOUNDARY - 10, EditorGUI.BOTTOMBOUNDARY);
            toolDialog.pos = new Vector2(toolButton.pos.x + 20, editor.editorGui.graphics.height - EditorGUI.BOTTOMBOUNDARY);
            toolDialog.add(background);

            minusButton = new GUIButton(editor.editorGui, rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/029_minus.png"))), rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/030_minusPressed.png"))));
            minusButton.pos = new Vector2(5, 40);
            toolDialog.add(minusButton);

            defaultButton = new GUIButton(editor.editorGui, rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/025_radio.png"))), rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/024_radio.png"))));
            defaultButton.pos = defaultButton.pos = new Vector2(20, 40);
            toolDialog.add(defaultButton);

            plusButton = new GUIButton(editor.editorGui, rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/027_plus.png"))), rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/028_plusPressed.png"))));
            plusButton.pos = plusButton.pos = new Vector2(35, 40);
            toolDialog.add(plusButton);

            GUILabel zoomLabel = new GUILabel(editor.editorGui, text: "Enhance");
            zoomLabel.pos = new Vector2(55, 40);
            toolDialog.add(zoomLabel);

            centerButton = new GUIButton(editor.editorGui, rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/025_radio.png"))), rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/024_radio.png"))));
            centerButton.pos =  new Vector2(5, 60);
            toolDialog.add(centerButton);

            GUILabel centerLabel = new GUILabel(editor.editorGui, text: "Center");
            centerLabel.pos = new Vector2(25, 60);
            toolDialog.add(centerLabel);

            pauseCheckBox = new GUICheckBox(editor.editorGui, editor.pauseEngine, "Pause Engine");
            pauseCheckBox.pos = new Vector2(5, 20);
            toolDialog.add(pauseCheckBox);

            gridCheckBox = new GUICheckBox(editor.editorGui, editor.showGrid, "Show Grid");
            gridCheckBox.pos = new Vector2(105, 20);
            toolDialog.add(gridCheckBox);

            overlayCheckBox = new GUICheckBox(editor.editorGui, editor.showOverlay, "Show Overlay");
            overlayCheckBox.pos = new Vector2(205, 20);
            toolDialog.add(overlayCheckBox);

            ambienceCheckBox = new GUICheckBox(editor.editorGui, editor.fullAmbient, "Extra Light");
            ambienceCheckBox.pos = new Vector2(105, 40);
            toolDialog.add(ambienceCheckBox);

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

        void gridToggle(bool isDown)
        {
            editor.showGrid = isDown;
        }

        void ambienceToggle(bool isDown)
        {
            editor.fullAmbient = isDown;
        }

        void overlayToggle(bool isDown)
        {
            editor.showOverlay = isDown;
        }

        void pauseToggle(bool isDown)
        {
            editor.pauseEngine = isDown;
            if (!isDown && editor.showGrid) gridCheckBox.toggle();
        }
    }
}
