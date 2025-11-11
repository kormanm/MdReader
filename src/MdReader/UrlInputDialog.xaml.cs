using System.Windows;
using System.Windows.Input;

namespace MdReader;

public partial class UrlInputDialog : Window
{
    public string Url => UrlTextBox.Text;

    public UrlInputDialog()
    {
        InitializeComponent();
        UrlTextBox.Focus();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void UrlTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            DialogResult = true;
            e.Handled = true;
        }
    }
}
