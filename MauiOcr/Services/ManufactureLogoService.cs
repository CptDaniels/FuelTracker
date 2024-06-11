using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MauiOcr.Model;

namespace MauiOcr.Services
{
    public static class ManufacturerLogoService
    {
        public static List<ManufacturerData> LoadManufacturerData(string filePath)
        {
            using (StreamReader file = File.OpenText(filePath))
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<ManufacturerData>>(file.ReadToEnd(), options);
            }
        }
    }
}
