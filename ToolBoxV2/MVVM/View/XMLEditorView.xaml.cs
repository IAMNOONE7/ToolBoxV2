using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
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
        private FoldingManager? _foldingManager;
        private XmlFoldingStrategy _foldingStrategy = new XmlFoldingStrategy();
        public XMLEditorView()
        {
            InitializeComponent();
            // Syntax highlighting
            XmlEditor.SyntaxHighlighting =
                HighlightingManager.Instance.GetDefinition("XML");

            // Folding
            _foldingManager = FoldingManager.Install(XmlEditor.TextArea);
            XmlEditor.TextChanged += (s, e) => UpdateFoldings();

            // Enable XML syntax highlighting
            TemplateEditor.SyntaxHighlighting =
                HighlightingManager.Instance.GetDefinition("XML");

            // Bind caret + selection updates
            TemplateEditor.TextArea.Caret.PositionChanged += TemplateEditor_CaretChanged;
            TemplateEditor.TextArea.SelectionChanged += TemplateEditor_SelectionChanged;
        }

        private void UpdateFoldings()
        {
            if (_foldingManager == null) return;
            _foldingStrategy.UpdateFoldings(_foldingManager, XmlEditor.Document);
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

        private void TemplateEditor_CaretChanged(object? sender, EventArgs e)
        {
            if (DataContext is XMLEditorViewModel vm)
            {
                vm.SelectionStart = TemplateEditor.CaretOffset;
                vm.SelectionLength = 0; // caret move clears selection
            }
        }

        private void TemplateEditor_SelectionChanged(object? sender, EventArgs e)
        {
            if (DataContext is XMLEditorViewModel vm)
            {
                var selection = TemplateEditor.TextArea.Selection;

                if (selection.IsEmpty)
                {
                    vm.SelectionStart = TemplateEditor.CaretOffset;
                    vm.SelectionLength = 0;
                }
                else
                {
                    // Convert line/column positions to offsets in the document
                    var startOffset = TemplateEditor.Document.GetOffset(
                        selection.StartPosition.Location);
                    var endOffset = TemplateEditor.Document.GetOffset(
                        selection.EndPosition.Location);

                    vm.SelectionStart = Math.Min(startOffset, endOffset);
                    vm.SelectionLength = Math.Abs(endOffset - startOffset);
                }
            }
        }
    }
}
