using System;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using GeocacheLocatorV2.PCL.ViewModels;
using GeocachingToolbox;

namespace GeocacheLocatorV2.UWP.Controls
{
    public sealed partial class GeoPushPin : UserControl
    {
        public ILocationViewModel Cache { get; protected set; }
        private static readonly ImageSource Unknown = new BitmapImage(new Uri("ms-appx:/Assets/PinImages/type_unknown.png"));
        private static readonly ImageSource MultiCache = new BitmapImage(new Uri("ms-appx:/Assets/PinImages/type_multi.png"));
        private static readonly ImageSource WhereIGo = new BitmapImage(new Uri("ms-appx:/Assets/PinImages/type_whereigo.png"));
        private static readonly ImageSource LetterBox = new BitmapImage(new Uri("ms-appx:/Assets/PinImages/type_letterbox.png"));
        private static readonly ImageSource Traditionnal = new BitmapImage(new Uri("ms-appx:/Assets/PinImages/type_traditional.png"));
        private static readonly ImageSource Mystery = new BitmapImage(new Uri("ms-appx:/Assets/PinImages/type_Mystery.png"));
        private static readonly ImageSource Earth = new BitmapImage(new Uri("ms-appx:/Assets/PinImages/type_earth.png"));

        private static readonly ImageSource Found = new BitmapImage(new Uri("ms-appx:/Assets/PinImages/marker_found2.png"));
        private static readonly ImageSource Own = new BitmapImage(new Uri("ms-appx:/Assets/PinImages/marker_own.png"));

        private static readonly Brush NotAccurateColor = new SolidColorBrush(Colors.Orange);
        private static readonly Brush AccurateColor = new SolidColorBrush(Colors.Gray);

        private static readonly Brush CacheAvailableColor = new SolidColorBrush(Colors.White);
        private static readonly Brush CacheNotAvailableColor = new SolidColorBrush(Colors.Gray);

        public GeoPushPin()
        {
            InitializeComponent();
        }

        public void Update(ILocationViewModel locationVm)
        {
            Cache = locationVm;
            Color = Cache.IsDetailed ? AccurateColor : NotAccurateColor;
            FillColor = Cache.CacheStatus == GeocacheStatus.Published ? CacheAvailableColor : CacheNotAvailableColor;
            if (locationVm.Found)
                Image = Found;
            else if (locationVm.Own)
            {
                Image = Own;
            }
            else
            {
                switch (Cache.CacheType)
                {
                    case GeocacheType.Traditional:
                        Image = Traditionnal;
                        break;
                    case GeocacheType.Mystery:
                        Image = Mystery;
                        break;
                    case GeocacheType.Unknown:
                        Image = Unknown;
                        break;
                    case GeocacheType.Multicache:
                        Image = MultiCache;
                        break;
                    case GeocacheType.Whereigo:
                        Image = WhereIGo;
                        break;
                    case GeocacheType.LetterboxHybrid:
                        Image = LetterBox;
                        break;
                    case GeocacheType.Earthcache:
                        Image = Earth;
                        break;
                    default:
                        Image = Unknown;
                        break;
                }
            }
        }

        public GeoPushPin(ILocationViewModel cache) : this()
        {

            Update(cache);
        }

        private ImageSource Image
        {
            set { image.Source = value; }
        }

        private Brush Color
        {
            set { outside.Stroke = value; }
        }

        private Brush FillColor
        {
            set { outside.Fill = value; }
        }
    }
}
