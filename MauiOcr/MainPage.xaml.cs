using MauiOcr.ViewModel;
using System.Runtime.InteropServices;

namespace MauiOcr
{
    public partial class MainPage : ContentPage
    {

        public MainPage(MainViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }

}
