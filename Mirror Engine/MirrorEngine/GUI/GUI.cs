using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tao.Sdl;
using Tao.OpenGl;

namespace Engine
{
    public class GUI
    {
        public readonly GraphicsComponent graphics;

        protected GUIControl currentDialog;

        public List<GUIItem> items;     ///< List of current GUI Items
        protected GUIItem focused;  ///< The GUIItem with focus

        public string rootDirectory;
        public string defaultFontPath = "Fonts/Vera.ttf";

        public GUI(GraphicsComponent graphics)
        {
            this.rootDirectory = ResourceComponent.DEFAULTROOTDIRECTORY;
            this.graphics = graphics;
            this.items = new List<GUIItem>();
            currentDialog = null;
        }

        public virtual void initialize()
        {
            InputComponent ic = graphics.engine.inputComponent;
            object saved = ic.getContext(typeof(InputComponent.GUIBindings));
            ic.setContext(typeof(InputComponent.GUIBindings), this);
            (ic[InputComponent.GUIBindings.MOUSEBUTTON] as MouseKeyBinding).mouseKeyDown += handleClickDown;
            (ic[InputComponent.GUIBindings.MOUSEBUTTON] as MouseKeyBinding).mouseKeyUp += handleClickUp;

            (ic[InputComponent.GUIBindings.TEXT] as TextBinding).charEntered += handleText;
            (ic[InputComponent.GUIBindings.TEXT] as TextBinding).keyLifted += handleTextUp;
            (ic[InputComponent.GUIBindings.HORIZONTALAXIS] as AxisBinding).valChanged += handleHorizontal;
            (ic[InputComponent.GUIBindings.VERTICALAXIS] as AxisBinding).valChanged += handleVertical;
            ic.setContext(typeof(InputComponent.GUIBindings), saved);
            ic.setContext(typeof(InputComponent.EngineBindings), this); //Binding for fullscreen.
            (ic[InputComponent.EngineBindings.FULLSCREEN] as SinglePressBinding).downEvent += graphics.toggleFullScreen;
        }

        public virtual void loadContent()
        {
        }

        /**
         * Add the item to the GUI
         */
        public virtual void add(GUIItem item)
        {
            if (item == null) return;
            GUIControl control = item as GUIControl;
            if (control != null)
            {
                add(control);
            }
            else
            {
                items.Add(item);
            }
        }

        /**
        * Add the control to the GUI
        */
        public virtual void add(GUIControl control)
        {
            if (control == null) return;
            items.Add(control);
            foreach (GUIItem item in control.items)
            {
                add(item);
            }
        }

        /* Kicks out the previous singular dialog and adds a new one
         */ 
        public void switchDialogs(GUIControl dialog)
        {
            remove(currentDialog);
            add(dialog);
            currentDialog = dialog;
        }

        /* Attempts to remove, then adds a dialog in succession
         * Useful for moving a dialog to the front 
         * and refreshing its fields
         */ 
        public void refreshDialog(GUIControl dialog)
        {
            remove(dialog);
            add(dialog);
        }

        /**
        * Attempts to remove the item from the GUI
        */
        public virtual void remove(GUIItem item)
        {
            if (item == null) return;
            GUIControl control = item as GUIControl;
            if (control != null)
            {
                remove(control);
            }
            else
            {
                try
                {
                    items.Remove(item);
                }
                catch (Exception e) { }
            }
        }

        /**
        * Remove the control from the GUI
        */
        public virtual void remove(GUIControl control)
        {
            if (control == null) return;
            foreach (GUIItem item in control.items)
            {
                remove(item);
            }
            items.Remove(control);
        }

        public virtual void draw()
        {
            if (items == null)
                return;

            if(focused != null && focused is GUITextBox)
                (focused as GUITextBox).update();

            foreach (GUIItem item in items) {
                if (item.visible) {
                    item.draw();
                }
            }
        }

        private void handleClickDown(MouseKeyBinding.MouseButton m)
        {
            handleClick(m);
        }

        private void handleClickUp(MouseKeyBinding.MouseButton m)
        {
            InputComponent ic = graphics.engine.inputComponent;
            Vector2 pos = ic.getMousePosition();
            if (focused != null) focused.handleMouseUp(pos, m);
        }

        //Obtains the clicked item, calls that items click handler, and sets focus.
        private void handleClick(MouseKeyBinding.MouseButton button)
        {
            if (items == null) return;

            //Find the last (top) clicked item
            InputComponent ic = graphics.engine.inputComponent;
            Vector2 pos = ic.getMousePosition();
            GUIItem clickedItem = getItemAt(pos);
            if (clickedItem != null) clickedItem.handleMouseDown(pos, button);

            //Set Focus
            if (focused != null) focused.onBlur();
            if (clickedItem == null || clickedItem.focusable) focused = clickedItem;
            if (focused != null) focused.onFocus();
        }

        //Gets the GUIItem at the given location, or returns null if none
        public GUIItem getItemAt(Vector2 pos)
        {
            GUIItem lastItem = null;
            foreach (GUIItem item in items)
            {
                if (item.isOver(pos))
                {
                    lastItem = item;
                }
            }

            return lastItem;
        }

        private void handleText(char c)
        {
            if (focused != null) {
                focused.onText(c);
            }
        }

        private void handleTextUp(char c)
        {
            if (focused != null)
            {
                focused.offText(c);
            }
        }

        private void handleVertical(int i)
        {
            onVerticalChange(i);
        }

        private void handleHorizontal(int i)
        {
            onHorizontalChange(i);
        }

        protected virtual void onVerticalChange(int i)
        {
        }

        protected virtual void onHorizontalChange(int i)
        {
        }
    }
}