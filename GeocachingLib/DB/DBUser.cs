using SQLite.Net.Attributes;

namespace GeocachingLib.DB
{
    public class DBUser
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public int FoundGeocachesCount { get; set; }
        public string Uuid { get; set; }
        public Origin Origin { get; set; }
    }
}