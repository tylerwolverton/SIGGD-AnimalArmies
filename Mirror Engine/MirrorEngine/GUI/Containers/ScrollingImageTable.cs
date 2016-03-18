using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public class ScrollingImageTable : GUIControl
    {
        public enum ScrollDirection
        {
            VERTICAL, HORIZONTAL
        }

        public Vector2 cellDim { get; set; }
        public int rows { get; protected set; } //How many rows are in the table (by default)?
        public int columns { get; protected set; } //How many columns are in the table (by default)?
        private int filledSpots { get; set; } //How many spots in the table have been filled?
        private int currentPos { get; set; } //How many positions have we scrolled?
        public ScrollDirection orientation { get; protected set; } //Does the table scroll horizontally or vertically?

        private Vector2 _pos;
        public override Vector2 pos {
            get
            {
                return _pos;
            }
            set
            {
                _pos = value;
            }
        }

        public override Vector2 size
        {
            get
            {
                return new Vector2(columns * cellDim.x, rows * cellDim.y);
            }
        }

        public GUIItem firstScrollButton;
        public GUIItem secondScrollButton;
        public GUIItem scrollBar;
        public GUIItem scrollBarBar;
        public GUI gui;

        public int padding = 0;

        public ScrollingImageTable(GUI gui, int rows, int columns, int cellWidth,int cellHeight, ScrollDirection disposition, Vector2 pos)
            : base(gui, pos.x, pos.y)
        {
            this.rows = rows;
            this.columns = columns;
            this.cellDim = new Vector2(cellWidth, cellHeight);
            this.orientation = disposition;
            this.pos = pos;
            this.gui = gui;
            this.currentPos = 0;
            filledSpots = 0;
        }
        public void performLayout()
        {
            if (orientation == ScrollDirection.HORIZONTAL)
            {
                for (int i = 0; i < filledSpots; i++)
                {
                    if((i)% rows >= currentPos && i % rows < currentPos + columns)
                    {
                        GUIItem square = this.items[i];//there was a +4 here i assume for commented out constructor, WHY!??! -James
                        
                        square.pos = new Vector2(padding+pos.x + ((i) % rows - currentPos) * cellDim.x, padding+pos.y + ((i) / rows) * cellDim.y);
                    }
                }
            }
            if (orientation == ScrollDirection.VERTICAL)
            {
                for (int i = 0; i < filledSpots; i++)
                {
                    if ((i) % columns >= currentPos && i % columns < currentPos + rows)
                    {
                        GUIItem square = this.items[i]; //there was a +4 here i assume for commented out constructor, WHY!??! -James
                        square.pos = new Vector2(padding + pos.x + ((i) % columns) * cellDim.x, padding + pos.y + ((i) / columns - currentPos) * cellDim.y);
                    }
                }
            }
        }

        public GUIItem getItem(int ind)
        {
            if (ind < 0 || ind >= items.Count) return null;
            return items[ind];
        }
        
        public override void add(GUIItem item)
        {
            this.items.Add(item);
            filledSpots++;
        }

        public override void remove(GUIItem item)
        {
            base.remove(item);
            filledSpots--;
        }
    }
}
