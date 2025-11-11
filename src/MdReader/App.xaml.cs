using System.Windows;

namespace MdReader;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Handle command line arguments for file opening
        if (e.Args.Length > 0)
        {
            foreach (var arg in e.Args)
            {
                if (MainWindow is MainWindow mainWindow)
                {
                    mainWindow.OpenFile(arg);
                }
            }
        }
    }
}
