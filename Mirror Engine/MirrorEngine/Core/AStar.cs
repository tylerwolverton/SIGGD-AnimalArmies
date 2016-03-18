/**
 * @file
 * @author PURDUE ACM, SIGGD <siggd.purdue@gmail.com>
 * @version 1.0
 * 
 * @section LICENSE
 * 
 * @section DESCRIPTION
 */

using System;
using System.Collections.Generic;

namespace Engine
{
    /// brief class desc here
    /**
    * classdeschere
    */
    public class AStar
    {

        public const int MAXCONSIDERED = 500; ///< The pathfinder will consider at most this many tiles to try to get to its destination
        
        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        public static Vector2 findPath(World world, RectangleF fromActorBounds, RectangleF toActorBounds)
        {
            int nCons = 0;  // Number of tiles considered so far
            Vector2 fromPos = new Vector2(fromActorBounds.center.x, fromActorBounds.center.y);  // Position of the source
            Vector2 toPos = new Vector2(toActorBounds.center.x, toActorBounds.center.y);        // Position of the dest

            if (world.hasLineOfSight(fromActorBounds, toActorBounds))
            {
                world.getTileAt(toPos).MAKERED = false;
                return Vector2.Zero;
            }

            Tile from = world.getTileAt(fromPos);
            Tile to = world.getTileAt(toPos);

            Heap<Tile> openPriorityQueue = new Heap<Tile>((a, b) => (a.fScore.CompareTo(b.fScore)));
            HashSet<Tile> openSet = new HashSet<Tile>();
            HashSet<Tile> closedSet = new HashSet<Tile>();

            from.previous = null;
            from.gScore = 0;
            from.fScore = from.gScore + from.heuristic(to);

            from.openPQNode = openPriorityQueue.insert(from);
            openSet.Add(from);

            while (!openPriorityQueue.isEmpty())
            {
                // Limiting of number of tiles considered, for performance reasons
                if (nCons++ >= MAXCONSIDERED)
                    return fromPos;

                Tile curNode = openPriorityQueue.deleteMin();
                curNode.openPQNode = null;
                openSet.Remove(curNode);
                closedSet.Add(curNode);

                if (curNode == to)
                {

                    Tile iter = to;

                    while (iter != null)
                    {
                        RectangleF iterrect = new RectangleF(iter.x + 1, iter.y + 1, Tile.size - 1, Tile.size - 1);

                        if (world.hasLineOfSight(fromActorBounds, iterrect))
                        {
                            iter.MAKERED = false;
                            return new Vector2(iterrect.center.x, iterrect.center.y);
                        }

                        iter = iter.previous;
                    }

                    return fromPos;
                }

                foreach (Tile adjNode in curNode.adjacent)
                {

                    if (adjNode == null || adjNode.solidity) continue;
                    if (closedSet.Contains(adjNode)) continue;

                    float tmpGScore = curNode.gScore + 1;

                    if (!openSet.Contains(adjNode))
                    {

                        adjNode.previous = curNode;
                        adjNode.gScore = tmpGScore;
                        adjNode.fScore = adjNode.gScore + adjNode.heuristic(to);

                        adjNode.openPQNode = openPriorityQueue.insert(adjNode);
                        openSet.Add(adjNode);
                    }
                    else if (tmpGScore < adjNode.gScore)
                    {

                        adjNode.previous = curNode;
                        adjNode.gScore = tmpGScore;
                        adjNode.fScore = adjNode.gScore + adjNode.heuristic(to);

                        openPriorityQueue.decreaseKey(adjNode.openPQNode);
                    }
                }
            }

            return fromPos;
        }
    }
}