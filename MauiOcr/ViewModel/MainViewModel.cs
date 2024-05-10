using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TesseractOcrMaui;
using TesseractOcrMaui.Enums;
using TesseractOcrMaui.Results;
using MauiOcr.Services;

namespace MauiOcr.ViewModel
{
    public partial class MainViewModel : ObservableObject
    {
        //private readonly IStringService _stringService;
        public MainViewModel(ITesseract tesseract/*, IStringService stringService*/)
        {
            Tesseract = tesseract;
            //_stringService = stringService;
        }
        ITesseract Tesseract { get; }    

        [ObservableProperty]
        string output;
        [ObservableProperty]
        string conf;

        [RelayCommand]
        Task Navigate() => Shell.Current.GoToAsync(nameof(SelectPage));


        [RelayCommand]
        private async Task GetText()
        {
            //string pickResult = _stringService.FilePath;
            var pickResult = await GetUserSelectedImagePath();
            //var pickResult = SelectPage.FilePath;

            // null if user cancelled the operation
            if (pickResult is null)
            {
                return;
            }
            Tesseract.EngineConfiguration = (engine) =>
            {
                engine.DefaultSegmentationMode = TesseractOcrMaui.Enums.PageSegmentationMode.AutoOsd;
                //engine.SetCharacterWhitelist("0123456789");   // These characters ocr is looking for
                //engine.SetCharacterBlacklist("abc");        // These characters ocr is not looking for

            };
            // Recognize image
            var result = await Tesseract.RecognizeTextAsync(pickResult);

            // Show output
            Conf = $"Confidence: {result.Confidence}";
            if (result.NotSuccess())
            {
                Output = $"Recognizion failed: {result.Status}";
                return;
            }
            Output = result.RecognisedText;
        }

        private static async Task<string?> GetUserSelectedImagePath()
        {
        var pickResult = await FilePicker.PickAsync(new PickOptions()
        {
            PickerTitle = "Pick jpeg or png image",
            // Currently usable image types are png and jpeg
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>()
            {
                [DevicePlatform.Android] = new List<string>() { "image/png", "image/jpeg" },
                [DevicePlatform.WinUI] = new List<string>() { ".png", ".jpg", ".jpeg" },
            })
        });
        return pickResult?.FullPath;
        }
    }
}
