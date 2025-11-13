using ICSharpCode.AvalonEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ToolBoxV2.Presentation.WPF.Helpers
{
    public static class AvalonEditBinding
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.RegisterAttached(
                "Text",
                typeof(string),
                typeof(AvalonEditBinding),
                new FrameworkPropertyMetadata(default(string),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnTextChanged));

        public static void SetText(DependencyObject d, string value)
            => d.SetValue(TextProperty, value);

        public static string GetText(DependencyObject d)
            => (string)d.GetValue(TextProperty);

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextEditor editor)
            {
                editor.TextChanged -= Editor_TextChanged;
                editor.Text = e.NewValue as string ?? string.Empty;
                editor.TextChanged += Editor_TextChanged;
            }
        }

        private static void Editor_TextChanged(object? sender, EventArgs e)
        {
            if (sender is TextEditor editor)
            {
                SetText(editor, editor.Text);
            }
        }
    }
}
