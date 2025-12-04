using Avalonia.Controls;
using NeoHal.Desktop.Helpers;

namespace NeoHal.Desktop.Views;

public partial class UrunGrubuView : UserControl
{
    public UrunGrubuView()
    {
        InitializeComponent();
        EnterNavigationHelper.AttachToUserControl(this);
    }
}
