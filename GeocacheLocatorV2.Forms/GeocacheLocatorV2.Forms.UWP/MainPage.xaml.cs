using Caliburn.Micro;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GeocacheLocatorV2.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage 
    {
        public MainPage()
        {
            this.InitializeComponent();
            LoadApplication(new GeocacheLocatorV2.PCL.App(IoC.Get<WinRTContainer>()));
        }
    }
}
