using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Linq;
using System.Threading.Tasks;
using MauiOcr.Model;

public class FirebaseService
{
    private readonly FirebaseAuthProvider authProvider;
    private readonly FirebaseClient firebaseClient;
    private readonly string apiKey = "AIzaSyDjUpmkuwx6BYlVyoNq3AU9gemaTq0h4iQ";
    private readonly string databaseURL = "https://fueltracker-23763-default-rtdb.europe-west1.firebasedatabase.app/";

    public FirebaseService()
    {
        authProvider = new FirebaseAuthProvider(new FirebaseConfig(apiKey));
        firebaseClient = new FirebaseClient(databaseURL);
    }

    public async Task<string> SignInAnonymouslyAsync()
    {
        var auth = await authProvider.SignInAnonymouslyAsync();
        return auth.User.LocalId;
    }

    public async Task AddFuelRecordAsync(string vin, double fuelConsumption, double mileage, DateTime date)
    {
        var record = new FuelModel
        {
            FuelConsumption = fuelConsumption,
            Mileage = mileage,
            Date = date
        };
        await firebaseClient.Child("vehicles").Child(vin).PostAsync(record);
    }

    public async Task<FuelModel[]> GetFuelRecordsAsync(string vin)
    {
        var records = await firebaseClient.Child("vehicles").Child(vin).OnceAsync<FuelModel>();
        return records.Select(r => r.Object).ToArray();
    }
    public async Task<List<string>> GetAllVINsAsync()
    {
        var vinRecords = await firebaseClient.Child("vehicles").OnceAsync<object>();
        return vinRecords.Select(r => r.Key).ToList();
    }
    public async Task<FuelModel> GetLastFuelRecordAsync(string vin)
    {
        var records = await GetFuelRecordsAsync(vin);

        if (records.Length > 0)
        {
            return records[records.Length - 1];
        }
        else
        {
            Console.WriteLine($"No fuel records found for VIN: {vin}");
            return null;
        }
    }
}