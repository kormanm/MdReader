# Changelog

All notable changes to the MdReader project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-01-11

### Added - Initial Release

#### Core Application
- Windows desktop application built with WPF and .NET 9.0
- Tabbed interface for multiple Markdown files
- Markdown rendering using Markdig and Markdig.Wpf libraries
- Support for all Markdig extensions (tables, task lists, etc.)

#### File Operations
- Open local .md and .markdown files via file dialog
- Drag-and-drop support for Markdown files
- Command-line argument support for opening files
- Open remote Markdown files from HTTP/HTTPS URLs
- Duplicate file prevention (same file won't open twice)
- File path validation and security checks

#### Windows Integration
- File Explorer integration via registry file
- Double-click .md files to open in MdReader
- "Open With" context menu support
- Application manifest for Windows 10/11 compatibility

#### Session Management
- Automatic tab persistence across sessions
- Saves open files when closing
- Restores all tabs when reopening
- JSON-based state storage in %APPDATA%
- State validation on load

#### User Interface
- Zoom controls (50% to 300% range, 10% increments)
- Zoom In, Zoom Out, and Reset buttons
- Per-tab zoom levels (each tab remembers its zoom)
- Search functionality with next/previous navigation
- Keyboard shortcuts (Enter for next, Shift+Enter for previous)
- Tab close buttons
- Toolbar with all essential controls

#### Navigation
- Hyperlink support for Markdown links
- Same-tab navigation by default
- External links open in default browser
- Relative link resolution (files and URLs)
- Path resolution for linked Markdown files

#### Security
- URL validation with proper URI parsing
- HTTP client timeout (30 seconds)
- File path validation and sanitization
- JSON deserialization validation
- All dependencies verified free of vulnerabilities

#### Documentation
- README.md - Project overview and quick start
- QUICK_REFERENCE.md - User guide for common tasks
- ARCHITECTURE.md - Technical design and structure
- INSTALLATION.md - Detailed installation guide
- CONFIGURATION.md - Customization options
- CONTRIBUTING.md - Contribution guidelines
- SAMPLE.md - Example Markdown document
- LICENSE - MIT License

#### Build & Deployment
- Visual Studio solution file
- build.bat - Windows build script
- publish.bat - Create standalone executable
- GitHub Actions workflow for automated builds
- Comprehensive .gitignore for C#/Visual Studio

#### Project Management
- MIT License for open source use
- Contributing guidelines
- GitHub Actions CI/CD pipeline
- Professional project structure

### Technical Details

#### Dependencies
- .NET 9.0 (net9.0-windows)
- Markdig 0.37.0 (Markdown parsing)
- Markdig.Wpf 0.7.0 (WPF rendering)
- Newtonsoft.Json 13.0.3 (State persistence)

#### Architecture
- MVVM-style WPF application
- Event-driven UI updates
- Separation of concerns (UI, logic, state)
- Asynchronous file/URL loading
- Exception handling throughout

#### Files Created
- 8 C# code files (.cs)
- 3 XAML UI files (.xaml)
- 6 documentation files (.md)
- 1 project file (.csproj)
- 1 solution file (.sln)
- 1 manifest file
- 1 registry file (.reg)
- 2 batch scripts (.bat)
- 1 GitHub Actions workflow (.yml)
- 1 .gitignore file
- 1 LICENSE file

### Requirements Met

All original requirements from the issue have been implemented:
- ✅ Windows local machine application (C#)
- ✅ Read and interpret Markdown content
- ✅ Display as formatted text
- ✅ File Explorer integration
- ✅ Open on double-click
- ✅ "Open with" menu support
- ✅ Open URLs (md files only)
- ✅ Drag and drop support
- ✅ Single window with tabs per file
- ✅ Link navigation (same tab, external browser)
- ✅ Remember opened files and tabs
- ✅ Reopen tabs after restart/kill
- ✅ Zoom in/out buttons
- ✅ Search with next/prev from current position
- ✅ Comprehensive documentation

### Known Limitations

- Search is basic (no highlighting yet)
- Right-click context menu for links not implemented (uses mouse button detection)
- No export functionality (PDF, HTML)
- No dark mode/themes yet
- No installer (manual installation required)

### Future Enhancements

Planned for future releases:
- Enhanced search with highlighting
- Dark mode and themes
- Export to PDF/HTML
- Print support
- Bookmarks/Favorites
- Custom CSS styling
- Plugin system
- Installer creation

---

## How to Use This Changelog

This changelog follows [semantic versioning](https://semver.org/):
- MAJOR version for incompatible API changes
- MINOR version for new functionality in a backward compatible manner
- PATCH version for backward compatible bug fixes
