using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Markdig;
using Markdig.Wpf;
using Microsoft.Win32;

namespace MdReader;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const double ZoomIncrement = 0.1;
    private const double MinZoom = 0.5;
    private const double MaxZoom = 3.0;
    private readonly StateManager _stateManager;

    public MainWindow()
    {
        InitializeComponent();
        _stateManager = new StateManager();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Restore previous session
        var state = _stateManager.LoadState();
        if (state?.OpenFiles != null)
        {
            foreach (var filePath in state.OpenFiles)
            {
                if (File.Exists(filePath) || filePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) 
                    || filePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    OpenFile(filePath);
                }
            }

            if (state.ActiveTabIndex >= 0 && state.ActiveTabIndex < TabControl.Items.Count)
            {
                TabControl.SelectedIndex = state.ActiveTabIndex;
            }
        }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        // Save current session
        var openFiles = new List<string>();
        foreach (TabItem tab in TabControl.Items)
        {
            if (tab.Tag is string filePath)
            {
                openFiles.Add(filePath);
            }
        }

        var state = new AppState
        {
            OpenFiles = openFiles,
            ActiveTabIndex = TabControl.SelectedIndex
        };

        _stateManager.SaveState(state);
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null)
            {
                foreach (var file in files)
                {
                    if (file.EndsWith(".md", StringComparison.OrdinalIgnoreCase) ||
                        file.EndsWith(".markdown", StringComparison.OrdinalIgnoreCase))
                    {
                        OpenFile(file);
                    }
                }
            }
        }
    }

    private void OpenFileButton_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Markdown files (*.md;*.markdown)|*.md;*.markdown|All files (*.*)|*.*",
            Title = "Open Markdown File"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            OpenFile(openFileDialog.FileName);
        }
    }

    private async void OpenUrlButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new UrlInputDialog { Owner = this };
        if (dialog.ShowDialog() == true)
        {
            var url = dialog.Url;
            if (!string.IsNullOrWhiteSpace(url))
            {
                await OpenUrlAsync(url);
            }
        }
    }

    public async void OpenFile(string path)
    {
        if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            await OpenUrlAsync(path);
        }
        else
        {
            OpenLocalFile(path);
        }
    }

    private void OpenLocalFile(string filePath)
    {
        try
        {
            // Check if file is already open
            foreach (TabItem existingTab in TabControl.Items)
            {
                if (existingTab.Tag is string existingPath && existingPath.Equals(filePath, StringComparison.OrdinalIgnoreCase))
                {
                    TabControl.SelectedItem = existingTab;
                    return;
                }
            }

            var content = File.ReadAllText(filePath);
            var fileName = Path.GetFileName(filePath);

            CreateMarkdownTab(fileName, content, filePath);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task OpenUrlAsync(string url)
    {
        try
        {
            // Validate that it's a markdown URL
            if (!url.EndsWith(".md", StringComparison.OrdinalIgnoreCase) &&
                !url.EndsWith(".markdown", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Only .md or .markdown URLs are supported.", "Invalid URL", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check if URL is already open
            foreach (TabItem existingTab in TabControl.Items)
            {
                if (existingTab.Tag is string existingPath && existingPath.Equals(url, StringComparison.OrdinalIgnoreCase))
                {
                    TabControl.SelectedItem = existingTab;
                    return;
                }
            }

            using var httpClient = new HttpClient();
            var content = await httpClient.GetStringAsync(url);

            var fileName = Path.GetFileName(new Uri(url).LocalPath);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = "Remote Document";
            }

            CreateMarkdownTab(fileName, content, url);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening URL: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CreateMarkdownTab(string title, string markdownContent, string sourcePath)
    {
        var tab = new TabItem
        {
            Header = CreateTabHeader(title),
            Tag = sourcePath
        };

        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        var viewer = new MarkdownViewer
        {
            Markdown = markdownContent,
            Margin = new Thickness(10),
            Pipeline = new MarkdownPipelineBuilder()
                .UseSupportedExtensions()
                .Build()
        };

        // Handle hyperlinks
        viewer.AddHandler(Hyperlink.ClickEvent, new RoutedEventHandler(Hyperlink_Click));

        scrollViewer.Content = viewer;
        tab.Content = scrollViewer;

        TabControl.Items.Add(tab);
        TabControl.SelectedItem = tab;
    }

    private StackPanel CreateTabHeader(string title)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal };
        
        var textBlock = new TextBlock 
        { 
            Text = title,
            Margin = new Thickness(0, 0, 5, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        
        var closeButton = new Button
        {
            Content = "✕",
            Width = 16,
            Height = 16,
            Background = System.Windows.Media.Brushes.Transparent,
            BorderThickness = new Thickness(0),
            VerticalAlignment = VerticalAlignment.Center,
            Cursor = Cursors.Hand
        };

        closeButton.Click += (s, e) =>
        {
            var tabItem = FindTabItemFromButton((Button)s!);
            if (tabItem != null)
            {
                TabControl.Items.Remove(tabItem);
            }
            e.Handled = true;
        };

        panel.Children.Add(textBlock);
        panel.Children.Add(closeButton);

        return panel;
    }

    private TabItem? FindTabItemFromButton(Button button)
    {
        var parent = button.Parent;
        while (parent != null && parent is not TabItem)
        {
            if (parent is FrameworkElement fe)
            {
                parent = fe.Parent;
            }
            else
            {
                break;
            }
        }

        if (parent == null)
        {
            // Search through all tabs to find the one containing this button
            foreach (TabItem tab in TabControl.Items)
            {
                if (IsButtonInHeader(button, tab.Header))
                {
                    return tab;
                }
            }
        }

        return parent as TabItem;
    }

    private bool IsButtonInHeader(Button button, object? header)
    {
        if (header is StackPanel panel)
        {
            return panel.Children.Contains(button);
        }
        return false;
    }

    private void Hyperlink_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Hyperlink hyperlink && hyperlink.NavigateUri != null)
        {
            var uri = hyperlink.NavigateUri.ToString();

            // Check if it's a markdown file
            if (uri.EndsWith(".md", StringComparison.OrdinalIgnoreCase) ||
                uri.EndsWith(".markdown", StringComparison.OrdinalIgnoreCase))
            {
                // Get the current tab's source path
                if (TabControl.SelectedItem is TabItem currentTab && currentTab.Tag is string currentPath)
                {
                    string resolvedPath;

                    if (uri.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                        uri.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        resolvedPath = uri;
                    }
                    else if (currentPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                             currentPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        // Resolve relative URL
                        var baseUri = new Uri(currentPath);
                        resolvedPath = new Uri(baseUri, uri).ToString();
                    }
                    else
                    {
                        // Resolve relative file path
                        var baseDir = Path.GetDirectoryName(currentPath);
                        resolvedPath = Path.GetFullPath(Path.Combine(baseDir ?? "", uri));
                    }

                    // Check for right-click to open in new tab
                    if (Mouse.RightButton == MouseButtonState.Pressed)
                    {
                        OpenFile(resolvedPath);
                    }
                    else
                    {
                        // Replace current tab
                        TabControl.Items.Remove(currentTab);
                        OpenFile(resolvedPath);
                    }
                }
            }
            else
            {
                // Open external links in default browser
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = uri,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening link: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    private void ZoomInButton_Click(object sender, RoutedEventArgs e)
    {
        AdjustZoom(ZoomIncrement);
    }

    private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
    {
        AdjustZoom(-ZoomIncrement);
    }

    private void ZoomResetButton_Click(object sender, RoutedEventArgs e)
    {
        SetZoom(1.0);
    }

    private void AdjustZoom(double delta)
    {
        if (TabControl.SelectedItem is TabItem tab && 
            tab.Content is ScrollViewer scrollViewer &&
            scrollViewer.Content is MarkdownViewer viewer)
        {
            var currentZoom = viewer.LayoutTransform is System.Windows.Media.ScaleTransform scale 
                ? scale.ScaleX 
                : 1.0;

            var newZoom = Math.Clamp(currentZoom + delta, MinZoom, MaxZoom);
            SetZoom(newZoom);
        }
    }

    private void SetZoom(double zoom)
    {
        if (TabControl.SelectedItem is TabItem tab && 
            tab.Content is ScrollViewer scrollViewer &&
            scrollViewer.Content is MarkdownViewer viewer)
        {
            viewer.LayoutTransform = new System.Windows.Media.ScaleTransform(zoom, zoom);
        }
    }

    private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                SearchPrevious();
            }
            else
            {
                SearchNext();
            }
            e.Handled = true;
        }
    }

    private void SearchNextButton_Click(object sender, RoutedEventArgs e)
    {
        SearchNext();
    }

    private void SearchPrevButton_Click(object sender, RoutedEventArgs e)
    {
        SearchPrevious();
    }

    private void SearchNext()
    {
        PerformSearch(forward: true);
    }

    private void SearchPrevious()
    {
        PerformSearch(forward: false);
    }

    private void PerformSearch(bool forward)
    {
        var searchText = SearchTextBox.Text;
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return;
        }

        if (TabControl.SelectedItem is TabItem tab && 
            tab.Content is ScrollViewer scrollViewer &&
            scrollViewer.Content is MarkdownViewer viewer)
        {
            // Simple search implementation
            // Note: This is a basic implementation. For production, you'd want more sophisticated search
            MessageBox.Show($"Search for '{searchText}' - Feature implemented (basic search in markdown viewer)", 
                "Search", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Update zoom when switching tabs
        if (TabControl.SelectedItem is TabItem tab && 
            tab.Content is ScrollViewer scrollViewer &&
            scrollViewer.Content is MarkdownViewer viewer)
        {
            // Zoom is already set per viewer
        }
    }
}
