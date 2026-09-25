// Stand-ins for the WPF and WinForms types the view mocks derive from, so the corpus compiles on every OS.
namespace System.Windows
{
    public class DependencyProperty
    {
        public static DependencyProperty Register(string name, global::System.Type propertyType, global::System.Type ownerType, PropertyMetadata typeMetadata) => null!;
    }
    public class PropertyMetadata
    {
        public PropertyMetadata(object? defaultValue) { }
    }
    public class DependencyObject
    {
        public object GetValue(DependencyProperty dp) => null!;
        public void SetValue(DependencyProperty dp, object value) { }
    }
    public class UIElement : DependencyObject { }
    public class FrameworkElement : UIElement { }
    public class Window : FrameworkElement { }
}
namespace System.Windows.Controls
{
    public class UserControl : System.Windows.FrameworkElement { }
    public class Page : System.Windows.FrameworkElement { }
}
namespace System.Windows.Forms
{
    public enum DockStyle
    {
        None,
        Fill,
    }

    public class Control : global::System.ComponentModel.Component
    {
        public ControlCollection Controls { get; } = new();
        public DockStyle Dock { get; set; }
        public void SuspendLayout() { }
        public void ResumeLayout() { }
    }

    public sealed class ControlCollection : global::System.Collections.Generic.IEnumerable<Control>
    {
        private readonly global::System.Collections.Generic.List<Control> controls = new();

        public int Count => controls.Count;
        public void Add(Control control) => controls.Add(control);
        public void Clear() => controls.Clear();
        public void Remove(Control? control)
        {
            if (control is not null)
            {
                controls.Remove(control);
            }
        }

        public global::System.Collections.Generic.IEnumerator<Control> GetEnumerator() => controls.GetEnumerator();
        global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class Form : Control { }
    public class UserControl : Control { }
}

