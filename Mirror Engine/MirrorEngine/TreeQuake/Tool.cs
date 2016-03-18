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

namespace Engine
{
    /// brief classdeschere
    public class Tool
    {
        //GUI
        public GUIContainer gui;
        public GUIControl toolDialog;
        public GUIButton toolButton;

        //Editor
        public EditorComponent editor; 
        public Boolean active { get; set; }     // Whether or not the tool is active
        
        //Tool
        public static Stack<ToolAction> toolAction = new Stack<ToolAction>();    // List of actions performed 
        public static Stack<ToolAction> undos = new Stack<ToolAction>();         // List of Undos since last action
        public Boolean isDragging = false; 

        //Floatingpic
        public GUILabel floatingPic;

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        public static void redo()
        {
            if (undos.Count() > 0)
            {
                ToolAction tool = undos.Pop();
                toolAction.Push(tool);
                tool.doAction();
            }
        }

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        public static void undo()
        {
            if (toolAction.Count() > 0)
            {
                ToolAction tool = toolAction.Pop();
                undos.Push(tool);
                tool.undoAction();
            }
        }

        public class ToolAction
        {

            public Tool parent;

            /**
            * desc here
            *
            * @param paramsdeschere
            *
            * @return returndeschere
            */
            public ToolAction(Tool parent)
            {
                this.parent = parent;
            }

            /**
            * desc here
            *
            * @param paramsdeschere
            *
            * @return returndeschere
            */
            public void doAction()
            {
                parent.redoAction(this);
            }

            /**
            * desc here
            *
            * @param paramsdeschere
            *
            * @return returndeschere
            */
            public void undoAction()
            {
                parent.undoAction(this);
            }
        }

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        public Tool(EditorComponent editor)
        {
            this.editor = editor;

            InputComponent input = this.editor.engine.inputComponent;

            toolAction = new Stack<ToolAction>();
            undos = new Stack<ToolAction>();

            toolButton = new GUIButton(editor.editorGui, null, null);
            toolButton.mouseClickEvent += (pos, button) => { editor.selectTool(this); };
            toolButton.mouseUpEvent += (pos, button) => { toolButton.texture = this.active ? toolButton.pressedImg : toolButton.unpressedImg; };
        }

        public void activateAction(Vector2 pos, MouseKeyBinding.MouseButton button)
        {
            activate();
        }

        /// <summary>
        /// Switches the editor context to this tool, deselecting
        /// all other active tool buttons and initiatializing the tool.
        /// </summary>
        public virtual void activate()
        {
            if (editor.activeTool != null) editor.activeTool.deactivate();
            editor.activeTool = this;

            //Switch dialogs
            if (toolDialog != null) editor.editorGui.switchDialogs(toolDialog);
            editor.editorGui.resetAllBut(toolButton);

            toolButton.texture = toolButton.pressedImg;
            active = true;
        }

        public virtual void deactivate()
        {
            this.active = false;
        }

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        public virtual void undoAction(ToolAction action) { }

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        public virtual void redoAction(ToolAction action) { }

        /**
        * Gets called when the mouse clicked on a tile
        *
        * @button the mousebutton that went down
        */
        public virtual void downAction(MouseKeyBinding.MouseButton button) { isDragging = true; }

        /**
        * Gets called when a mousebutton was released
        *
        * @button the mousebutton that went up
        */
        public virtual void upAction(MouseKeyBinding.MouseButton button) { isDragging = false; }


        /**
         * Gets called every tick
         */ 
        public virtual void update() { }

        /**
        * Gets called when the tile under the mouse has changed
        */
        public virtual void moveAction(Vector2 position) { }
    }

    ///
    public struct TextureData
    {

        public int x; 
        public int y; 
        public byte tileIndex; 

         
        public TextureData(int x, int y, byte tileIndex = 0)
        {
            this.x = x;
            this.y = y;
            this.tileIndex = tileIndex;
        }
    }
}
