using Avalonia.Controls;
using Avalonia.Input;
using NeoHal.Desktop.Helpers;
using NeoHal.Desktop.ViewModels;

namespace NeoHal.Desktop.Views;

public partial class GirisIrsaliyesiView : UserControl
{
    public GirisIrsaliyesiView()
    {
        InitializeComponent();
        EnterNavigationHelper.AttachToUserControl(this);
        KeyDown += OnKeyDown;
    }
    
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not GirisIrsaliyesiListViewModel vm) return;
        
        // Platform-agnostic kısayollar (hem Windows Ctrl hem macOS ⌘)
        var isMeta = e.KeyModifiers.HasFlag(KeyModifiers.Meta);
        var isCtrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        var isCmd = isMeta || isCtrl;
        
        switch (e.Key)
        {
            case Key.F5: // F5 - Yenile
                vm.RefreshCommand.Execute(null);
                e.Handled = true;
                break;
                
            case Key.F6: // F6 - Onayla
                if (vm.SelectedIrsaliye != null)
                {
                    vm.OnaylaCommand.Execute(null);
                    e.Handled = true;
                }
                break;
                
            case Key.Delete: // Delete/Backspace - Sil
            case Key.Back:
                if (vm.SelectedIrsaliye != null)
                {
                    vm.DeleteCommand.Execute(null);
                    e.Handled = true;
                }
                break;
                
            case Key.Enter: // Enter - Düzenle
                if (vm.SelectedIrsaliye != null)
                {
                    vm.EditIrsaliyeCommand.Execute(null);
                    e.Handled = true;
                }
                break;
        }
        
        // Cmd/Ctrl + tuş kombinasyonları
        if (isCmd)
        {
            switch (e.Key)
            {
                case Key.R: // Cmd+R / Ctrl+R - Yenile
                    vm.RefreshCommand.Execute(null);
                    e.Handled = true;
                    break;
            }
        }
    }
}
