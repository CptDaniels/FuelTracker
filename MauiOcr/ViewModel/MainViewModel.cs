using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TesseractOcrMaui;
using TesseractOcrMaui.Enums;
using TesseractOcrMaui.Results;
using MauiOcr.Services;
using MauiOcr.Model;
using CommunityToolkit.Maui.Core;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Microsoft.Maui;
using System.Text.Json.Nodes;
using System.Linq;
using CommunityToolkit.Maui.Views;
using System.Globalization;


namespace MauiOcr.ViewModel
{
    public partial class MainViewModel : ObservableObject,INotifyPropertyChanged
    {
        private readonly FirebaseService firebaseService;
        private readonly IImagePass _imagePass;
        private readonly VINLookup vinlookup;

        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<FuelModel> FuelRecords { get; }
        private List<ManufacturerData> manufacturerLogos;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public MainViewModel(ITesseract tesseract,IImagePass imagePass)
        {
            Tesseract = tesseract;
            _imagePass = imagePass;
            firebaseService = new FirebaseService();
            FuelRecords = new ObservableCollection<FuelModel>();
            vinlookup = new VINLookup();
            Vin = Preferences.Get("LastVIN", "");
            ConsumptionOut = Preferences.Get("LastConsumption", "");
            LoadManufacturerLogos();
            if (!string.IsNullOrEmpty(Vin))
            {
                LoadRecord();
            }
            MessagingCenter.Subscribe<SelectVinPopupViewModel, string>(this, "VinSelected", (sender, selectedVin) =>
            {
                Vin = selectedVin;
                LoadRecord();
            });
            MessagingCenter.Subscribe<AddVinPopupViewModel, string>(this, "FuelRecordAdded", (sender, fuelConsumption) =>
            {
                ConsumptionOut = fuelConsumption;
                OnPropertyChanged(nameof(ConsumptionOut));
                LoadRecord();
            });
        }
        ITesseract Tesseract { get; }    

        [ObservableProperty]
        string output;
        [ObservableProperty]
        string conf;
        [ObservableProperty]
        ImageSource imageSource;

        [ObservableProperty]
        double fuelAmount;
        [ObservableProperty]
        double mileage;
        [ObservableProperty]
        DateTime date = DateTime.Today;
        [ObservableProperty]
        string showMake;
        [ObservableProperty]
        string showModel;
        [ObservableProperty]
        string showFuelType;
        [ObservableProperty]
        string showModelYear;
        [ObservableProperty]
        private string averageFuelConsumption;
        [ObservableProperty]
        private string logo;

        private string consumptionOut;
        public string ConsumptionOut
        {
            get => consumptionOut;
            set
            {
                if (SetProperty(ref consumptionOut, value))
                {
                    Preferences.Set("LastConsumption", value);
                }
            }
        }

        private string vin;
        public string Vin
        {
            get => vin;
            set
            {
                if (SetProperty(ref vin, value))
                {
                    Preferences.Set("LastVIN", value);
                }
            }
        }
        [RelayCommand]
        private void OpenVinPopup()
        {
            var vm = new SelectVinPopupViewModel();
            var popup = new SelectVinPopup(vm);
            Application.Current.MainPage.ShowPopup(popup);
        }
        [RelayCommand]
        private void AddVinPopup()
        {
            var vm = new AddVinPopupViewModel(Vin);
            var popup = new AddVinPopup(vm);
            Application.Current.MainPage.ShowPopup(popup);
        }
        [RelayCommand]
        private async Task LoadVehicleDetailsAsync()
        {
            if (!string.IsNullOrEmpty(Vin))
                {
                string json = await vinlookup.GetVehicleDetails(Vin);
                if (json != null)
                {
                    var vehicleDetailsList = VINLookup.DeserializeVehicleDetails(json);
                    if (vehicleDetailsList.Count > 0)
                    {
                        ShowMake = vehicleDetailsList[0].Make;
                        OnPropertyChanged(nameof(ShowMake));
                        ShowModel = vehicleDetailsList[0].Model;
                        OnPropertyChanged(nameof(ShowModel));
                        ShowModelYear = vehicleDetailsList[0].ModelYear;
                        OnPropertyChanged(nameof(ShowModelYear));
                        ShowFuelType = vehicleDetailsList[0].FuelTypePrimary;
                        UpdateManufacturerLogo();
                    }
                }
            }
        }

        private void LoadManufacturerLogos()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(baseDirectory, "Resources/Json/data.json");
            manufacturerLogos = ManufacturerLogoService.LoadManufacturerData(filePath);
        }
        private void UpdateManufacturerLogo()
        {
            if (manufacturerLogos != null && !string.IsNullOrEmpty(ShowMake))
            {
                string formattedShowMake = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(ShowMake.ToLower());

                var manufacturer = manufacturerLogos.FirstOrDefault(m => m.Name == formattedShowMake);
                if (manufacturer != null)
                {
                    Logo = manufacturer.Image.Optimized;
                    OnPropertyChanged(nameof(Logo));
                }
                else
                {
                    Logo = null;
                }
            }
            else
            {
                Logo = null;
            }
        }
        [RelayCommand]
        private async Task LoadRecord()
        {
            var records = await firebaseService.GetFuelRecordsAsync(Vin);
            FuelRecords.Clear();

            double totalConsumption = 0;
            int count = 0;

            foreach (var record in records)
            {
                FuelRecords.Add(record);
                if (record.FuelConsumption > 0)
                {
                    totalConsumption += record.FuelConsumption;
                    count++;
                }
            }
            double sum = count > 0 ? totalConsumption / count : 0;
            sum = Math.Round(sum, 2);
            AverageFuelConsumption = sum.ToString("F2");

            OnPropertyChanged(nameof(AverageFuelConsumption));

            await LoadVehicleDetailsAsync();
        }

        [RelayCommand]
        Task Navigate() => Shell.Current.GoToAsync(nameof(SelectPage));


        [RelayCommand]
        private async Task GetText()
        {
            //var pickResult = await GetUserSelectedImagePath();
            byte[] pickResult = GetImageBytes();

            if (pickResult is null)
            {
                return;
            }
            Tesseract.EngineConfiguration = (engine) =>
            {
                engine.DefaultSegmentationMode = TesseractOcrMaui.Enums.PageSegmentationMode.Auto;
                //engine.SetCharacterWhitelist("0123456789");   // These characters ocr is looking for
                //engine.SetCharacterBlacklist("abc");        // These characters ocr is not looking for

            };
            var result = await Tesseract.RecognizeTextAsync(pickResult);

            Conf = $"Confidence: {result.Confidence}";
            if (result.NotSuccess())
            {
                Output = $"Recognizion failed: {result.Status}";
                return;
            }
            Output = result.RecognisedText;

            ImageSource = ImageSource.FromStream(() => new MemoryStream(pickResult));
        }

        public byte[] GetImageBytes()
        {
            return _imagePass.ImageBytes;
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
