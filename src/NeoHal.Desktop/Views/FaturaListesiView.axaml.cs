using Avalonia.Controls;
using NeoHal.Desktop.Helpers;

namespace NeoHal.Desktop.Views;

public partial class FaturaListesiView : UserControl
{
    public FaturaListesiView()
    {
        InitializeComponent();
        EnterNavigationHelper.AttachToUserControl(this);
    }
}
