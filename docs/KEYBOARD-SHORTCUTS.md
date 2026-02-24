# MdReader Keyboard Shortcuts

Complete list of keyboard shortcuts available in MdReader.

## Document Navigation

| Shortcut | Action |
|----------|--------|
| **Ctrl+Home** | Jump to start of document |
| **Ctrl+End** | Jump to end of document |
| **Page Up** | Scroll up one page |
| **Page Down** | Scroll down one page |
| **Mouse Wheel** | Scroll up/down (zoom-aware) |

## Search

| Shortcut | Action |
|----------|--------|
| **Ctrl+F** | Focus search box |
| **F3** | Find next match |
| **Shift+F3** | Find previous match |
| **Enter** (in search box) | Find next match |
| **Shift+Enter** (in search box) | Find previous match |
| **Esc** (in search box) | Clear search and results |

## Zoom

| Shortcut | Action | Button |
|----------|--------|--------|
| N/A | Zoom in (+10%) | ??+ |
| N/A | Zoom out (-10%) | ??- |
| N/A | Reset to 100% | ?? 100% |

*Note: Zoom controls currently require mouse clicks. Keyboard shortcuts for zoom may be added in a future version.*

## Links

| Shortcut | Action |
|----------|--------|
| **Ctrl+Click** | Follow markdown link in same tab |
| **Ctrl+Shift+Click** | Follow markdown link in new tab |

## File Operations

| Shortcut | Action | Button |
|----------|--------|--------|
| N/A | Open file | ?? Open |
| N/A | Open URL | ?? Open URL |
| N/A | Edit in Notepad++ | ?? Edit |
| Drag & Drop | Open file(s) | Drop .md, .markdown, or .txt files on window |

**Notes:** 
- The "Edit" button opens the current file in Notepad++
- Automatically detects Notepad++ in standard installation locations
- Falls back to system PATH if not found in default locations
- Remote files (URLs) cannot be edited
- File operation shortcuts may be added in a future version

## Tab Management

| Shortcut | Action |
|----------|--------|
| Click tab header | Switch to tab |
| Click **?** on tab | Close tab |

*Note: Keyboard shortcuts for tab navigation (Ctrl+Tab, etc.) may be added in a future version.*

## Quick Reference Card

### Most Used Shortcuts
```
Ctrl+F          - Search
F3              - Find Next
Shift+F3        - Find Previous
Ctrl+Home       - Jump to Top
Ctrl+End        - Jump to Bottom
Ctrl+Click      - Follow Link
Esc             - Clear Search
```

## Tips

### Efficient Navigation
1. Use **Ctrl+Home** to quickly return to the top of long documents
2. Use **Ctrl+End** to jump to the end
3. Use **F3** for quick search navigation without taking hands off keyboard

### Search Workflow
1. Press **Ctrl+F** to focus search box
2. Type your search term (matches appear as you type)
3. Press **F3** to jump to next match
4. Press **Shift+F3** to go back to previous match
5. Press **Esc** when done searching

### Link Navigation
1. Hold **Ctrl** and click on markdown links
2. Add **Shift** to open in a new tab instead of replacing current tab
3. External links (http/https) open in your default browser automatically

## Accessibility

- All major features are accessible via mouse and keyboard
- Search box accepts keyboard input for navigation
- Toolbar buttons have tooltips showing keyboard equivalents where available

## Future Enhancements

Planned keyboard shortcuts for future versions:
- **Ctrl+O** - Open file dialog
- **Ctrl+W** - Close current tab
- **Ctrl+Tab** - Next tab
- **Ctrl+Shift+Tab** - Previous tab
- **Ctrl++** - Zoom in
- **Ctrl+-** - Zoom out
- **Ctrl+0** - Reset zoom

## Customization

To customize keyboard shortcuts, you'll need to modify the source code in `MainWindow.xaml.cs`, specifically the `Window_PreviewKeyDown` method.

Example of adding Ctrl+O for open:
```csharp
if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && e.Key == Key.O)
{
    OpenFileButton_Click(sender, new RoutedEventArgs());
    e.Handled = true;
    return;
}
```

---

**Last Updated**: January 2025  
**Version**: 1.0
