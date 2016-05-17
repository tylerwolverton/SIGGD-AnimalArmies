using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Engine;
namespace Game.AI
{
	public class ComputerPlayer : Player
	{
		LinkedList<AI.Platoon> platoons;

		LinkedList<AnimalActor> oldUnitList;

		private Thread turnThread;
		private bool running;

		private bool finished = false;

		public ComputerPlayer(GameWorld world, team_t team)
			: base(world, team)
		{
			// For right now, we'll group all our units into one platoon
			// Typically, you would create new, smaller, mutually exclusive lists,
			// and feed one to each platoon
			Platoon dfltPlat = new Platoon(this.world, this.team);
			//Order dfltOrder = new HoldPositionOrder(dfltPlat, null, 5);
			//Order dfltOrder = new SurroundOrder(dfltPlat);
			Order dfltOrder = new SurroundOrder2(dfltPlat);
            //Order dfltOrder = new AttackOrder(dfltPlat, null);
			dfltPlat.setOrder(dfltOrder);
			platoons = new LinkedList<Platoon>();
			platoons.AddLast(dfltPlat);
			turnThread = null;

			saveUnitList();
		}

		private void saveUnitList()
		{
			oldUnitList = new LinkedList<AnimalActor>();
			foreach (AnimalActor act in units)
			{
				oldUnitList.AddLast(act);
			}
		}

		private LinkedList<AnimalActor> extractNewUnits()
		{
			LinkedList<AnimalActor> newUnits = new LinkedList<AnimalActor>();
			foreach (AnimalActor act in units)
			{
				if (!oldUnitList.Contains(act))
				{
					newUnits.AddLast(act);
				}
			}
			Console.WriteLine();
			return newUnits;
		}

		private void updateUnits()
		{
			LinkedList<AnimalActor> newUnits = extractNewUnits();
			foreach (AnimalActor act in newUnits)
			{
				platoons.First.Value.addUnit(act);
			}

			foreach (Platoon p in platoons)
			{
				p.pruneUnits();
			}
		}
		
		public override void startTurn()
		{
			finished = false;
			updateUnits();
			foreach (Platoon platoon in platoons)
			{
				platoon.takeTurn();
			}

			saveUnitList();

			foreach (AnimalActor actor in units)
			{
				updateStatusEffects(actor);
			}

			finished = true;
		}

		public override void Update()
		{
			if (finished)
				world.endTurn();
		}
	}
}