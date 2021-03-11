using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Prism.Ioc;
using Prism.Unity;
using Tidal.Client;
using Tidal.Client.Helpers;
using Tidal.Constants;
using Tidal.Dialogs.ViewModels;
using Tidal.Dialogs.Views;
using Tidal.Models.Messages;
using Tidal.Services.Abstract;
using Tidal.Services.Actual;
using Tidal.ViewModels;
using Tidal.Views;
using TinyIpc.Messaging;

namespace Tidal
{
    public partial class App : PrismApplication
    {
        private const string MutexName = @"Global\Tidal.Public.Net48";
        private const string IpcChannel = "Tidal.Public.Net48.IpcChannel";

        private TinyMessageBus listener;
        private Mutex globalMutex;

        protected override Window CreateShell() => Container.Resolve<ShellView>();


        protected override async void OnInitialized()
        {
            base.OnInitialized();
            await Task.CompletedTask;
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            // Try to create a global mutex. If this is the first instance then
            // the thisIsTheFirstInstance param will be true. If it's not, then
            // we need to send the command line arguments through to the
            // existing instance then GTFO.
            globalMutex = new Mutex(true, MutexName, out var thisIsTheFirstInstance);
            if (!thisIsTheFirstInstance)
            {
                await SendArgs(e.Args);
                Current.Shutdown();
            }

            // This is where we set up the side of the channel that the second
            // instance will talk to.
            listener = new TinyMessageBus(IpcChannel);
            listener.MessageReceived += Listener_MessageReceived;

            // Intercept mouse-up events and map the forward and back buttons to
            // navigation commands
            EventManager.RegisterClassHandler(typeof(Window),
                                              UIElement.PreviewMouseUpEvent,
                                              new MouseButtonEventHandler(OnPreviewMouseUp));

            base.OnStartup(e);

            Container.Resolve<IMessenger>().Send(new StartupMessage(e.Args, true));
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Container.Resolve<IMessenger>().Send(new ShutdownMessage());
            listener.MessageReceived -= Listener_MessageReceived;
            listener.Dispose();
            globalMutex.Dispose();
            base.OnExit(e);
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Client Services
            containerRegistry.RegisterSingleton<IClient, Client.Client>();

            // App Services
            containerRegistry.RegisterSingleton<IBrokerService, BrokerService>();
            containerRegistry.RegisterSingleton<IFileService, FileService>();
            containerRegistry.RegisterSingleton<IGeoService, GeoService>();
            containerRegistry.RegisterSingleton<IHostService, HostService>();
            containerRegistry.RegisterSingleton<IMessenger, Messenger>();
            containerRegistry.RegisterSingleton<INotificationService, NotificationService>();
            containerRegistry.RegisterSingleton<ISettingsService, SettingsService>();
            containerRegistry.RegisterSingleton<ITaskService, TaskService>();
            containerRegistry.RegisterSingleton<ITorrentStatusService, TorrentStatusService>();

            // Dialogs
            containerRegistry.RegisterDialog<FirstHostView, FirstHostViewModel>(PageKeys.FirstHost);
            containerRegistry.RegisterDialog<AddMagnetView, AddMagnetViewModel>(PageKeys.AddMagnet);
            containerRegistry.RegisterDialog<AddTorrentView, AddTorrentViewModel>(PageKeys.AddTorrent);
            containerRegistry.RegisterDialog<RemoveTorrentView, RemoveTorrentViewModel>(PageKeys.RemoveTorrents);
            containerRegistry.RegisterDialog<TorrentPropertiesView, TorrentPropertiesViewModel>(PageKeys.TorrentProperties);

            // Views
            containerRegistry.RegisterForNavigation<ShellView, ShellViewModel>();
            containerRegistry.RegisterForNavigation<HostView, HostViewModel>(PageKeys.Hosts);
            containerRegistry.RegisterForNavigation<MainView, MainViewModel>(PageKeys.Main);
            containerRegistry.RegisterForNavigation<SettingsView, SettingsViewModel>(PageKeys.Settings);
        }

        private static async Task SendArgs(string[] args)
        {
            // This is called only by the secondary instance and does nothing
            // more than send the current command line args to the first
            // instance via the TinyIpc.

            using (TinyMessageBus bus = new TinyMessageBus(IpcChannel))
            {
                byte[] argsAsJsonBytes = Json.ToJSONBytes(args);
                await bus.PublishAsync(argsAsJsonBytes);
            }
        }

        private void Listener_MessageReceived(object sender, TinyMessageReceivedEventArgs e)
        {
            // This is invoked by a second instance via the TinyIpc channel The
            // args might be empty, but we don't worry about that here, we
            // simply try to bring the MainWindow to the front (doesn't always
            // work), and send the args to the first instance.
            Current.Dispatcher.Invoke(() =>
            {
                var args = Json.ToObject<string[]>(e.Message);
                if (args != null && args.Length > 0)
                    Container.Resolve<IMessenger>().Send(new StartupMessage(args, false));

                if (Current.MainWindow.WindowState == WindowState.Minimized)
                    Current.MainWindow.WindowState = WindowState.Normal; //despite saying Normal this actually will restore
                Current.MainWindow.Activate(); //now activate window
            });
        }

        private void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // Map the Back and Forward buttons of the mouse to a
            // MouseNavigationMessage and send that.

            switch (e.ChangedButton)
            {
                case MouseButton.XButton1:
                    e.Handled = true;
                    Container.Resolve<IMessenger>().Send(new MouseNavMessage(MouseNavDirection.GoBack));
                    break;
                case MouseButton.XButton2:
                    e.Handled = true;
                    Container.Resolve<IMessenger>().Send(new MouseNavMessage(MouseNavDirection.GoForward));
                    break;
            }
        }
    }
}
