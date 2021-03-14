using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Items
{
    public abstract class Item : IEquatable<Item>
    {
        private readonly string name;

        public Sprite sprite;

        public bool used_pending = false;

        protected Item(string name)
        {
            sprite = Sprite.MissingTexture(1, 1);
            this.name = name;
        }

        protected Item(string name, Sprite sprite)
        {
            this.name = name;
            this.sprite = sprite;
        }

        public virtual void RenderHUD(Screen screen, int x, int y, int fontColor)
        {
            string dispName = GetDisplayName();
            sprite.Render(screen, x, y);
            Font.DrawBackground(dispName, screen, x + 8, y, fontColor);
        }

        public virtual bool InteractOn(Tile tile, Level level, int xt, int yt, Player player, Direction attackDir)
        {
            return false;
        }

        public virtual bool IsDepleted()
        {
            return false;
        }

        public virtual bool CanAttack()
        {
            return false;
        }

        public override bool Equals(object obj)
        {
            return obj is Item item && item.Equals(this);
        }

        public bool Equals(Item item)
        {
            return item is not null && item.GetType().Equals(GetType()) && item.name.Equals(name);
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public abstract Item Clone();

        public override string ToString()
        {
            return $"{name}-Item";
        }

        public virtual string GetData()
        {
            return name;
        }

        public virtual string GetName() => name;

        public virtual string GetDisplayName()
        {
            return " " + Localization.GetLocalized(GetName());
        }

        public virtual bool InteractsWithWorld() => true;
    }
}
