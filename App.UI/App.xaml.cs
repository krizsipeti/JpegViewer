using System;
using JpegViewer.App.Core.Interfaces;
using JpegViewer.App.Core.Services;
using JpegViewer.App.UI.Services;
using JpegViewer.App.Vmd;
using JpegViewer.App.Vmd.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace JpegViewer.App.UI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;

        /// <summary>
        /// Holds the service provider for dependency injection.
        /// </summary>
        public IServiceProvider Services { get; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();

            // Setup dependency injection regarding viewmodels
            var services = new ServiceCollection();
            services.AddSingleton<VmdMainWindow>();
            services.AddSingleton<VmdCtrlFolderPicker>();
            services.AddSingleton<VmdCtrlPhoto>();
            services.AddSingleton<VmdCtrlTimeline>();
            services.AddSingleton<IDispatcherService, DispatcherService>();
            services.AddSingleton<IImageService, ImageService>();
            Services = services.BuildServiceProvider();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _window.Activate();
        }

        /// <summary>
        /// Retrieves a service of the specified type from the application's service provider.
        /// </summary>
        public static T GetService<T>() where T : class
        {
            if ((Current as App)!.Services.GetService(typeof(T)) is not T service)
            {
                throw new ArgumentException($"{typeof(T)} needs to be registered in App constructor within App.xaml.cs.");
            }

            return service;
        }
    }
}
