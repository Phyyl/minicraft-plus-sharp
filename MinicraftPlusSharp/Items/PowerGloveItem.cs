namespace MinicraftPlusSharp.Items
{
    public class PowerGloveItem : Item
    {
        public PowerGloveItem()
            : base("Power Glove")
        {
        }

        public override PowerGloveItem Clone()
        {
            return new PowerGloveItem();
        }
    }
}