using System.IO;
using Windows.Storage;
using GeocacheLocatorV2.PCL;
using GeocacheLocatorV2.UWP;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;
using Xamarin.Forms;

[assembly: Dependency(typeof(SQLite_UWP))]
namespace GeocacheLocatorV2.UWP
{
    public class SQLite_UWP : ISQLite
    {
        public SQLiteConnectionWithLock GetConnection()
        {
            var sqliteFilename = "TodoSQLite.db3";
            string path = Path.Combine(ApplicationData.Current.LocalFolder.Path, sqliteFilename);
            SQLiteConnectionWithLock connWithLock = new SQLiteConnectionWithLock(new SQLitePlatformWinRT(), new SQLiteConnectionString(path, true));
            return connWithLock;
        }
    }
}