/**
 * @file EraserTool.cs
 * @author SIGGD, Purdue ACM <siggd.purdue@gmail.com>
 * @version 2.0
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Engine;
using System.IO;

namespace Engine
{
    /*
     * The eraser tool. Erases tiles/actors in the editor
     */
    public class EraserTool : Tool
    {
        private Vector2 minSize;    //The minimum and maximum sizes for the eraser radius
        private Vector2 maxSize;

        private GUIButton plus;
        private GUIButton minus;

        /**
        * EraserTool constructor. Sets the images for the buttons, registers events, and initializes data
        *
        * @param editor The current EditorComponent
        */
        public EraserTool(EditorComponent editor)
            : base(editor)
        {
            ResourceComponent rc = editor.engine.resourceComponent;
            toolButton.unpressedImg = rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/009_eraser.png")));
            toolButton.pressedImg = rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/010_eraser_inv.png")));
            editor.editorGui.addToolButton(toolButton, true);

            createDialog();
            plus.mouseClickEvent += increaseSizeAction;
            minus.mouseClickEvent += decreaseSizeAction;

            isDragging = false;

            floatingPic = new GUILabel(editor.editorGui, rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/026_eraserCircle.png"))));
            floatingPic.tintTex = new Color(1f, 1f, 1f, .5f);
            floatingPic.size = new Vector2(32, 32);
            minSize = new Vector2(16,16);
            maxSize = new Vector2(32*32, 32*32);
        }

        /*
         * Creates all of the GUI components for display
         */ 

        void createDialog()
        {
            ResourceComponent rc = editor.engine.resourceComponent;
            toolDialog = new GUIControl(editor.editorGui);

            //Right menu
            GUILabel background = new GUILabel(editor.editorGui, editor.editorGui.leftBG, text: "Eraser Tool");
            background.size = new Vector2(EditorGUI.RIGHTBOUNDARY, editor.engine.graphicsComponent.height - EditorGUI.BOTTOMBOUNDARY);
            toolDialog.pos = new Vector2(editor.engine.graphicsComponent.width - EditorGUI.RIGHTBOUNDARY, 0);
            toolDialog.add(background);

            minus = new GUIButton(editor.editorGui, rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/029_minus.png"))), rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/030_minusPressed.png"))));
            minus.textColor = new Color(0, 0, 0);
            minus.pos = new Vector2(0, 20);
            toolDialog.add(minus);

            plus = new GUIButton(editor.editorGui, rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/027_plus.png"))), rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/028_plusPressed.png"))));
            plus.textColor = new Color(0, 0, 0);
            plus.pos = new Vector2(minus.size.x, 20);
            toolDialog.add(plus);
        }

        /*
         * The function called when the plus button is pressed.
         * Increases the eraser circle size.
         */

        void increaseSizeAction(Vector2 pos, MouseKeyBinding.MouseButton mouseButton)
        {
            if(!floatingPic.size.Equals(maxSize))
                floatingPic.size = new Vector2(floatingPic.size.x*2, floatingPic.size.y*2);
            plus.texture = plus.unpressedImg;
        }

        /*
         * The function called when the minus button is pressed.
         * Decreases the eraser circle size.
         */

        void decreaseSizeAction(Vector2 pos, MouseKeyBinding.MouseButton mouseButton)
        {
            if(!floatingPic.size.Equals(minSize))
                floatingPic.size = new Vector2(floatingPic.size.x/2, floatingPic.size.y/2);

            minus.texture = minus.unpressedImg;
        }

       /**
       * This function is called when the mouse button is pressed down.
       *
       * @param button The MouseKeyBinding.MouseButton that was clicked down
       */
        public override void downAction(MouseKeyBinding.MouseButton button)
        {
            Vector2 screenPos = editor.engine.inputComponent.getMousePosition();
            Vector2 worldPos = editor.engine.graphicsComponent.camera.screen2World(screenPos);
            
            IEnumerable<Actor> actorsToErase = editor.engine.world.getActorsInCone(worldPos, (floatingPic.size.x / 2) / (editor.engine.graphicsComponent.camera.scale), new Vector2(16, 16), 360, null);
            
            foreach (Actor target in actorsToErase)
            {
                Mapfile.ActorData w = new Mapfile.ActorData();
                w.id = (byte)editor.engine.world.actorFactory.names[target.actorName];
                w.x = target.spawn.x;
                w.y = target.spawn.y;
                w.z = 0;
                editor.engine.world.file.worldActorData.Remove(w);
                editor.engine.world.actors.Remove(target);
                target.removeMe = true;
            }

            isDragging = true;
        }

        /**
        * Called when the mouse moves into a new tile. If the mouse is clicked down and dragging, it will erase anything within the radius of the eraser circle.
        *
        * @param position The position that the mouse moved to
        */
        public override void moveAction(Vector2 position)
        {
            if (isDragging)
            {
                Vector2 worldPos = editor.engine.graphicsComponent.camera.screen2World(position);

                IEnumerable<Actor> actorsToErase = editor.engine.world.getActorsInCone(worldPos, (floatingPic.size.x / 2) / (editor.engine.graphicsComponent.camera.scale), new Vector2(16, 16), 360, null);

                foreach (Actor target in actorsToErase)
                {
                    Mapfile.ActorData w = new Mapfile.ActorData();
                    w.id = (byte)editor.engine.world.actorFactory.names[target.actorName];
                    w.x = target.spawn.x;
                    w.y = target.spawn.y;
                    w.z = 0;
                    editor.engine.world.file.worldActorData.Remove(w);
                    editor.engine.world.actors.Remove(target);
                }
            }
        }

        /** 
         * Draws the red eraser circle and tints items under it. 
         */ 

        public override void update()
        {
            Vector2 screenPos = editor.engine.inputComponent.getMousePosition();
            Vector2 worldPos = editor.engine.graphicsComponent.camera.screen2World(screenPos);
            
            floatingPic.pos = new Vector2(screenPos.x - floatingPic.size.x / 2, screenPos.y - floatingPic.size.y / 2);
            if (editor.editorGui.getItemAt(screenPos) == null)
            {
                IEnumerable<Actor> theseActors = editor.engine.world.getActorsInCone(worldPos, (floatingPic.size.x / 2) / (editor.engine.graphicsComponent.camera.scale), new Vector2(16, 16), 360, null);
                foreach (Actor here in theseActors)
                {
                    here.tint = new Color(1f, 1f, 1f, .4f);
                }
            }
        }

        /**
        * Called when an EraserTool action has been undone
        *
        * @param action The action that is being undone
        */
        public override void undoAction(ToolAction action)
        {
            Mapfile.ActorData w = (action as EraserToolAction).actor;
            editor.engine.world.file.worldActorData.Add(w);
        }

        /**
        * Called when an EraserTool action has been redone
        *
        * @param action The action being redone
        */
        public override void redoAction(ToolAction action)
        {
            Mapfile.ActorData w = (action as EraserToolAction).actor;
            editor.engine.world.file.worldActorData.Remove(w);
        }

        /**
         * Eraser tool action. Used for undoing/redoing
         */
 
        public class EraserToolAction : ToolAction
        {

            public Mapfile.ActorData actor; //The actor that an action was performed on

            /**
            * Builds the EraserToolAction
            *
            * @param actor The actor that an action was performed on
            * @param parent The editor tool that the action originated from
            */
            public EraserToolAction(Mapfile.ActorData actor, Tool parent)
                : base(parent)
            {
                this.actor = actor;
            }
        }
    }
}
