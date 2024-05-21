using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiOcr.Services
{
    public interface IImagePass
    {
         byte[] ImageBytes { get; set; }
    }
    public class ImagePass : IImagePass
    {
        public byte[] ImageBytes { get; set; }
    }
}
