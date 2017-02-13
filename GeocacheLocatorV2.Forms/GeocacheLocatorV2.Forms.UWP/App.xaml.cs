using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Caliburn.Micro;
using GeocacheLocatorV2.PCL;
using GeocacheLocatorV2.PCL.ViewModels;

namespace GeocacheLocatorV2.UWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App
    {
        private WinRTContainer _container;

        public App()
        {
            InitializeComponent();
        }

        protected override void Configure()
        {
            _container = new WinRTContainer();
            _container.RegisterWinRTServices();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (args.PrelaunchActivated)
                return;

            Xamarin.FormsMaps.Init(ApiKeys.XamarinFormsMapUwpKey);
            Xamarin.Forms.Forms.Init(args);

            Debug.WriteLine(args.PreviousExecutionState);

            if (args.PreviousExecutionState == ApplicationExecutionState.Terminated ||
                      args.PreviousExecutionState == ApplicationExecutionState.NotRunning ||
                      args.PreviousExecutionState == ApplicationExecutionState.ClosedByUser)
                DisplayRootView<MainPage>();

            Window.Current.Activate();
        }

        protected override object GetInstance(Type service, string key)
        {
            return _container.GetInstance(service, key);
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            return new[]
            {
                GetType().GetTypeInfo().Assembly,
                typeof (ShellViewModel).GetTypeInfo().Assembly
            };
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }
    }
}
