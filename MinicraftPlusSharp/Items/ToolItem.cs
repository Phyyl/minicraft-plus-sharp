using MinicraftPlusSharp.Core;
using MinicraftPlusSharp.Entities;
using MinicraftPlusSharp.Entities.Mobs;
using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Java;
using System.Collections.Generic;

namespace MinicraftPlusSharp.Items
{
    public class ToolItem : Item
    {
        internal static Item[] GetAllInstances()
        {
            List<Item> items = new();

            foreach (ToolType tool in ToolType.All)
            {
                if (!tool.noLevel)
                {
                    for (int lvl = 0; lvl <= 4; lvl++)
                    {
                        items.Add(new ToolItem(tool, lvl));
                    }
                }
                else
                {
                    items.Add(new ToolItem(tool));
                }
            }

            return items.ToArray();
        }

        private JavaRandom random = new();

        public static readonly string[] LEVEL_NAMES = { "Wood", "Rock", "Iron", "Gold", "Gem" }; // The names of the different levels. A later level means a stronger tool.

        public ToolType type; // Type of tool (Sword, hoe, axe, pickaxe, shovel)
        public int level; // Level of said tool
        public int dur; // the durability of the tool

        /** Tool Item, requires a tool type (ToolType.Sword, ToolType.Axe, ToolType.Hoe, etc) and a level (0 = wood, 2 = iron, 4 = gem, etc) */
        public ToolItem(ToolType type, int level)
            : base(LEVEL_NAMES[level] + " " + type.Name, new Sprite(type.xPos, type.yPos + level, 0))
        {
            this.type = type;
            this.level = level;

            dur = type.durability * (level + 1); // initial durability fetched from the ToolType
        }

        public ToolItem(ToolType type)
            : base(type.Name, new Sprite(type.xPos, type.yPos, 0))
        {
            this.type = type;
            dur = type.durability;
        }

        /** Gets the name of this tool (and it's type) as a display string. */
        public override string GetDisplayName()
        {
            if (!type.noLevel)
            {
                return " " + Localization.GetLocalized(LEVEL_NAMES[level]) + " " + Localization.GetLocalized(type.ToString());
            }
            else
            {
                return " " + Localization.GetLocalized(type.ToString());
            }
        }

        public override bool IsDepleted()
        {
            return dur <= 0 && type.durability > 0;
        }

        /** You can attack mobs with tools. */
        public override bool CanAttack()
        {
            return type != ToolType.Shear;
        }

        public bool PayDurability()
        {
            if (dur <= 0)
            {
                return false;
            }

            if (!Game.IsMode("creative"))
            {
                dur--;
            }

            return true;
        }

        /** Gets the attack damage bonus from an item/tool (sword/axe) */
        public int GetAttackDamageBonus(Entity e)
        {
            if (!PayDurability())
            {
                return 0;
            }

            if (e is Mob)
            {
                if (type == ToolType.Axe)
                {
                    return (level + 1) * 2 + random.NextInt(4); // wood axe damage: 2-5; gem axe damage: 10-13.
                }

                if (type == ToolType.Sword)
                {
                    return (level + 1) * 3 + random.NextInt(2 + level * level); // wood: 3-5 damage; gem: 15-32 damage.
                }

                if (type == ToolType.Claymore)
                {
                    return (level + 1) * 3 + random.NextInt(4 + level * level * 3); // wood: 3-6 damage; gem: 15-66 damage.
                }

                return 1; // all other tools do very little damage to mobs.
            }

            return 0;
        }

        public override string GetData()
        {
            return base.GetData() + "_" + dur;
        }

        /** Sees if this item equals another. */
        public override bool Equals(Item item)
        {
            if (item is ToolItem other)
            {
                return other.type == type && other.level == level;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return type.Name.GetHashCode() + level;
        }

        public override ToolItem Clone()
        {
            ToolItem ti;
            if (type.noLevel)
            {
                ti = new ToolItem(type);
            }
            else
            {
                ti = new ToolItem(type, level);
            }
            ti.dur = dur;
            return ti;
        }
    }
}