using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using NeoHal.Desktop.ViewModels;

namespace NeoHal.Desktop.Views;

public partial class HizliSatisView : UserControl
{
    public HizliSatisView()
    {
        InitializeComponent();
        
        // Klavye kısayollarını yakala
        KeyDown += OnKeyDown;
        
        // Enter navigasyonu için tunnel handler
        AddHandler(KeyDownEvent, OnInputKeyDown, RoutingStrategies.Tunnel);
    }
    
    /// <summary>
    /// Input alanlarında Enter tuşu ile navigasyon
    /// </summary>
    private void OnInputKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        if (e.Source is not Control sourceControl) return;
        
        // Buton üzerinde Enter ise çalışmasına izin ver
        if (sourceControl is Button || IsInsideButton(sourceControl)) return;
        
        var vm = DataContext as HizliSatisViewModel;
        if (vm == null) return;
        
        // Fiyat alanında Enter → Yeni satır ekle
        if (IsFiyatField(sourceControl))
        {
            e.Handled = true;
            vm.YeniSatirCommand.Execute(null);
            Dispatcher.UIThread.Post(() => FocusLastRowFirstControl(), DispatcherPriority.Background);
            return;
        }
        
        // AutoCompleteBox veya NumericUpDown içinde → Sonraki input'a geç
        if (IsEditableInputField(sourceControl))
        {
            e.Handled = true;
            FocusNextEditableInput(sourceControl);
            return;
        }
    }
    
    /// <summary>
    /// Fiyat alanı mı kontrol et (Column 6)
    /// </summary>
    private bool IsFiyatField(Control control)
    {
        var numericUpDown = control as NumericUpDown ?? FindParent<NumericUpDown>(control);
        if (numericUpDown == null) return false;
        
        // Grid içindeki sütun pozisyonunu kontrol et
        var parent = numericUpDown.GetVisualParent();
        while (parent != null)
        {
            if (parent is Grid)
            {
                var column = Grid.GetColumn(numericUpDown);
                // Column 6 = BirimFiyat sütunu (HizliSatisView'da)
                if (column == 6)
                {
                    // KalemlerList içinde mi?
                    var itemsControl = FindParent<ItemsControl>(numericUpDown);
                    if (itemsControl?.Name == "KalemlerList")
                    {
                        return true;
                    }
                }
                break;
            }
            parent = parent.GetVisualParent();
        }
        return false;
    }
    
    /// <summary>
    /// Düzenlenebilir input alanı mı?
    /// </summary>
    private bool IsEditableInputField(Control control)
    {
        return control is TextBox || 
               control is NumericUpDown ||
               control is AutoCompleteBox ||
               IsInsideAutoCompleteBox(control) ||
               IsInsideNumericUpDown(control);
    }
    
    /// <summary>
    /// Sonraki düzenlenebilir input alanına git
    /// </summary>
    private void FocusNextEditableInput(Control current)
    {
        // Mevcut kalem satırını bul
        var kalemBorder = FindParent<Border>(current);
        if (kalemBorder?.DataContext is not HizliSatisKalem currentKalem) return;
        
        var grid = kalemBorder.Child as Grid;
        if (grid == null) return;
        
        // Mevcut kontrolün sütununu bul
        var currentNumeric = current as NumericUpDown ?? FindParent<NumericUpDown>(current);
        var currentAutoComplete = current as AutoCompleteBox ?? FindParent<AutoCompleteBox>(current);
        
        int currentColumn = -1;
        if (currentNumeric != null) currentColumn = Grid.GetColumn(currentNumeric);
        else if (currentAutoComplete != null) currentColumn = Grid.GetColumn(currentAutoComplete);
        
        // Düzenlenebilir sütunlar: 0=Ürün, 1=Kap, 2=Adet, 3=Brüt, 6=Fiyat
        int[] editableColumns = { 0, 1, 2, 3, 6 };
        
        // Sonraki düzenlenebilir sütunu bul
        int nextColumnIndex = -1;
        for (int i = 0; i < editableColumns.Length; i++)
        {
            if (editableColumns[i] == currentColumn && i < editableColumns.Length - 1)
            {
                nextColumnIndex = editableColumns[i + 1];
                break;
            }
        }
        
        if (nextColumnIndex >= 0 && nextColumnIndex < grid.Children.Count)
        {
            var nextControl = grid.Children[nextColumnIndex];
            if (nextControl is AutoCompleteBox acb) acb.Focus();
            else if (nextControl is NumericUpDown nud) nud.Focus();
        }
    }
    
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not HizliSatisViewModel vm) return;
        
        // F tuşları - Windows ve macOS uyumlu (fn tuşu ile birlikte çalışır)
        switch (e.Key)
        {
            case Key.F1: // F1 - Alıcı ara
                AliciComboBox?.Focus();
                e.Handled = true;
                return;
                
            case Key.F2: // F2 - Ürün ara
                FocusCurrentRowControl(0);
                e.Handled = true;
                return;
                
            case Key.F3: // F3 - Kap ara
                FocusCurrentRowControl(1);
                e.Handled = true;
                return;
                
            case Key.F5: // F5 - Yeni satır
                vm.YeniSatirCommand.Execute(null);
                Dispatcher.UIThread.Post(() => FocusLastRowFirstControl());
                e.Handled = true;
                return;
                
            case Key.F9: // F9 - Fatura oluştur
                vm.FaturaOlusturCommand.Execute(null);
                e.Handled = true;
                return;
                
            case Key.Escape:
                vm.IptalCommand.Execute(null);
                e.Handled = true;
                return;
        }
        
        // MacBook uyumlu kısayollar: ⌘ (Command) + tuş
        var isMeta = e.KeyModifiers.HasFlag(KeyModifiers.Meta); // ⌘ tuşu (macOS)
        var isCtrl = e.KeyModifiers.HasFlag(KeyModifiers.Control); // Ctrl tuşu (Windows/Linux)
        var isCmd = isMeta || isCtrl;
        
        if (isCmd)
        {
            switch (e.Key)
            {
                case Key.D1: // ⌘1 veya Ctrl+1 - Alıcı listesi
                    AliciComboBox?.Focus();
                    e.Handled = true;
                    break;
                    
                case Key.D2: // ⌘2 veya Ctrl+2 - Ürün listesi
                    FocusCurrentRowControl(0);
                    e.Handled = true;
                    break;
                    
                case Key.D3: // ⌘3 veya Ctrl+3 - Kap listesi
                    FocusCurrentRowControl(1);
                    e.Handled = true;
                    break;
                    
                case Key.D5: // ⌘5 veya Ctrl+5 - Yeni satır
                    vm.YeniSatirCommand.Execute(null);
                    Avalonia.Threading.Dispatcher.UIThread.Post(() => FocusLastRowFirstControl());
                    e.Handled = true;
                    break;
                    
                case Key.S: // ⌘S veya Ctrl+S - Kaydet/Fatura oluştur
                    vm.FaturaOlusturCommand.Execute(null);
                    e.Handled = true;
                    break;
            }
        }
    }
    
    private void FocusCurrentRowControl(int columnIndex)
    {
        if (DataContext is HizliSatisViewModel vm && vm.Kalemler.Count > 0)
        {
            var itemsControl = this.FindControl<ItemsControl>("KalemlerList");
            if (itemsControl != null)
            {
                var lastItem = vm.Kalemler.LastOrDefault();
                if (lastItem != null)
                {
                    var container = itemsControl.ContainerFromItem(lastItem);
                    if (container is ContentPresenter cp && cp.Child is Border border && border.Child is Grid grid)
                    {
                        if (columnIndex < grid.Children.Count)
                        {
                            var control = grid.Children[columnIndex];
                            if (control is AutoCompleteBox acb)
                            {
                                acb.Focus();
                            }
                            else if (control is NumericUpDown nud)
                            {
                                nud.Focus();
                            }
                            else if (control is TextBox tb)
                            {
                                tb.Focus();
                            }
                        }
                    }
                }
            }
        }
    }
    
    private void FocusLastRowFirstControl()
    {
        FocusCurrentRowControl(0);
    }
    
    private static T? FindParent<T>(Avalonia.Visual? visual) where T : Avalonia.Visual
    {
        while (visual != null)
        {
            if (visual is T result) return result;
            visual = visual.GetVisualParent();
        }
        return null;
    }
    
    private static bool IsInsideButton(Control control)
    {
        return FindParent<Button>(control) != null;
    }
    
    private static bool IsInsideAutoCompleteBox(Control control)
    {
        return FindParent<AutoCompleteBox>(control) != null;
    }
    
    private static bool IsInsideNumericUpDown(Control control)
    {
        return FindParent<NumericUpDown>(control) != null;
    }
}
