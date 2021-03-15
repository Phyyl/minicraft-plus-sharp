using MinicraftPlusSharp.Gfx;

namespace MinicraftPlusSharp.Items
{
    public class UnknownItem : StackableItem
    {
        protected UnknownItem(string reqName)
            : base(reqName, Sprite.MissingTexture(1, 1))
        {
        }

        public override UnknownItem Clone()
        {
            return new UnknownItem(GetName());
        }
    }
}