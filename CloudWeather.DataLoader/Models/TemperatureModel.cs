using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudWeather.DataLoader.Models
{
    public class TemperatureModel
    {
        public DateTime CreatedOn { get; set; }
        public decimal AmountInches { get; set; }
        public string WeatherType { get; set; }
        public string ZipCode { get; set; }
        public decimal TempLowF { get; set; }
        public decimal TempHighF { get; set; }

    }
}
