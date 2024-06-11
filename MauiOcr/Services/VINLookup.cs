using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MauiOcr.Model;
using System.Text.Json;

namespace MauiOcr.Services
{
    public class VINLookup
    {
        public async Task<string> GetVehicleDetails(string vin)
        {
            string url = $"https://vpic.nhtsa.dot.gov/api//vehicles/DecodeVinValues/{vin}?format=json";

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string result = await response.Content.ReadAsStringAsync();
                        return result;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception err)
                {
                    return null;
                }
            }
        }
        public static List<VehicleDetails> DeserializeVehicleDetails(string json)
        {
            List<VehicleDetails> vehicleDetailsList = new List<VehicleDetails>();

            try
            {
                var jsonObject = JsonDocument.Parse(json);

                if (jsonObject.RootElement.TryGetProperty("Results", out JsonElement resultsElement) && resultsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var result in resultsElement.EnumerateArray())
                    {
                        var vehicleDetails = new VehicleDetails
                        {
                            Make = result.GetProperty("Make").GetString(),
                            Model = result.GetProperty("Model").GetString(),
                            FuelTypePrimary = result.GetProperty("FuelTypePrimary").GetString(),
                            ModelYear = result.GetProperty("ModelYear").GetString()
                        };

                        vehicleDetailsList.Add(vehicleDetails);
                    }
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error deserializing JSON: {ex.Message}");
            }

            return vehicleDetailsList;
        }
        
    }

}
