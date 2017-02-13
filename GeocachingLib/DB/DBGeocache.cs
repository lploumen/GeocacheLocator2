using System;
using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;

namespace GeocachingLib.DB
{
    public class DBGeocache
    {
        [PrimaryKey]
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public string ShortDescription { get; set; }
        public string Hint { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }
        public int Size { get; set; }
        public float Difficulty { get; set; }
        public float Terrain { get; set; }
        public DateTime DateHidden { get; set; }
        [ForeignKey(typeof(DBUser))]     // Specify the foreign key
        public int OwnerId { get; set; }
        [OneToOne(CascadeOperations = CascadeOperation.CascadeInsert)]
        public DBUser Owner { get; set; }
        public string DetailsUrl { get; set; }
        public bool IsPremium { get; set; }
        public float WayPointLong { get; set; }
        public float WayPointLat { get; set; }
        public Origin Origin { get; set; }
        public bool IsDetailed { get; set; }
        public bool Found { get; set; }
        public string Logs { get; set; }
        public bool Own { get; set; }
    }
}