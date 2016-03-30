using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;
namespace Game
{
	public abstract class Player
	{
		protected GameWorld world;

        // Note: units is just a reference to the list of units maintained by the GameWorld.  We don't have any control over it.
		protected readonly LinkedList<AnimalActor> units;
        public team_t team { get; protected set; }

		public Player(GameWorld world, team_t team)
		{
			this.world = world;
            this.team = team;
            this.units = TeamDictionary.TeamDict[team].ActorList;
		}

		//Called at beginning of turn, call world.endTurn() once turn is done.
		public abstract void startTurn();

		//Called every Tick by gameworld when turn is active.
		public abstract void Update();

        public void updateStatusEffects(AnimalActor actor)
        {
            if (actor.isPoisoned)
            {
                if (actor.poisonCount == 0)
                {
                    actor.attackDamage = actor.baseAttack;
                    actor.defense = actor.baseDefense;
                    actor.isPoisoned = false;
                    return;
                }

                actor.poisonCount--;
            }
        }

	}
}
