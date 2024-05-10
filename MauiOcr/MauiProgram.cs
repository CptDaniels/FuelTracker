using Microsoft.Extensions.Logging;
using TesseractOcrMaui;
using Camera.MAUI;
using MauiOcr.ViewModel;
using CommunityToolkit.Maui;

namespace MauiOcr
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCameraView()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder.Services.AddLogging();

    #if DEBUG
    		builder.Logging.AddDebug();
    #endif

            builder.Services.AddTesseractOcr(
            files =>
            {
                // must have matching files in Resources/Raw folder
                //files.AddFile("eng.traineddata");
                files.AddFile("pol.traineddata");
                files.AddFile("osd.traineddata");
            });
            // Inject main page, so services are injected to its constructor
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<MainViewModel>();

            builder.Services.AddTransient<SelectPage>();
            builder.Services.AddTransient<SelectViewModel>();

            return builder.Build();
        }
    }
}
