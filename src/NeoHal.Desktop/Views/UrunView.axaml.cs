using System;
using Avalonia.Controls;
using NeoHal.Desktop.Helpers;
using NeoHal.Desktop.ViewModels;

namespace NeoHal.Desktop.Views;

public partial class UrunView : UserControl
{
    public UrunView()
    {
        InitializeComponent();
        EnterNavigationHelper.AttachToUserControl(this);
    }

    protected override async void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        
        if (DataContext is UrunViewModel viewModel)
        {
            await viewModel.LoadDataAsync();
        }
    }
}
