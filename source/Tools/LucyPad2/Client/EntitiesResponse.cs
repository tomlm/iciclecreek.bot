using Lucy;

namespace LucyPad2.Client
{
    public class EntitiesResponse
    {
        public long elapsed { get; set; }
        public string message { get; set; }
        public LucyEntity[] entities { get; set; }
    }
}
