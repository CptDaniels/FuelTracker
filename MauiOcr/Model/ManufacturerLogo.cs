using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiOcr.Model
{
    public class ManufacturerData
    {
        public string Name { get; set; }
        public ManufacturerImage Image { get; set; }
    }

    public class ManufacturerImage
    {
        public string Source { get; set; }
        public string Thumb { get; set; }
        public string Optimized { get; set; }
        public string Original { get; set; }
        public string LocalThumb { get; set; }
        public string LocalOptimized { get; set; }
        public string LocalOriginal { get; set; }
    }
}
