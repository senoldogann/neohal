using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using NeoHal.Core.Entities;

namespace NeoHal.Desktop.Views;

public partial class HalKayitView : UserControl
{
    private int _currentColumnIndex = 0;
    
    public HalKayitView()
    {
        InitializeComponent();
        
        Loaded += (s, e) =>
        {
            if (MainDataGrid != null)
            {
                MainDataGrid.AddHandler(KeyDownEvent, MainDataGrid_KeyDown, Avalonia.Interactivity.RoutingStrategies.Tunnel);
                
                // İlk yüklemede DataGrid'e odaklan
                FocusDataGridFirstCell();
            }
        };
        
        KeyDown += OnKeyDown;
    }
    
    private void FocusDataGridFirstCell()
    {
        if (MainDataGrid == null) return;
        if (DataContext is not ViewModels.HalKayitViewModel vm) return;
        
        Dispatcher.UIThread.Post(() =>
        {
            if (vm.Kalemler.Count > 0)
            {
                MainDataGrid.SelectedIndex = 0;
                vm.SelectedKalem = vm.Kalemler[0];
            }
            
            if (MainDataGrid.Columns.Count > 0)
            {
                _currentColumnIndex = 0;
                MainDataGrid.CurrentColumn = MainDataGrid.Columns[0];
                MainDataGrid.Focus();
                MainDataGrid.BeginEdit();
                
                Dispatcher.UIThread.Post(() => FocusCurrentEditingCell(), DispatcherPriority.Input);
            }
        }, DispatcherPriority.Background);
    }
    
    private void MainDataGrid_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            HandleEnterInDataGrid();
        }
    }
    
    private void HandleEnterInDataGrid()
    {
        if (MainDataGrid == null) return;
        if (DataContext is not ViewModels.HalKayitViewModel vm) return;
        
        var columns = MainDataGrid.Columns;
        var currentColumn = MainDataGrid.CurrentColumn;
        
        if (currentColumn == null)
        {
            _currentColumnIndex = 0;
        }
        else
        {
            _currentColumnIndex = columns.IndexOf(currentColumn);
        }
        
        MainDataGrid.CommitEdit();
        
        // Sütun indexleri: Komisyoncu(0), Ürün(1), KapTipi(2), KapAdet(3), DaralıKg(4), NetKg(5-readonly), Fiyat(6), Tutar(7-readonly), İşlem(8)
        // Düzenlenebilir: 0, 1, 2, 3, 4, 6
        int[] editableColumnIndexes = { 0, 1, 2, 3, 4, 6 };
        
        int nextEditableIndex = -1;
        foreach (var idx in editableColumnIndexes)
        {
            if (idx > _currentColumnIndex)
            {
                nextEditableIndex = idx;
                break;
            }
        }
        
        if (nextEditableIndex == -1)
        {
            var currentKalem = vm.SelectedKalem;
            if (currentKalem != null && currentKalem.Urun != null)
            {
                vm.YeniSatirCommand.Execute(null);
                
                Dispatcher.UIThread.Post(() =>
                {
                    MainDataGrid.SelectedIndex = vm.Kalemler.Count - 1;
                    _currentColumnIndex = 0;
                    
                    if (columns.Count > 0)
                    {
                        MainDataGrid.CurrentColumn = columns[0];
                        MainDataGrid.BeginEdit();
                        
                        Dispatcher.UIThread.Post(() => FocusCurrentEditingCell(), DispatcherPriority.Input);
                    }
                }, DispatcherPriority.Background);
            }
        }
        else
        {
            Dispatcher.UIThread.Post(() =>
            {
                _currentColumnIndex = nextEditableIndex;
                if (nextEditableIndex < columns.Count)
                {
                    MainDataGrid.CurrentColumn = columns[nextEditableIndex];
                    MainDataGrid.BeginEdit();
                    
                    Dispatcher.UIThread.Post(() => FocusCurrentEditingCell(), DispatcherPriority.Input);
                }
            }, DispatcherPriority.Background);
        }
    }
    
    private void FocusCurrentEditingCell()
    {
        if (MainDataGrid == null) return;
        
        // DataGrid içindeki aktif düzenleme hücresini bul
        var control = MainDataGrid.GetVisualDescendants()
            .OfType<Control>()
            .Where(c => c.Focusable && c.IsVisible && c.IsEffectivelyEnabled)
            .FirstOrDefault(c => c is TextBox || c is AutoCompleteBox || c is ComboBox || c is NumericUpDown);
        
        if (control != null)
        {
            // AutoCompleteBox ise içindeki TextBox'ı bul
            if (control is AutoCompleteBox acb)
            {
                var innerTextBox = acb.GetVisualDescendants().OfType<TextBox>().FirstOrDefault();
                if (innerTextBox != null)
                {
                    innerTextBox.Focus(NavigationMethod.Directional);
                    innerTextBox.SelectAll();
                    innerTextBox.CaretIndex = innerTextBox.Text?.Length ?? 0;
                }
                else
                {
                    acb.Focus(NavigationMethod.Directional);
                }
            }
            else if (control is TextBox tb)
            {
                tb.Focus(NavigationMethod.Directional);
                tb.SelectAll();
                tb.CaretIndex = tb.Text?.Length ?? 0;
            }
            else if (control is ComboBox cb)
            {
                cb.Focus(NavigationMethod.Directional);
            }
            else
            {
                control.Focus(NavigationMethod.Directional);
            }
        }
    }
    
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not ViewModels.HalKayitViewModel vm) return;
        
        switch (e.Key)
        {
            case Key.F3:
                vm.YeniCommand.Execute(null);
                e.Handled = true;
                break;
                
            case Key.F5:
                vm.KaydetCommand.Execute(null);
                e.Handled = true;
                break;
                
            case Key.F6:
                vm.OnaytaCommand.Execute(null);
                e.Handled = true;
                break;
        }
    }
}
