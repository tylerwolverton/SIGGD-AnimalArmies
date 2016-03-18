using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;
using Game;

namespace Game
{
    public class Turtle: AnimalActor
    {
        public Turtle(GameWorld world, Vector2 position, Vector2 velocity, float diameter, Vector2 size, Vector2 world2model, int imgIndex)
            : base(world, new Vector2(position.x - (position.x % 16), position.y - (position.y % 16)), velocity, diameter, size, world2model, imgIndex)
        {
            this.actorName = "Turtle";
            // Attack damage and defense normally and when poisoned
            this.attackDamage = randomizeStat(4);
            this.defense = randomizeStat(5);
            this.baseAttack = attackDamage;
            this.baseDefense = defense;
            this.poisonAttack = attackDamage - 2;
            this.poisonDefense = defense - 2;

            this.moveRange = 3;
            this.attackRange = 1;
            this.criticalRange = .8;
            this.spawnCost = 3;

			anim = new Animation(world.engine.resourceComponent, "Sprites/002_turtle/turtle_pirate/");
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
                    anim = new Animation(world.engine.resourceComponent, "Sprites/002_turtle/turtle_blue");
                }
                else
                {
                    anim = new Animation(world.engine.resourceComponent, "Sprites/002_turtle/turtle_blue/down");
                }
            }
            else if (this.team == team_t.Purple)
            {
                if (isGrey)
                {
                    anim = new Animation(world.engine.resourceComponent, "Sprites/002_turtle/turtle_purple");
                }
                else
                {
                    anim = new Animation(world.engine.resourceComponent, "Sprites/002_turtle/turtle_purple/down");
                }
            }
            else if (this.team == team_t.Red)
            {
                if (isGrey)
                {
                    anim = new Animation(world.engine.resourceComponent, "Sprites/002_turtle/turtle_red", 0, 0, 6, true, 3);
                }
                else
                {
                    anim = new Animation(world.engine.resourceComponent, "Sprites/002_turtle/turtle_red/down");//, 0, 0, 6, true, 3);
                }
            }
            else if (this.team == team_t.Yellow)
            {
                if (isGrey)
                {
                    anim = new Animation(world.engine.resourceComponent, "Sprites/002_turtle/turtle_yellow");
                }
                else
                {
                    anim = new Animation(world.engine.resourceComponent, "Sprites/002_turtle/turtle_yellow/down");
                }
            }
        }
		
    }
}
