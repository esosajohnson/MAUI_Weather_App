using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WeatherApp_CW.NVVM.Models
{
    public partial class SavedLocationWeather : ObservableObject
    {
        [ObservableProperty]
        private string locationName;

        [ObservableProperty]
        private float temperature;

        [ObservableProperty]
        private int weatherCode;

        [ObservableProperty]
        private DateTime savedAt;

        [ObservableProperty]
        private string userId;

        [ObservableProperty]
        private string firebaseKey;
    }


}
