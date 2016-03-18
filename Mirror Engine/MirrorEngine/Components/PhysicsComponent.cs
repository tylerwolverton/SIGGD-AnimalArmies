/**
 * @file PhysicsComponent.cs
 * @author SIGGD, Purdue ACM <siggd.purdue@gmail.com>
 * @version 2.0
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Engine
{
    /*
     * Defines physics operations called by the game loop and--in some cases--the world and actors themselves. 
     * Can be thought of as a set of constraints under which the world operates and actors behave under.
     */
    public class PhysicsComponent : Component
    {
        protected const float BUFFER = 0.01f; //Buffer used to guarantee an actor is always in valid space
        protected const float UNKNOWNVALUE = -1f; //Represents an unknown value to solve for

        public PhysicsComponent(MirrorEngine theEngine) : base(theEngine) { }

        /**
        * Performs collision detection and resolution for all of the world's actors (AvA and AvE)
        */
        public virtual void Update()
        {
            if (engine.world == null) return;

            //AvA for each actor. 
            //Should probably choose both actors to perform on, 
            //to avoid calling AvA multiple times on the same pair.
            for (LinkedListNode<Actor> a = engine.world.actors.First; a != null; a = a.Next)
            {
                actorVsActor(a);
            }

            //AvE for each actor.
            foreach (Actor a in engine.world.actors)
            {
                if (a.ignoreAvE) continue;
                verletWithEnvironment(a);
            }
        }

        /*
         * Resolves actor vs actor collisions.
         */
        public static void avaResolve(Actor a, Actor b)
        {
            Vector2 vect1 = a.position - b.position;
            Vector2 vect2 = b.position - a.position;
            a.force += vect1 * 2000; //Shouldn't have magic numbers.
        }

        /*
         * Pushes two actors away at a certain magnitude.
         */
        public static void pushAway(Actor a, Actor b, float mag)
        {
            Vector2 vect1 = a.position - b.position;
            Vector2 vect2 = b.position - a.position;
            a.force += vect1 * 2000 * mag; //Shouldn't have magic numbers.
        }

        /*
        * Detects collisions between all actors in the world, but doesn't apply resolution itself.
        *
        * @param actors linkedlist of actors to check
        */
        private void actorVsActor(LinkedListNode<Actor> actors)
        {
            Actor actor = actors.Value;
            World world = actor.world;
            LinkedListNode<Actor> end;

            float bound = actor.diameter / 2 + world.maxActorRadius;
            float yEnd = actor.position.y + bound;

            //Find begin and end of possible collisions and record them in yBegin and yEnd
            for (end = actors; end != null && end.Value.position.y <= yEnd; end = end.Next) ;

            //Check against possible collisions
            for (LinkedListNode<Actor> iter = actors.Next; iter != end; iter = iter.Next)
            {

                Actor target = iter.Value;
                if(iter != actors)
                {
                    //These are rectangles where the actors are to determine if there is a collison or not
                    RectangleF actorRECT = new RectangleF(actor.position.x, actor.position.y, actor.size.x, actor.size.y);
                    RectangleF targetRECT = new RectangleF(target.position.x, target.position.y, target.size.x, target.size.y);

                    //float deltax = (actor.position.X - target.position.X);
                    //float deltay = (actor.position.Y - target.position.Y);
                    //float distsquared = deltax * deltax + deltay * deltay;

                    //Used to shake out perfect overlaps
                    if (RectangleF.intersects(actorRECT, targetRECT))
                    {
                        actor.collide(target);
                        target.collide(actor);
                    }
                }
            }
        }

        /**
         * Solves the projection angle for either an unknown X or unknown Y
         * 
         * @return The value of the unknown 
         */
        protected float getProjAtAxis(Vector2 orig, Vector2 proj, float X, float Y)
        {
            if (X != UNKNOWNVALUE && Y != UNKNOWNVALUE)
            {
                throw new Exception("Failed Sanity Check: Can't solve for no unknowns.");
            }

            //Solve for X
            if (X == UNKNOWNVALUE)
            {  
                return proj.x * (Y - orig.y) / proj.y + orig.x;
            }
            else //Solve for Y
            {       
                return proj.y * (X - orig.x) / proj.x + orig.y;
            }
        }

        /**
         * Checks if an edge is collided, and returns the tile collided with
         *
         * @param corner   The point that begins the edge 
         * @param edge     The vector that forms the edge
         * @param clipList Return: List of tiles collided with
         * @return         The clipped projection vector
         */
        protected Vector2 checkEdge(Vector2 corner, Vector2 edge, Vector2 proj, Actor actor, List<Tile> clipList)
        {
            if (corner == Vector2.Zero || edge == Vector2.Zero) return proj;
            
            //Add collision box draw for debugging
#if ENGINEDEBUG
            actor.customDraw += (a) => 
            {
                a.world.engine.graphicsComponent.drawLines(Color.RED, 1f, corner, corner + edge);
            };
#endif

            World w = engine.world;

            //Horizontal edge
            if (edge.y == 0) 
            {  
                //Are we moving up or down
                int dir = (proj.y > 0) ? 1 : -1;  

                //Offset to add to tile.Y to get colliding edge
                float tileYOffset = (proj.y > 0) ? 0 : Tile.size;  

                //Check a rectangular block of tiles that contains the trapezoid of the edge's movement
                int xBegin = (int)(Math.Min(corner.x, (corner + proj).x) / Tile.size);
                int xEnd = (int)(Math.Max((corner + edge).x, (corner + edge + proj).x) / Tile.size);
                int yBegin = (int)(corner.y / Tile.size);
                int yEnd = (int)((corner + proj).y / Tile.size);

                for (int j = yBegin; j != yEnd + dir; j += dir) 
                {
                    Tile clipTile = null;
                    for (int i = xBegin; i <= xEnd; i++) 
                    {
                        Tile t = w.getTile(i, j);
                        if (t != null && t.solidity) 
                        {
                            // Get the position of the projected edge aligned with tile's edge
                            float xLineBegin = getProjAtAxis(corner, proj, UNKNOWNVALUE, t.y + tileYOffset);

                            // If the tile's edge and the projected edge overlap (invert the non-overlap condition, which is end of one is before beginning of the other) (conservative: better to collide than to not)
                            if (!(xLineBegin + actor.size.x < t.x || t.x + Tile.size < xLineBegin)) 
                            {
                                // Record tile as clip tile
                                if (clipTile == null) clipTile = t;
                                clipList.Add(t);
                            }
                        }
                    }
                    
                    if (clipTile != null) 
                    {
                        return new Vector2(proj.x, clipTile.y + tileYOffset - dir * BUFFER - corner.y);
                    }
                }
            } 
            else if (edge.x == 0) //Vertical edge
            {
                //Are we moving left or right
                int dir = (proj.x > 0) ? 1 : -1; 

                //Offset to add to tile.X to get colliding edge
                float tileXOffset = (proj.x > 0) ? 0 : Tile.size; 

                //Check a rectangular block of tiles that contains the trapezoid of the edge's movement
                int yBegin = (int)(Math.Min(corner.y, (corner + proj).y) / Tile.size);
                int yEnd = (int)(Math.Max((corner + edge).y, (corner + edge + proj).y) / Tile.size);
                int xBegin = (int)(corner.x / Tile.size);
                int xEnd = (int)((corner + proj).x / Tile.size);

                for (int j = xBegin; j != xEnd + dir; j += dir) 
                {
                    Tile clipTile = null;
                    for (int i = yBegin; i <= yEnd; i++) 
                    {
                        Tile t = w.getTile(j, i);
                        if (t != null && t.solidity) 
                        {
                            // Get the position of the projected edge aligned with tile's edge
                            float yLineBegin = getProjAtAxis(corner, proj, t.x + tileXOffset, UNKNOWNVALUE);

                            //If the tile's edge and the projected edge overlap (invert the non-overlap condition, which is end of one is before beginning of the other) (conservative: better to collide than to not)
                            if (!(yLineBegin + actor.size.y < t.y || t.y + Tile.size < yLineBegin)) 
                            {
                                // Record tile as clip tile
                                if (clipTile == null) clipTile = t;
                                clipList.Add(t);
                            }
                        }
                    }

                    //Clip to the edge
                    if (clipTile != null) 
                    {
                        return new Vector2(clipTile.x + tileXOffset - dir * BUFFER - corner.x, proj.y);
                    }
                }
            } 
            else 
            {
                throw new Exception("Sanity check failed: Edge neither hor or ver");
            }

            return proj;
        }
         

        /*
        * Detects and resolves collision between actors and environment.
        *
        * @param actor the actor to check
        */
        private void verletWithEnvironment(Actor actor)
        {
            if (actor.size.x == 0 || actor.size.y == 0) 
            {
                throw new Exception("Error: Actor cannot have zero width or height.");
            }

            //For more insight into these equations, view: http://graphics.cs.cmu.edu/nsp/course/15-869/2006/papers/jakobsen.htm
            
            //To store pos before update
            Vector2 tmpPos = actor.position;  

            //Determine ideal new position
            Vector2 aFriction = actor.frictionCoefficient * actor.fNormal * Vector2.Normalize(actor.velocity) * MirrorEngine.MINTICK * MirrorEngine.MINTICK / 1000 / 1000 / actor.mass;
            Vector2 netaccel = actor.force * MirrorEngine.MINTICK * MirrorEngine.MINTICK / 1000 / 1000 / actor.mass;

            //If friction would increase speed, grind the actor to a halt
            if ((actor.velocity + netaccel - aFriction).getLengthSquared() 
                    > (actor.velocity + netaccel).getLengthSquared()) 
            {
                aFriction = netaccel + actor.velocity;
            }

            Vector2 proj = actor.velocity + netaccel - aFriction;

            //Clip to surfaces
            //Assumption: Current position is completely valid.
            //Goal: Clip new position to be valid.
            //Idea: The line that forms the edge moves along the same path as the adjacent diagonal
            World w = actor.world;
            Vector2 cornerHor, cornerVer;
            Vector2 edgeHor, edgeVer;

            // Setup horizontal edge and corner
            edgeHor = new Vector2(actor.size.x, 0);
            cornerHor = Vector2.Zero;
            if (proj.y > 0) 
            {
                cornerHor = new Vector2(actor.position.x - actor.size.x / 2, actor.position.y + actor.size.y / 2);
            }
            else if (proj.y < 0) 
            {
                cornerHor = new Vector2(actor.position.x - actor.size.x / 2, actor.position.y - actor.size.y / 2);
            }

            // Setup vertical edge and corner
            edgeVer = new Vector2(0, actor.size.y);
            cornerVer = Vector2.Zero;
            if (proj.x > 0) 
            {
                cornerVer = new Vector2(actor.position.x + actor.size.x / 2, actor.position.y - actor.size.y / 2);
            }
            else if (proj.x < 0) 
            {
                cornerVer = new Vector2(actor.position.x - actor.size.x / 2, actor.position.y - actor.size.y / 2);
            }

            //Reset actor's collision box draw if debugging
#if ENGINEDEBUG
            //Reset custom draw
            actor.customDraw = null;
#endif

            //Project in both orders (one order may be incorrect)
            Vector2 proj1;
            List<Tile> horTiles1 = new List<Tile>();
            List<Tile> verTiles1 = new List<Tile>();

            //Check horizontal edge
            proj1 = checkEdge(cornerHor, edgeHor, proj, actor, horTiles1);

            //Check vertical edge
            proj1 = checkEdge(cornerVer, edgeVer, proj1, actor, verTiles1);

            Vector2 proj2;
            List<Tile> horTiles2 = new List<Tile>();
            List<Tile> verTiles2 = new List<Tile>();

            //Check verical edge
            proj2 = checkEdge(cornerVer, edgeVer, proj, actor, horTiles2);

            //Check horizontal edge
            proj2 = checkEdge(cornerHor, edgeHor, proj2, actor, verTiles2);

            //Choose better project (the longer one)
            List<Tile> horTiles;
            List<Tile> verTiles;
            if (proj1.getLengthSquared() > proj2.getLengthSquared()) 
            {
                proj = proj1;
                horTiles = horTiles1;
                verTiles = verTiles1;
            } 
            else 
            {
                proj = proj2;
                horTiles = horTiles2;
                verTiles = verTiles2;
            }

            //Set clipped tiles list
            if (proj.x > 0) actor.rightClipTiles = horTiles;
            else actor.leftClipTiles = horTiles;
            
            //Set clipped tiles list
            if (proj.y > 0) actor.botClipTiles = verTiles;
            else actor.topClipTiles = verTiles;

            //Apply desired movement, after clipping
            actor.position += proj;
            actor.lastPos = tmpPos;

            //Remove forces from last frame
            actor.force = Vector2.Zero;
        }
    }
}
