# Contributing to MdReader

Thank you for your interest in contributing to MdReader! This document provides guidelines and instructions for contributing.

## Getting Started

### Prerequisites
- Windows 10 or Windows 11
- .NET 9.0 SDK or later
- Visual Studio 2022 or VS Code (recommended)
- Git

### Setting Up Development Environment

1. Fork the repository on GitHub
2. Clone your fork:
   ```bash
   git clone https://github.com/YOUR_USERNAME/MdReader.git
   cd MdReader
   ```
3. Add the upstream repository:
   ```bash
   git remote add upstream https://github.com/kormanm/MdReader.git
   ```
4. Build the project:
   ```bash
   build.bat
   ```

## Development Workflow

### Making Changes

1. Create a new branch for your feature or bug fix:
   ```bash
   git checkout -b feature/your-feature-name
   ```
   or
   ```bash
   git checkout -b fix/issue-number-description
   ```

2. Make your changes following the coding standards (see below)

3. Test your changes thoroughly

4. Commit your changes with a clear message:
   ```bash
   git commit -m "Add feature: description of your changes"
   ```

5. Push to your fork:
   ```bash
   git push origin feature/your-feature-name
   ```

6. Create a Pull Request from your fork to the main repository

### Coding Standards

- **Language**: C# 12 with nullable reference types enabled
- **Framework**: .NET 9.0 Windows
- **UI Framework**: WPF with XAML
- **Naming Conventions**:
  - PascalCase for classes, methods, properties
  - camelCase for local variables and parameters
  - _camelCase for private fields
- **Code Style**:
  - Use file-scoped namespaces
  - Prefer `var` when type is obvious
  - Use expression-bodied members when appropriate
  - Keep methods focused and concise
- **Comments**:
  - XML documentation comments for public APIs
  - Inline comments for complex logic only
  - Clear variable and method names over excessive comments

### Testing

- Test your changes on Windows 10 and Windows 11 if possible
- Test with various Markdown files (local and remote)
- Test drag-and-drop functionality
- Test session persistence (close and reopen app)
- Verify zoom and search features work correctly

## Types of Contributions

### Bug Fixes
- Check existing issues first
- Create an issue if one doesn't exist
- Reference the issue number in your PR

### New Features
- Discuss the feature in an issue first
- Ensure it aligns with the project goals
- Update documentation as needed

### Documentation
- Fix typos or clarify existing docs
- Add examples or tutorials
- Improve installation instructions

### Code Quality
- Refactoring for better maintainability
- Performance improvements
- Security enhancements

## Pull Request Guidelines

### Before Submitting
- [ ] Code builds without errors or warnings
- [ ] All features work as expected
- [ ] No security vulnerabilities introduced
- [ ] Documentation updated if needed
- [ ] Commit messages are clear and descriptive

### PR Description Should Include
- Summary of changes
- Related issue number (if applicable)
- Testing performed
- Screenshots (for UI changes)

### Review Process
1. Maintainers will review your PR
2. Address any feedback or requested changes
3. Once approved, your PR will be merged

## Project Structure

```
MdReader/
├── src/
│   └── MdReader/           # Main application project
│       ├── *.xaml          # UI files
│       ├── *.xaml.cs       # Code-behind files
│       └── *.cs            # Other C# files
├── docs/                   # Documentation
├── build.bat              # Build script
├── publish.bat            # Publish script
└── README.md              # Project readme
```

## Areas for Contribution

Here are some areas where contributions would be especially welcome:

### High Priority
- Full-text search with highlighting
- Better error handling and user feedback
- Theme support (dark mode)
- Keyboard shortcuts for common actions

### Medium Priority
- Export to PDF/HTML
- Print support
- Bookmarks/Favorites
- Recent files menu

### Nice to Have
- Custom CSS for Markdown rendering
- Plugin system
- Git repository integration
- Syntax highlighting improvements

## Code of Conduct

### Our Standards
- Be respectful and inclusive
- Welcome newcomers
- Accept constructive criticism gracefully
- Focus on what's best for the project

### Unacceptable Behavior
- Harassment or discriminatory language
- Trolling or insulting comments
- Publishing others' private information
- Other unprofessional conduct

## Getting Help

- **Questions**: Open an issue with the "question" label
- **Bugs**: Open an issue with the "bug" label
- **Feature Requests**: Open an issue with the "enhancement" label

## License

By contributing to MdReader, you agree that your contributions will be licensed under the MIT License.

## Recognition

Contributors will be acknowledged in the project. Significant contributions may result in being added as a project maintainer.

---

Thank you for contributing to MdReader! 🎉
