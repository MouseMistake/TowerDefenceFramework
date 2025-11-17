using GameFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Strategy {
    public class NielsSoldier : Soldier {
        public bool kamikazeOverride = false;
        public override void Move() {
            if (speed > 0 && posY < PlayerLane.HEIGHT) {
                int x = posX;
                int y = posY;

                if (kamikazeOverride) {
                    for (int i = speed; i > 0; i--) {
                        MoveTo(x, y + i); // Oh my boy if the override is on we are GOING TO THE FRONT
                        return;

                    }

                }

                /* The strategy behind this is we want to avoid to go against towers if possible,
                 * basically making a run for it hopefully.
                 * But, if health is too low, we go kamikaze to try to take down the enemy's towers. */
                if (CheckLaneForTower(x, y) && health > 3) {
                    kamikazeOverride = false;
                    for (int i = speed; i > 0; i--) {
                        if (MoveTo(x + i, y)) return; // Try to go right
                        if (MoveTo(x - i, y)) return; // If that doesn't work, try to go left
                        if (MoveTo(x - i, y - i)) return; // Damn it all, go left and stay back a turn
                        if (MoveTo(x + i, y - i)) return; // Damn it all, go right and stay back a turn

                    }

                } else {
                    kamikazeOverride = true; // If there's no tower on the way, it's free real estate

                }

            }

        }

        private bool CheckLaneForTower(int x, int y) {
            /* We want to check if there any towers in front of us */
            for(int i = 0; i < PlayerLane.HEIGHT; i++) {
                if (player.EnemyLane.GetCellAt(x, i)?.Unit is Tower && i > y) return true;

            }
            return false;

        }

    }

}
