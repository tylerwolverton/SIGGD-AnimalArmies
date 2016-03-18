/**
 * @file Life.cs
 * @author SIGGD, PURDUE ACM <siggd.purdue@gmail.com>
 * @version 2.0
 */

using System;
using System.Collections.Generic;

namespace Engine
{
    //An interface for adding life to an actor
    public interface ILife
    {
        Life life { get; }
    }

    //Provides functionality for having health, getting hurt, and dying to an actor
    public class Life
    {
        Actor actor;            //The actor
        public float maxlife;   //The most health the actor can have
        public bool isGod;      //Whether the actor can take damage

        public delegate void DeathEventHandler();               //Delegate for handling the death of the actor
        public delegate void LifeChangeHandler(float oldLife);  //Delegate for handling damage to the actor
        public event DeathEventHandler deathEvent;              //Event for handling death
        public event LifeChangeHandler lifeChangingEvent;       //Event for handling damage

        private float _health; //The health of the actor
        public float health
        {
            get { return _health; }

            set
            {
                if (isGod && value < _health)
                    return;

                float oldLife = _health;

                _health = value;

                if (_health < 0)
                    _health = 0;
                if (_health > maxlife)
                    _health = maxlife;

                if (_health != oldLife)
                {
                    if (lifeChangingEvent != null)
                    {
                        lifeChangingEvent(oldLife);
                    }

                    if (dead)
                    {
                        actor.active = false;

                        if (deathEvent != null)
                        {
                            deathEvent();
                        }
                    }
                }
            }
        }

        //Determines whether the actor has enough health to live
        public bool dead
        {
            get { return health <= 0f; }
        }

        /**
        * Creates a life for an actor
        *
        * @param a the actor that this life belongs to
        * @param maxlife the maxlife of the actor
        */
        public Life(Actor a, int maxlife)
        {
            this.actor = a;
            this.health = this.maxlife = maxlife;
        }

        /**
        * Performs damage to the actor
        *
        * @param taget the actor to damage
        * @param damage the amount of health to subtract
        */
        static public void damage(Actor target, int damage)
        {
            if (target == null) return;
            ILife targetLife = target as ILife;
            if (targetLife == null || targetLife.life.dead) return;
            
            if (!targetLife.life.isGod)
            {
                targetLife.life.health -= damage;
            }
        }
    }
}