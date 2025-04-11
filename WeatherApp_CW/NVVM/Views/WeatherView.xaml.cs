using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherApp_CW.NVVM.ViewModels;

namespace WeatherApp_CW.NVVM.Views;

public partial class WeatherView : ContentPage
{
    private WeatherViewModel vm;
    public WeatherView()
    {
        InitializeComponent();
        vm = new WeatherViewModel();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!string.IsNullOrEmpty(searchBar.Text))
        {
            var vm = BindingContext as WeatherViewModel;
            if (vm != null)
            {
                vm.SearchCommand.Execute(searchBar.Text);
            }
        }
    }

}