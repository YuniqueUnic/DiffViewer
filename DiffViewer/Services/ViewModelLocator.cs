using DiffViewer.Managers;
using Microsoft.Extensions.DependencyInjection;
using MvvmDialogs;
using Serilog;
using Serilog.Events;
using System;

namespace DiffViewer.Services;

public class ViewModelLocator
{
    public ViewModelLocator( )
    {
        // init IOC Container and register VM services
        Services = InitIoc();
    }

    public IServiceProvider Services { get; set; }


    /// <summary>
    /// Init IOC Container
    /// </summary>
    /// <returns></returns>
    public IServiceProvider InitIoc( )
    {
        // create IOC Container
        var services = new ServiceCollection();

        // register application services
        services.AddSingleton<ILogger>(_ =>
        {
            return new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Debug()
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Debug)
                    .WriteTo.Console())
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Information)
                    .WriteTo.File(      AppConfigManager.LogPath ,
                                       outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}" ,
                                       rollingInterval: RollingInterval.Hour ,
                                       fileSizeLimitBytes: 1024 * 1024 * 10 ,
                                       retainedFileCountLimit: 31 ,
                                       rollOnFileSizeLimit: true ,
                                       flushToDiskInterval: new(
                                                                          0 ,
                                                                          0 ,
                                                                          30
                                                                      ) ,
                                       buffered: false))
                .CreateLogger();
        });
        services.AddSingleton<IDialogService>(_ => { return new DialogService(); });
        services.AddSingleton<AppConfigManager>();

        // register VM services
        services.AddTransient<ViewModels.AboutViewModel>();
        services.AddTransient<ViewModels.VSTSSettingViewModel>();
        services.AddTransient<ViewModels.MainWindowViewModel>();

        // register View services
        services.AddTransient<IWindow[]>(sp => new IWindow[]
        {
            new Views.AboutWindow { DataContext = About_ViewModel } ,
            new Views.RawDataWindow(),
            new Views.VSTSSettingWindow{ DataContext = VSTSSetting_ViewModel} ,
        });

        services.AddSingleton<Views.MainWindow>(sp => new Views.MainWindow { DataContext = MainWindow_ViewModel });

        // return IOC Container
        return services.BuildServiceProvider();
    }

    public ILogger Logger => Services.GetRequiredService<ILogger>();


    #region ViewModels

    public ViewModels.MainWindowViewModel MainWindow_ViewModel => Services.GetRequiredService<ViewModels.MainWindowViewModel>();
    public ViewModels.AboutViewModel About_ViewModel => Services.GetRequiredService<ViewModels.AboutViewModel>();
    public ViewModels.VSTSSettingViewModel VSTSSetting_ViewModel => Services.GetRequiredService<ViewModels.VSTSSettingViewModel>();

    #endregion ViewModels


    #region Views

    public Views.MainWindow Main_Window => Services.GetRequiredService<Views.MainWindow>();

    /// <summary>
    /// Get all IWindow instances
    /// </summary>
    public IWindow[] ITransientWindowsCollection => Services.GetRequiredService<IWindow[]>();

    #endregion Views
}
