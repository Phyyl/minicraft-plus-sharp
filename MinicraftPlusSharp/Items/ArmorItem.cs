using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Items
{
    public class ArmorItem : StackableItem
    {
        internal new static Item[] GetAllInstances()
        {
            List<Item> items = new List<Item>();

            items.Add(new ArmorItem("Leather Armor", new Sprite(0, 9, 0), .3f, 1));
            items.Add(new ArmorItem("Snake Armor", new Sprite(1, 9, 0), .4f, 2));
            items.Add(new ArmorItem("Iron Armor", new Sprite(2, 9, 0), .5f, 3));
            items.Add(new ArmorItem("Gold Armor", new Sprite(3, 9, 0), .7f, 4));
            items.Add(new ArmorItem("Gem Armor", new Sprite(4, 9, 0), 1f, 5));

            return items.ToArray();
        }

        private readonly float armor;
        private readonly int staminaCost;
        public readonly int level;

        private ArmorItem(string name, Sprite sprite, float health, int level)
            : this(name, sprite, 1, health, level)
        {
        }

        private ArmorItem(string name, Sprite sprite, int count, float health, int level)
            : base(name, sprite, count)
        {
            this.armor = health;
            this.level = level;
            staminaCost = 9;
        }

        public override bool interactOn(Tile tile, Level level, int xt, int yt, Player player, Direction attackDir)
        {
            bool success = false;

            if (player.curArmor is null && player.payStamina(staminaCost))
            {
                player.curArmor = this; // set the current armor being worn to this.
                player.armor = (int)(armor * Player.maxArmor); // armor is how many hits are left
                success = true;
            }

            return base.InteractOn(success);
        }


        public override bool InteractsWithWorld()
        {
            return false;
        }

        public override ArmorItem Clone()
        {
            return new ArmorItem(GetName(), sprite, count, armor, level);
        }
    }
}
