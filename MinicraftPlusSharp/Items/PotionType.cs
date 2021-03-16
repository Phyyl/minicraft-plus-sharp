using MinicraftPlusSharp.Core;
using MinicraftPlusSharp.Entities.Mobs;
using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Java;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MinicraftPlusSharp.Items
{
    public class PotionType : JavaEnum<PotionType>
    {
        public static readonly PotionType None = new(Color.Get(1, 22, 22, 137), 0);
        public static readonly PotionType Speed = new SpeedPotionType(Color.Get(1, 23, 46, 23), 4200);
        public static readonly PotionType Light = new(Color.Get(1, 183, 183, 91), 6000);
        public static readonly PotionType Swim = new(Color.Get(1, 17, 17, 85), 4800);
        public static readonly PotionType Energy = new(Color.Get(1, 172, 80, 57), 8400);
        public static readonly PotionType Regen = new(Color.Get(1, 168, 54, 146), 1800);
        public static readonly PotionType Health = new HealthPotionType(Color.Get(1, 161, 46, 69), 0);
        public static readonly PotionType Time = new(Color.Get(1, 102), 1800);
        public static readonly PotionType Lava = new(Color.Get(1, 129, 37, 37), 7200);
        public static readonly PotionType Shield = new(Color.Get(1, 65, 65, 157), 5400);
        public static readonly PotionType Haste = new(Color.Get(1, 106, 37, 106), 4800);
        public static readonly PotionType Escape = new EscapePotionType(Color.Get(1, 85, 62, 62), 0);

        public int dispColor, duration;
        public string name;
        public Func<Player, bool, bool> toggleEffect;
        public Func<bool> transmitEffect;
        public readonly int ordinal;

        private PotionType(int col, int dur, [CallerMemberName] string name = default)
            : base(name)
        {
            dispColor = col;
            duration = dur;

            if (this.ToString().Equals("None"))
            {
                name = "Potion";
            }
            else
            {
                name = this + " Potion";
            }
        }

        public virtual bool ToggleEffect(Player player, bool addEffect)
        {
            return duration > 0;
        }

        public virtual bool TransmitEffect()
        {
            return true;
        }

        private class SpeedPotionType : PotionType
        {
            public SpeedPotionType(int col, int dur, [CallerMemberName] string name = null) 
                : base(col, dur, name)
            {
            }

            public override bool ToggleEffect(Player player, bool addEffect)
            {
                player.moveSpeed += (double)(addEffect ? 1 : player.moveSpeed > 1 ? -1 : 0);

                return true;
            }
        }

        private class HealthPotionType : PotionType
        {
            public HealthPotionType(int col, int dur, [CallerMemberName] string name = default)
                : base(col, dur, name)
            {
            }

            public override bool ToggleEffect(Player player, bool addEffect)
            {
                if (addEffect)
                {
                    player.Heal(5);
                }

                return true;
            }
        }

        private class EscapePotionType : PotionType
        {
            public EscapePotionType(int col, int dur, [CallerMemberName] string name = default)
                : base(col, dur, name)
            {
            }

            public override bool ToggleEffect(Player player, bool addEffect)
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
            }
        }
    }
}