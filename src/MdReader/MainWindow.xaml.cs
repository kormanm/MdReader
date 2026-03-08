using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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
    private double _currentHorizontalMargin = 10.0;
    private string _lastSearchText = string.Empty;
    private SearchMatch? _activeSearchMatch;
    private TextRange? _activeHighlight;
    private bool _suppressSearchClearOnTabChange;

    private sealed class SearchMatch
    {
        public required TabItem Tab { get; init; }
        public required TextRange Range { get; init; }
        public required int Offset { get; init; }
    }

    public MainWindow()
    {
        InitializeComponent();
        _stateManager = new StateManager();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var startupArgs = ((App)Application.Current).StartupArgs;
        if (startupArgs.Length > 0)
        {
            // File(s) passed via command line (e.g. double-click association)
            var state = _stateManager.LoadState();
            _currentHorizontalMargin = state?.HorizontalMargin ?? _currentHorizontalMargin;

            foreach (var arg in startupArgs)
            {
                OpenFile(arg);
            }
            return;
        }

        // Restore previous session
        var savedState = _stateManager.LoadState();
        if (savedState?.OpenFiles != null)
        {
            _currentHorizontalMargin = savedState.HorizontalMargin;

            foreach (var filePath in savedState.OpenFiles)
            {
                if (File.Exists(filePath) || filePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                    || filePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    OpenFile(filePath);
                }
            }

            if (savedState.ActiveTabIndex >= 0 && savedState.ActiveTabIndex < TabControl.Items.Count)
            {
                TabControl.SelectedIndex = savedState.ActiveTabIndex;
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
            ActiveTabIndex = TabControl.SelectedIndex,
            HorizontalMargin = _currentHorizontalMargin
        };

        _stateManager.SaveState(state);
    }

    private void Window_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
            e.Effects = DragDropEffects.Copy;
        else
            e.Effects = DragDropEffects.None;
        e.Handled = true;
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
                        file.EndsWith(".markdown", StringComparison.OrdinalIgnoreCase) ||
                        file.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                    {
                        OpenFile(file);
                    }
                }
            }
        }
        e.Handled = true;
    }

    private void OpenFileButton_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Supported files (*.md;*.markdown;*.txt)|*.md;*.markdown;*.txt|Markdown files (*.md;*.markdown)|*.md;*.markdown|Text files (*.txt)|*.txt|All files (*.*)|*.*",
            Title = "Open File"
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

    private void OpenInNotepadButton_Click(object sender, RoutedEventArgs e)
    {
        if (TabControl.SelectedItem is TabItem tab && tab.Tag is string filePath)
        {
            // Check if it's a local file (not a URL)
            if (filePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                filePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Cannot edit remote files. Only local files can be opened in Notepad++.", 
                    "Remote File", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Check if file exists
            if (!File.Exists(filePath))
            {
                MessageBox.Show($"File not found: {filePath}", "File Not Found", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Try common Notepad++ installation paths first
                string? notepadPath = null;
                var commonPaths = new[]
                {
                    @"C:\Program Files\Notepad++\notepad++.exe",
                    @"C:\Program Files (x86)\Notepad++\notepad++.exe",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Notepad++\notepad++.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Notepad++\notepad++.exe")
                };

                foreach (var path in commonPaths)
                {
                    if (File.Exists(path))
                    {
                        notepadPath = path;
                        break;
                    }
                }

                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = notepadPath ?? "notepad++.exe", // Fall back to PATH if not found in common locations
                    Arguments = $"\"{filePath}\"",
                    UseShellExecute = false
                };

                System.Diagnostics.Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                // If Notepad++ is not found, show error with helpful message
                MessageBox.Show($"Failed to open Notepad++. Please ensure Notepad++ is installed.\n\nYou can download it from: https://notepad-plus-plus.org/\n\nError: {ex.Message}", 
                    "Notepad++ Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        else
        {
            MessageBox.Show("No file is currently open.", "No File", 
                MessageBoxButton.OK, MessageBoxImage.Information);
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
            // Validate file path
            if (!Path.IsPathFullyQualified(filePath))
            {
                filePath = Path.GetFullPath(filePath);
            }

            // Check if file exists
            if (!File.Exists(filePath))
            {
                MessageBox.Show($"File not found: {filePath}", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
            // Validate URL format
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                MessageBox.Show("Invalid URL. Only HTTP and HTTPS URLs are supported.", "Invalid URL", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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

            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
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
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };

        var viewer = new MarkdownViewer
        {
            Markdown = markdownContent,
            Margin = new Thickness(_currentHorizontalMargin, 10, _currentHorizontalMargin, 10),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Pipeline = new MarkdownPipelineBuilder()
                .UseSupportedExtensions()
                .Build()
        };

        // Handle hyperlinks
        viewer.AddHandler(Hyperlink.ClickEvent, new RoutedEventHandler(Hyperlink_Click));

        // Handle mouse wheel for scrolling
        viewer.PreviewMouseWheel += Viewer_PreviewMouseWheel;

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

        var contextMenu = new ContextMenu();

        var openFolderMenuItem = new MenuItem { Header = "Open containing folder in Explorer" };
        openFolderMenuItem.Click += TabHeaderOpenContainingFolder_Click;

        var copyFolderPathMenuItem = new MenuItem { Header = "Copy folder path to clipboard" };
        copyFolderPathMenuItem.Click += TabHeaderCopyFolderPath_Click;

        var copyFullPathMenuItem = new MenuItem { Header = "Copy full path to clipboard" };
        copyFullPathMenuItem.Click += TabHeaderCopyFullPath_Click;

        contextMenu.Items.Add(openFolderMenuItem);
        contextMenu.Items.Add(copyFolderPathMenuItem);
        contextMenu.Items.Add(copyFullPathMenuItem);
        contextMenu.Opened += TabHeaderContextMenu_Opened;
        panel.ContextMenu = contextMenu;

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

    private void TabHeaderContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is not ContextMenu contextMenu || contextMenu.PlacementTarget is not FrameworkElement headerElement)
        {
            return;
        }

        var tabItem = FindTabItemFromHeaderElement(headerElement);
        var sourcePath = tabItem?.Tag as string;
        var isLocalFile = !string.IsNullOrWhiteSpace(sourcePath) &&
            !sourcePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !sourcePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

        foreach (var item in contextMenu.Items.OfType<MenuItem>())
        {
            if (item.Header is string header &&
                (header.StartsWith("Open containing folder", StringComparison.OrdinalIgnoreCase) ||
                 header.StartsWith("Copy folder path", StringComparison.OrdinalIgnoreCase)))
            {
                item.IsEnabled = isLocalFile;
            }
            else
            {
                item.IsEnabled = !string.IsNullOrWhiteSpace(sourcePath);
            }
        }
    }

    private TabItem? FindTabItemFromHeaderElement(FrameworkElement headerElement)
    {
        foreach (TabItem tab in TabControl.Items)
        {
            if (ReferenceEquals(tab.Header, headerElement))
            {
                return tab;
            }
        }

        return null;
    }

    private void TabHeaderOpenContainingFolder_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem ||
            menuItem.Parent is not ContextMenu contextMenu ||
            contextMenu.PlacementTarget is not FrameworkElement headerElement)
        {
            return;
        }

        var tabItem = FindTabItemFromHeaderElement(headerElement);
        if (tabItem?.Tag is not string filePath)
        {
            return;
        }

        if (filePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            filePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("Remote files do not have a local containing folder.", "Not Available", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var folderPath = Path.GetDirectoryName(filePath);
        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
        {
            MessageBox.Show("Containing folder not found.", "Folder Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{folderPath}\"",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to open folder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void TabHeaderCopyFolderPath_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem ||
            menuItem.Parent is not ContextMenu contextMenu ||
            contextMenu.PlacementTarget is not FrameworkElement headerElement)
        {
            return;
        }

        var tabItem = FindTabItemFromHeaderElement(headerElement);
        if (tabItem?.Tag is not string filePath)
        {
            return;
        }

        if (filePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            filePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("Remote files do not have a local folder path.", "Not Available", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var folderPath = Path.GetDirectoryName(filePath);
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            MessageBox.Show("Folder path is not available.", "Not Available", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Clipboard.SetText(folderPath);
    }

    private void TabHeaderCopyFullPath_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem ||
            menuItem.Parent is not ContextMenu contextMenu ||
            contextMenu.PlacementTarget is not FrameworkElement headerElement)
        {
            return;
        }

        var tabItem = FindTabItemFromHeaderElement(headerElement);
        if (tabItem?.Tag is string sourcePath && !string.IsNullOrWhiteSpace(sourcePath))
        {
            Clipboard.SetText(sourcePath);
        }
    }

    private void Hyperlink_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Hyperlink hyperlink && hyperlink.NavigateUri != null)
        {
            // Require Ctrl key to be pressed for navigation
            if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
            {
                e.Handled = true;
                return;
            }

            var uri = hyperlink.NavigateUri.ToString();

            // Check if it's an internal anchor link (starts with #)
            if (uri.StartsWith("#"))
            {
                ScrollToAnchor(uri.Substring(1));
                e.Handled = true;
                return;
            }

            // Check if it's a markdown file
            if (uri.EndsWith(".md", StringComparison.OrdinalIgnoreCase) ||
                uri.EndsWith(".markdown", StringComparison.OrdinalIgnoreCase))
            {
                // Get the current tab's source path
                if (TabControl.SelectedItem is TabItem currentTab && currentTab.Tag is string currentPath)
                {
                    string resolvedPath;
                    string? fragment = null;

                    // Extract fragment if present
                    var fragmentIndex = uri.IndexOf('#');
                    if (fragmentIndex >= 0)
                    {
                        fragment = uri.Substring(fragmentIndex + 1);
                        uri = uri.Substring(0, fragmentIndex);
                    }

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

                    // Check for Shift+Ctrl to open in new tab
                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    {
                        OpenFile(resolvedPath);
                        if (!string.IsNullOrEmpty(fragment))
                        {
                            Dispatcher.BeginInvoke(new Action(() => ScrollToAnchor(fragment)), System.Windows.Threading.DispatcherPriority.Loaded);
                        }
                    }
                    else
                    {
                        // Replace current tab
                        TabControl.Items.Remove(currentTab);
                        OpenFile(resolvedPath);
                        if (!string.IsNullOrEmpty(fragment))
                        {
                            Dispatcher.BeginInvoke(new Action(() => ScrollToAnchor(fragment)), System.Windows.Threading.DispatcherPriority.Loaded);
                        }
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
        else if (e.Key == Key.Escape)
        {
            ClearSearch();
            SearchTextBox.Clear();
            e.Handled = true;
        }
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = SearchTextBox.Text;

        if (string.IsNullOrWhiteSpace(searchText))
        {
            ClearSearch();
            return;
        }

        if (!searchText.Equals(_lastSearchText, StringComparison.Ordinal))
        {
            _lastSearchText = searchText;
            PerformIncrementalSearch();
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
        if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
        {
            return;
        }

        PerformSearch(forward: true);
    }

    private void SearchPrevious()
    {
        if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
        {
            return;
        }

        PerformSearch(forward: false);
    }

    private void PerformIncrementalSearch()
    {
        var searchText = SearchTextBox.Text;
        if (string.IsNullOrWhiteSpace(searchText))
        {
            ClearSearch();
            return;
        }

        _activeSearchMatch = null;
        if (!TryNavigateToMatch(searchText, forward: true, continueFromCurrentMatch: false))
        {
            ClearCurrentHighlight();
        }
    }

    private void PerformSearch(bool forward)
    {
        var searchText = SearchTextBox.Text;
        if (string.IsNullOrWhiteSpace(searchText))
        {
            ClearSearch();
            return;
        }

        var searchTextChanged = !searchText.Equals(_lastSearchText, StringComparison.Ordinal);
        _lastSearchText = searchText;
        if (searchTextChanged)
        {
            _activeSearchMatch = null;
        }

        if (!TryNavigateToMatch(searchText, forward, continueFromCurrentMatch: true))
        {
            MessageBox.Show($"No matches found for '{searchText}'", "Search", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private bool TryNavigateToMatch(string searchText, bool forward, bool continueFromCurrentMatch)
    {
        var tabs = TabControl.Items.OfType<TabItem>().ToList();
        if (tabs.Count == 0)
        {
            return false;
        }

        var selectedTab = TabControl.SelectedItem as TabItem ?? tabs[0];
        var anchorTab = selectedTab;
        var startOffset = GetVisibleStartOffset(anchorTab);
        if (continueFromCurrentMatch && _activeSearchMatch == null)
        {
            startOffset += forward ? 1 : -1;
        }

        if (continueFromCurrentMatch &&
            _activeSearchMatch != null &&
            tabs.Any(t => ReferenceEquals(t, _activeSearchMatch.Tab)) &&
            _lastSearchText.Equals(searchText, StringComparison.Ordinal))
        {
            anchorTab = _activeSearchMatch.Tab;
            startOffset = forward ? _activeSearchMatch.Offset + 1 : _activeSearchMatch.Offset - 1;
        }

        var anchorIndex = tabs.IndexOf(anchorTab);
        var anchorMatches = BuildSearchMatches(anchorTab, searchText);
        var match = forward
            ? FindFirstAtOrAfter(anchorMatches, startOffset)
            : FindLastAtOrBefore(anchorMatches, startOffset);

        if (match == null)
        {
            for (int step = 1; step < tabs.Count; step++)
            {
                var tabIndex = forward
                    ? (anchorIndex + step) % tabs.Count
                    : (anchorIndex - step + tabs.Count) % tabs.Count;

                var tabMatches = BuildSearchMatches(tabs[tabIndex], searchText);
                if (tabMatches.Count == 0)
                {
                    continue;
                }

                match = forward ? tabMatches[0] : tabMatches[^1];
                break;
            }
        }

        if (match == null)
        {
            match = forward
                ? FindFirstBefore(anchorMatches, startOffset)
                : FindLastAfter(anchorMatches, startOffset);
        }

        if (match == null)
        {
            return false;
        }

        ActivateSearchMatch(match);
        return true;
    }

    private List<SearchMatch> BuildSearchMatches(TabItem tab, string searchText)
    {
        var matches = new List<SearchMatch>();

        if (string.IsNullOrWhiteSpace(searchText) ||
            tab.Content is not ScrollViewer scrollViewer ||
            scrollViewer.Content is not MarkdownViewer viewer)
        {
            return matches;
        }

        var document = GetFlowDocument(viewer);
        if (document == null)
        {
            return matches;
        }

        var textPointer = document.ContentStart;
        var offset = 0;
        while (textPointer != null)
        {
            if (textPointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
            {
                var text = textPointer.GetTextInRun(LogicalDirection.Forward);
                var index = text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);

                while (index >= 0)
                {
                    var start = textPointer.GetPositionAtOffset(index);
                    var end = start?.GetPositionAtOffset(searchText.Length);

                    if (start != null && end != null)
                    {
                        matches.Add(new SearchMatch
                        {
                            Tab = tab,
                            Range = new TextRange(start, end),
                            Offset = offset + index
                        });
                    }

                    index = text.IndexOf(searchText, index + 1, StringComparison.OrdinalIgnoreCase);
                }

                offset += text.Length;
            }

            textPointer = textPointer.GetNextContextPosition(LogicalDirection.Forward);
        }

        return matches;
    }

    private int GetVisibleStartOffset(TabItem tab)
    {
        if (tab.Content is not ScrollViewer scrollViewer ||
            scrollViewer.Content is not MarkdownViewer viewer)
        {
            return 0;
        }

        var document = GetFlowDocument(viewer);
        if (document == null)
        {
            return 0;
        }

        var totalTextLength = new TextRange(document.ContentStart, document.ContentEnd).Text.Length;
        if (totalTextLength <= 0 || scrollViewer.ScrollableHeight <= 0)
        {
            return 0;
        }

        var ratio = Math.Clamp(scrollViewer.VerticalOffset / scrollViewer.ScrollableHeight, 0.0, 1.0);
        return (int)Math.Clamp(Math.Floor(ratio * totalTextLength), 0, Math.Max(0, totalTextLength - 1));
    }

    private SearchMatch? FindFirstAtOrAfter(List<SearchMatch> matches, int offset)
    {
        foreach (var match in matches)
        {
            if (match.Offset >= offset)
            {
                return match;
            }
        }

        return null;
    }

    private SearchMatch? FindLastAtOrBefore(List<SearchMatch> matches, int offset)
    {
        for (int i = matches.Count - 1; i >= 0; i--)
        {
            if (matches[i].Offset <= offset)
            {
                return matches[i];
            }
        }

        return null;
    }

    private SearchMatch? FindFirstBefore(List<SearchMatch> matches, int offset)
    {
        foreach (var match in matches)
        {
            if (match.Offset < offset)
            {
                return match;
            }
        }

        return null;
    }

    private SearchMatch? FindLastAfter(List<SearchMatch> matches, int offset)
    {
        for (int i = matches.Count - 1; i >= 0; i--)
        {
            if (matches[i].Offset > offset)
            {
                return matches[i];
            }
        }

        return null;
    }

    private void ActivateSearchMatch(SearchMatch match)
    {
        ClearCurrentHighlight();

        if (!ReferenceEquals(TabControl.SelectedItem, match.Tab))
        {
            _suppressSearchClearOnTabChange = true;
            try
            {
                TabControl.SelectedItem = match.Tab;
            }
            finally
            {
                _suppressSearchClearOnTabChange = false;
            }
        }

        _activeSearchMatch = match;
        _activeHighlight = match.Range;
        match.Range.ApplyPropertyValue(TextElement.BackgroundProperty, System.Windows.Media.Brushes.Yellow);

        if (match.Tab.Content is ScrollViewer scrollViewer &&
            scrollViewer.Content is MarkdownViewer viewer)
        {
            CenterMatchInViewport(match, scrollViewer, viewer);
        }
        else
        {
            var start = match.Range.Start;
            if (start.Parent is FrameworkElement element)
            {
                element.BringIntoView();
                return;
            }

            start.Paragraph?.BringIntoView();
        }
    }

    private void CenterMatchInViewport(SearchMatch match, ScrollViewer scrollViewer, MarkdownViewer viewer)
    {
        var document = GetFlowDocument(viewer);
        if (document == null)
        {
            match.Range.Start.Paragraph?.BringIntoView();
            return;
        }

        var totalTextLength = new TextRange(document.ContentStart, document.ContentEnd).Text.Length;
        if (totalTextLength <= 0 || scrollViewer.ScrollableHeight <= 0)
        {
            match.Range.Start.Paragraph?.BringIntoView();
            return;
        }

        var ratio = Math.Clamp((double)match.Offset / Math.Max(1, totalTextLength - 1), 0.0, 1.0);
        var targetCenter = ratio * scrollViewer.ScrollableHeight;
        var targetOffset = Math.Clamp(targetCenter - (scrollViewer.ViewportHeight / 2.0), 0.0, scrollViewer.ScrollableHeight);
        scrollViewer.ScrollToVerticalOffset(targetOffset);
    }

    private void ClearCurrentHighlight()
    {
        _activeHighlight?.ApplyPropertyValue(TextElement.BackgroundProperty, null);
        _activeHighlight = null;
    }

    private void ClearSearch()
    {
        ClearCurrentHighlight();
        _activeSearchMatch = null;
        _lastSearchText = string.Empty;
    }

    private FlowDocument? GetFlowDocument(DependencyObject parent)
    {
        var childCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childCount; i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            
            if (child is FlowDocumentScrollViewer flowDocViewer)
            {
                return flowDocViewer.Document;
            }

            var result = GetFlowDocument(child);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressSearchClearOnTabChange)
        {
            return;
        }

        ClearSearch();
    }


    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Handle Ctrl+F for search
        if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && e.Key == Key.F)
        {
            SearchTextBox.Focus();
            SearchTextBox.SelectAll();
            e.Handled = true;
            return;
        }

        // Handle Ctrl+Home to jump to start of document
        if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && e.Key == Key.Home)
        {
            if (TabControl.SelectedItem is TabItem tab &&
                tab.Content is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToTop();
                e.Handled = true;
            }
            return;
        }

        // Handle Ctrl+End to jump to end of document
        if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && e.Key == Key.End)
        {
            if (TabControl.SelectedItem is TabItem tab &&
                tab.Content is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToBottom();
                e.Handled = true;
            }
            return;
        }

        // Handle F3 for find next
        if (e.Key == Key.F3)
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
            return;
        }

        if (e.Key == Key.PageUp || e.Key == Key.PageDown)
        {
            if (TabControl.SelectedItem is TabItem tab &&
                tab.Content is ScrollViewer scrollViewer)
            {
                if (e.Key == Key.PageUp)
                {
                    scrollViewer.PageUp();
                }
                else
                {
                    scrollViewer.PageDown();
                }
                e.Handled = true;
            }
        }
    }

    private void Viewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is MarkdownViewer viewer)
        {
            var scrollViewer = FindScrollViewer(viewer);
            if (scrollViewer != null)
            {
                // Get the current zoom level
                var zoom = viewer.LayoutTransform is System.Windows.Media.ScaleTransform scale 
                    ? scale.ScaleX 
                    : 1.0;
                
                // Calculate scroll amount based on zoom level
                // Use a base scroll amount and adjust by zoom
                const double baseScrollAmount = 48.0; // 3 lines worth of pixels
                var scrollAmount = baseScrollAmount * zoom;
                
                if (e.Delta > 0)
                {
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - scrollAmount);
                }
                else
                {
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + scrollAmount);
                }
                e.Handled = true;
            }
        }
    }

    private ScrollViewer? FindScrollViewer(DependencyObject child)
    {
        var parent = System.Windows.Media.VisualTreeHelper.GetParent(child);
        while (parent != null)
        {
            if (parent is ScrollViewer scrollViewer)
            {
                return scrollViewer;
            }
            parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
        }
        return null;
    }

    private void ScrollToAnchor(string anchorId)
    {
        if (TabControl.SelectedItem is TabItem tab &&
            tab.Content is ScrollViewer scrollViewer &&
            scrollViewer.Content is MarkdownViewer viewer)
        {
            var element = FindElementByName(viewer, anchorId);
            if (element != null)
            {
                element.BringIntoView();
            }
        }
    }

    private FrameworkElement? FindElementByName(DependencyObject parent, string name)
    {
        var childCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childCount; i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            
            if (child is FrameworkElement fe && fe.Name == name)
            {
                return fe;
            }

            var result = FindElementByName(child, name);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }
}
