using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherApp_CW.NVVM.Models
{
    public class SavedLocationWeather
    {
        public string LocationName { get; set; }
        public float Temperature { get; set; }
        public int WeatherCode { get; set; }
        public DateTime SavedAt { get; set; }
        public string UserId { get; set; }
    }


}
