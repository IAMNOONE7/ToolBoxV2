using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ToolBoxV2.Presentation.WPF.Helpers
{
    public static class TextBoxHelper
    {
        public static readonly DependencyProperty MeaningProperty =
       DependencyProperty.RegisterAttached(
           "Meaning", typeof(string), typeof(TextBoxHelper), new PropertyMetadata(string.Empty));

        public static void SetMeaning(UIElement element, string value)
        {
            element.SetValue(MeaningProperty, value);
        }

        public static string GetMeaning(UIElement element)
        {
            return (string)element.GetValue(MeaningProperty);
        }
    }
}
