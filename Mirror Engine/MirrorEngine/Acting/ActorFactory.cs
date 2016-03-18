/**
 * @file ActorFactory.cs
 * @author SIGGD, PURDUE ACM <siggd.purdue@gmail.com>
 * @version 2.0
 */

using System;
using System.Collections.Generic;

namespace Engine
{
    //Spawns actors on the world, given the actor's id and initial state
    public abstract class ActorFactory
    {

        public Dictionary<string, int> names; //A dictionary of names to ids, used to identify actors from map data.

        /**
        * Initializes and spawns an actor into the world
        *
        * @param id the mapdata id of the actor to spawn
        * @param position the spawn location
        * @param velocity the initial velocity of the actor
        * @param color the initial color of the actor
        *
        * @return the actor that was spawned
        */
        public abstract Actor createActor(int id, Vector2 position, Vector2 velocity = null, double color = -1);

        /**
        * Gets the id corresponding to an actor name
        *
        * @param name the actor name
        *
        * @return the id corresponding to the name
        */
        public int getActorId(string name)
        {
            return names[name];
        }
    }
}