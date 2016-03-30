using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;
namespace Game
{
	class Base : GameActor
	{
		team_t team;
		public Base(GameWorld world, Vector2 position, Vector2 velocity, float diameter, Vector2 size, Vector2 world2model, int imgIndex, team_t color, Color teamColor, int maxlife = 20 )
			: base(world, new Vector2(position.x - (position.x % 32),position.y - (position.y % 32)), velocity,diameter, size, world2model, imgIndex, maxlife)
		{
			team = color;
			myBehavior = new BaseBehavior(world, this,team, teamColor);
            if (team == team_t.Red)
            {
                this.actorName = "Red Base";
                this.anim = new Animation(world.engine.resourceComponent, "Sprites/003_redBase/");
            }
            else if(team == team_t.Blue)
            {
                this.anim = new Animation(world.engine.resourceComponent, "Sprites/004_blueBase/");
                this.actorName = "Blue Base";
            }
            else if (team == team_t.Yellow)
            {
                this.anim = new Animation(world.engine.resourceComponent, "Sprites/005_yellowBase/");
                this.actorName = "Yellow Base";
            }
            else if (team == team_t.Purple)
            {
                this.anim = new Animation(world.engine.resourceComponent, "Sprites/006_purpleBase/");
                this.actorName = "Purple Base";
            }
            
            position = new Vector2(position.x - (position.x % 32), position.y - (position.y % 32));	
		}
	}
}
