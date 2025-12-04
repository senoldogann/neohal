using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using System.Linq;

namespace NeoHal.Desktop.Helpers;

/// <summary>
/// Tüm formlarda Enter tuşu ile input'lar arasında geçiş yapmayı sağlayan yardımcı sınıf
/// </summary>
public static class EnterNavigationHelper
{
    /// <summary>
    /// Bir UserControl'e Enter navigasyonu ekler
    /// </summary>
    public static void AttachToUserControl(UserControl control)
    {
        control.AddHandler(InputElement.KeyDownEvent, OnKeyDown, Avalonia.Interactivity.RoutingStrategies.Tunnel);
    }
    
    /// <summary>
    /// Bir Window'a Enter navigasyonu ekler
    /// </summary>
    public static void AttachToWindow(Window window)
    {
        window.AddHandler(InputElement.KeyDownEvent, OnKeyDown, Avalonia.Interactivity.RoutingStrategies.Tunnel);
    }
    
    private static void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        
        // Kaynak kontrolünü bul
        if (e.Source is not Control sourceControl) return;
        
        // Buton üzerinde Enter ise işlemi engelleme (butonun çalışmasını sağla)
        if (sourceControl is Button || IsInsideButton(sourceControl))
        {
            return;
        }
        
        // DataGrid içinde özel işlem gerekebilir, bu durumda atla
        if (IsInsideDataGrid(sourceControl))
        {
            // DataGrid kendi Enter işlemini yapsın, bu helper karışmasın
            return;
        }
        
        // Input kontrolü mü kontrol et (TextBox, NumericUpDown, AutoCompleteBox, ComboBox, DatePicker)
        var isInputControl = sourceControl is TextBox || 
                             sourceControl is NumericUpDown ||
                             sourceControl is AutoCompleteBox ||
                             sourceControl is ComboBox ||
                             sourceControl is DatePicker ||
                             IsInsideAutoCompleteBox(sourceControl) ||
                             IsInsideNumericUpDown(sourceControl) ||
                             IsInsideComboBox(sourceControl);
        
        if (!isInputControl) return;
        
        // Bir sonraki tab-edilebilir kontrole geç
        var next = KeyboardNavigationHandler.GetNext(sourceControl, NavigationDirection.Next);
        
        // Eğer null ise veya aynı parent içinde değilse, ana kontrolden aramaya başla
        if (next == null)
        {
            // En yakın focuslanabilir kontrolü bul
            var parent = sourceControl.GetVisualParent();
            while (parent != null && next == null)
            {
                if (parent is Control parentControl)
                {
                    next = KeyboardNavigationHandler.GetNext(parentControl, NavigationDirection.Next);
                }
                parent = parent.GetVisualParent();
            }
        }
        
        if (next != null)
        {
            next.Focus();
            
            // TextBox ise tüm metni seç
            if (next is TextBox tb)
            {
                tb.SelectAll();
            }
            
            e.Handled = true;
        }
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
}
