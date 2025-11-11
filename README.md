# MdReader

A lightweight, free Windows desktop application for reading and displaying Markdown files with a beautiful, formatted view.

## Features

- **Multiple File Formats**: Open local `.md` and `.markdown` files, or load from URLs
- **Tabbed Interface**: Work with multiple Markdown files simultaneously in separate tabs
- **Session Persistence**: Automatically saves and restores your open tabs between sessions
- **File Integration**: 
  - Double-click `.md` files in Windows Explorer to open
  - Drag and drop files onto the application
  - "Open With" context menu support
- **Zoom Controls**: Easily adjust text size from 50% to 300%
- **Search Functionality**: Find text within documents with next/previous navigation
- **Hyperlink Support**: 
  - Click Markdown links to navigate in the same tab
  - External links open in your default browser
- **Modern UI**: Clean, intuitive interface built with WPF

## Quick Start

### Requirements
- Windows 10 or Windows 11
- .NET 9.0 Desktop Runtime

### Installation

1. Clone and build:
   ```bash
   git clone https://github.com/kormanm/MdReader.git
   cd MdReader/src/MdReader
   dotnet build -c Release
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

Or see [INSTALLATION.md](docs/INSTALLATION.md) for detailed installation instructions.

### Basic Usage

1. **Open a file**: Click the "📂 Open" button or drag and drop a `.md` file
2. **Open from URL**: Click "🌐 Open URL" and enter a Markdown file URL
3. **Zoom**: Use the zoom buttons to adjust text size
4. **Search**: Type in the search box and press Enter to find text
5. **Close a tab**: Click the ✕ button on the tab header

## Documentation

- [Architecture](docs/ARCHITECTURE.md) - Technical design and structure
- [Installation Guide](docs/INSTALLATION.md) - Detailed setup instructions
- [Configuration](docs/CONFIGURATION.md) - Customization options

## Technology Stack

- **Framework**: .NET 9.0 Windows
- **UI**: WPF (Windows Presentation Foundation)
- **Markdown**: Markdig and Markdig.Wpf
- **Language**: C# 12

## Screenshots

*(Screenshots will be added after the application is built and running)*

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

## License

This project is open source. See the LICENSE file for details.

## Support

For issues, questions, or suggestions, please open an issue on GitHub.
