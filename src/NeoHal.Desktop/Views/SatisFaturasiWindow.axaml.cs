using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using NeoHal.Core.Entities;
using NeoHal.Desktop.ViewModels;

namespace NeoHal.Desktop.Views;

public partial class SatisFaturasiWindow : Window
{
    public SatisFaturasiWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        KeyDown += OnKeyDown;
        
        // Enter navigasyonu için tunnel handler ekle
        AddHandler(KeyDownEvent, OnInputKeyDown, RoutingStrategies.Tunnel);
    }
    
    private void OnInputKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        
        // Kaynak kontrolünü bul
        if (e.Source is not Control sourceControl) return;
        
        // DataGrid içinde özel işlem - karışmayalım
        if (IsInsideDataGrid(sourceControl)) return;
        
        // Buton üzerinde Enter ise çalışmasına izin ver
        if (sourceControl is Button || IsInsideButton(sourceControl)) return;
        
        var vm = DataContext as SatisFaturasiEditViewModel;
        
        // Fiyat alanında Enter basılırsa - yeni satır ekle
        if (IsFiyatField(sourceControl))
        {
            e.Handled = true;
            
            // Önce mevcut satırın hesaplamasını yap
            if (vm != null)
            {
                RecalculateCurrentKalem(sourceControl);
                
                // Yeni satır ekle
                vm.AddKalemCommand.Execute(null);
                
                // İlk sütuna (Ürün) focuslan
                Dispatcher.UIThread.Post(() => FocusLastRowFirstColumn(), DispatcherPriority.Background);
            }
            return;
        }
        
        // Düzenlenebilir alan mı kontrol et
        if (IsKalemEditableField(sourceControl))
        {
            e.Handled = true;
            FocusNextEditableInKalem(sourceControl);
            return;
        }
        
        // Diğer input alanları için varsayılan Enter davranışı (bir sonraki kontrole geç)
        var isInputControl = sourceControl is TextBox || 
                             sourceControl is NumericUpDown ||
                             sourceControl is AutoCompleteBox ||
                             sourceControl is ComboBox ||
                             sourceControl is DatePicker ||
                             IsInsideAutoCompleteBox(sourceControl) ||
                             IsInsideNumericUpDown(sourceControl) ||
                             IsInsideComboBox(sourceControl);
        
        if (!isInputControl) return;
        
        // Bir sonraki kontrole geç
        var next = KeyboardNavigationHandler.GetNext(sourceControl, NavigationDirection.Next);
        if (next != null)
        {
            next.Focus();
            if (next is TextBox tb) tb.SelectAll();
            e.Handled = true;
        }
    }
    
    /// <summary>
    /// Kalem satırındaki düzenlenebilir alan mı?
    /// </summary>
    private bool IsKalemEditableField(Control control)
    {
        var itemsControl = FindParent<ItemsControl>(control);
        if (itemsControl?.Name != "KalemlerList") return false;
        
        return control is AutoCompleteBox || 
               control is NumericUpDown ||
               IsInsideAutoCompleteBox(control) ||
               IsInsideNumericUpDown(control);
    }
    
    /// <summary>
    /// Kalem satırında sonraki düzenlenebilir alana git
    /// Sıra: Ürün(0) → Kap(1) → Adet(2) → NetKg(3) → Fiyat(4) → Yeni satır
    /// </summary>
    private void FocusNextEditableInKalem(Control current)
    {
        // Mevcut kalem satırını bul
        var kalemBorder = FindParent<Border>(current);
        if (kalemBorder?.DataContext is not SatisFaturasiKalem) return;
        
        var grid = kalemBorder.Child as Grid;
        if (grid == null) return;
        
        // Mevcut kontrolün sütununu bul
        var currentNumeric = current as NumericUpDown ?? FindParent<NumericUpDown>(current);
        var currentAutoComplete = current as AutoCompleteBox ?? FindParent<AutoCompleteBox>(current);
        
        int currentColumn = -1;
        if (currentNumeric != null) currentColumn = Grid.GetColumn(currentNumeric);
        else if (currentAutoComplete != null) currentColumn = Grid.GetColumn(currentAutoComplete);
        
        // SatisFaturasiWindow kalem sütunları (sadeleştirilmiş):
        // 0=Ürün(AutoComplete), 1=Kap(AutoComplete), 2=Adet, 3=NetKg, 4=Fiyat, 
        // 5=Tutar(RO), 6=Rusum(RO), 7=Komisyon(RO), 8=Stopaj(RO), 9=Sil
        int[] editableColumns = { 0, 1, 2, 3, 4 };
        
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
        
        if (nextColumnIndex >= 0)
        {
            // Grid.Children'dan doğru sütundaki kontrolü bul
            foreach (var child in grid.Children)
            {
                if (child is Control c && Grid.GetColumn(c) == nextColumnIndex)
                {
                    if (c is AutoCompleteBox acb) 
                    {
                        acb.Focus();
                        return;
                    }
                    else if (c is NumericUpDown nud) 
                    {
                        nud.Focus();
                        return;
                    }
                }
            }
        }
    }
    
    private bool IsFiyatField(Control control)
    {
        // NumericUpDown içindeki TextBox'ı da kontrol et
        var numericUpDown = control as NumericUpDown;
        if (numericUpDown == null)
        {
            numericUpDown = control.GetVisualParent() as NumericUpDown;
        }
        
        if (numericUpDown == null) return false;
        
        // Kalem satırı içindeki BirimFiyat alanı mı?
        var parent = numericUpDown.GetVisualParent();
        while (parent != null)
        {
            if (parent is Grid grid)
            {
                var column = Grid.GetColumn(numericUpDown);
                // Column 4 = BirimFiyat sütunu
                if (column == 4)
                {
                    // ItemsControl içinde mi kontrol et
                    var itemsControl = FindParent<ItemsControl>(numericUpDown);
                    if (itemsControl?.Name == "KalemlerList")
                    {
                        return true;
                    }
                }
            }
            parent = parent.GetVisualParent();
        }
        return false;
    }
    
    private void FocusLastRowFirstColumn()
    {
        if (DataContext is not SatisFaturasiEditViewModel vm) return;
        if (vm.Kalemler.Count == 0) return;
        
        // KalemlerList'teki son satırın ilk AutoCompleteBox'ını bul
        var itemsControl = this.FindControl<ItemsControl>("KalemlerList");
        if (itemsControl == null) return;
        
        // Son container'ı bul
        var containers = itemsControl.GetVisualDescendants()
            .OfType<Border>()
            .Where(b => b.DataContext is SatisFaturasiKalem)
            .ToList();
        
        var lastContainer = containers.LastOrDefault();
        if (lastContainer == null) return;
        
        // İçindeki ilk AutoCompleteBox'ı bul (Ürün seçimi)
        var autoCompleteBox = lastContainer.GetVisualDescendants()
            .OfType<AutoCompleteBox>()
            .FirstOrDefault();
        
        autoCompleteBox?.Focus();
    }
    
    private void OnFiyatLostFocus(object? sender, RoutedEventArgs e)
    {
        RecalculateCurrentKalem(sender as Control);
    }
    
    private void RecalculateCurrentKalem(Control? control)
    {
        if (control == null) return;
        if (DataContext is not SatisFaturasiEditViewModel vm) return;
        
        // Kalem DataContext'ini bul
        var kalem = FindKalemFromControl(control);
        if (kalem != null)
        {
            vm.RecalculateKalemCommand.Execute(kalem);
        }
    }
    
    private SatisFaturasiKalem? FindKalemFromControl(Control control)
    {
        var parent = control as Avalonia.Visual;
        while (parent != null)
        {
            if (parent is Control c && c.DataContext is SatisFaturasiKalem kalem)
            {
                return kalem;
            }
            parent = parent.GetVisualParent();
        }
        return null;
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
    
    private static bool IsInsideDataGrid(Control control)
    {
        var parent = control.GetVisualParent();
        while (parent != null)
        {
            if (parent is DataGrid) return true;
            parent = parent.GetVisualParent();
        }
        return false;
    }
    
    private static bool IsInsideButton(Control control)
    {
        var parent = control.GetVisualParent();
        while (parent != null)
        {
            if (parent is Button) return true;
            parent = parent.GetVisualParent();
        }
        return false;
    }
    
    private static bool IsInsideAutoCompleteBox(Control control)
    {
        var parent = control.GetVisualParent();
        while (parent != null)
        {
            if (parent is AutoCompleteBox) return true;
            parent = parent.GetVisualParent();
        }
        return false;
    }
    
    private static bool IsInsideNumericUpDown(Control control)
    {
        var parent = control.GetVisualParent();
        while (parent != null)
        {
            if (parent is NumericUpDown) return true;
            parent = parent.GetVisualParent();
        }
        return false;
    }
    
    private static bool IsInsideComboBox(Control control)
    {
        var parent = control.GetVisualParent();
        while (parent != null)
        {
            if (parent is ComboBox) return true;
            parent = parent.GetVisualParent();
        }
        return false;
    }
    
    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is SatisFaturasiEditViewModel vm)
        {
            vm.CloseRequested += (s, saved) => Close();
            vm.PrintPreviewRequested += OnPrintPreviewRequested;
        }
    }
    
    private async void OnPrintPreviewRequested(object? sender, (string Title, string Html, Action<bool> Callback) args)
    {
        try
        {
            // HTML içeriğini geçici dosyaya kaydet
            var tempFile = Path.Combine(Path.GetTempPath(), $"NeoHal_Fatura_{DateTime.Now:yyyyMMddHHmmss}.html");
            await File.WriteAllTextAsync(tempFile, args.Html);
            
            // Platforma göre tarayıcıda aç
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = tempFile,
                    UseShellExecute = true
                });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", tempFile);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", tempFile);
            }
            
            args.Callback(true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Yazdırma hatası: {ex.Message}");
            args.Callback(false);
        }
    }
    
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not SatisFaturasiEditViewModel vm) return;
        
        // F tuşları - Windows ve macOS uyumlu
        switch (e.Key)
        {
            case Key.F1: // F1 - Alıcı
                AliciAutoComplete?.Focus();
                e.Handled = true;
                return;
                
            case Key.F2: // F2 - Ürün listesi
                e.Handled = true;
                return;
                
            case Key.F3: // F3 - Kap listesi
                e.Handled = true;
                return;
                
            case Key.F4: // F4 - İrsaliye ekle
                IrsaliyeAutoComplete?.Focus();
                e.Handled = true;
                return;
                
            case Key.F5: // F5 - Yeni satır
                vm.AddKalemCommand.Execute(null);
                e.Handled = true;
                return;
                
            case Key.F9: // F9 - Kaydet
                vm.SaveCommand.Execute(null);
                e.Handled = true;
                return;
                
            case Key.F10: // F10 - Fatura Kes
                vm.FaturaKesCommand.Execute(null);
                e.Handled = true;
                return;
                
            case Key.F11: // F11 - Yazdır
                vm.PrintCommand.Execute(null);
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
                case Key.D1: // ⌘1 veya Ctrl+1 - Alıcı
                    AliciAutoComplete?.Focus();
                    e.Handled = true;
                    break;
                    
                case Key.D2: // ⌘2 veya Ctrl+2 - Ürün listesi
                    e.Handled = true;
                    break;
                    
                case Key.D3: // ⌘3 veya Ctrl+3 - Kap listesi
                    e.Handled = true;
                    break;
                    
                case Key.D4: // ⌘4 veya Ctrl+4 - İrsaliye ekle
                    IrsaliyeAutoComplete?.Focus();
                    e.Handled = true;
                    break;
                    
                case Key.D5: // ⌘5 veya Ctrl+5 - Yeni satır
                    vm.AddKalemCommand.Execute(null);
                    e.Handled = true;
                    break;
                    
                case Key.S: // ⌘S veya Ctrl+S - Kaydet
                    vm.SaveCommand.Execute(null);
                    e.Handled = true;
                    break;
                    
                case Key.P: // ⌘P veya Ctrl+P - Yazdır
                    vm.PrintCommand.Execute(null);
                    e.Handled = true;
                    break;
            }
        }
    }
}
