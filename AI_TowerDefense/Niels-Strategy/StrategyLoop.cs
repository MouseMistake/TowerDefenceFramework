using GameFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Strategy {
    internal class StrategyLoop : AbstractStrategy {
        /* Keeping logs */
        private int messageCounter = 1;
        /* Keeping a random generator if we need one */
        private static Random random = new Random();

        /* Constructor */
        public StrategyLoop(Player player) : base(player) { }

        /* Called by game loop to deploy Soldiers */
        public override void DeploySoldiers() {
            DebugLogger.Log("#" + messageCounter + " Soldier deployed.");
            messageCounter++;

            throw new NotImplementedException();

        }

        /* Called by game loop to deploy Towers */
        public override void DeployTowers() {
            DebugLogger.Log("#" + messageCounter + " Tower deployed.");
            messageCounter++;

            throw new NotImplementedException();

        }

        public override List<Soldier> SortedSoldierArray(List<Soldier> unsortedList) {
            throw new NotImplementedException();

        }

        public override List<Tower> SortedTowerArray(List<Tower> unsortedList) {
            throw new NotImplementedException();


        }


    }

    
}
