using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using NeoHal.Desktop.ViewModels;

namespace NeoHal.Desktop.Views;

public partial class GirisIrsaliyesiWindow : Window
{
    private DataGrid? _mainDataGrid;
    
    public GirisIrsaliyesiWindow()
    {
        InitializeComponent();
        KeyDown += OnKeyDown;
        
        // DataGrid referansını al
        _mainDataGrid = this.FindControl<DataGrid>("MainDataGrid");
        
        // Enter navigasyonu için tüm TextBox ve NumericUpDown'ları dinle
        AddHandler(KeyDownEvent, OnInputKeyDown, Avalonia.Interactivity.RoutingStrategies.Tunnel);
    }
    
    // Düzenlenebilir sütun indeksleri:
    // 0: Komisyoncu, 1: Ürün, 2: KapTipi, 3: KapAdet, 4: DaraliKg, 5: NetKg(RO), 6: Fiyat, 7: Tutar(RO), 8: Butonlar
    private static readonly int[] EditableColumnIndexes = { 0, 1, 2, 3, 4, 6 };
    
    private void OnInputKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        
        // Kaynak kontrolünü bul
        if (e.Source is not Control sourceControl) return;
        
        // DataGrid içinde miyiz kontrol et
        var dataGrid = sourceControl.FindAncestorOfType<DataGrid>();
        if (dataGrid != null && dataGrid == _mainDataGrid)
        {
            e.Handled = true;
            
            // DataGrid'deki mevcut hücreyi bul
            var currentColumn = dataGrid.CurrentColumn;
            
            if (currentColumn != null)
            {
                // Mevcut sütun indeksini al
                var columnIndex = dataGrid.Columns.IndexOf(currentColumn);
                
                // Edit modundan çık
                dataGrid.CommitEdit();
                
                // Sonraki düzenlenebilir sütunu bul
                var currentEditableIndex = Array.IndexOf(EditableColumnIndexes, columnIndex);
                
                if (currentEditableIndex >= 0 && currentEditableIndex < EditableColumnIndexes.Length - 1)
                {
                    // Sonraki düzenlenebilir sütuna geç
                    var nextColumnIndex = EditableColumnIndexes[currentEditableIndex + 1];
                    
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        dataGrid.CurrentColumn = dataGrid.Columns[nextColumnIndex];
                        dataGrid.BeginEdit();
                        
                        // Focus'u düzenleme kontrolüne ver
                        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                        {
                            // Editing kontrollerini bul ve focus ver
                            var autoComplete = dataGrid.GetVisualDescendants().OfType<AutoCompleteBox>()
                                .FirstOrDefault(a => a.IsVisible && a.IsKeyboardFocusWithin == false);
                            var textBox = dataGrid.GetVisualDescendants().OfType<TextBox>()
                                .Where(t => t.IsVisible && !(t.Parent is AutoCompleteBox))
                                .FirstOrDefault();
                            
                            // AutoCompleteBox sütunları: 0, 1, 2
                            if (nextColumnIndex <= 2 && autoComplete != null)
                            {
                                autoComplete.Focus();
                            }
                            else if (textBox != null)
                            {
                                textBox.Focus();
                                textBox.SelectAll();
                            }
                        }, Avalonia.Threading.DispatcherPriority.Background);
                    }, Avalonia.Threading.DispatcherPriority.Input);
                }
                else
                {
                    // Son düzenlenebilir sütundaysa (Fiyat), yeni satır ekle
                    if (DataContext is GirisIrsaliyesiEditViewModel vm)
                    {
                        vm.YeniSatirCommand.Execute(null);
                        
                        // Yeni satırın ilk hücresine (Komisyoncu) git
                        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                        {
                            var itemCount = vm.Kalemler.Count;
                            if (itemCount > 0)
                            {
                                dataGrid.SelectedIndex = itemCount - 1;
                                dataGrid.CurrentColumn = dataGrid.Columns[0]; // Komisyoncu sütunu
                                dataGrid.BeginEdit();
                                
                                // AutoCompleteBox'a focus
                                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                                {
                                    var autoComplete = dataGrid.GetVisualDescendants()
                                        .OfType<AutoCompleteBox>()
                                        .FirstOrDefault(a => a.IsVisible);
                                    autoComplete?.Focus();
                                }, Avalonia.Threading.DispatcherPriority.Background);
                            }
                        }, Avalonia.Threading.DispatcherPriority.Background);
                    }
                }
                
                return;
            }
        }
        
        // DataGrid dışındaki kontroller için standart navigasyon
        var isInputControl = sourceControl is TextBox || 
                             sourceControl is NumericUpDown ||
                             sourceControl is AutoCompleteBox ||
                             sourceControl.GetVisualParent() is AutoCompleteBox ||
                             sourceControl.GetVisualParent() is NumericUpDown;
        
        if (!isInputControl) return;
        
        // Bir sonraki tab-edilebilir kontrole geç
        var nextControl = KeyboardNavigationHandler.GetNext(sourceControl, NavigationDirection.Next);
        if (nextControl != null)
        {
            nextControl.Focus();
            e.Handled = true;
        }
    }
    
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        
        if (DataContext is GirisIrsaliyesiEditViewModel vm)
        {
            vm.CloseRequested += (sender, result) =>
            {
                Close(result);
            };
        }
    }
    
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not GirisIrsaliyesiEditViewModel vm) return;
        
        // F tuşları - Windows ve macOS uyumlu
        switch (e.Key)
        {
            case Key.F1: // F1 - DataGrid’e fokusla
                MainDataGrid?.Focus();
                e.Handled = true;
                return;
                
            case Key.F2: // F2 - Ürün listesi
                e.Handled = true;
                return;
                
            case Key.F3: // F3 - Kap listesi
                e.Handled = true;
                return;
                
            case Key.F5: // F5 - Yeni satır
                vm.YeniSatirCommand.Execute(null);
                e.Handled = true;
                return;
                
            case Key.F9: // F9 - Kaydet
                vm.SaveCommand.Execute(null);
                e.Handled = true;
                return;
                
            case Key.Escape:
                vm.CancelCommand.Execute(null);
                e.Handled = true;
                return;
        }
        
        // MacBook uyumlu kısayollar: ⌘ (Command) + tuş
        var isMeta = e.KeyModifiers.HasFlag(KeyModifiers.Meta);
        var isCtrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        var isCmd = isMeta || isCtrl;
        
        if (isCmd)
        {
            switch (e.Key)
            {
                case Key.D1: // ⌘１ veya Ctrl+1 - DataGrid’e fokusla
                    MainDataGrid?.Focus();
                    e.Handled = true;
                    break;
                    
                case Key.D2: // ⌘2 veya Ctrl+2 - Ürün listesi
                    e.Handled = true;
                    break;
                    
                case Key.D3: // ⌘3 veya Ctrl+3 - Kap listesi
                    e.Handled = true;
                    break;
                    
                case Key.D5: // ⌘5 veya Ctrl+5 - Yeni satır
                    vm.YeniSatirCommand.Execute(null);
                    e.Handled = true;
                    break;
                    
                case Key.S: // ⌘S veya Ctrl+S - Kaydet
                    vm.SaveCommand.Execute(null);
                    e.Handled = true;
                    break;
            }
        }
    }
}
