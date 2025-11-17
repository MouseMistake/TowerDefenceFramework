using GameFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Strategy {
    public class NielsSoldier : Soldier {
        public override void Move() {
            if (speed > 0 && posY < PlayerLane.HEIGHT) {
                int x = posX;
                int y = posY;

                /* The strategy behind this is we want to avoid to go against towers if possible,
                 * basically making a run for it hopefully. */
                if (CheckLaneForTower(x)) {
                    for (int i = speed; i > 0; i--) {
                        if (MoveTo(x + i, y)) return; // Try to go right
                        if (MoveTo(x - i, y)) return; // If that doesn't work, try to go left
                        if (MoveTo(x - i, y - i)) return; // Damn it all, go left and stay back a turn

                    }

                } else {
                    for (int i = speed; i > 0; i--) {
                        if (MoveTo(x, y + i)) return; // If there's no tower on the way, it's free real estate

                    }

                }

            }

        }

        private bool CheckLaneForTower(int x) {
            for(int i = 0; i < PlayerLane.HEIGHT; i++) {
                if (player.EnemyLane.GetCellAt(x, i)?.Unit is Tower) return true;

            }
            return false;

        }

    }

}
