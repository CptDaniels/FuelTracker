using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiOcr.Services
{
    public interface IStringService
    {
        string FilePath { get; set; }
    }

    public class StringService : IStringService
    {
        public string FilePath { get; set; }
    }
}
