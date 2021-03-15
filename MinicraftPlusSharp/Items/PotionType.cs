using MinicraftPlusSharp.Core;
using MinicraftPlusSharp.Entities.Mobs;
using MinicraftPlusSharp.Gfx;
using System;
using System.Collections.Generic;

namespace MinicraftPlusSharp.Items
{
    public class PotionType
    {
        private static readonly List<PotionType> all = new();
        public static PotionType[] All => all.ToArray();

        public static readonly PotionType None = new(Color.Get(1, 22, 22, 137), 0, 0);
        public static readonly PotionType Speed = new(Color.Get(1, 23, 46, 23), 4200, 1, (player, addEffect) =>
        {
            player.moveSpeed += (double)(addEffect ? 1 : player.moveSpeed > 1 ? -1 : 0);
            return true;
        });

        public static readonly PotionType Light = new(Color.Get(1, 183, 183, 91), 6000, 2);
        public static readonly PotionType Swim = new(Color.Get(1, 17, 17, 85), 4800, 3);
        public static readonly PotionType Energy = new(Color.Get(1, 172, 80, 57), 8400, 4);
        public static readonly PotionType Regen = new(Color.Get(1, 168, 54, 146), 1800, 5);
        public static readonly PotionType Health = new(Color.Get(1, 161, 46, 69), 0, 6, (player, addEffect) =>
        {
            if (addEffect)
            {
                player.Heal(5);
            }

            return true;
        });

        public static readonly PotionType Time = new(Color.Get(1, 102), 1800, 7);
        public static readonly PotionType Lava = new(Color.Get(1, 129, 37, 37), 7200, 8);
        public static readonly PotionType Shield = new(Color.Get(1, 65, 65, 157), 5400, 9);
        public static readonly PotionType Haste = new(Color.Get(1, 106, 37, 106), 4800, 10);

        public static readonly PotionType Escape = new(Color.Get(1, 85, 62, 62), 0, 11, (player, addEffect) =>
        {
            if (addEffect)
            {
                int playerDepth = player.GetLevel().depth;

                if (playerDepth == 0)
                {
                    if (!Game.IsValidServer())
                    {
                        // player is in overworld
                        string note = "You can't escape from here!";
                        Game.notifications.Add(note);
                    }

                    return false;
                }

                int depthDiff = playerDepth > 0 ? -1 : 1;

                World.ScheduleLevelChange(depthDiff, () =>
                {
                    Level plevel = World.levels[World.lvlIdx(playerDepth + depthDiff)];

                    if (plevel != null && !plevel.GetTile(player.x >> 4, player.y >> 4).MayPass(plevel, player.x >> 4, player.y >> 4, player))
                    {
                        player.FindStartPos(plevel, false);
                    }
                });
            }

            return true;
        });

        public int dispColor, duration;
        public string name;
        public Func<Player, bool, bool> toggleEffect;
        public Func<bool> transmitEffect;
        public readonly int ordinal;

        private PotionType(int col, int dur, int ordinal, Func<Player, bool, bool> toggleEffet = default, Func<bool> transmitEffect = default)
        {
            dispColor = col;
            duration = dur;
            this.ordinal = ordinal;

            if (this.ToString().Equals("None"))
            {
                name = "Potion";
            }
            else
            {
                name = this + " Potion";
            }

            this.toggleEffect = toggleEffet ?? ((p, a) => duration > 0);
            this.transmitEffect = transmitEffect ?? (() => true);

            all.Add(this);
        }
    }
}