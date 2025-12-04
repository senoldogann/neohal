using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using NeoHal.Core.Entities;

namespace NeoHal.Desktop.Views;

public partial class SevkiyatGirisView : UserControl
{
    private int _currentColumnIndex = 0;
    
    public SevkiyatGirisView()
    {
        InitializeComponent();
        
        Loaded += (s, e) =>
        {
            if (SubeComboBox != null)
            {
                SubeComboBox.SelectionChanged += SubeComboBox_SelectionChanged;
                
                // Şube seçildikten sonra Enter'a basınca DataGrid'e geç
                SubeComboBox.KeyDown += (sender, args) =>
                {
                    if (args.Key == Key.Enter && SubeComboBox.SelectedItem != null)
                    {
                        args.Handled = true;
                        FocusDataGridFirstCell();
                    }
                };
            }
            
            if (MainDataGrid != null)
            {
                MainDataGrid.AddHandler(KeyDownEvent, MainDataGrid_KeyDown, Avalonia.Interactivity.RoutingStrategies.Tunnel);
            }
        };
        
        KeyDown += OnKeyDown;
    }
    
    private void FocusDataGridFirstCell()
    {
        if (MainDataGrid == null) return;
        if (DataContext is not ViewModels.SevkiyatGirisViewModel vm) return;
        
        Dispatcher.UIThread.Post(() =>
        {
            // İlk satırı seç
            if (vm.Kalemler.Count > 0)
            {
                MainDataGrid.SelectedIndex = 0;
                vm.SelectedKalem = vm.Kalemler[0];
            }
            
            // İlk sütuna git ve düzenleme moduna al
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
    
    private void SubeComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is ViewModels.SevkiyatGirisViewModel vm && 
            SubeComboBox?.SelectedItem is CariHesap sube)
        {
            vm.SelectedSube = sube;
        }
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
        if (DataContext is not ViewModels.SevkiyatGirisViewModel vm) return;
        
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
        
        // Sütun indexleri: Ürün(0), KapTipi(1), KapAd(2), DaralıKg(3), Net(4-readonly), Fiyat(5), Tutar(6-readonly), Sil(7)
        // Düzenlenebilir: 0, 1, 2, 3, 5
        int[] editableColumnIndexes = { 0, 1, 2, 3, 5 };
        
        // Mevcut index'ten sonraki düzenlenebilir sütunu bul
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
            // Son sütundan sonra - yeni satır ekle
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
                        
                        // Düzenleme hücresine focus ver
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
                    
                    // Düzenleme hücresine focus ver
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
        if (DataContext is not ViewModels.SevkiyatGirisViewModel vm) return;
        
        switch (e.Key)
        {
            case Key.F2:
                SubeComboBox?.Focus();
                e.Handled = true;
                break;
                
            case Key.F3:
                vm.YeniCommand.Execute(null);
                e.Handled = true;
                break;
                
            case Key.F5:
                vm.KaydetCommand.Execute(null);
                e.Handled = true;
                break;
                
            case Key.F9:
                vm.FaturaKesCommand.Execute(null);
                e.Handled = true;
                break;
                
            case Key.F10:
                vm.YazdirCommand.Execute(null);
                e.Handled = true;
                break;
                
            case Key.F12:
                // Ürün arama - DataGrid'de ilk sütuna odaklan
                if (MainDataGrid?.SelectedItem != null)
                {
                    MainDataGrid.CurrentColumn = MainDataGrid.Columns.FirstOrDefault();
                    MainDataGrid.BeginEdit();
                }
                e.Handled = true;
                break;
                
            case Key.Escape:
                // Dialog açıksa kapat
                if (vm.FaturaKesDialogAcik)
                {
                    vm.FaturaKesDialogAcik = false;
                    e.Handled = true;
                }
                break;
        }
    }
}
