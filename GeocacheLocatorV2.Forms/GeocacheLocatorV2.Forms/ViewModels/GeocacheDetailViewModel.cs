using System.Diagnostics;
using System.Text;
using Acr.UserDialogs;
using Caliburn.Micro;
using GeocacheLocatorV2.PCL.Resources;
using GeocacheLocatorV2.PCL.Services;
using GeocacheLocatorV2.PCL.Views;
using GeocachingToolbox;
using Plugin.Geolocator;
using Xamarin.Forms;

namespace GeocacheLocatorV2.PCL.ViewModels
{
    public class GeocacheDetailViewModel : Screen
    {
        private readonly IMasterNavigationService m_navigationService;
        private Geocache m_geocache;
        public Command NavigateToCacheCommand { get; set; }
        public Command ShowHintCommand { get; set; }

        public GeocacheDetailViewModel(IMasterNavigationService navigationService)
        {
            m_navigationService = navigationService;
            ShowHintCommand = new Command(ShowHint, CanShowHint);
            NavigateToCacheCommand = new Command(Navigate);
        }

        private async void Navigate()
        {
            if (!CrossGeolocator.Current.IsGeolocationEnabled)
            {
                await UserDialogs.Instance.AlertAsync(AppResource.GeolocationNotAuthorized);
                return;
            }

            await m_navigationService.NavigateToViewModelAsync(typeof(CompassViewModel), m_geocache);
        }

        public void RefreshDescription()
        {
            string desc = Parameter.ShortDescription + Parameter.Description;
            (GetView() as GeocacheDetailView).SetDescription(GetHtmlDescription(desc));
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            Debug.WriteLine("Activate");
            RefreshDescription();
            Debug.WriteLine("Description set");
        }

        private bool CanShowHint()
        {
            return !string.IsNullOrEmpty(Parameter?.Hint);
        }

        public void ShowHint()
        {
            UserDialogs.Instance.Alert(Parameter.Hint);
        }

        private string GetHtmlDescription(string description)
        {
            if (description == null)
                return "";
            var sb = new StringBuilder();
            sb.Append("<html lang=\"en\" class=\"no-js\">");
            sb.Append(
                "<head id=\"ctl00_Head1\"><meta charset =\"utf-8\"/><meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge,chrome=1\"/>");
            sb.Append("</head>");
            sb.Append("<body>");
            sb.Append(description);
            sb.Append("</body>");
            sb.Append("</html>");
            Debug.WriteLine("Description size " + sb.Length / 1024 + "Kb");
            return sb.ToString();
        }

        public Geocache Parameter
        {
            get { return m_geocache; }
            set
            {
                if (Equals(value, m_geocache)) return;
                m_geocache = value;
                NotifyOfPropertyChange();
                ShowHintCommand.ChangeCanExecute();
            }
        }
    }
}
