using AI_TowerDefense;
using GameFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Strategy {
    internal class StrategyLoop : AbstractStrategy {
        /* Keeping a random generator because we can have some fun with it */
        private static Random random = new Random();

        /* General Memory Values */
        private int towerCounter = 0;
        private int soldierCounter = 0;
        private int numberOfTimesSpawnedAtZero = 0;

        /* Constructor */
        public StrategyLoop(Player player) : base(player) { }

        /* Called by game loop to deploy Soldiers */
        public override void DeploySoldiers() {
            int attempt = 0;
            while (player.Gold > 5 && attempt < 10) {
                attempt++;

                /* The randomizer is only there so we can have some value if there are towers on every lane */
                int x = random.Next(PlayerLane.WIDTH);
                int y = 0;

                for (int i = 0;  i < PlayerLane.WIDTH; i++) {
                    if(!CheckLaneForTower(i, y)) {
                        x = i;
                        break;

                    }

                }
                /* Let us be unpredictable if we spawn at 0 too long */
                if(numberOfTimesSpawnedAtZero > 4) x = random.Next(PlayerLane.WIDTH);

                /* We keep track of this in case we spawn too often at 0, just to spice things up then. */
                if (x == 0) numberOfTimesSpawnedAtZero++;
                else numberOfTimesSpawnedAtZero = 0;

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
            int attempt = 0;
            if (player.Gold > 8 && attempt < 10) {
                attempt++;

                int x = GetMostImportantLaneToDefend();
                int y = random.Next(PlayerLane.HEIGHT);
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

        /* Tries to decide which line to defend according to where the most soldiers are */
        private int GetMostImportantLaneToDefend() {
            List<(int soldiers, int lane)> soldiersInLanes = new List<(int soldiers, int lane)>();
            for (int i = 0; i < PlayerLane.WIDTH; i++) {
                soldiersInLanes.Add((CheckLaneForSoldiers(i), i));

            }
            soldiersInLanes.Sort((a, b) => b.soldiers.CompareTo(a.soldiers));
            return soldiersInLanes[0].lane;

        }

        /* Reused from my soldier AI thing */
        private bool CheckLaneForTower(int x, int y) {
            /* We want to check if there any towers in front of us */
            for (int i = 0; i < PlayerLane.HEIGHT; i++) {
                if (player.EnemyLane.GetCellAt(x, i)?.Unit is Tower && i > y) return true;

            }
            return false;

        }

        private int CheckLaneForSoldiers(int x) {
            int soldiers = 0;
            /* We want to check if there any soldiers so we can build towers there */
            for (int i = 0; i < PlayerLane.HEIGHT; i++) {
                if (player.HomeLane.GetCellAt(x, i)?.Unit is Soldier) soldiers++;

            }
            return soldiers;

        }


    }

    
}
