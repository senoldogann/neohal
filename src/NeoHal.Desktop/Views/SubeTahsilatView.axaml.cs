using Avalonia.Controls;
using NeoHal.Desktop.Helpers;

namespace NeoHal.Desktop.Views;

public partial class SubeTahsilatView : UserControl
{
    public SubeTahsilatView()
    {
        InitializeComponent();
        EnterNavigationHelper.AttachToUserControl(this);
    }
}
