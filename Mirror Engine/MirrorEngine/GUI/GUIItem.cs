using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Engine
{
    /**
     * Represents a visible element in the GUI.
     */
    public abstract class GUIItem
    {
        public readonly GUI gui;  ///< Reference to the owning GUI
        public int id = -1;

        public abstract Vector2 pos { get; set; }  ///< Position, in screen coordinates
        public abstract Vector2 size { get; set; } ///< Size
        public virtual bool visible { get; set; }  ///< True if the item is visible
        public bool focusable;  ///< True if the item can receive focus

        public delegate void ClickHandler(Vector2 pos, MouseKeyBinding.MouseButton button);  ///< Delegate for a function to handle clicks
        public event ClickHandler mouseDownEvent;            ///< Event fired whenever the GUI Item is clicked
        public event ClickHandler mouseUpEvent;

        public Handle texture;

        public event ClickHandler mouseClickEvent;       ///< Event fired whenever a mouse button is lifted


        public float top ///< Y coord of the top, in screen coords. Settable.
        {
            get { return pos.y; }
            set { pos = new Vector2(pos.x, value); }
        }

        public float bot ///< Y coord of the bottom, in screen coords. Settable.
        {
            get { return pos.y + size.y; }
            set { pos = new Vector2(pos.x, value - size.y); }
        }

        public float left ///< X coord of the left, in screen coords. Settable.
        {
            get { return pos.x; }
            set { pos = new Vector2(value, pos.y); }
        }

        public float right ///< X coord of the right, in screen coords. Settable.
        {
            get { return pos.x + size.x; }
            set { pos = new Vector2(value - size.x, pos.y); }
        }

        public GUIItem(GUI gui)
        {
            //this.pos = new Vector2(0, 0);
            this.gui = gui;
            this.visible = true;
            this.focusable = true;
        }

        /**
         * Determines whether a screen position is over the GUIItem.
         *
         * @param pos The position to test, in screen coordinates
         * @return True if the position is inside the GUIItem
         */
        public bool isOver(Vector2 pos)
        {
            return this.pos.x <= pos.x && pos.x <= this.pos.x + this.size.x &&
                   this.pos.y <= pos.y && pos.y <= this.pos.y + this.size.y;
        }

        //GUIItem's reaction to clicks
        internal virtual void handleMouseDown(Vector2 pos, MouseKeyBinding.MouseButton button)
        {
            if (!visible) return;

            //Handle Clicks
            onMouseDown(pos, button);
            if (mouseDownEvent != null) mouseDownEvent(pos, button);
        }

        //GUIItem's reaction to released clicks
        internal virtual void handleMouseUp(Vector2 pos, MouseKeyBinding.MouseButton button)
        {
            if (!visible) return;

            //Handle Clicks
            onMouseUp(pos, button);
            if(mouseUpEvent != null) mouseUpEvent(pos, button);

            if (this.pos.x <= pos.x && pos.x <= this.pos.x + this.size.x
             && this.pos.y <= pos.y && pos.y <= this.pos.y + this.size.y)
            {
                onMouseClick(pos, button);
                if (mouseClickEvent != null) mouseClickEvent(pos, button);
            }
        }

        /*
         * Called whenever the GUIItem is released successfully from a click.
         */ 
        public virtual void onMouseClick(Vector2 pos, MouseKeyBinding.MouseButton button)
        {
        }

        /**
         * Called whenever the GUIItem is clicked.
         */
        public virtual void onMouseDown(Vector2 pos, MouseKeyBinding.MouseButton button)
        {
        }

        /**
         * Called whenever the GUIItem is unclicked.
         */
        public virtual void onMouseUp(Vector2 pos, MouseKeyBinding.MouseButton button)
        {
        }

        /**
         * Called whenever the GUIItem receives focus
         */
        public virtual void onFocus()
        {
        }

        /**
         * Called whenever the GUIItem loses focus
         */
        public virtual void onBlur()
        {
        }

        /**
         * Called whenever the GUIItem is focused and receives text
         */
        public virtual void onText(char c)
        {
        }

        public virtual void offText(char c)
        {
        }

        /**
         * Draw the GUIItem on the screen
         */
        public abstract void draw();
    }
}
