using Acr.UserDialogs;
// #if DEBUG
// using Plugin.Maui.DebugRainbows;
// #endif
using Plugin.Maui.OCR;
using BarcodeScanning;
using AiForms.Settings;
using PanCardView;
using SkiaSharp.Views.Maui.Controls.Hosting;
using FFImageLoading.Maui;
using UraniumUI;
using Polly;
using Polly.Retry;
using Shiny.Extensions.Stores;
using SpaghettiManager.App.Pages;
using SpaghettiManager.App.ViewModels;

namespace SpaghettiManager.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp
            .CreateBuilder()
            .UseMauiApp<App>()
            .UseShiny()
            .UseUserDialogs()
            .UseBarcodeScanning()
            .UseUraniumUI()
            .UseUraniumUIMaterial()
            .UseUraniumUIBlurs()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMediaElement()
            .UseMauiCommunityToolkitMarkup()
            .UseMauiCommunityToolkitCamera()
            .UseSettingsView()
            .UseFFImageLoading()
            .UseSkiaSharp()
            .UseCardsView()
            .UseOcr()
            .ConfigureEssentials(x => x
                //     .AddAppAction("app_info", "App Info", "Subtitle", "app_info_action_icon")
                //     .AddAppAction("battery_info", "Battery Info")
                .OnAppAction(y =>
                    Shiny.Hosting.Host.GetService<SpaghettiManager.App.Delegates.AppActionDelegate>()!.Handle(y))
            )
#if DEBUG
            //.UseDebugRainbows(
                //     new DebugRainbowOptions{
                //         ShowRainbows = true,
                //         ShowGrid = true,
                //         HorizontalItemSize = 20,
                //         VerticalItemSize = 20,
                //         MajorGridLineInterval = 4,
                //         MajorGridLines = new GridLineOptions { Color = Color.FromRgb(255, 0, 0), Opacity = 1, Width = 4 },
                //         MinorGridLines = new GridLineOptions { Color = Color.FromRgb(255, 0, 0), Opacity = 1, Width = 1 },
                //         GridOrigin = DebugGridOrigin.TopLeft,
                //     }
            //)
#endif
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddMaterialIconFonts();
            });

        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<MySqliteConnection>();
        builder.Services.AddSingleton<SpaghettiDatabase>();

        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<InventoryListPage>();
        builder.Services.AddTransient<InventoryDetailPage>();
        builder.Services.AddTransient<InventoryActionPage>();
        builder.Services.AddTransient<ScanPage>();
        builder.Services.AddTransient<ScanReviewPage>();
        builder.Services.AddTransient<CatalogHomePage>();
        builder.Services.AddTransient<CatalogSectionPage>();
        builder.Services.AddTransient<CatalogEanMappingsPage>();
        builder.Services.AddTransient<SettingsPage>();

        builder.Services.AddTransient<HomePageViewModel>();
        builder.Services.AddTransient<InventoryListViewModel>();
        builder.Services.AddTransient<InventoryDetailViewModel>();
        builder.Services.AddTransient<InventoryActionViewModel>();
        builder.Services.AddTransient<ScanPageViewModel>();
        builder.Services.AddTransient<ScanReviewViewModel>();
        builder.Services.AddTransient<CatalogHomeViewModel>();
        builder.Services.AddTransient<CatalogSectionViewModel>();
        builder.Services.AddTransient<CatalogEanMappingsViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Configuration.AddJsonPlatformBundle();
        // builder.AddRemoteConfigurationMaui("https://todo");
        // builder.Services.AddOptions<MyConfig>().BindConfiguration("");
#if DEBUG
        builder.Logging.SetMinimumLevel(LogLevel.Trace);
        builder.Logging.AddDebug();
#endif
        // pass false as second argument if you don't want to use built-in middleware
        builder.Services.AddShinyMediator(x => x
                .AddMediatorRegistry() // optional - enables registration scanning 
                .AddMauiPersistentCache()
                .AddDataAnnotations()
                .AddConnectivityBroadcaster()
                .AddResiliencyMiddleware(
                    ("Default", pipeline =>
                    {
                        pipeline.AddRetry(new RetryStrategyOptions
                        {
                            MaxRetryAttempts = 2,
                            MaxDelay = TimeSpan.FromSeconds(1.0),
                        });
                        pipeline.AddTimeout(TimeSpan.FromSeconds(5));
                    })
                )
                .UseMaui() // pass false here if you don't want to use built-in offline support with MAUI
        );
        builder.Services.AddSingleton(OcrPlugin.Default);
        builder.Services.AddSingleton(MediaPicker.Default);
        builder.Services.AddSingleton(FilePicker.Default);
        builder.Services.AddPersistentService<AppSettings>();
        builder.Services.AddJob(typeof(SpaghettiManager.App.Delegates.MyJob));
        builder.Services.AddHttpTransfers<SpaghettiManager.App.Delegates.MyHttpTransferDelegate>();
#if ANDROID
        // if you want http transfers to also show up as progress notifications, include this
        builder.Services.AddShinyService<Shiny.Net.Http.PerTransferNotificationStrategy>();
#endif
        builder.Services.AddPush<SpaghettiManager.App.Delegates.MyPushDelegate>();
        builder.Services.AddSingleton(Plugin.Maui.Audio.AudioManager.Current);

        // this is source generated by Shiny.Extensions.DependencyInjection - simply add [Singleton], [Scoped], or [Transient] to your classes
        builder.Services.AddGeneratedServices();

        var app = builder.Build();

        return app;
    }
}
