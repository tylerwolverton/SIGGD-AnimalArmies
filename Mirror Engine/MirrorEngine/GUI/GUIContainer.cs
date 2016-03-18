using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    /**
     * This class represents a virtualized container of GUIItems.
     * GUIContainers act as mini layout engines, not truly containing objects, but rather just setting their positions as if they were in some kind of container.
     * To use a GUIContainer, simply add the desired items to the container, then call performLayout(). The positions of the items will be overwritten to
     * follow the abstraction provided by the GUIContainer. The positions of the items can be then changed at will without affecting other items; i.e., performLayout() must always be
     * called manually when the GUIContainer's functionality is desired.
     */
    public abstract class GUIContainer
    {
        public List<GUIItem> items;   ///< Items managed by the GUIContainer

        /**
         * Construct a GUIContainer
         *
         * @param items The items to initally add to the container, if any
         */
        public GUIContainer(IEnumerable<GUIItem> items = null)
        {
            if (items != null) {
                this.items = new List<GUIItem>(items);
            } else {
                this.items = new List<GUIItem>();
            }
        }

        /**
         * Add an item 
         *
         * @param item The item to add
         */
        public virtual void addItem(GUIItem item)
        {
            items.Add(item);
        }

        /**
         * Add several items
         *
         * @param items The items to add
         */
        public void addItems(IEnumerable<GUIItem> items)
        {
            this.items.AddRange(items);
        }

        /**
         * Remove an item 
         *
         * @param item The item to remove
         */
        public void removeItem(GUIItem item)
        {
            items.Add(item);
        }

        /**
         * Remove several items
         *
         * @param items The items to remove
         */
        public void removeItems(IEnumerable<GUIItem> items)
        {
            foreach (GUIItem item in items) {
                this.items.Remove(item);
            }
        }

        /**
         * Remove all items from the GUIContainer
         */
        public void removeAll()
        {
            items.Clear();
        }

        /**
         * Perform layout on the items in the GUIContainer, setting their positions according to some abstraction
         *
         * @param startPos The "start" position of the container. Precise meaning is implementation-dependent.
         */
        public abstract void performLayout(Vector2 startPos);
    }
}
