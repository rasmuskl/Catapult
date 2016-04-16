namespace Catapult.Core.Config
{
    public class JsonUserConfiguration
    {
        public JsonUserConfiguration()
        {
            Paths = new string[0];
        }

        public string[] Paths { get; set; }
    }
}