using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    /**
     * This class represents a list of GUIItems.
     * The list can be oriented vertically (top->bot) or horizontally(left->right), with the edges aligned along the left, right, top, or bottom. The items can also be centered.
     * Top and Bottom alignment can only be used with horizontal orientation, and Left and Right alignment can only be used with vertical orientation. Violating this will produce an exception.
     */
    public class GUIList : GUIContainer
    {
        public enum Orientation
        {
            HORIZONTAL,  ///< Items flow from left to right
            VERTICAL,    ///< Items flow from top to bottom
        }
        public Orientation orientation =   ///< The direction in which the list will flow
            Orientation.VERTICAL;

        public enum Align
        {
            TOP,     ///< Edges aligned along top
            BOTTOM,  ///< Edges aligned along bottom
            LEFT,    ///< Edges aligned along left
            RIGHT,   ///< Edges aligned along right
            CENTER,  ///< Not yet implemented; Do Not Use
        }
        public Align align =   ///< The edge along which items will be aligned
            Align.LEFT;

        //public int margin;

        /**
         * Construct a GUIList
         * 
         * @param items The initial items, if any
         */
        public GUIList(IEnumerable<GUIItem> items = null)
            : base(items)
        {
        }

        /**
         * Lays out the list
         * 
         * @param startPos If not centering, specifies the position of the left-most/top-most corner of the first item along the alignment edge, e.g., the top-right corner if align == RIGHT.
         *                 If centering, specifies the position of the center of the left-most/top-most edge of the first item, depending on orientation.
         *                 To summarize, it's where you want the first item to go, try things until you get it right. TODO: Clean up this description.
         */
        public override void performLayout(Vector2 startPos)
        {
            if (items == null || items.Count < 2) {
                return;
            }

            GUIItem prevItem = null;
            foreach (GUIItem item in items) {
                // Handle first iteration
                if (prevItem == null) {
                    if (orientation == Orientation.HORIZONTAL) {
                        item.left = startPos.x;

                        if (align == Align.TOP) {
                            item.top = startPos.y;
                        } else if (align == Align.BOTTOM) {
                            item.bot = startPos.y;
                        } else if (align == Align.CENTER) {
                            // TODO: Handle centering
                            throw new NotImplementedException("Haven't written CENTER yet");
                        } else {
                            throw new Exception("Invalid Align/Orientation combination.");
                        }
                    } else {  // orientation == Orientation.VERTICAL
                        item.top = startPos.y;

                        if (align == Align.LEFT) {
                            item.left = startPos.x;
                        } else if (align == Align.RIGHT) {
                            item.right = startPos.x;
                        } else if (align == Align.CENTER) {
                            // TODO: Handle centering
                            throw new NotImplementedException("Haven't written CENTER yet");
                        } else {
                            throw new Exception("Invalid Align/Orientation combination.");
                        }
                    }
                } else {  // prevItem != null
                    if (orientation == Orientation.HORIZONTAL) {
                        item.left = prevItem.right;

                        if (align == Align.TOP) {
                            item.top = prevItem.top;
                        } else if (align == Align.BOTTOM) {
                            item.bot = prevItem.bot;
                        } else if (align == Align.CENTER) {
                            // TODO: Handle centering
                            throw new NotImplementedException("Haven't written CENTER yet");
                        } else {
                            throw new Exception("Invalid Align/Orientation combination.");
                        }
                    } else {  // orientation == Orientation.VERTICAL
                        item.top = prevItem.bot;

                        if (align == Align.LEFT) {
                            item.left = prevItem.left;
                        } else if (align == Align.RIGHT) {
                            item.right = prevItem.right;
                        } else if (align == Align.CENTER) {
                            // TODO: Handle centering
                            throw new NotImplementedException("Haven't written CENTER yet");
                        } else {
                            throw new Exception("Invalid Align/Orientation combination.");
                        }
                    }
                }

                prevItem = item;
            }
        }
    }
}
