using System.Text;
using Caliburn.Micro;
using GeocachingToolbox;

namespace GeocacheLocatorV2.PCL.ViewModels
{
    public class GeocacheDescriptionViewModel : Screen
    {
        private Geocache m_geocache;
        private string m_description;

        public string Title => "Description";

        public string Description
        {
            get { return m_description; }
            set
            {
                if (value == m_description) return;
                m_description = value;
                NotifyOfPropertyChange();
            }
        }

        private void SetDescription(string description)
        {
            if (description == null)
                return;
            var sb = new StringBuilder();
            sb.Append("<html lang=\"en\" class=\"no-js\">");
            sb.Append(
                "<head id=\"ctl00_Head1\"><meta charset =\"utf-8\"/><meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge,chrome=1\"/>");
            sb.Append("</head>");
            sb.Append("<body>");
            sb.Append(description);
            sb.Append("</body>");
            sb.Append("</html>");
            Description = sb.ToString();
        }

        public Geocache Geocache
        {
            get { return m_geocache; }
            set
            {
                if (Equals(value, m_geocache)) return;
                m_geocache = value;
                SetDescription(m_geocache?.Description);
                NotifyOfPropertyChange();
            }
        }
    }
}
