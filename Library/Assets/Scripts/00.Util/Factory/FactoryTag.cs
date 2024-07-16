namespace Util
{
    public class FactoryTag : Inherited.FactoryObject<string>
    {
        private void OnValidate()
        {
            if (key != null && this.key.CompareTo(string.Empty) == 0)
            {
                this.key = gameObject.name;
            }
        }
    }
}