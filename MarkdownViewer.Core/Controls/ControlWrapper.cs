using Avalonia.Controls;

namespace MarkdownViewer.Core.Controls
{
    public class ControlWrapper : IControl
    {
        private readonly Control _control;

        public ControlWrapper(Control control)
        {
            _control = control;
        }

        public Control CreateControl()
        {
            return _control;
        }
    }
}
