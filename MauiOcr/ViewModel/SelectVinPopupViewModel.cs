using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Views;
using MauiOcr.Model;
using MauiOcr.Services;
using System.Windows.Input;

namespace MauiOcr.ViewModel
{
    public partial class SelectVinPopupViewModel : ObservableObject
    {
        private readonly FirebaseService firebaseService;

        public SelectVinPopupViewModel()
        {
            firebaseService = new FirebaseService();
            VinList = new ObservableCollection<string>();
            LoadVinsAsync();
            AddVinCommand = new Command(async () => await AddVinAsync());
        }

        [ObservableProperty]
        ObservableCollection<string> vinList;

        [ObservableProperty]
        string selectedVin;
        [ObservableProperty]
        string newVin;

        [ObservableProperty]
        string newVinMileage;
        [ObservableProperty]
        private DateTime date = DateTime.Today;

        public ICommand AddVinCommand { get; }
        public async Task AddVinAsync()
        {
            if (string.IsNullOrEmpty(selectedVin))
                return;

            if (VinList.Contains(selectedVin))
            {
                SelectVin();
                return;
            }
                
            string tempVin = selectedVin;
            VinList.Add(selectedVin);
            SelectedVin = tempVin;
            double newVinMileageDub;
            if (!double.TryParse(NewVinMileage, out newVinMileageDub))
            {
                return;
            }
            await firebaseService.AddFuelRecordAsync(SelectedVin, 0, newVinMileageDub, Date);

            SelectVin();
        }

        [RelayCommand]
        private void SelectVin()
        {
            if (SelectedVin != null)
            {
                MessagingCenter.Send(this, "VinSelected", SelectedVin);
            }
            ClosePopup();
        }

        [RelayCommand]
        private void ClosePopup()
        {
            var currentPopup = Application.Current.MainPage.Navigation.ModalStack.OfType<Popup>().FirstOrDefault();
            if (currentPopup != null)
            {
                currentPopup.Close();
            }
        }

        private async Task LoadVinsAsync()
        {
            var vins = await firebaseService.GetAllVINsAsync();
            foreach (var vin in vins)
            {
                VinList.Add(vin);
            }
        }
    }
}
