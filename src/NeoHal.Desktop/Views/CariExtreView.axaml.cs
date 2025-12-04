using Avalonia.Controls;
using NeoHal.Desktop.Helpers;

namespace NeoHal.Desktop.Views;

public partial class CariExtreView : UserControl
{
    public CariExtreView()
    {
        InitializeComponent();
        EnterNavigationHelper.AttachToUserControl(this);
    }
}
