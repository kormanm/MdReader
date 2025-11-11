# MdReader Architecture

## Overview
MdReader is a Windows desktop application built with WPF (Windows Presentation Foundation) and .NET 9.0 for reading and displaying Markdown files.

## Technology Stack
- **Framework**: .NET 9.0 (Windows)
- **UI Framework**: WPF (Windows Presentation Foundation)
- **Markdown Parsing**: Markdig and Markdig.Wpf
- **State Persistence**: Newtonsoft.Json
- **Language**: C# 12

## Application Structure

### Core Components

#### 1. MainWindow
- **Purpose**: Main application window that hosts all functionality
- **Key Features**:
  - Tab-based document interface
  - Toolbar with file operations, zoom, and search controls
  - Drag-and-drop support for files
  - Session state management

#### 2. StateManager
- **Purpose**: Manages application state persistence
- **Storage Location**: `%APPDATA%\MdReader\state.json`
- **Persisted Data**:
  - List of open file paths
  - Active tab index
- **Behavior**: 
  - Saves state on application close
  - Restores state on application start

#### 3. UrlInputDialog
- **Purpose**: Dialog for entering URLs to remote Markdown files
- **Validation**: Ensures only .md or .markdown URLs are opened

### Key Features

#### File Opening
- **Local Files**: Via file dialog, drag-and-drop, or command-line arguments
- **Remote Files**: Via URL input dialog (HTTP/HTTPS)
- **Formats Supported**: .md, .markdown

#### Tab Management
- **Multiple Tabs**: Each file opens in a separate tab
- **Tab Persistence**: Open tabs are restored on restart
- **Tab Closing**: Close button on each tab header
- **Duplicate Prevention**: Same file won't open twice

#### Markdown Rendering
- **Library**: Markdig.Wpf for native WPF rendering
- **Extensions**: All supported Markdig extensions enabled
- **Hyperlinks**: 
  - Markdown links open in same tab by default
  - Right-click to open in new tab (requires context menu)
  - External links open in default browser

#### Zoom Functionality
- **Range**: 50% to 300%
- **Increment**: 10% per click
- **Per-Tab**: Each tab maintains its own zoom level
- **Controls**: Zoom In, Zoom Out, Reset buttons

#### Search Functionality
- **Direction**: Forward (Next) and Backward (Previous)
- **Keyboard Shortcuts**: 
  - Enter: Search Next
  - Shift+Enter: Search Previous
- **Scope**: Searches within current tab

#### File Association
- **Integration**: Registry file for Windows Explorer integration
- **Capabilities**:
  - Double-click to open .md files
  - "Open With" menu support
  - Default icon association

## Data Flow

```
Application Startup
├─> Load State (StateManager)
├─> Restore Tabs (if any)
└─> Wait for User Input

User Opens File/URL
├─> Validate file/URL
├─> Check for duplicate
├─> Load content (local or HTTP)
├─> Parse Markdown (Markdig)
├─> Render to WPF (MarkdownViewer)
└─> Add to TabControl

Application Close
├─> Collect open file paths
├─> Get active tab index
└─> Save State (StateManager)
```

## Design Patterns

### Separation of Concerns
- **UI**: XAML for declarative UI definition
- **Logic**: C# code-behind for behavior
- **State**: Separate StateManager class

### Event-Driven Architecture
- UI events trigger handlers in MainWindow
- Hyperlink clicks handled through routed events
- Tab selection changes update UI state

## File Structure
```
MdReader/
├── src/
│   └── MdReader/
│       ├── MdReader.csproj
│       ├── App.xaml[.cs]              # Application entry point
│       ├── MainWindow.xaml[.cs]       # Main window
│       ├── UrlInputDialog.xaml[.cs]   # URL input dialog
│       ├── StateManager.cs            # State persistence
│       ├── app.manifest               # Windows manifest
│       ├── app.ico                    # Application icon
│       └── install-file-association.reg  # Registry file
├── docs/
│   ├── ARCHITECTURE.md                # This file
│   ├── INSTALLATION.md                # Installation guide
│   └── CONFIGURATION.md               # Configuration guide
└── README.md                          # Project overview
```

## Future Enhancements
- Full-text search with highlighting
- Export to PDF/HTML
- Themes/Dark mode
- Custom CSS styling
- Git integration for viewing markdown from repositories
- Bookmarks/Favorites
- Print support
