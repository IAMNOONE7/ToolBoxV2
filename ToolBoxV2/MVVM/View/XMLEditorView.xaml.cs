using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ToolBoxV2.Domain.XMLEditor;
using ToolBoxV2.Presentation.WPF.MVVM.ViewModel;

namespace ToolBoxV2.Presentation.WPF.MVVM.View
{
    /// <summary>
    /// Interaction logic for XMLEditorView.xaml
    /// </summary>
    public partial class XMLEditorView : UserControl
    {
        public XMLEditorView()
        {
            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // When user clicks on an XML node in the TreeView,
            // we notify the ViewModel so it can load that node’s raw XML into the editor.
            if (DataContext is XMLEditorViewModel vm && e.NewValue is XMLNodeModel node)
            {
                vm.OnNodeSelected(node);
            }
        }

        private void XmlTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            // Forwards the current caret/selection position from the TextBox
            // to the ViewModel (so VM knows which substring to replace later).
            if (DataContext is XMLEditorViewModel vm && sender is TextBox tb)
            {
                vm.SelectionStart = tb.SelectionStart;
                vm.SelectionLength = tb.SelectionLength;
            }
        }
    }
}
