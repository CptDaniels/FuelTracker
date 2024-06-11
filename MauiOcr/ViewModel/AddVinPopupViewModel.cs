using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiOcr.ViewModel
{
    public partial class AddVinPopupViewModel : ObservableObject
    {
        private readonly FirebaseService firebaseService;

        public AddVinPopupViewModel(string vin)
        {
            Vin = vin;
            firebaseService = new FirebaseService();
        }
        [ObservableProperty]
        private double fuelAmount;
        
        [ObservableProperty]
        private double mileage;
        
        [ObservableProperty]
        private DateTime date = DateTime.Today;

        public string Vin { get; set; }

        [RelayCommand]
        public async Task AddRecordAsync()
        {
            var records = await firebaseService.GetFuelRecordsAsync(Vin);

            double consumption = 0;

            if (records != null && records.Length > 0)
            {
                consumption = await GetLastConsumptionAsync();
                await firebaseService.AddFuelRecordAsync(Vin, consumption, Mileage, Date);
            }
            else
            {
                await firebaseService.AddFuelRecordAsync(Vin, 0, Mileage, Date);
            }
            string fuelConsumption = consumption.ToString();

            MessagingCenter.Send(this, "FuelRecordAdded", fuelConsumption);
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

        [RelayCommand]
        private async Task<double> GetLastConsumptionAsync()
        {
            var records = await firebaseService.GetLastFuelRecordAsync(Vin);
            double current = Mileage;

            if (records != null)
            {
                if (records.Mileage > 0 && FuelAmount > 0)
                {
                    double distance = current - records.Mileage;

                    if (distance > 0)
                    {
                        double consumption = (double)FuelAmount / distance * 100;
                        return Math.Round(consumption, 2);
                    }
                }
                else
                {
                    return records.FuelConsumption;
                }
            }

            return 0.0;
        }
    }
}
