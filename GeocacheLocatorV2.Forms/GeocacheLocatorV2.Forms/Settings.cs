using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using GeocacheLocatorV2.PCL.Services;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

//ACR Settings
namespace GeocacheLocatorV2.PCL
{
    public class SettingsBase
    {
        private static ISettings AppSettings => CrossSettings.Current;

        public void Save()
        {
            PropertyInfo[] properties = GetType().GetRuntimeProperties().ToArray();// GetProperties();
            foreach (var property in properties)
            {
                AppSettings.AddOrUpdateValue(property.Name, property.GetValue(this));
            }
        }

        public void Load()
        {
            PropertyInfo[] properties = GetType().GetRuntimeProperties().ToArray();
            foreach (var property in properties)
            {
                if (!property.CanWrite)
                    return;
                object def = null;
                def = property.PropertyType == typeof(string) ? "" : Activator.CreateInstance(property.PropertyType);

                var att = property.GetCustomAttribute<DefaultValueAttribute>();
                property.SetValue(this, CrossSettings.Current.GetValueOrDefault(property.Name, att != null ? att.Value : def));
            }
        }

        public object GetDefault(Type t)
        {
            var method = GetType().GetRuntimeMethod("GetDefaultGeneric", null);
            return method.MakeGenericMethod(t).Invoke(this, null);
        }
        public T GetDefaultGeneric<T>()
        {
            return default(T);
        }
    }

    public class SettingsService : SettingsBase, ISettingsService
    {
        public bool UserLocationEnabled { get; set; }
        public bool UserLocationAsked { get; set; }

        public string GeocachingUsername { get; set; }
        public string GeocachingPassword { get; set; }
        public bool GeocachingLoginValidated { get; set; }
        public bool UseGeocachingService { get; set; }

        public string OpencachingPlUsername { get; set; }
        public string OpencachingPlPassword { get; set; }
        public bool OpencachingPlLoginValidated { get; set; }
        public bool UseOpencachingPlService { get; set; }
        public string OpencachingToken { get; set; }
        public string OpencachingTokenSecret { get; set; }

        [DefaultValue(12.434915746562183)]
        public double LastLocationRectangleX { get; set; }
        [DefaultValue(41.957810539752245)]
        public double LastLocationRectangleY { get; set; }
        [DefaultValue(0.12353039346635342)]
        public double LastLocationRectangleWidth { get; set; }
        [DefaultValue(-0.13330260291695595)]
        public double LastLocationRectangleHeight { get; set; }
        [DefaultValue(41.891159238293767)]
        public double LastLocationCenterLatitude { get; set; }
        [DefaultValue(12.49668094329536)]
        public double LastLocationCenterLongitude { get; set; }

        public bool LoginValidated => GeocachingLoginValidated || OpencachingPlLoginValidated;

        public void SetLastLocationRectangle(Rectangle rect)
        {
            LastLocationRectangleX = rect.X;
            LastLocationRectangleY = rect.Y;
            LastLocationRectangleWidth = rect.Width;
            LastLocationRectangleHeight = rect.Height;
        }

        public void SetLastLocationCenter(Position pos)
        {
            LastLocationCenterLatitude = pos.Latitude;
            LastLocationCenterLongitude = pos.Longitude;
        }

        public Rectangle GetLastLocationRectangle()
        {
            return new Rectangle(LastLocationRectangleX, LastLocationRectangleY, LastLocationRectangleWidth, LastLocationRectangleHeight);
        }

        public Position GetLastLocationCenter()
        {
            return new Position(LastLocationCenterLatitude, LastLocationCenterLongitude);
        }
    }
}
