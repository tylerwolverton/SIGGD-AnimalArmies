using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;
using Game;

namespace Game
{
    public class Llama: AnimalActor
    {
        public Llama(GameWorld world, Vector2 position, Vector2 velocity, float diameter, Vector2 size, Vector2 world2model, int imgIndex)
            : base(world, new Vector2(position.x - (position.x % 16), position.y - (position.y % 16)), velocity, diameter, size, world2model, imgIndex)
        {
            this.actorName = "Llama";
            // Attack damage and defense normally and when poisoned
            this.attackDamage = randomizeStat(6);
            this.defense = randomizeStat(2);
            this.baseAttack = attackDamage;
            this.baseDefense = defense;
            this.poisonAttack = attackDamage - 2;
            this.poisonDefense = defense - 2;

            this.moveRange = 4;
            this.attackRange = 3;
            this.criticalRange = .8;
            this.spawnCost = 2;
            
            anim = new Animation(world.engine.resourceComponent, "Sprites/010_llama/llama_blue");
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
                    anim = new Animation(world.engine.resourceComponent, "Sprites/010_llama/llama_blue");
                }
                else
                {
                    anim = new Animation(world.engine.resourceComponent, "Sprites/010_llama/llama_blue/down");
                }
            }
            else if (this.team == team_t.Purple)
            {
                if (isGrey)
                {
                    anim = new Animation(world.engine.resourceComponent, "Sprites/010_llama/llama_purple");
                }
                else
                {
                    anim = new Animation(world.engine.resourceComponent, "Sprites/010_llama/llama_purple/down");
                }
            }
            else if (this.team == team_t.Red)
            {
                if (isGrey)
                {
                    anim = new Animation(world.engine.resourceComponent, "Sprites/010_llama/llama_red");
                }
                else
                {
                    anim = new Animation(world.engine.resourceComponent, "Sprites/010_llama/llama_red/down");
                }
            }
            else if (this.team == team_t.Yellow)
            {
                if (isGrey)
                {
                    anim = new Animation(world.engine.resourceComponent, "Sprites/010_llama/llama_yellow");
                }
                else
                {
                    anim = new Animation(world.engine.resourceComponent, "Sprites/010_llama/llama_yellow/down");
                }
            }
        }
    }
}
