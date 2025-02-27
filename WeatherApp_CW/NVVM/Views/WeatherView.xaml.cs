using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherApp_CW.NVVM.ViewModels;

namespace WeatherApp_CW.NVVM.Views;

public partial class WeatherView : ContentPage
{
    public WeatherView()
    {
        InitializeComponent();
        BindingContext = new WeatherViewModel();
    }
}