using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;
namespace Game
{
    public class GameTile : Tile
    {

		
        public int defense;
        public String type;
            
                                                
		
		public GameTile(World world, int xIndex, int yIndex, Mapfile.TileData tileData) : base(world,xIndex,yIndex,tileData)
        {

        }
		public override Handle texture
		{
			get
			{
				return _texture;
			}
			set
			{
				_texture = value;

				//Get dimensions
				imageWidth = Tile.size;
				imageHeight = Tile.size;
				if (texture.key != "")
				{
					Texture2D temp = texture.getResource<Texture2D>();
					imageWidth = temp.width;
					imageHeight = temp.height;
				}
				imageRect = new RectangleF(x, y, imageWidth, imageHeight);
				
                //Set Tile Defense
                defense = SetDefense(type);
                
			}
		}

        /*
         * Set the defense of this tile based on its type 
         */
        private int SetDefense(string type)
        {
            switch (type)
            {
                case "grass":
                    return 0;
                case "forest":
                    return 1;
                case "mountain":
                    return 2;
                case "water":
                    return 0;
                default:
                    return 0; //  No defense boost if type is weird
            }
        }

        // Euclidian distance from this tile to other tile
        // Units are TILE INDICES, not pixels.
        public double euclidian(GameTile other)
        {
            if (other == null)
            {
                return double.NaN;
            }
            double dx = this.xIndex - other.xIndex;
            double dy = this.yIndex - other.yIndex;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        // Manhattan distance from this tile to other tile
        // Units are TILE INDICES, not pixels.
        public double manhattan(GameTile other)
        {
            if (other == null)
            {
                return double.NaN;
            }
            double dx = Math.Abs(this.xIndex - other.xIndex);
            double dy = Math.Abs(this.yIndex - other.yIndex);
            return dx + dy;
        }

        // Units are TILE INDICES, not pixels.
        public override string ToString()
        {
            return "<" + xIndex + ", " + yIndex + ">";
        }
    }
}
