using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GeocacheLocatorV2.UWP.Controls
{
    public sealed partial class UserLocation : UserControl
    {
        public UserLocation()
        {
            this.InitializeComponent();
        }

        public void Rotate(float angleDeg)
        {
            rotate.Angle = angleDeg;
        }
    }
}
