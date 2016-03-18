/*
 * @file World.cs
 * @author SIGGD, Purdue ACM <siggd.purdue@gmail.com>
 * @version 2.0
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace Engine
{
    //Represents a space made up of tiles where actors exist and behave.
    public abstract class World : Component
    {
        public Mapfile file; //The data used to create and store the world

        public float ambientLight = 1f; //The world's light level
        internal float maxActorRadius;   //Radius in which actors are potential collision candidates.
        
        public string worldName {get{ return file.worldName; }} //The name of the world
        public Behavior myBehavior;                             //Defines the world's behavior
        public Tile[,] tileArray;                               //The world's tiles
        public LinkedList<Actor> actors;                        //The actors in the world

        public int width;  //Width of the map in tiles
        public int height; //Height of the map in tiles
        
        public ActorFactory actorFactory { get; protected set; } //Used to create new actors

        public delegate void VisitTile(Tile tile); //An action to perform on a tile
        
        internal bool firstUpdate; //Whether or not the world has been updated previously

        /**
        * Constructor.
        *
        * @param engine the engine
        * @param file the Mapfile to load the world from
        */
        public World(MirrorEngine engine, Mapfile file) : base(engine)
        {
            this.file = file;

            if (engine.resourceComponent == null) throw new Exception("World: ResourceComponent not initialized");
        }

        /**
        * Creates the gameActorFactory and loads the tiles, actors, and world behavior
        */
        public override void initialize()
        {
            //Create the actor factory
            constructGameActorFactory();

            //Load all the tiles from that file
            initTiles(file.worldTileData);

            //Load all the actors from that file
            initActors(file.worldActorData);

            //Load the world's behavior
            if (!file.worldBehaviorKey.Equals(""))
                setWorldBehavior(engine.resourceComponent.get(file.worldBehaviorKey));

            //Make sure the actors that overlap do so properly
            sortActors();
        }

        /**
         * Constructs the GameActorFactory
         * Must be implemented by the GameWorld
         */
        protected abstract void constructGameActorFactory();

        /**
        * Creates the tile array given an array of TileData
        *
        * @param tileData the 3D TileData array to construct the world's tiles with
        */
        protected virtual void initTiles(Mapfile.TileData[,,] tileData)
        {
            //Get dimensions
            width = tileData.GetLength(1);
            height = tileData.GetLength(2);
            tileArray = new Tile[width, height];

            //Fill tile array
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tileArray[x, y] = new Tile(this, x, y, file.worldTileData[0, x, y]);
                }
            }
        }

        /**
        * Loads all the actors given a list of actor data
        *
        * @param actorData the data to load the actors from
        */
        protected virtual void initActors(List<Mapfile.ActorData> actorData)
        {
            //Fill the actor list
            actors = new LinkedList<Actor>();
            for (int i = 0; i < file.worldActorData.ToArray().GetLength(0); i++)
            {
                addActor(actorFactory.createActor(file.worldActorData[i].id, new Vector2(file.worldActorData[i].x, file.worldActorData[i].y), Vector2.Zero));
            }
        }

        /**
         * Constructs a behavior for the world
         * 
         * @param script the script to decorate the world's behavior with
         */
        public virtual void setWorldBehavior(Handle script = null)
        {
            if (script == null || script.key == "") return;
            myBehavior = new Behavior(this, script: script);
        }

        /**
         * Constructs a behavior for a tile
         * 
         * @param tile the tile that will receive the behavior
         * @param script the script to decorate the tile's behavior with
         */
        public virtual Behavior constructTileBehavior(Tile tile, Handle script = null)
        {
            if (tile == null || script == null || script.key == "") return null;
            return new Behavior(this, tile: tile, script: script);
        }

        /**
        * Runs routines necessary when a world has started--for use by a Game-specific world class
        */
        protected virtual void start()
        {
            //Call the start method in the world's behavior
            if (myBehavior != null)
            {
                dynamic start = myBehavior.getVariable("start");
                try
                {
                    if (start != null) start();
                }
                catch (Exception e)
                {
                    Trace.WriteLine(Script.getRuntimeError(e, worldName));
                }
            }
        }

        /**
        * Updates the world, its actors, and its tiles
        */
        public virtual void Update()
        {
            //If first update, perform startup work
            if (firstUpdate) 
            {
                start();
                firstUpdate = false;
            }

            //Make sure the actors that overlap do so properly.
            sortActors();

            //Run the world's behavior
            if(myBehavior != null) myBehavior.run();

            //Let every actor's behavior be run this tick.
            for (LinkedListNode<Actor> a = actors.First; a != null; a = a.Next)
            {
                a.Value.Update();
            }

            //If an actor wants to be removed from the map, remove it.
            LinkedListNode<Actor> actor = actors.First;
            while (actor != null)
            {
                LinkedListNode<Actor> tmpNext = actor.Next;

                //This needs major cleanup
                if (actor.Value.removeMe)
                {
                    actor.Value.notifyRemoved();
                    actors.Remove(actor);
                }

                actor = tmpNext;
            }

            //Update Max Half-Height
            maxActorRadius = 0;
            foreach (Actor a in actors)
            {
                float radius = a.diameter / 2;
                if (radius > maxActorRadius) maxActorRadius = radius;
            }

            RectangleF viewRect = engine.graphicsComponent.camera.viewRect;

            //Calculate the range of tiles to consider for updating
            int tileLeft = (int)(viewRect.left * 2 - viewRect.right) / Tile.size;
            int tileTop = (int)(viewRect.top * 2 - viewRect.bottom) / Tile.size;
            int tileRight = (int)(viewRect.right * 2 - viewRect.left) / Tile.size;
            int tileBottom = (int)(viewRect.bottom * 2 - viewRect.top) / Tile.size;

            //Make sure in tile bounds
            if (tileLeft < 0) tileLeft = 0;
            if (tileTop < 0) tileTop = 0;
            if (tileRight >= width) tileRight = width - 1;
            if (tileBottom >= height) tileBottom = height - 1;
            if (tileRight < 0 || tileBottom < 0) return;

            //Update tiles
            for (int x = tileLeft; x <= tileRight; x++)
            {
                for (int y = tileTop; y <= tileBottom; y++)
                {
                    tileArray[x, y].update();
                }
            }

            sortActors();

            //Trace.WriteLine(stopwatch.ElapsedMilliseconds);
        }

        /**
        * Adds the given actor to the world
        *
        * @param a the actor to add
        *
        * @return whether the add was successful
        */
        public bool addActor(Actor a)
        {
            if (a == null) return false;

            if ((a.position.x >= 0 && a.position.x < width * Tile.size
                && a.position.y >= 0 && a.position.y < height * Tile.size))
            {
                //Check if area is ok to spawn in
                if (!isAreaOpen(new RectangleF(a.position.x, a.position.y, a.size.x, a.size.y)) && !a.ignoreAvE)
                    return false;

                //Add actor
                actors.AddFirst(a);

                //update max radius
                float radius = a.diameter / 2;
                if (radius > maxActorRadius) maxActorRadius = radius;
            }

            return true;
        }

        /**
        * Make sure that the actors that overlap do so properly.
        */
        internal void sortActors()
        {
            LinkedListNode<Actor> iter = actors.First;
            while (iter != null)
            {
                LinkedListNode<Actor> next = iter.Next;

                while (iter.Previous != null)
                {
                    if (iter.Value.position.y >= iter.Previous.Value.position.y) break;

                    // Swap the unsorted value with the previous
                    LinkedListNode<Actor> tmp = iter.Previous;
                    actors.Remove(iter);
                    actors.AddBefore(tmp, iter);
                }

                iter = next;
            }
        }

        /**
         * Gets the center of the map in world coordinates
         */
        public Vector2 getCenter()
        {
            return new Vector2(Tile.size * width / 2, Tile.size * height / 2);
        }

        /**
        * Gets the tile at the given world coordinate
        *
        * @param position the position relative to the world
        *
        * @return the tile at the given position, or null if nonexistant
        */
        public Tile getTileAt(Vector2 position)
        {
            return getTileAt(position.x, position.y);
        }

        /**
        * Gets the tile at the given location
        *
        * @param x the x coordinate relative to the world
        * @param y the y coordinate relative to the world
        *
        * @return the tile at the given position, or null if nonexistant
        */
        public Tile getTileAt(float x, float y)
        {
            //Get coordinates in tiles
            int xtile = (int)(x / Tile.size);
            int ytile = (int)(y / Tile.size);

            //Check bounds and return tile
            if (x >= 0 && y >= 0 && x < width * Tile.size && y < height * Tile.size)
            {
                return tileArray[xtile, ytile];
            }
            else
            {
                return null;
            }
        }

        /**
        * Gets the tile at the given tile coordinates
        *
        * @param xIndex the x coordinate in tiles
        * @param yIndex the y coordinate in tiles
        */
        public Tile getTile(int xIndex, int yIndex)
        {
            //Check bounds or return the tile
            if (xIndex < 0 || xIndex >= width || yIndex < 0 || yIndex >= height) return null;
            return tileArray[xIndex, yIndex];
        }

        /**
         * Gets a list of all the actors inside the rectangle
         * 
         * @param rectangle the rectangle to consider
         * @param hint not implemented yet
         */
        public List<Actor> getActorsInRect(RectangleF rectangle, Actor hint = null)
        {
            List<Actor> nearbyActors = new List<Actor>();

            foreach (Actor a in actors)
            {
                if (RectangleF.intersects(rectangle, new RectangleF(a.position.x - a.diameter / 2, a.position.y - a.diameter / 2, a.diameter, a.diameter)))
                {
                    nearbyActors.Add(a);
                }
            }

            return nearbyActors;
        }

        /**
        * Gets a list of all the actors around a position in a radius in a direction of a certain cone width.
        *
        * @param position the origin of the cone
        * @param radius the radius of the cone
        * @param direction the direction of the cone
        * @param coneWidth Angle of cone, in degrees.
        * @param hint the actor to start the search with
        */
        public IEnumerable<Actor> getActorsInCone(Vector2 position, float radius, Vector2 direction, float coneWidth, Actor hint = null)
        {
            LinkedList<Actor> nearbyActors = new LinkedList<Actor>();

            LinkedListNode<Actor> begin, end;
            float yBegin = position.y - radius;
            float yEnd = position.y + radius;
            float xBegin = position.x - radius;
            float xEnd = position.x + radius;
            float rsquared = radius * radius;

            //use hint as starting point to search for position
            if (hint != null)
            {
                begin = actors.Find(hint).Next;
                end = begin;
                //begin = hint.node;
                //end = hint.node;
            }
            else
            {
                begin = actors.First;
                end = actors.First;
            }

            if (begin == null)
            {
                return new LinkedList<Actor>();
            }

            //Compose a list of all actors that could possibly be in the cone (broad phase)
            //This list consists of all actors within circle formed by the cone

            //Find the first and last actor that could be within the circle (considering only Y values).
            if (begin.Value.position.y > yBegin)
            {
                for (; begin.Previous != null && begin.Previous.Value.position.y - begin.Previous.Value.diameter / 2 >= yBegin; begin = begin.Previous) ;
            }
            else
            {
                for (; begin.Next != null && begin.Next.Value.position.y + begin.Next.Value.diameter / 2 <= yBegin; begin = begin.Next) ;
            }

            if (end.Value.position.y < yEnd)
            {
                for (; end != null && end.Value.position.y + end.Value.diameter / 2 <= yEnd; end = end.Next) ;
            }
            else
            {
                for (; end != null && end.Value.position.y - end.Value.diameter / 2 >= yEnd; end = end.Previous) ;
            }

            if (end != null && end.Value.position.y < begin.Value.position.y) return nearbyActors;

            //Determine which of the actors in the range are within the circle, by checking dist from center
            LinkedListNode<Actor> iter;
            for (iter = begin; iter != end; iter = iter.Next)
            {
                if (iter.Value.position.x < xBegin || iter.Value.position.x > xEnd) continue;

                float dx = iter.Value.position.x - position.x;
                float dy = iter.Value.position.y - position.y;

                //Check the radius
                if (Math.Sqrt(dx * dx + dy * dy) - iter.Value.diameter / 2 < radius)
                {
                    nearbyActors.AddFirst(iter.Value);
                }
            }

            //Determine which of the "nearby" actors are actually in the cone
            //Calculate cos and sine
            float c = (float)Math.Cos(coneWidth / 2);
            float s = (float)Math.Sin(coneWidth / 2);

            //Beginning and ending vectors for the cone
            Vector2 beginVect = new Vector2(c * direction.x + s * direction.y, -s * direction.x + c * direction.y);
            Vector2 endVect = new Vector2(c * direction.x - s * direction.y, s * direction.x + c * direction.y);

            if (coneWidth != 360)
            {
                iter = nearbyActors.First;
                while (iter != null)
                {
                    LinkedListNode<Actor> tmp = iter.Next;

                    //Remove Actor if not in Cone
                    Vector2 v = iter.Value.position - position;
                    Vector2 perp = Vector2.Normalize(new Vector2(-v.y, v.x)) * iter.Value.diameter / 2;

                    //Beginning and ending vectors for the actor's periphery (perpendicular to the line from the center to the actor's center):w
                    Vector2 beginAVect = v - perp;
                    Vector2 endAVect = v + perp;

                    //We are trying to determine whether the two begin/end pairs overlap.
                    //The easiest way to do this is to check if they *don't* overlap. This is the case,
                    //iff the end of one is before the beginning of the other.
                    //Compare the beginnings and endings
                    float crossP1 = beginVect.x * endAVect.y - endAVect.x * beginVect.y;
                    float crossP2 = beginAVect.x * endVect.y - endVect.x * beginAVect.y;

                    bool hasLOS = hasLineOfSight(position, iter.Value.position, false);

                    //If either of the beginnings comes after an ending, remove it from the nearby actors list
                    if (crossP1 > 0 || crossP2 > 0 || !hasLOS) nearbyActors.Remove(iter);

                    iter = tmp;
                }
            }

            //What's left is all the actors in the cone.
            return nearbyActors;
        }

        /**
        * Check if an area is clear of solidity
        *
        * @param rect the rectangle to consider
        */
        public bool isAreaOpen(RectangleF rect)
        {
            //TODO: expand to cover area with dimensions larger than tile.size
            Tile tile;

            tile = getTileAt(rect.left, rect.top);
            if (tile == null || tile.solidity) return false;

            tile = getTileAt(rect.left, rect.bottom);
            if (tile == null || tile.solidity) return false;

            tile = getTileAt(rect.right, rect.top);
            if (tile == null || tile.solidity) return false;

            tile = getTileAt(rect.right, rect.bottom);
            if (tile == null || tile.solidity) return false;

            return true;
        }

        /**
        * Checks whether the given ray is obstructed or not
        *
        * @param from the beginning of the ray
        * @param to the end of the ray
        * @param ignoreSolidity whether to ignore solidity when ray casting
        */
        public bool hasLineOfSight(Vector2 from, Vector2 to, bool ignoreSolidity)
        {
            //Bounds checking
            bool los = false;
            Tile fromTile = getTileAt(from);
            Tile destTile = getTileAt(to);
            if (fromTile == null || destTile == null) return los;
            
            //Cast the ray
            castRay(from, to-from, (tile) => 
                {
                    los = (tile == destTile);
                    return los || (tile.opacity);
                });

            return los;
        }

        /**
        * Accurately checks the two lines-of-sight that completely encompass the given rectangles
        *
        * @param from the first rectangle
        * @param to the second rectangle
        */
        public bool hasLineOfSight(RectangleF from, RectangleF to)
        {
            int tempBuf = 0;
            
            //If the two rectangles are aligned horizontally
            if (from.center.x == to.center.x)
            {
                return (hasLineOfSight(new Vector2(from.center.x, from.top + tempBuf), new Vector2(to.center.x, to.top + tempBuf), false)
                      && hasLineOfSight(new Vector2(from.center.x, from.bottom - tempBuf), new Vector2(to.center.x, to.bottom - tempBuf), false));
            }

            //If the two rectangles are aligned vertically
            else if (from.center.y == to.center.y)
            {
                return (hasLineOfSight(new Vector2(from.left + tempBuf, from.center.y), new Vector2(to.left + tempBuf, to.center.y), false)
                      && hasLineOfSight(new Vector2(from.right - tempBuf, from.center.y), new Vector2(to.right - tempBuf, to.center.y), false));
            }

            //If the rectangles are arranged in a positively-sloped line
            else if ((from.center.x < to.center.x && from.center.y > to.center.y) 
                || (from.center.x > to.center.x && from.center.y < to.center.y))
            {
                //(Since 0,0 is the top left corner, a positively sloped line will actually have opposite comparator checks for X and Y)
                return (hasLineOfSight(new Vector2(from.left + tempBuf, from.top + tempBuf), new Vector2(to.left + tempBuf, to.top + tempBuf), false)
                      && hasLineOfSight(new Vector2(from.right - tempBuf, from.bottom - tempBuf), new Vector2(to.right - tempBuf, to.bottom - tempBuf), false));
            }
            
            //If the rectangles are arranged in a negatively sloped line.
            else
            {
                return (hasLineOfSight(new Vector2(from.right + tempBuf, from.top + tempBuf), new Vector2(to.right + tempBuf, to.top + tempBuf), false)
                      && hasLineOfSight(new Vector2(from.left - tempBuf, from.bottom - tempBuf), new Vector2(to.right - tempBuf, to.bottom - tempBuf), false));
            }
        }

        /**
        * Sends out a cone of light that starts at position and moves out in direction with color.
        *
        * @param position  Vertex of the cone
        * @param direction Direction of the axis of the cone
        * @param coneWidth Angle of the cone, in degrees
        * @param numRays   Number of rays to shoot within the cone
        * @param color     Color of the rays to shoot
        */
        public void castLightCone(Vector2 position, Vector2 direction, float coneWidth, int numRays, Color color)
        {
            RectangleF viewRect = engine.graphicsComponent.camera.viewRect;
            if (!((position.x > viewRect.right && direction.x > 0) || (position.x < viewRect.left && direction.x < 0) ||
                (position.y < viewRect.bottom && direction.y > 0) || (position.y > viewRect.top && direction.y < 0)))
            {
                return;
            }

            castLightCone(position, direction, coneWidth, numRays, (tile) => { tile.color += color; });
        }

        /**
         * Sends out a cone of light that starts at position and moves out in direction.
         * 
         * @param position  Vertex of the cone
         * @param direction Direction of the axis of the cone
         * @param coneWidth Angle of the cone, in degrees
         * @param numRays   Number of rays to shoot within the cone
         * @param mod       The action to perform on each visited tile
         */
        public void castLightCone(Vector2 position, Vector2 direction, float coneWidth, int numRays, VisitTile mod)
        {
            if (coneWidth < 0) throw new Exception("World.castCone(): coneWidth < 0");
            if (numRays < 0)   throw new Exception("World.castCone(): numRays < 0");
            
            //Calculate cos and sine for angle of cone
            float c = (float)Math.Cos(coneWidth / 180 * Math.PI / 2);
            float s = (float)Math.Sin(coneWidth / 180 * Math.PI / 2);

            //Beginning vector for the cone
            Vector2 v = new Vector2(c * direction.x + s * direction.y, -s*direction.x + c*direction.y);

            //Recalculate c and s for angle between rays
            c = (float)Math.Cos(coneWidth / 180 * Math.PI / numRays);
            s = (float)Math.Sin(coneWidth / 180 * Math.PI / numRays);

            for (int i = 0; i < numRays; i++) 
            {
                castLightRay(position, v, mod);
                v = new Vector2(c * v.x - s * v.y, s*v.x + c*v.y);
            }
        }

        /**
        * Sends out a ray of light that starts at the given position and moves out in the given direction with color.
        *
        * @param position the starting position
        * @param direction the direction to proceed in
        * @param color the color of the light ray
        */
        private void castLightRay(Vector2 position, Vector2 direction, Color color)
        {
            RectangleF viewRect = engine.graphicsComponent.camera.viewRect;
            if (!((position.x > viewRect.right && direction.x > 0) || (position.x < viewRect.left && direction.x < 0) ||
                (position.y < viewRect.bottom && direction.y > 0) || (position.y > viewRect.top && direction.y < 0)))
            {
                return;
            }

            castRay(position, direction, (tile) => (tile.opacity), (tile) => { tile.color += color; });
        }

        /**
        * Sends out a ray that starts at the given position and moves out in the given direction.
        *
        * @param position the starting position
        * @param direction the direction to proceed in
        * @param visitTile the action to perform on each visited tile
        */
        private void castLightRay(Vector2 position, Vector2 direction, VisitTile visitTile)
        {
            RectangleF viewRect = engine.graphicsComponent.camera.viewRect;
            if (!((position.x > viewRect.right && direction.x > 0) || (position.x < viewRect.left && direction.x < 0) ||
                (position.y < viewRect.bottom && direction.y > 0) || (position.y > viewRect.top && direction.y < 0)))
            {
                return;
            }

            castRay(position, direction, (tile) => (tile.opacity), visitTile);
        }

        /**
        * Shoot an imaginary ray from a point in a direction, ending on some condition
        *
        * @param position  The starting position
        * @param direction The direction of the ray
        * @param visitTile The operation (if any) to perform on each tile
        * @param endCond   Condition when the ray should end
        *
        * @return Length of the ray
        */
        private float castRay(Vector2 position, Vector2 direction, Predicate<Tile> endCondition, VisitTile visitTile = null)
        {
            //Note: taken from http://www.cse.yorku.ca/~amana/research/grid.pdf
            if (endCondition == null)
            {
                throw new Exception("Error: An ending condition for the ray must be specified.");
            }

            //Bounds checking
            Tile curTile = getTileAt(position);
            if (curTile == null || direction == Vector2.Zero) return 0;

            int X = curTile.xIndex; //X index of current tile
            int Y = curTile.yIndex; //Y index of current tile
            int xDir = (direction.x >= 0) ? 1 : -1; //Direction of scanning along X
            int yDir = (direction.y >= 0) ? 1 : -1; //Direction of scanning along Y

            //Handle hor. and ver. cases separately
            if (direction.x == 0)
            {
                while (!endCondition(curTile))
                {
                    if (visitTile != null) visitTile(curTile);
                    Y += yDir;
                    if (Y < 0 || Y >= height) return getDistToTile(position, xDir, yDir, X, Y);
                    curTile = tileArray[X, Y];
                }

                //Apply to the last tile
                if (visitTile != null) visitTile(curTile);

                return getDistToTile(position, xDir, yDir, X, Y);
            }
            else if (direction.y == 0)
            {
                while (!endCondition(curTile))
                {
                    if (visitTile != null) visitTile(curTile);
                    X += xDir;
                    if (X < 0 || X >= width) return getDistToTile(position, xDir, yDir, X, Y);
                    curTile = tileArray[X, Y];
                }

                //Apply to the last tile
                if (visitTile != null) visitTile(curTile);

                return getDistToTile(position, xDir, yDir, X, Y);
            }

            float tMaxX = (Tile.size * (X + (xDir + 1) / 2) - position.x) / direction.x;  // t at vert tile boundary 
            float tMaxY = (Tile.size * (Y + (yDir + 1) / 2) - position.y) / direction.y;  // t at horiz tile boundary
            float tDeltaX = Math.Abs(Tile.size / direction.x);     // T required to move Tile.size in X
            float tDeltaY = Math.Abs(Tile.size / direction.y);     // T required to move Tile.size in Y

            while (!endCondition(curTile))
            {
                if (visitTile != null) visitTile(curTile);

                //Find next tile in ray
                if (tMaxX < tMaxY)
                {
                    tMaxX += tDeltaX;
                    X += xDir;
                }
                else
                {
                    tMaxY += tDeltaY;
                    Y += yDir;
                }

                if (X < 0 || X >= width || Y < 0 || Y >= height)
                {
                    return getDistToTile(position, xDir, yDir, X, Y);
                }
                curTile = tileArray[X, Y];
            }

            //Apply to the last tile
            if (visitTile != null) visitTile(curTile);

            return getDistToTile(position, xDir, yDir, X, Y);
        }

        /**
         * Returns the distance from a point along a ray to the edge of a tile
         *
         * @param position Beginning of the ray
         * @param xDir    -1 if decreasing, 1 if increasing
         * @param yDir    -1 if decreasing, 1 if increasing
         * @param xIndex   X index of end tile
         * @param yIndex   X index of end tile
         */
        private float getDistToTile(Vector2 position, int xDir, int yDir, int xIndex, int yIndex)
        {
            //A bit of funky math helps us to find the appropriate edge of the tile without a conditional. (1-step)/2 will be 0 if increasing, 1 if decreasing.
            Vector2 end = new Vector2(Tile.size * ((1 - xDir) / 2 + xIndex), Tile.size * ((1 - yDir) / 2 + yIndex));
            return (end - position).getLength();
        }
    }
}