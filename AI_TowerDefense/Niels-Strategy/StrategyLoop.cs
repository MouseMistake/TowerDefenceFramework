using AI_TowerDefense;
using GameFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Strategy {
    internal class StrategyLoop : AbstractStrategy {
        /* Keeping a random generator if we need one */
        private static Random random = new Random();

        private enum PlayerState {
            AGGRESSIVE,
            DEFENSIVE,
            NEUTRAL,

        }
        /* General Memory Values */
        private int towerCounter = 0;
        private int soldierCounter = 0;
        private bool isDefenseUp = false;

        /* Constructor */
        public StrategyLoop(Player player) : base(player) { }

        /* Called by game loop to deploy Soldiers */
        public override void DeploySoldiers() {
            DebugLogger.Log(player.EnemyLane.SoldierCount());
            if (player.EnemyLane.SoldierCount() > 10) return;

            int attempt = 0;
            while (player.Gold > 5 && attempt < 10) {
                attempt++;
                int x = random.Next(PlayerLane.WIDTH);
                int y = 0;

                if (player.EnemyLane.GetCellAt(x, y).Unit == null) {
                    var trybuy = player.TryBuySoldier<NielsSoldier>(x);

                    if (trybuy == Player.SoldierPlacementResult.Success) {
                        soldierCounter++;
                        DebugLogger.Log($"(P{player.Name}) S #{soldierCounter} deployed a-t {attempt}.");

                    }

                }

            }

        }

        /* Called by game loop to deploy Towers */
        public override void DeployTowers() {
            if (player.HomeLane.TowerCount() > 10) return;

            int attempt = 0;
            if (player.Gold > 8 && attempt < 10) {
                attempt++;

                int x = random.Next(PlayerLane.WIDTH);
                int y = random.Next(PlayerLane.HEIGHT - 1) + 1;
                if (player.HomeLane.GetCellAt(x, y).Unit == null) {
                    var trybuy = player.TryBuyTower<Tower>(x, y);

                    if (trybuy == Player.TowerPlacementResult.Success) {
                        towerCounter++;
                        DebugLogger.Log($"(P{player.Name}) T #{towerCounter} deployed a-t {attempt}.");

                    }

                }

            }

        }

        public override List<Soldier> SortedSoldierArray(List<Soldier> unsortedList) {
            return unsortedList;

        }

        public override List<Tower> SortedTowerArray(List<Tower> unsortedList) {
            return unsortedList;

        }


    }

    
}
