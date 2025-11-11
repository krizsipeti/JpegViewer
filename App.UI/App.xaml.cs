using System;
using System.Runtime.InteropServices;
using JpegViewer.App.Core.Interfaces;
using JpegViewer.App.Core.Services;
using JpegViewer.App.UI.Services;
using JpegViewer.App.Vmd;
using JpegViewer.App.Vmd.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using WinRT.Interop;

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
            services.AddSingleton<ITimelineService, TimelineService>();
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

            // Load app icon
            var hwnd = WindowNative.GetWindowHandle(_window);

            // Path to your .ico in app output folder
            var exePath = System.AppContext.BaseDirectory;
            var iconPath = System.IO.Path.Combine(exePath, "Assets", "AppLogo.ico");

            // load large and small icons
            IntPtr hIconLarge = LoadImage(IntPtr.Zero, iconPath, IMAGE_ICON, 128, 128, LR_LOADFROMFILE);
            IntPtr hIconSmall = LoadImage(IntPtr.Zero, iconPath, IMAGE_ICON, 64, 64, LR_LOADFROMFILE);

            if (hIconLarge != IntPtr.Zero)
            {
                SendMessage(hwnd, WM_SETICON, (IntPtr)ICON_BIG, hIconLarge);
            }

            if (hIconSmall != IntPtr.Zero)
            {
                SendMessage(hwnd, WM_SETICON, (IntPtr)ICON_SMALL, hIconSmall);
            }
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

        #region Imports

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr LoadImage(IntPtr hinst, string lpszName, uint uType, int cx, int cy, uint fuLoad);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        const uint IMAGE_ICON = 1;
        const uint LR_LOADFROMFILE = 0x00000010;
        const uint WM_SETICON = 0x0080;
        const int ICON_SMALL = 0;
        const int ICON_BIG = 1;

        #endregion Imports
    }
}
