using MauiOcr.ViewModel;
using CommunityToolkit.Maui.Views;

namespace MauiOcr;

public partial class SelectVinPopup : Popup
{
    public SelectVinPopup(SelectVinPopupViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}