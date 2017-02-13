using System;
using System.IO;
using System.Xml.Serialization;
using GeocachingToolbox.Opencaching;

namespace ConsoleTester
{
    class Program
    {
        static void Main()
        {
            // Read config file from .exe directory
            XmlSerializer ser = new XmlSerializer(typeof(ApiKeys));
            ApiKeys settings;
            using (TextReader tr = new StreamReader("ApiKeys.xml"))
            {
                settings = ser.Deserialize(tr) as ApiKeys;
            }


            ApiAccessKeysImpl apiKeys = new ApiAccessKeysImpl(settings.OpencachingConsumerKey, settings.OpencachingConsumerSecret);
            AccessTokenStore accessTokenStore = new AccessTokenStore();
            OCClient ocClient = new OCClient("http://opencaching.pl/okapi/", null, accessTokenStore, apiKeys);
            if (ocClient.NeedsAuthorization)
            {
                var authUrl = ocClient.GetAuthorizationUrl().GetAwaiter().GetResult();
                Console.Write("Please open this link in your browser: " + authUrl + " and enter pin here:");
                string pin = Console.ReadLine();
                ocClient.EnterAuthorizationPin(pin).GetAwaiter().GetResult();
            }
            ocClient.Connect().GetAwaiter().GetResult();
        }
    }
}
