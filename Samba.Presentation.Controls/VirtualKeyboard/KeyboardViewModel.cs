using Microsoft.Practices.Prism.Commands;

namespace Samba.Presentation.Controls.VirtualKeyboard
{
    public class KeyboardViewModel
    {
        public VKeyboard Model { get; set; }
        public DelegateCommand<VKey> PressKeyCommand { get; set; }

        public KeyboardViewModel()
        {
            Model = new VKeyboard();
            PressKeyCommand = new DelegateCommand<VKey>(OnKeyPress);
        }

        private void OnKeyPress(VKey obj)
        {
            Model.ProcessKey(obj.VirtualKey);
        }
    }
}
