using CommunityToolkit.Mvvm.Messaging;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using System.Windows.Threading;

using CommunityToolkit.DependencyInjection;

namespace SampleApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    [STAThread]
    public static void Main(string[] args)
    {
        // TODO: Ideally this is what I want (kind of! Probably using services.UseSourceGeneratedDefaultConstructors() inside of CreateHostBuilder() is the ideal approach)
        /*
        using IHost host = CreateHostBuilder(args).UseSourceGeneratedDefaultConstructors(config =>
            {
                config.SomeOption = 42; // modify the configuration (if needed)
            }
        ).Build();
        */

        // TODO: Because the above does not work, I am using this workaround for now
        using IHost host = CreateHostBuilder(args).BuildWithSourceGeneratedDefaultConstructors(config =>
        {
            config.SomeOption = 42; // modify the configuration (if needed)
        });

        host.Start();

        App app = new();
        app.InitializeComponent();
        app.MainWindow = host.Services.GetRequiredService<MainWindow>();
        app.MainWindow.Visibility = Visibility.Visible;
        app.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder)
            => configurationBuilder.AddUserSecrets(typeof(App).Assembly))
        .ConfigureServices((hostContext, services) =>
        {
            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<MyUserControlViewModel>();

            services.AddSingleton<WeakReferenceMessenger>();
            services.AddSingleton<IMessenger, WeakReferenceMessenger>(provider => provider.GetRequiredService<WeakReferenceMessenger>());

            services.AddSingleton(_ => Current.Dispatcher);

            services.AddTransient<ISnackbarMessageQueue>(provider =>
            {
                Dispatcher dispatcher = provider.GetRequiredService<Dispatcher>();
                return new SnackbarMessageQueue(TimeSpan.FromSeconds(3.0), dispatcher);
            });
        });
}
