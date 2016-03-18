﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;
using Game;

namespace Game
{
    public class Rat: AnimalActor
    {
        public Rat(GameWorld world, Vector2 position, Vector2 velocity, float diameter, Vector2 size, Vector2 world2model, int imgIndex)
            : base(world, new Vector2(position.x - (position.x % 16), position.y - (position.y % 16)), velocity, diameter, size, world2model, imgIndex)
        {
            this.actorName = "Rat";
            // Attack damage and defense normally and when poisoned
            this.attackDamage = randomizeStat(2);
            this.defense = randomizeStat(1);
            this.baseAttack = attackDamage;
            this.baseDefense = defense;
            this.poisonAttack = attackDamage - 2;
            this.poisonDefense = defense - 2;

            this.moveRange = 5;
            this.attackRange = 1;
            this.criticalRange = 0;
            this.spawnCost = 0;

            anim = new Animation(world.engine.resourceComponent, "Sprites/000_rat/rat_red/down");//, 0, 0, 6, true, 10);
        }

        public void poison(AnimalActor victim)
        {
            victim.isPoisoned = true;
        }

        public override void collide(Actor a)
        {
            
        }
        public override void changeTeam(team_t newTeam)
        {
            team = newTeam;
            changeActorSprite(false);
            base.changeTeam(newTeam);
        }

        public override void changeActorSprite(bool isGrey)
        {
            if (this.team == team_t.Blue)
            {
                if (isGrey)
                {
                    anim = new Animation(world.engine.resourceComponent, "Sprites/000_rat/rat_blue");
                }
                else
                {
                    anim = new Animation(world.engine.resourceComponent, "Sprites/000_rat/rat_blue/down");
                }
            }
            else if (this.team == team_t.Purple)
            {
                if (isGrey)
                {
                    anim = new Animation(world.engine.resourceComponent, "Sprites/000_rat/rat_purple");
                }
                else
                {
                    anim = new Animation(world.engine.resourceComponent, "Sprites/000_rat/rat_purple/down");
                }
            }
            else if (this.team == team_t.Red)
            {
                if (isGrey)
                {
                    anim = new Animation(world.engine.resourceComponent, "Sprites/000_rat/rat_red");
                }
                else
                {
                    anim = new Animation(world.engine.resourceComponent, "Sprites/000_rat/rat_red/down");
                }
            }
            else if (this.team == team_t.Yellow)
            {
                if (isGrey)
                {
                    anim = new Animation(world.engine.resourceComponent, "Sprites/000_rat/rat_yellow");
                }
                else
                {
                    anim = new Animation(world.engine.resourceComponent, "Sprites/000_rat/rat_yellow/down");
                }
            }
        }
    }
}
