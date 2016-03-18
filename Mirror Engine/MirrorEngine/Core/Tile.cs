/**
 * @file Tile.cs
 * @author SIGGD, PURDUE ACM <siggd.purdue@gmail.com>
 * @version 2.0
 */

using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;

namespace Engine
{
    //Represents a tile in the world and its behavior, as well as data for physics, drawing and pathfinding
    public class Tile
    {
        public readonly World world;                        //The world that the tile belongs to
        internal Mapfile.TileData tileData;                 //The data used to construct the tile
        public Behavior myBehavior { get; private set; }    //The tile's behavior

        //Static tile sizing
        public static int size = 16;                                            //The size of each tile
        public static int halfsize { get { return size / 2; } }                 //Half the tilesize
        public static int logsize { get { return (int)Math.Log(size, 2); } }    //Log2 of the tilesize

        //Position
        public int xIndex;                              //The tile's x coordinate
        public int yIndex;                              //The tile's y coordinate
        public int x { get { return xIndex * size; } }  //The tile's x position
        public int y { get { return yIndex * size; } }  //The tile's y position
        List<Actor> actorsOn;                           //The set of actors currently touching the tile

        //Properties
        public bool solidity;   //The solidity of a tile
        public bool opacityFlip; //Flips the opacity from its default
        public bool opacity { get { return solidity ^ opacityFlip; } } //The opacity of a tile

        //Image
        public RectangleF imageRect;    //The rectangle representing the boundaries of the image
        public int imageWidth;          //The width of the image
        public int imageHeight;         //The height of the image
        protected Handle _texture;        //The image 
        public virtual Handle texture
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
            }
        }

        //Lighting
        public Color color;         //The color of the tile. Add to this to create light (The alpha channel controls intensity)
        public float decay = .96f;  //The rate at which the color gravitates towards 1 and its alpha gravitates towards the ambient light

        //Adjacent tiles
        private List<Tile> _adjacent; //Adjacent is used in A-Star in a deep loop
        public IEnumerable<Tile> adjacent
        {
            get
            {
                if (_adjacent == null) _adjacent = new List<Tile> { up, right, down, left };
                return _adjacent;
            }
        }
        public Tile left
        {
            get
            {
                if (xIndex == 0) return null;
                return world.tileArray[xIndex - 1, yIndex];
            }
        }
        public Tile right
        {
            get
            {
                if (xIndex == world.width - 1) return null;
                return world.tileArray[xIndex + 1, yIndex];
            }
        }
        public Tile up
        {
            get
            {
                if (yIndex == 0) return null;

                return world.tileArray[xIndex, yIndex - 1];
            }
        }
        public Tile down
        {
            get
            {
                if (yIndex == world.height - 1) return null;
                return world.tileArray[xIndex, yIndex + 1];
            }
        }

        //A*
        internal HeapNode<Tile> openPQNode;   //Dunno
        internal float fScore;                //Dunno
        internal float gScore;                //Dunno
        internal Tile previous;               //Dunno
        internal bool MAKERED;                //Dunno

        /**
        * Constructor. Initializes the tile using the given tileData
        */
        public Tile(World world, int xIndex, int yIndex, Mapfile.TileData tileData)
        {
            this.world = world;
            this.xIndex = xIndex;
            this.yIndex = yIndex;
            actorsOn = new List<Actor>();
            color = new Color(world.ambientLight, world.ambientLight);

            initFromTileData(tileData);
        }

        /*
        * Uses the given TileData to update the tile's state
        */
        private void initFromTileData(Mapfile.TileData tileData)
        {
            this.tileData = tileData;
            texture = world.engine.resourceComponent.get(tileData.texture);
            myBehavior = world.constructTileBehavior(this, world.engine.resourceComponent.get(tileData.behavior));

            solidity = tileData.solidity == 1;
            opacityFlip = tileData.opacityFlip == 1;
        }

        /*
        * Overwrites the tile's state based on the non-ignore values of the given TileData
        */
        internal void overWriteFromTileData(Mapfile.TileData data)
        {
            tileData.overWriteData(data);

            if (data.texture != Mapfile.TileData.IGNORESTRING && data.texture != texture.key) texture = world.engine.resourceComponent.get(data.texture);
            if (data.behavior != Mapfile.TileData.IGNORESTRING && (myBehavior == null || data.behavior != myBehavior.scriptKey)) myBehavior = world.constructTileBehavior(this, world.engine.resourceComponent.get(data.behavior));
            if (data.solidity != Mapfile.TileData.IGNOREBYTE) solidity = data.solidity == 1;
            if (data.opacityFlip != Mapfile.TileData.IGNOREBYTE) opacityFlip = data.opacityFlip == 1;
            //if (data.leftSlope != MapFile.TileData.IGNOREFLOAT) leftSlope = data.leftSlope;
            //if (data.rightSlope != MapFile.TileData.IGNOREFLOAT) rightSlope = data.rightSlope;
            //if (data.normal != MapFile.TileData.IGNOREBYTE) normal = data.normal;
        }

        /*
        * Update routine for the tile. Run's the tile's behavior. 
        * Calls steppedOn and steppedOff for the relevant actors.
        */
        internal void update()
        {
            //Update lighting
            performDecay();

            if (myBehavior == null) return;

            //Update actorsOn
            List<Actor> newActorsOn = world.getActorsInRect(new RectangleF(x, y, size, size));

            //Call steppedOff for any actors that are not on the tile anymore
            foreach (Actor a in actorsOn)
            {
                if (!newActorsOn.Contains(a))
                {
                    dynamic steppedOff = myBehavior.getVariable("steppedOff");
                    try
                    {
                        if (steppedOff != null) steppedOff(a);
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(Script.getRuntimeError(e, world.worldName));
                    }
                }
            }

            //Call steppedOn for any actors that are new to the tile
            foreach (Actor a in newActorsOn)
            {
                if (!actorsOn.Contains(a))
                {
                    dynamic steppedOn = myBehavior.getVariable("steppedOn");
                    try
                    {
                        if (steppedOn != null) steppedOn(a);
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(Script.getRuntimeError(e, world.worldName));
                    }
                }
            }

            //update actorsOn
            actorsOn = newActorsOn;

            //Run tile behavior
            myBehavior.run();
        }

        /**
        * Gravitates the color towards (1,1,1, world.ambientLight)
        * Rate is determined by the property decay
        */
        protected virtual void performDecay()
        {
            //Color decay toward 1
            if (color.getBrightness() != 1f)
            {
                color += -1f;
                color *= decay;
                if (Math.Abs(color.getBrightness()) < 0.00005f)
                {
                    color = new Color(0, color.a);
                }
                color += 1f;
            }

            //Intensity decay toward ambience
            color.a -= world.ambientLight;
            color.a *= decay;
            if (color.a < 0.00005f) color.a = 0;
            color.a += world.ambientLight;
        }

        /*
         * Retrieves the final color value of the tile and normalizes color
         * The final color is the color multiplied by its intensity
         * The final color is opaque
         */
        public virtual Color getFinalColor()
        {
            Color finalColor = color.normalize() * color.a;
            finalColor.a = 1f;
            return finalColor;
        }

        /**
        * Gets a hueristic for A*
        * 
        * @param node the tile 
        * 
        * @return something...
        */
        internal float heuristic(Tile node)
        {
            return Math.Abs(node.y - y) + Math.Abs(node.x - x);
        }
    }	
}