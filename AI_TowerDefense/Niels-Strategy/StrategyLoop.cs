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
        private int minimumGoldForSoldier = 2;
        private int minimumGoldForTower = 5;
        private int lastScore = 0;
        private int staleChecker = 0;
        private bool spamTowerBuilding = false;
        private int spamTowerThreshold = 200;

        /* Constructor */
        public StrategyLoop(Player player) : base(player) { }
        
        /* SIDE ONE */

        /* Called by game loop to deploy Soldiers */
        public override void DeploySoldiers() {
            /* Records the number of turns in which the score has updated */
            if (lastScore != player.Score) staleChecker++;
            else staleChecker = 0;

            /* If we got too many people attacking on our home turf, let's focus on defending. Post-battle day: Continue spawning soldiers anyway if we get Dustin'ed */
            if (player.Gold > spamTowerThreshold) {
                spamTowerBuilding = true;
                return;

            } else {
                spamTowerBuilding = false;

            }
            if (player.HomeLane.SoldierCount() > 15 && player.HomeLane.SoldierCount() < 130) return;
            if (TowerDefense.Instance.Turns < 20) return;
            /* Keeping this in to show the thinking process, but this was frankly unnecessary and embarassing */
            // minimumGoldForSoldier *= player.HomeLane.TowerCount();
            // minimumGoldForSoldier = Clamp(minimumGoldForSoldier, 2, 50); // Admittedly pathetic attempt at introducing some dynamism

            int attempt = 0;
            while (player.Gold > minimumGoldForSoldier && attempt < 10) {
                attempt++;

                /* The randomizer is only there so we can have some value if there are towers on every lane */
                int x = random.Next(PlayerLane.WIDTH);
                int y = 0;

                for (int i = 0;  i < PlayerLane.WIDTH; i++) {
                    if(!CheckLaneForTower(i, y)) {
                        /* Checking where we need the place the soldier on the X axis */
                        x = i;
                        break;

                    }

                }
                /* Let us be unpredictable if we spawn at 0 too long, this is just for fun any dynamism. Could just spam at 0 if we wanted to */
                if(numberOfTimesSpawnedAtZero > 4) x = random.Next(PlayerLane.WIDTH);

                /* We keep track of this in case we spawn too often at 0, just to spice things up then */
                if (x == 0) numberOfTimesSpawnedAtZero++;
                else numberOfTimesSpawnedAtZero = 0;

                if (player.EnemyLane.GetCellAt(x, y).Unit == null) {
                    var trybuy = player.TryBuySoldier<NielsSoldier>(x);

                    /* If we suceed in placing the soldier */
                    if (trybuy == Player.SoldierPlacementResult.Success) {
                        soldierCounter++;

                        /* Accessing the soldier by reference, because if we're spamming too much we're going kamikaze */
                        NielsSoldier soldier = player.EnemyLane.GetCellAt(x, y)?.Unit as NielsSoldier;

                        /* We also go kamikaze if the score hasn't moved in 20 rounds, in an attempt to get unstuck if such were the case */
                        if (soldier != null && (player.EnemyLane.SoldierCount() > 25 || staleChecker > 20)) {
                            soldier.kamikazeOverride = true;

                        }
                        DebugLogger.Log($"(P{player.Name}) S #{soldierCounter} deployed a-t {attempt}.");

                    }

                }

            }

        }

        /* Called by game loop to deploy Towers */
        public override void DeployTowers() {
            int attempt = 0;

            /* Only on the first rounds */
            if(TowerDefense.Instance.Turns < 20) {
                if (TowerDefense.Instance.Turns < 5) spamTowerBuilding = true; // Overrides the spamTowerBuilding at first before DeploySoldiers can
                for (int initWallY = 2; initWallY < 4; initWallY++) {
                    for (int x = 0; x < 5; x++) {
                        var trybuy = player.TryBuyTower<Tower>(x, initWallY);

                        if (trybuy == Player.TowerPlacementResult.Success) {
                            towerCounter++;
                            DebugLogger.Log($"(P{player.Name}) T #{towerCounter} deployed a-t {attempt}.");

                        }

                    }

                }

            }

            /* Spawning an army of towers if we have too much money to spare */
            if (spamTowerBuilding) {
                for (int initWallY = PlayerLane.HEIGHT; initWallY > 0; initWallY--) {
                    for (int x = 0; x < PlayerLane.WIDTH; x++) {
                        var trybuy = player.TryBuyTower<Tower>(x, initWallY);

                        if (trybuy == Player.TowerPlacementResult.Success) {
                            towerCounter++;
                            /* SM Stands for spam mode */
                            DebugLogger.Log($"(P{player.Name}) - SM - T #{towerCounter} deployed a-t {attempt}.");

                        }

                    }

                }

            }

            /* Standard placement method I used in the tournament */
            if (player.Gold > minimumGoldForTower && attempt < 30) {
                attempt++;

                /* Doesn't look like much, because it's nested methods (see below) */
                int x = GetMostImportantLaneToDefend();
                int y = GetWhereMostSoldiersAre() + 2; // Always offset by 2 to not jump the soldiers like an antifa protester seeing a cop
                if (player.HomeLane.GetCellAt(x, Clamp(y, 0, PlayerLane.HEIGHT - 1)).Unit == null) {
                    var trybuy = player.TryBuyTower<Tower>(x, y);

                    if (trybuy == Player.TowerPlacementResult.Success) {
                        towerCounter++;
                        DebugLogger.Log($"(P{player.Name}) T #{towerCounter} deployed a-t {attempt}.");

                    }

                }

            }

        }

        /* SIDE TWO */

        public override List<Soldier> SortedSoldierArray(List<Soldier> unsortedList) {
            /* The soldiers closest to the home run get their command first */
            return unsortedList.OrderByDescending(s => s.PosY).ToList();

        }

        public override List<Tower> SortedTowerArray(List<Tower> unsortedList) {
            /* Basically we're forcing the towers to go where the most soldiers in sorting as well */
            return unsortedList.OrderByDescending(t => CheckRowForSoldiers(t.PosY)).ToList();

        }

        /* Tries to decide which row to position our defense on according to where the most soldiers are */
        private int GetWhereMostSoldiersAre() {
            List<(int soldiers, int row)> soldiersInRows = new List<(int soldiers, int row)>();
            for (int i = 0; i < PlayerLane.HEIGHT; i++) {
                soldiersInRows.Add((CheckRowForSoldiers(i), i));

            }
            soldiersInRows.Sort((a, b) => b.soldiers.CompareTo(a.soldiers));
            if (soldiersInRows[0].soldiers > 15) return soldiersInRows[1].row; // We return the second best lane in case a lane gets overtly crowded in soldiers and we can't do much anymore
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
