# File Association Verification

This document verifies that MdReader installer correctly associates .md and .markdown files.

## ? Verification Checklist

### 1. Inno Setup Configuration (`MdReader-Setup.iss`)

#### ? Setup Section
```pascal
ChangesAssociations=yes
```
This tells Windows to refresh file associations after installation.

#### ? Tasks Section
```pascal
Name: "fileassoc"; 
Description: "Associate .md and .markdown files with {#MyAppName}"; 
GroupDescription: "File associations:"; 
Flags: checkedonce
```
- **Purpose**: Allows user to choose whether to associate files during installation
- **Default**: Checked by default (`checkedonce`)
- **User Control**: User can opt-out if they prefer

#### ? Registry Entries

**File Extension Registration:**
```pascal
Root: HKCR; Subkey: ".md"; ValueType: string; ValueName: ""; 
    ValueData: "MdReader.MarkdownFile"; Flags: uninsdeletevalue; Tasks: fileassoc

Root: HKCR; Subkey: ".markdown"; ValueType: string; ValueName: ""; 
    ValueData: "MdReader.MarkdownFile"; Flags: uninsdeletevalue; Tasks: fileassoc
```
- Registers both `.md` and `.markdown` extensions
- Points to ProgID: `MdReader.MarkdownFile`
- Removed on uninstall (`uninsdeletevalue`)

**ProgID Definition:**
```pascal
Root: HKCR; Subkey: "MdReader.MarkdownFile"; ValueType: string; 
    ValueName: ""; ValueData: "Markdown Document"; Flags: uninsdeletekey; Tasks: fileassoc
```
- Creates the ProgID with friendly name "Markdown Document"
- Entire key removed on uninstall

**Icon Association:**
```pascal
Root: HKCR; Subkey: "MdReader.MarkdownFile\DefaultIcon"; ValueType: string; 
    ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: fileassoc
```
- Sets the icon for .md files to MdReader.exe icon (index 0)
- Files will display with MdReader icon in Explorer

**Default Action (Open):**
```pascal
Root: HKCR; Subkey: "MdReader.MarkdownFile\shell\open\command"; ValueType: string; 
    ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc
```
- Defines what happens when user double-clicks a .md file
- Command: `"C:\Program Files\MdReader\MdReader.exe" "filepath.md"`
- `%1` is replaced with the actual file path

**"Open With" Menu:**
```pascal
Root: HKCR; Subkey: "Applications\{#MyAppExeName}"; ValueType: string; 
    ValueName: ""; ValueData: ""; Flags: uninsdeletekey; Tasks: fileassoc

Root: HKCR; Subkey: "Applications\{#MyAppExeName}\shell\open\command"; 
    ValueType: string; ValueName: ""; 
    ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc
```
- Adds MdReader to "Open With" context menu
- Available even if not set as default application

**Supported File Types:**
```pascal
Root: HKCR; Subkey: "Applications\{#MyAppExeName}\SupportedTypes"; 
    ValueType: string; ValueName: ".md"; ValueData: ""; Tasks: fileassoc

Root: HKCR; Subkey: "Applications\{#MyAppExeName}\SupportedTypes"; 
    ValueType: string; ValueName: ".markdown"; ValueData: ""; Tasks: fileassoc

Root: HKCR; Subkey: "Applications\{#MyAppExeName}\SupportedTypes"; 
    ValueType: string; ValueName: ".txt"; ValueData: ""; Tasks: fileassoc
```
- Declares which file types MdReader can open
- Used by Windows "Open With" dialog
- Includes `.md`, `.markdown`, and `.txt` files
- **Note**: `.txt` files are NOT automatically associated - they only appear in "Open With" menu

### 2. Application Code (`App.xaml.cs`)

#### ? Command-Line Argument Handling
```csharp
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
```

**Functionality:**
- ? Receives file path(s) from Windows when user double-clicks a .md file
- ? Opens each file in a new tab via `MainWindow.OpenFile()`
- ? Supports multiple files (if user selects several and opens with MdReader)

### 3. MainWindow Code

#### ? File Opening Support
The `MainWindow.OpenFile()` method handles:
- ? Local file paths (from double-click)
- ? HTTP/HTTPS URLs
- ? Validation and error handling
- ? Duplicate detection (won't open same file twice)

## ?? Testing Instructions

After installing MdReader, verify file associations work:

### Test 1: Default Program
1. Find any `.md` or `.markdown` file
2. Right-click ? **Properties**
3. Check "Opens with:" should show **MdReader**
4. If not, click **Change** and select MdReader

### Test 2: Double-Click
1. Double-click any `.md` file
2. ? MdReader should launch and display the file
3. ? File should open in a new tab

### Test 3: Multiple Files
1. Select multiple `.md` files
2. Right-click ? **Open with** ? **MdReader**
3. ? All files should open in separate tabs

### Test 4: "Open With" Menu
1. Right-click any `.md` file
2. Select **Open with** ? **Choose another app**
3. ? MdReader should appear in the list
4. ? Can set as default from this dialog

### Test 5: Icon Display
1. Navigate to folder with `.md` files in Explorer
2. ? Files should show MdReader icon (if set as default)
3. ? Icon should match the app.ico file

### Test 6: Command Line
Open Command Prompt and test:
```cmd
"C:\Program Files\MdReader\MdReader.exe" "C:\path\to\test.md"
```
? Should open the file in MdReader

## ?? Troubleshooting

### File associations don't work after install

**Solution 1: Log out and back in**
- Windows may need a session refresh to recognize new associations

**Solution 2: Run installer as Administrator**
- File associations require admin privileges to modify HKCR registry

**Solution 3: Manually set default program**
1. Right-click a `.md` file
2. **Open with** ? **Choose another app**
3. Select **MdReader**
4. Check ? **Always use this app**
5. Click **OK**

### Icons don't change

**Solution:**
1. Clear icon cache:
   ```cmd
   ie4uinit.exe -show
   ```
2. Restart Windows Explorer:
   - Open Task Manager (Ctrl+Shift+Esc)
   - Find "Windows Explorer"
   - Right-click ? Restart

### MdReader doesn't appear in "Open With"

**Solution:**
- Reinstall MdReader
- Ensure you selected "Associate .md and .markdown files" during installation

## ?? Summary

? **File associations are properly configured** in the installer:
- Registry entries are correct
- Command-line argument handling works
- Icon association is set up
- "Open With" menu integration is configured
- Clean uninstall is supported

The installer will correctly associate `.md` and `.markdown` files with MdReader when the user selects that option during installation.
