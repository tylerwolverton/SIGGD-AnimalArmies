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
			Order dfltOrder = new SurroundOrder(dfltPlat);
			//Order dfltOrder = new SurroundOrder2(dfltPlat);
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

		/* Naive attempt at multi-threading.
		 * Turns out, ME is not safe, and this is going to take ~10-15 hours of dev time.
		 * Probably a won't fix.
		public override void startTurn()
		{
			running = true;
			turnThread = new Thread(new ThreadStart(runTurn));
			turnThread.Start();
		}

		private void runTurn()
		{
			updateUnits();
			foreach (Platoon platoon in platoons)
			{
				platoon.takeTurn();
			}

			saveUnitList();
			running = false;
		}

		public override void Update()
		{
			if (running)
			{
				return;
			}

			if (turnThread != null)
			{
				turnThread.Join();
				turnThread = null;
			}
			world.endTurn();
		}
		 * */

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


		/*getGameImage (suite of functions) - snapshot of game: contains all tiles (terrain), and location of actors (differentiate between teams)
		moveActor - take actor object and where it should move
		attack
		wait
		information about rules (tentative)
		put all this stuff in AnimalActor above
		in player below
		initialization
		takeTurn function
		tell AI it's done with the current game
		notifications about units (death, damage)*/
	}
}