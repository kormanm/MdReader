# MdReader Quick Reference

## Opening Files

### Method 1: File Dialog
1. Click **📂 Open** button in toolbar
2. Select a `.md` or `.markdown` file
3. Click **Open**

### Method 2: Drag and Drop
1. Drag a `.md` file from Windows Explorer
2. Drop it onto the MdReader window

### Method 3: File Explorer Integration
1. Right-click a `.md` file in Windows Explorer
2. Select **Open with → MdReader**
   - Or double-click if set as default

### Method 4: Command Line
```bash
MdReader.exe "C:\path\to\file.md"
```

### Method 5: URL
1. Click **🌐 Open URL** button
2. Enter URL (must end with `.md` or `.markdown`)
3. Click **OK**

Example URLs:
- `https://raw.githubusercontent.com/user/repo/main/README.md`
- `https://example.com/docs/guide.md`

## Navigation

### Tabs
- **Switch Tab**: Click on tab header
- **Close Tab**: Click **✕** on tab header
- **Auto-Restore**: Tabs automatically restore when you reopen the app

### Links in Markdown
- **Same Tab**: Left-click a Markdown link
- **External Links**: Open in default browser automatically
- **Relative Links**: Resolved relative to current file/URL

## Zoom Controls

| Button | Action |
|--------|--------|
| **🔍+** | Zoom In (increase 10%) |
| **🔍-** | Zoom Out (decrease 10%) |
| **🔍 100%** | Reset to original size |

- **Range**: 50% to 300%
- **Per-Tab**: Each tab has independent zoom level

## Search

### Using Search Box
1. Type search term in the search box
2. Press **Enter** or click **⏭ Next**
3. Use **⏮ Previous** to go backward

### Keyboard Shortcuts
- **Enter**: Find Next
- **Shift + Enter**: Find Previous

## Tips & Tricks

### Productivity
- Keep frequently used files in tabs
- Use zoom for better readability
- Search works within the current tab

### File Organization
- Open related files in separate tabs
- Use hyperlinks to navigate between documents
- Tabs are remembered between sessions

### Working with URLs
- Bookmark useful remote Markdown URLs
- Remote files behave like local files (tabs, zoom, search)
- Changes to remote files require reopening

### Performance
- Close unused tabs for better performance
- Large files may take a moment to render
- Clear state file if needed: `%APPDATA%\MdReader\state.json`

## Troubleshooting

### App Won't Start
**Problem**: Missing .NET Runtime  
**Solution**: Install [.NET 9.0 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/9.0)

### File Won't Open
**Problem**: File association not set  
**Solution**: Use "Open with" or run `install-file-association.reg`

### URL Won't Load
**Problem**: Invalid URL or network issue  
**Solution**: 
- Verify URL ends with `.md` or `.markdown`
- Check internet connection
- Try downloading file manually to test

### Tabs Not Restoring
**Problem**: Corrupted state file  
**Solution**: Delete `%APPDATA%\MdReader\state.json`

### Search Not Working
**Problem**: No matches or feature limitation  
**Solution**: Search is case-sensitive and searches current tab only

## File Locations

| Item | Location |
|------|----------|
| **Application** | Your installation folder |
| **State File** | `%APPDATA%\MdReader\state.json` |
| **Registry** | `HKEY_CLASSES_ROOT\MdReader.MarkdownFile` |

## Supported Markdown Features

✅ Headers (H1-H6)  
✅ Emphasis (bold, italic, strikethrough)  
✅ Lists (ordered, unordered, nested)  
✅ Links (absolute, relative, external)  
✅ Images  
✅ Code blocks and inline code  
✅ Blockquotes  
✅ Tables  
✅ Task lists  
✅ Horizontal rules  
✅ All Markdig extensions  

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| **Enter** (in search) | Find Next |
| **Shift + Enter** (in search) | Find Previous |

## Best Practices

### Security
- Only open Markdown files from trusted sources
- Be cautious with external links
- Remote files are downloaded each time (no caching)

### Organization
- Use descriptive filenames for easy tab identification
- Keep related files in the same directory
- Use relative links for portable documentation

### Performance
- Close tabs you're not using
- Restart app if it becomes slow
- Large files (>1MB) may render slowly

## Common Use Cases

### Reading Documentation
1. Open README.md files from projects
2. Navigate between docs using links
3. Zoom in for easier reading

### Technical Writing
1. Open your draft in MdReader
2. Preview formatting in real-time
3. Edit in your text editor, refresh in MdReader

### Remote Documentation
1. Open GitHub README directly from URL
2. Follow links to other docs
3. Bookmark frequently accessed URLs

### Multi-Document Work
1. Open multiple related files
2. Switch between tabs as needed
3. Keep reference docs open

## Updates

To update MdReader:
1. Close the application
2. Download new version
3. Replace old executable
4. Restart - your tabs will be preserved

## Support

- **Issues**: https://github.com/kormanm/MdReader/issues
- **Docs**: Check the `docs/` folder
- **Contributing**: See CONTRIBUTING.md

---

**Version**: 1.0  
**Last Updated**: 2025
