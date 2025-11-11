# MdReader Configuration Guide

## Configuration Files

### State File
**Location**: `%APPDATA%\MdReader\state.json`

This file stores the application state and is automatically created and updated.

**Structure**:
```json
{
  "OpenFiles": [
    "C:\\Path\\To\\Document1.md",
    "https://example.com/document2.md"
  ],
  "ActiveTabIndex": 0
}
```

**Fields**:
- `OpenFiles`: Array of file paths or URLs that were open when the app was closed
- `ActiveTabIndex`: Index of the tab that was active (0-based)

### Manual State Editing

You can manually edit the state file while MdReader is closed:

1. Close MdReader completely
2. Navigate to `%APPDATA%\MdReader\`
3. Edit `state.json` with any text editor
4. Save the file
5. Start MdReader

**Use Cases**:
- Pre-configure files to open on startup
- Remove invalid file paths
- Reset to empty state

## Customization Options

### File Associations

Edit `install-file-association.reg` to customize:

1. **Installation Path**: Update all paths to match your installation
2. **File Extensions**: Add more extensions if needed
   ```reg
   [HKEY_CLASSES_ROOT\.mdown]
   @="MdReader.MarkdownFile"
   ```

### Application Behavior

The application behavior can be modified by editing the source code:

#### Zoom Settings
In `MainWindow.xaml.cs`, modify these constants:
```csharp
private const double ZoomIncrement = 0.1;  // Change zoom step (default 10%)
private const double MinZoom = 0.5;        // Minimum zoom (default 50%)
private const double MaxZoom = 3.0;        // Maximum zoom (default 300%)
```

#### Window Size
In `MainWindow.xaml`, modify the Window attributes:
```xml
<Window ... Height="600" Width="900" ... >
```

#### Markdown Pipeline
In `MainWindow.xaml.cs`, in the `CreateMarkdownTab` method:
```csharp
Pipeline = new MarkdownPipelineBuilder()
    .UseSupportedExtensions()  // Modify to enable/disable extensions
    .Build()
```

Available Markdig extensions:
- `.UseAdvancedExtensions()`: All advanced features
- `.UseAbbreviations()`: Abbreviations
- `.UseAutoIdentifiers()`: Auto heading IDs
- `.UseCitations()`: Citations
- `.UseCustomContainers()`: Custom containers
- `.UseDefinitionLists()`: Definition lists
- `.UseEmphasisExtras()`: Strikethrough, inserted, marked
- `.UseFigures()`: Figures
- `.UseFooters()`: Footers
- `.UseFootnotes()`: Footnotes
- `.UseGridTables()`: Grid tables
- `.UseMathematics()`: Math expressions
- `.UseMediaLinks()`: Media links
- `.UsePipeTables()`: Pipe tables
- `.UseListExtras()`: Task lists
- `.UseTaskLists()`: Task lists with checkboxes
- `.UseAutoLinks()`: Auto-convert URLs to links

## Advanced Customization

### Theming

To add dark mode or custom themes:

1. Edit `App.xaml` and add resource dictionaries:
```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="Themes/Dark.xaml"/>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

2. Create theme files in `Themes/` folder

### Custom Markdown Styles

The Markdig.Wpf library uses WPF's FlowDocument for rendering. You can customize styles by:

1. Creating a custom style for the MarkdownViewer
2. Modifying the Document property after creation

Example:
```csharp
viewer.Document.FontFamily = new FontFamily("Consolas");
viewer.Document.FontSize = 14;
viewer.Document.Background = Brushes.LightGray;
```

### Keyboard Shortcuts

Add keyboard shortcuts by handling KeyDown events in MainWindow:

```csharp
protected override void OnKeyDown(KeyEventArgs e)
{
    if (e.Key == Key.O && Keyboard.Modifiers == ModifierKeys.Control)
    {
        OpenFileButton_Click(this, new RoutedEventArgs());
        e.Handled = true;
    }
    base.OnKeyDown(e);
}
```

## Environment Variables

You can control the state file location via environment variable:

1. Set `MDREADER_STATE_PATH` environment variable:
   ```
   MDREADER_STATE_PATH=C:\MyCustomPath\
   ```

2. Modify `StateManager.cs` to read this variable:
   ```csharp
   var appDataPath = Environment.GetEnvironmentVariable("MDREADER_STATE_PATH") 
       ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MdReader");
   ```

## Command-Line Arguments

MdReader accepts file paths as command-line arguments:

```bash
MdReader.exe "C:\path\to\file.md" "C:\path\to\another.md"
```

This opens both files in separate tabs.

### Integration with Other Tools

Create batch files or shortcuts:

**Quick Launch with Specific File**:
```batch
@echo off
"C:\Program Files\MdReader\MdReader.exe" "C:\MyDocs\README.md"
```

**Open All MD Files in Folder**:
```batch
@echo off
for %%f in (*.md) do (
    "C:\Program Files\MdReader\MdReader.exe" "%%f"
)
```

## Performance Tuning

### Large Files
For very large Markdown files, you may need to adjust:

1. **Scroll Performance**: In MainWindow.xaml:
   ```xml
   <ScrollViewer VirtualizingPanel.IsVirtualizing="True"
                 VirtualizingPanel.VirtualizationMode="Recycling">
   ```

2. **Memory Usage**: Limit number of open tabs by modifying state restoration logic

## Logging

To enable diagnostic logging, add to `App.xaml.cs`:

```csharp
protected override void OnStartup(StartupEventArgs e)
{
    var logPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MdReader", "log.txt");
    
    // Add logging code here
    
    base.OnStartup(e);
}
```

## Security Settings

### Disable External Links
To prevent external links from opening:

In `MainWindow.xaml.cs`, modify `Hyperlink_Click`:
```csharp
private void Hyperlink_Click(object sender, RoutedEventArgs e)
{
    if (sender is Hyperlink hyperlink && hyperlink.NavigateUri != null)
    {
        var uri = hyperlink.NavigateUri.ToString();
        
        // Only allow local files
        if (uri.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("External links are disabled.", "Security", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        // ... rest of code
    }
}
```

### Restrict URL Loading
To disable URL loading entirely, comment out or remove the `OpenUrlButton_Click` method and hide the button.

## Backup and Restore

### Backup Settings
Copy the state file:
```
%APPDATA%\MdReader\state.json
```

### Restore Settings
Replace the state file with your backup while MdReader is closed.

## Reset to Defaults

Delete the entire application data folder:
```
%APPDATA%\MdReader\
```

The folder will be recreated with default settings on next launch.
