namespace Util
{
    public class FactoryTag : FactoryObject
    {
        protected override void OnValidate()
        {
            if (ID != null && this.ID.CompareTo(string.Empty) == 0)
            {
                this.ID = gameObject.name;
            }
        }
    }
}