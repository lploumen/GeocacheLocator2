using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace GeocacheLocatorV2.PCL.Services
{
    public interface ISettingsService
    {
        bool GeocachingLoginValidated { get; set; }
        string GeocachingPassword { get; set; }
        string GeocachingUsername { get; set; }
        bool UserLocationEnabled { get; set; }
        bool LoginValidated { get; }
        string OpencachingPlUsername { get; set; }
        string OpencachingPlPassword { get; set; }
        bool OpencachingPlLoginValidated { get; set; }
        string OpencachingToken { get; set; }
        string OpencachingTokenSecret { get; set; }
        bool UseGeocachingService { get; set; }
        bool UseOpencachingPlService { get; set; }
        void SetLastLocationRectangle(Rectangle rect);
        void SetLastLocationCenter(Position pos);
        Rectangle GetLastLocationRectangle();
        Position GetLastLocationCenter();
        void Load();
        void Save();
    }
}