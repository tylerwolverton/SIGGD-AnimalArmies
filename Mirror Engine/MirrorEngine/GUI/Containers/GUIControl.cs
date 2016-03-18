/**
 * @file GUIControl.cs
 * @author SIGGD, Purdue ACM <siggd.purdue@gmail.com>
 * @version 2.0
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    /**
     * This class combines several GUIItems into a single logical item. This is similar to GUIContainer, but GUIControls do not publicly allow items to be added or removed,
     * and GUIControls also manage the lifetimes of their member items (by adding/removing to/from the GUI).
     */
    public class GUIControl : GUIItem
    {
        public List<GUIItem> items;   // Items managed by the GUIControl

        private Vector2 _pos;
        public override Vector2 pos {
            get
            {
                return _pos;
            }
            set
            {
                if (_pos == null) _pos = Vector2.Zero;
                Vector2 shift = value - _pos;
                _pos = value;

                foreach (GUIItem item in items) {
                    item.pos += shift;
                }
            }
        }

        public override Vector2 size {
            get
            {
                Vector2 size = Vector2.Zero;
                foreach (GUIItem item in items) {
                    size = Vector2.max(size, item.size);
                }
                return size;
            }
            set
            {
            }
        }

        public override bool visible {
            get
            {
                if (items.Count == 0) {
                    return false;
                }
                return items[0].visible;
            }
            set
            {
                if (items == null) return;

                foreach (GUIItem item in items) {
                    item.visible = value;
                }
            }
        }
 
        /**
         * Construct a GUIItem.
         * 
         * @param gui The game gui
         * @param x The x coordinate of the initial position for the GUIControl
         * @param y The y coordinate of the initial position for the GUIControl
         */
        public GUIControl(GUI gui, float x = 0, float y = 0)
            : base(gui)
        {
            this.items = new List<GUIItem>();
            _pos = new Vector2(x, y);
        }

        /**
         * Add item to this control
         * 
         * @param item The item to add
         */
        public virtual void add(GUIItem item)
        {
            item.pos += this.pos;
            items.Add(item);
        }

        /**
         * Add item from this control
         * 
         * @param item The item to add
         */
        public virtual void remove(GUIItem item)
        {
            items.Remove(item);
        }

        //This is just a collection, so there is no item to draw for this class. Each of the items will draw themselves.
        public override void draw(){}
    }
}
