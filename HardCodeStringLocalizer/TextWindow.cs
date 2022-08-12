using System.Windows;
using System.Windows.Controls;

namespace HardCodeStringLocalizer
{
    public class TextWindow : System.Windows.Window
    {
        TextBox text = new TextBox { MinWidth = 150 };
        Button confirm = new Button { Content = "确认", IsDefault = true };
        public TextWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            confirm.Click += Confirm_Click;
            Loaded += TextWindow_Loaded;
            WindowStyle = WindowStyle.ToolWindow;
            Title = "请输入";
            SizeToContent = SizeToContent.WidthAndHeight;
            Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children = { text, confirm }
            };
        }

        private void TextWindow_Loaded(object sender, RoutedEventArgs e)
        {
            text.Focus();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(text.Text)) return;
            DialogResult = true;
        }

        public static bool GetText(System.Windows.Window owner, out string text)
        {
            var window = new TextWindow();
            window.Owner = owner;
            if (window.ShowDialog() == true)
            {
                text = window.text.Text;
                return true;
            }
            text = null;
            return false;
        }
    }
}
