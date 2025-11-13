using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Configuration;
using System.Data;
using System.Windows;
using ToolBoxV2.Application;
using ToolBoxV2.Infrastracture;
using ToolBoxV2.Presentation.WPF.MVVM.View;
using ToolBoxV2.Presentation.WPF.MVVM.ViewModel;
using ToolBoxV2.Presentation.WPF.Services;

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

                    services.AddSingleton<IFileDialogService, FileDialogService>();

                    // viewmodels
                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<LocalMessagesViewModel>();
                    services.AddSingleton<XMLEditorViewModel>();
                    services.AddSingleton<InitViewModel>();

                    // windows
                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<LocalMessageView>();
                    services.AddSingleton<XMLEditorView>();
                    services.AddSingleton<InitView>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost.StartAsync();

            var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
            mainWindow.DataContext = AppHost.Services.GetRequiredService<MainViewModel>();            
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
