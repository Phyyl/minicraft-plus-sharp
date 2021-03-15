using MinicraftPlusSharp.Core;
using MinicraftPlusSharp.Entities;
using MinicraftPlusSharp.Entities.Mobs;
using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Levels;
using MinicraftPlusSharp.Levels.Tiles;
using System;
using System.Collections.Generic;

namespace MinicraftPlusSharp.Items
{
    public class PotionItem : StackableItem
    {
        internal new static Item[] GetAllInstances()
        {
            List<Item> items = new();

            foreach (PotionType type in PotionType.All)
            {
                items.Add(new PotionItem(type));
            }

            return items.ToArray();
        }

        public PotionType type;

        private PotionItem(PotionType type)
            : this(type, 1)
        {
        }

        private PotionItem(PotionType type, int count)
            : base(type.name, new Sprite(0, 7, 0), count)
        {
            this.type = type;
            sprite.color = type.dispColor;
        }

        // the return value is used to determine if the potion was used, which means being discarded.
        public override bool InteractOn(Tile tile, Level level, int xt, int yt, Player player, Direction attackDir)
        {
            return base.InteractOn(ApplyPotion(player, type, true));
        }

        /// only ever called to load from file
        public static bool ApplyPotion(Player player, PotionType type, int time)
        {
            bool result = ApplyPotion(player, type, time > 0);
            if (result && time > 0) player.AddPotionEffect(type, time); // overrides time
            return result;
        }
        /// main apply potion method
        public static bool ApplyPotion(Player player, PotionType type, bool addEffect)
        {
            if (player.GetPotionEffects().ContainsKey(type) != addEffect)
            { // if hasEffect, and is disabling, or doesn't have effect, and is enabling...
                if (!type.toggleEffect(player, addEffect))
                {
                    return false; // usage failed
                }

                // transmit the effect; server never uses potions without this.
                if (type.transmitEffect() && Game.IsValidServer() && player is RemotePlayer remotePlayer)
                {
                    Game.server.GetAssociatedThread(remotePlayer).SendPotionEffect(type, addEffect);
                }
            }

            if (addEffect && type.duration > 0)
            {
                player.potioneffects.put(type, type.duration); // add it
            }
            else
            {
                player.potioneffects.remove(type);
            }

            return true;
        }

        public override bool Equals(Item other)
        {
            return base.Equals(other) && ((PotionItem)other).type == type;
        }


        public override int GetHashCode()
        {
            return base.GetHashCode() + type.name.GetHashCode();
        }

        public override bool InteractsWithWorld()
        {
            return false;
        }

        public override PotionItem Clone()
        {
            return new PotionItem(type, count);
        }
    }
}