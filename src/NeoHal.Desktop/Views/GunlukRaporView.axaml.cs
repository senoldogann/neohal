using Avalonia.Controls;
using NeoHal.Desktop.Helpers;

namespace NeoHal.Desktop.Views;

public partial class GunlukRaporView : UserControl
{
    public GunlukRaporView()
    {
        InitializeComponent();
        EnterNavigationHelper.AttachToUserControl(this);
    }
}
