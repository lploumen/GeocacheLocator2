using System.Diagnostics;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace LPL.ViewModels
{
    public class TestViewModel : Screen
    {
        public async void Login()
        {
            int a = 987;
            Debug.WriteLine(a);
            await Task.Delay(100);
        }
    }
}