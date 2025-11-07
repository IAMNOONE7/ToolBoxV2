using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ToolBoxV2.Application;
using ToolBoxV2.Infrastracture;
using ToolBoxV2.Presentation.WPF.MVVM.View;
using ToolBoxV2.Presentation.WPF.MVVM.ViewModel;

namespace ToolBoxV2.Presentation.WPF
{ 
    public partial class App : System.Windows.Application
    {
        public static IHost AppHost { get; private set; } = null!;

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddApplication();
                    services.AddInfrastructure();

                    // viewmodels
                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<LocalMessagesViewModel>();

                    // windows
                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<LocalMessageView>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost.StartAsync();

            var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
            //mainWindow.DataContext = AppHost.Services.GetRequiredService<MainViewModel>();
            mainWindow.DataContext = AppHost.Services.GetRequiredService<LocalMessagesViewModel>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost.StopAsync();
            base.OnExit(e);
        }
    }

}
