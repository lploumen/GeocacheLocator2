using SQLite.Net;

namespace GeocacheLocatorV2.PCL
{
    public interface ISQLite
    {
        SQLiteConnectionWithLock GetConnection();
    }
}