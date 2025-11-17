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
            /* If we got too many people attacking on our home turf, let's focus on defending */
            if (player.HomeLane.SoldierCount() > 5) return;

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
            if (player.Gold > 8 && attempt < 30) {
                attempt++;

                int x = GetMostImportantLaneToDefend();
                int y = GetWhereMostSoldiersAre() + 2; // I feel bad for leaving the rando height but I swear this works best
                if (player.HomeLane.GetCellAt(x, Clamp(y, 0, PlayerLane.HEIGHT)).Unit == null) {
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

        /* Tries to decide which row to position our defense on according to where the most soldiers are */
        private int GetWhereMostSoldiersAre() {
            List<(int soldiers, int row)> soldiersInRows = new List<(int soldiers, int row)>();
            for (int i = 0; i < PlayerLane.HEIGHT; i++) {
                soldiersInRows.Add((CheckRowForSoldiers(i), i));

            }
            soldiersInRows.Sort((a, b) => b.soldiers.CompareTo(a.soldiers));
            return soldiersInRows[0].row;

        }

        /* Tries to decide which lane to defend according to where the most soldiers are */
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

        private int CheckRowForSoldiers(int y) {
            int soldiers = 0;
            /* We want to check if the amount of soldiers on a specified row */
            for (int i = 0; i < PlayerLane.WIDTH; i++) {
                if (player.HomeLane.GetCellAt(i, y)?.Unit is Soldier) soldiers++;

            }
            return soldiers;

        }

        // This was taken off online, because we're relying on .net framework 4 so it stupidly doesn't have clamp by default lmao
        int Clamp(int value, int min, int max) {
            return Math.Max(min, Math.Min(max, value));

        }

    }
    
}
