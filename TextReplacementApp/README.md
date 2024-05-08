<a name='assembly'></a>
# tra

## Contents

- [AppConfig](#T-TextReplacementApp-AppConfig 'TextReplacementApp.AppConfig')
  - [DirectoryPath](#P-TextReplacementApp-AppConfig-DirectoryPath 'TextReplacementApp.AppConfig.DirectoryPath')
  - [FindWhat](#P-TextReplacementApp-AppConfig-FindWhat 'TextReplacementApp.AppConfig.FindWhat')
  - [ReplaceWith](#P-TextReplacementApp-AppConfig-ReplaceWith 'TextReplacementApp.AppConfig.ReplaceWith')
- [ListExtensions](#T-TextReplacementApp-ListExtensions 'TextReplacementApp.ListExtensions')
  - [IsAnyOf\`\`1(value,testObjects)](#M-TextReplacementApp-ListExtensions-IsAnyOf``1-``0,``0[]- 'TextReplacementApp.ListExtensions.IsAnyOf``1(``0,``0[])')
- [MainWindow](#T-TextReplacementApp-MainWindow 'TextReplacementApp.MainWindow')
  - [#ctor()](#M-TextReplacementApp-MainWindow-#ctor 'TextReplacementApp.MainWindow.#ctor')
  - [appConfig](#F-TextReplacementApp-MainWindow-appConfig 'TextReplacementApp.MainWindow.appConfig')
  - [components](#F-TextReplacementApp-MainWindow-components 'TextReplacementApp.MainWindow.components')
  - [configFilePath](#F-TextReplacementApp-MainWindow-configFilePath 'TextReplacementApp.MainWindow.configFilePath')
  - [Dispose(disposing)](#M-TextReplacementApp-MainWindow-Dispose-System-Boolean- 'TextReplacementApp.MainWindow.Dispose(System.Boolean)')
  - [InitializeComponent()](#M-TextReplacementApp-MainWindow-InitializeComponent 'TextReplacementApp.MainWindow.InitializeComponent')
  - [LoadConfig()](#M-TextReplacementApp-MainWindow-LoadConfig 'TextReplacementApp.MainWindow.LoadConfig')
  - [OnClickBrowseButton(sender,e)](#M-TextReplacementApp-MainWindow-OnClickBrowseButton-System-Object,System-EventArgs- 'TextReplacementApp.MainWindow.OnClickBrowseButton(System.Object,System.EventArgs)')
  - [OnClickDoItButton(sender,e)](#M-TextReplacementApp-MainWindow-OnClickDoItButton-System-Object,System-EventArgs- 'TextReplacementApp.MainWindow.OnClickDoItButton(System.Object,System.EventArgs)')
  - [OnFormClosing(e)](#M-TextReplacementApp-MainWindow-OnFormClosing-System-Windows-Forms-FormClosingEventArgs- 'TextReplacementApp.MainWindow.OnFormClosing(System.Windows.Forms.FormClosingEventArgs)')
  - [OnTextChangedDirectoryPath(sender,e)](#M-TextReplacementApp-MainWindow-OnTextChangedDirectoryPath-System-Object,System-EventArgs- 'TextReplacementApp.MainWindow.OnTextChangedDirectoryPath(System.Object,System.EventArgs)')
  - [OnTextChangedReplaceText(sender,e)](#M-TextReplacementApp-MainWindow-OnTextChangedReplaceText-System-Object,System-EventArgs- 'TextReplacementApp.MainWindow.OnTextChangedReplaceText(System.Object,System.EventArgs)')
  - [OnTextChangedSearchText(sender,e)](#M-TextReplacementApp-MainWindow-OnTextChangedSearchText-System-Object,System-EventArgs- 'TextReplacementApp.MainWindow.OnTextChangedSearchText(System.Object,System.EventArgs)')
  - [ReadTextFromMemory(accessor,length)](#M-TextReplacementApp-MainWindow-ReadTextFromMemory-System-IO-UnmanagedMemoryAccessor,System-Int64- 'TextReplacementApp.MainWindow.ReadTextFromMemory(System.IO.UnmanagedMemoryAccessor,System.Int64)')
  - [ReplaceTextInFile(filePath,searchText,replaceText)](#M-TextReplacementApp-MainWindow-ReplaceTextInFile-System-String,System-String,System-String- 'TextReplacementApp.MainWindow.ReplaceTextInFile(System.String,System.String,System.String)')
  - [ReplaceTextInFiles(directoryPath,searchText,replaceText,progressReporter)](#M-TextReplacementApp-MainWindow-ReplaceTextInFiles-System-String,System-String,System-String,System-IProgress{TextReplacementApp-ProgressReport}- 'TextReplacementApp.MainWindow.ReplaceTextInFiles(System.String,System.String,System.String,System.IProgress{TextReplacementApp.ProgressReport})')
  - [SaveConfig()](#M-TextReplacementApp-MainWindow-SaveConfig 'TextReplacementApp.MainWindow.SaveConfig')
  - [UpdateTextBoxesFromConfig()](#M-TextReplacementApp-MainWindow-UpdateTextBoxesFromConfig 'TextReplacementApp.MainWindow.UpdateTextBoxesFromConfig')
  - [WriteTextToMemory(accessor,text)](#M-TextReplacementApp-MainWindow-WriteTextToMemory-System-IO-UnmanagedMemoryAccessor,System-String- 'TextReplacementApp.MainWindow.WriteTextToMemory(System.IO.UnmanagedMemoryAccessor,System.String)')
- [Program](#T-TextReplacementApp-Program 'TextReplacementApp.Program')
  - [Main()](#M-TextReplacementApp-Program-Main 'TextReplacementApp.Program.Main')
- [ProgressDialog](#T-TextReplacementApp-ProgressDialog 'TextReplacementApp.ProgressDialog')
  - [#ctor()](#M-TextReplacementApp-ProgressDialog-#ctor 'TextReplacementApp.ProgressDialog.#ctor')
  - [components](#F-TextReplacementApp-ProgressDialog-components 'TextReplacementApp.ProgressDialog.components')
  - [Dispose(disposing)](#M-TextReplacementApp-ProgressDialog-Dispose-System-Boolean- 'TextReplacementApp.ProgressDialog.Dispose(System.Boolean)')
  - [InitializeComponent()](#M-TextReplacementApp-ProgressDialog-InitializeComponent 'TextReplacementApp.ProgressDialog.InitializeComponent')
  - [OnLoad(e)](#M-TextReplacementApp-ProgressDialog-OnLoad-System-EventArgs- 'TextReplacementApp.ProgressDialog.OnLoad(System.EventArgs)')
  - [UpdateProgress(filePath,progressPercentage)](#M-TextReplacementApp-ProgressDialog-UpdateProgress-System-String,System-Int32- 'TextReplacementApp.ProgressDialog.UpdateProgress(System.String,System.Int32)')
- [ProgressReport](#T-TextReplacementApp-ProgressReport 'TextReplacementApp.ProgressReport')
  - [#ctor(currentFile,progressPercentage)](#M-TextReplacementApp-ProgressReport-#ctor-System-String,System-Int32- 'TextReplacementApp.ProgressReport.#ctor(System.String,System.Int32)')
  - [CurrentFile](#P-TextReplacementApp-ProgressReport-CurrentFile 'TextReplacementApp.ProgressReport.CurrentFile')
  - [ProgressPercentage](#P-TextReplacementApp-ProgressReport-ProgressPercentage 'TextReplacementApp.ProgressReport.ProgressPercentage')
- [Resources](#T-TextReplacementApp-Properties-Resources 'TextReplacementApp.Properties.Resources')
  - [Culture](#P-TextReplacementApp-Properties-Resources-Culture 'TextReplacementApp.Properties.Resources.Culture')
  - [ResourceManager](#P-TextReplacementApp-Properties-Resources-ResourceManager 'TextReplacementApp.Properties.Resources.ResourceManager')

<a name='T-TextReplacementApp-AppConfig'></a>
## AppConfig `type`

##### Namespace

TextReplacementApp

##### Summary

Represents the configuration settings of the application.

##### Remarks

This class encapsulates the configuration settings of the application,
including the directory path, search text, and replace text.

<a name='P-TextReplacementApp-AppConfig-DirectoryPath'></a>
### DirectoryPath `property`

##### Summary

Gets or sets the directory path stored in the configuration.

<a name='P-TextReplacementApp-AppConfig-FindWhat'></a>
### FindWhat `property`

##### Summary

Gets or sets the search text stored in the configuration.

<a name='P-TextReplacementApp-AppConfig-ReplaceWith'></a>
### ReplaceWith `property`

##### Summary

Gets or sets the replace text stored in the configuration.

<a name='T-TextReplacementApp-ListExtensions'></a>
## ListExtensions `type`

##### Namespace

TextReplacementApp

##### Summary

Exposes static extension methods for checking the contents of a list.

<a name='M-TextReplacementApp-ListExtensions-IsAnyOf``1-``0,``0[]-'></a>
### IsAnyOf\`\`1(value,testObjects) `method`

##### Summary

Compares the `value` object with the
`testObjects` provided, to see if any of the
`testObjects` is a match.

##### Returns

True if any of the `testObjects` equals the value;
false otherwise.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| value | [\`\`0](#T-``0 '``0') | Source object to check. |
| testObjects | [\`\`0[]](#T-``0[] '``0[]') | Object or objects that should be compared to value
with the [Equals](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Object.Equals 'System.Object.Equals') method. |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T | Type of the object to be tested. |

<a name='T-TextReplacementApp-MainWindow'></a>
## MainWindow `type`

##### Namespace

TextReplacementApp

##### Summary

Represents the main form of the TextReplacementApp application.

##### Remarks

The [MainWindow](#T-TextReplacementApp-MainWindow 'TextReplacementApp.MainWindow') class provides the
user interface for the application, allowing users to specify the directory
where text replacement should occur and the text to search for and replace. It
also initiates the text replacement process and displays progress using a
progress dialog.

<a name='M-TextReplacementApp-MainWindow-#ctor'></a>
### #ctor() `constructor`

##### Summary

Constructs a new instance of [MainWindow](#T-TextReplacementApp-MainWindow 'TextReplacementApp.MainWindow') and
returns a reference to it.

##### Parameters

This constructor has no parameters.

<a name='F-TextReplacementApp-MainWindow-appConfig'></a>
### appConfig `constants`

##### Summary

The application configuration settings.

##### Remarks

This field stores the application configuration settings,
including the directory path, search text, and replace text.

<a name='F-TextReplacementApp-MainWindow-components'></a>
### components `constants`

##### Summary

Required designer variable.

<a name='F-TextReplacementApp-MainWindow-configFilePath'></a>
### configFilePath `constants`

##### Summary

The fully-qualified pathname to the application configuration file.

##### Remarks

This field stores the location of the application configuration file
on the file system. The file is named `config.json` and is stored in the
`%LOCALAPPDATA%\xyLOGIX\File Text Replacer Tool` directory.

<a name='M-TextReplacementApp-MainWindow-Dispose-System-Boolean-'></a>
### Dispose(disposing) `method`

##### Summary

Clean up any resources being used.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| disposing | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | true if managed resources should be disposed; otherwise, false. |

<a name='M-TextReplacementApp-MainWindow-InitializeComponent'></a>
### InitializeComponent() `method`

##### Summary

Required method for Designer support - do not modify
the contents of this method with the code editor.

##### Parameters

This method has no parameters.

<a name='M-TextReplacementApp-MainWindow-LoadConfig'></a>
### LoadConfig() `method`

##### Summary

Loads the configuration settings from the specified JSON file.

##### Returns

An instance of [AppConfig](#T-TextReplacementApp-AppConfig 'TextReplacementApp.AppConfig')
containing the loaded configuration settings. If the file does not exist, a new
instance of [AppConfig](#T-TextReplacementApp-AppConfig 'TextReplacementApp.AppConfig') is returned.

##### Parameters

This method has no parameters.

##### Remarks

This method reads the configuration settings from the specified JSON
file and deserializes them into an
[AppConfig](#T-TextReplacementApp-AppConfig 'TextReplacementApp.AppConfig')
instance. If the file does not exist, a new instance of
[AppConfig](#T-TextReplacementApp-AppConfig 'TextReplacementApp.AppConfig') is returned with all properties
set to empty strings.

<a name='M-TextReplacementApp-MainWindow-OnClickBrowseButton-System-Object,System-EventArgs-'></a>
### OnClickBrowseButton(sender,e) `method`

##### Summary

Event handler for the [](#E-System-Windows-Forms-Control-Click 'System.Windows.Forms.Control.Click') event
event of the button.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| sender | [System.Object](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Object 'System.Object') | The object that triggered the event. |
| e | [System.EventArgs](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.EventArgs 'System.EventArgs') | The event arguments. |

##### Remarks

This method is called when the user clicks the button to
select a directory using the folder browser dialog. It updates the text of the
directory path textbox with the selected directory path.

<a name='M-TextReplacementApp-MainWindow-OnClickDoItButton-System-Object,System-EventArgs-'></a>
### OnClickDoItButton(sender,e) `method`

##### Summary

Event handler for the [](#E-System-Windows-Forms-Control-Click 'System.Windows.Forms.Control.Click') event
of the button.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| sender | [System.Object](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Object 'System.Object') | The object that triggered the event. |
| e | [System.EventArgs](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.EventArgs 'System.EventArgs') | The event arguments. |

##### Remarks

This method is called when the user clicks the button
to initiate the text replacement process. It performs validation,
creates a progress dialog, starts a background task for text replacement,
and displays the progress dialog modally.

<a name='M-TextReplacementApp-MainWindow-OnFormClosing-System-Windows-Forms-FormClosingEventArgs-'></a>
### OnFormClosing(e) `method`

##### Summary

Raises the [](#E-System-Windows-Forms-Form-FormClosing 'System.Windows.Forms.Form.FormClosing')
event.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| e | [System.Windows.Forms.FormClosingEventArgs](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Windows.Forms.FormClosingEventArgs 'System.Windows.Forms.FormClosingEventArgs') | A [FormClosingEventArgs](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Windows.Forms.FormClosingEventArgs 'System.Windows.Forms.FormClosingEventArgs')
that contains the event data. |

<a name='M-TextReplacementApp-MainWindow-OnTextChangedDirectoryPath-System-Object,System-EventArgs-'></a>
### OnTextChangedDirectoryPath(sender,e) `method`

##### Summary

Event handler for the
[](#E-System-Windows-Forms-Control-TextChanged 'System.Windows.Forms.Control.TextChanged') event of the
text box. Updates the
[DirectoryPath](#P-TextReplacementApp-AppConfig-DirectoryPath 'TextReplacementApp.AppConfig.DirectoryPath') property with the
trimmed text from the directory path text box.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| sender | [System.Object](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Object 'System.Object') | The object that raised the event. |
| e | [System.EventArgs](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.EventArgs 'System.EventArgs') | The event data. |

<a name='M-TextReplacementApp-MainWindow-OnTextChangedReplaceText-System-Object,System-EventArgs-'></a>
### OnTextChangedReplaceText(sender,e) `method`

##### Summary

Event handler for the
[](#E-System-Windows-Forms-Control-TextChanged 'System.Windows.Forms.Control.TextChanged') event of the
text box. Updates the
[ReplaceWith](#P-TextReplacementApp-AppConfig-ReplaceWith 'TextReplacementApp.AppConfig.ReplaceWith') property with the
trimmed text from the directory path text box.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| sender | [System.Object](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Object 'System.Object') | The object that raised the event. |
| e | [System.EventArgs](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.EventArgs 'System.EventArgs') | The event data. |

<a name='M-TextReplacementApp-MainWindow-OnTextChangedSearchText-System-Object,System-EventArgs-'></a>
### OnTextChangedSearchText(sender,e) `method`

##### Summary

Event handler for the
[](#E-System-Windows-Forms-Control-TextChanged 'System.Windows.Forms.Control.TextChanged') event of the
text box. Updates the
[FindWhat](#P-TextReplacementApp-AppConfig-FindWhat 'TextReplacementApp.AppConfig.FindWhat') property with the
trimmed text from the directory path text box.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| sender | [System.Object](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Object 'System.Object') | The object that raised the event. |
| e | [System.EventArgs](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.EventArgs 'System.EventArgs') | The event data. |

<a name='M-TextReplacementApp-MainWindow-ReadTextFromMemory-System-IO-UnmanagedMemoryAccessor,System-Int64-'></a>
### ReadTextFromMemory(accessor,length) `method`

##### Summary

Reads text from a memory-mapped file accessor.

##### Returns

The text read from memory.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| accessor | [System.IO.UnmanagedMemoryAccessor](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.IO.UnmanagedMemoryAccessor 'System.IO.UnmanagedMemoryAccessor') | The memory-mapped file accessor. |
| length | [System.Int64](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int64 'System.Int64') | The length of the text to read. |

##### Remarks

This method reads text from a memory-mapped file accessor and returns it as a
string.
It reads the specified length of bytes from the accessor and decodes them using
UTF-8 encoding.

<a name='M-TextReplacementApp-MainWindow-ReplaceTextInFile-System-String,System-String,System-String-'></a>
### ReplaceTextInFile(filePath,searchText,replaceText) `method`

##### Summary

Replaces text in the specified file.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| filePath | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The path of the file to perform text replacement on. |
| searchText | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The text to search for in the file. |
| replaceText | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The text to replace the search text with. |

##### Remarks

This method performs text replacement in the specified file. It reads
the content of the file, replaces occurrences of the search text with the
replace text, and writes the modified content back to the file. If the file
path contains specific directories (such as `.git`, `.vs`, etc.), it
skips the replacement.

<a name='M-TextReplacementApp-MainWindow-ReplaceTextInFiles-System-String,System-String,System-String,System-IProgress{TextReplacementApp-ProgressReport}-'></a>
### ReplaceTextInFiles(directoryPath,searchText,replaceText,progressReporter) `method`

##### Summary

Replaces text in all files within the specified directory and its
subdirectories.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| directoryPath | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The path of the directory to search for files. |
| searchText | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The text to search for in each file. |
| replaceText | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The text to replace the search text with. |
| progressReporter | [System.IProgress{TextReplacementApp.ProgressReport}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.IProgress 'System.IProgress{TextReplacementApp.ProgressReport}') | An object for reporting progress during text
replacement. |

##### Remarks

This method recursively searches for files within the specified
directory and its subdirectories. For each file found, it calls
[ReplaceTextInFile](#M-TextReplacementApp-MainWindow-ReplaceTextInFile-System-String,System-String,System-String- 'TextReplacementApp.MainWindow.ReplaceTextInFile(System.String,System.String,System.String)') to perform text replacement. Progress is
reported using the specified `progressReporter`. Certain
directories (e.g., `.git`, `.vs`, etc.) are excluded from text
replacement.

<a name='M-TextReplacementApp-MainWindow-SaveConfig'></a>
### SaveConfig() `method`

##### Summary

Saves the current application configuration to a JSON file.

##### Parameters

This method has no parameters.

##### Remarks

The method first checks if the directory containing the configuration
file exists, and creates it if it does not. Then, it serializes the
[AppConfig](#T-TextReplacementApp-AppConfig 'TextReplacementApp.AppConfig') object to JSON format using
`Newtonsoft.Json`, and writes the JSON string to the configuration file.

<a name='M-TextReplacementApp-MainWindow-UpdateTextBoxesFromConfig'></a>
### UpdateTextBoxesFromConfig() `method`

##### Summary

Updates the text boxes on the main form with the values stored in the
application configuration.

##### Parameters

This method has no parameters.

##### Remarks

This method retrieves the directory path, search text, and replace
text from the [AppConfig](#T-TextReplacementApp-AppConfig 'TextReplacementApp.AppConfig') object and sets the
corresponding text properties of the text boxes on the main form to these
values.

<a name='M-TextReplacementApp-MainWindow-WriteTextToMemory-System-IO-UnmanagedMemoryAccessor,System-String-'></a>
### WriteTextToMemory(accessor,text) `method`

##### Summary

Writes the specified text to a memory-mapped view accessor.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| accessor | [System.IO.UnmanagedMemoryAccessor](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.IO.UnmanagedMemoryAccessor 'System.IO.UnmanagedMemoryAccessor') | The memory-mapped view accessor to write to. |
| text | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The text to write. |

##### Remarks

This method writes the UTF-8 encoded bytes of the
`text` to the memory-mapped view accessor starting at the
beginning (offset zero). It is the responsibility of the caller to ensure that
the
length of the text matches the length of the memory-mapped view accessor.

<a name='T-TextReplacementApp-Program'></a>
## Program `type`

##### Namespace

TextReplacementApp

##### Summary

Defines the behavior of the application.

<a name='M-TextReplacementApp-Program-Main'></a>
### Main() `method`

##### Summary

The main entry point for the application.

##### Parameters

This method has no parameters.

<a name='T-TextReplacementApp-ProgressDialog'></a>
## ProgressDialog `type`

##### Namespace

TextReplacementApp

##### Summary

Represents a dialog used to display the progress of an operation.

##### Remarks

The [ProgressDialog](#T-TextReplacementApp-ProgressDialog 'TextReplacementApp.ProgressDialog') class provides
a simple dialog for displaying progress information during long-running
operations. It contains a label to show the current file being processed and a
progress bar to indicate the overall progress of the operation.

<a name='M-TextReplacementApp-ProgressDialog-#ctor'></a>
### #ctor() `constructor`

##### Summary

Constructs a new instance of [ProgressDialog](#T-TextReplacementApp-ProgressDialog 'TextReplacementApp.ProgressDialog')
and returns a reference to it.

##### Parameters

This constructor has no parameters.

<a name='F-TextReplacementApp-ProgressDialog-components'></a>
### components `constants`

##### Summary

Required designer variable.

<a name='M-TextReplacementApp-ProgressDialog-Dispose-System-Boolean-'></a>
### Dispose(disposing) `method`

##### Summary

Clean up any resources being used.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| disposing | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | true if managed resources should be disposed; otherwise, false. |

<a name='M-TextReplacementApp-ProgressDialog-InitializeComponent'></a>
### InitializeComponent() `method`

##### Summary

Required method for Designer support - do not modify
the contents of this method with the code editor.

##### Parameters

This method has no parameters.

<a name='M-TextReplacementApp-ProgressDialog-OnLoad-System-EventArgs-'></a>
### OnLoad(e) `method`

##### Summary

Raises the [](#E-System-Windows-Forms-Form-Load 'System.Windows.Forms.Form.Load') event.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| e | [System.EventArgs](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.EventArgs 'System.EventArgs') | An [EventArgs](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.EventArgs 'System.EventArgs') that contains the event
data. |

<a name='M-TextReplacementApp-ProgressDialog-UpdateProgress-System-String,System-Int32-'></a>
### UpdateProgress(filePath,progressPercentage) `method`

##### Summary

Updates the progress of the operation being displayed in the progress dialog.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| filePath | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The path of the current file being processed. |
| progressPercentage | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | The percentage of completion of the operation. |

##### Remarks

This method updates the text displayed for the current file being processed
and adjusts the progress bar to reflect the progress percentage.
If invoked from a different thread than the one that created the control,
it will use [Invoke](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Windows.Forms.Control.Invoke 'System.Windows.Forms.Control.Invoke') to marshal the call to the proper
thread.

<a name='T-TextReplacementApp-ProgressReport'></a>
## ProgressReport `type`

##### Namespace

TextReplacementApp

##### Summary

Represents a progress report containing information about the current file
being processed
and the progress percentage.

<a name='M-TextReplacementApp-ProgressReport-#ctor-System-String,System-Int32-'></a>
### #ctor(currentFile,progressPercentage) `constructor`

##### Summary

Initializes a new instance of the
[ProgressReport](#T-TextReplacementApp-ProgressReport 'TextReplacementApp.ProgressReport') class with the specified
current file and progress percentage.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| currentFile | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The path of the current file being processed. |
| progressPercentage | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | The progress percentage of the operation. |

<a name='P-TextReplacementApp-ProgressReport-CurrentFile'></a>
### CurrentFile `property`

##### Summary

Gets the path of the current file being processed.

<a name='P-TextReplacementApp-ProgressReport-ProgressPercentage'></a>
### ProgressPercentage `property`

##### Summary

Gets the progress percentage of the operation.

<a name='T-TextReplacementApp-Properties-Resources'></a>
## Resources `type`

##### Namespace

TextReplacementApp.Properties

##### Summary

A strongly-typed resource class, for looking up localized strings, etc.

<a name='P-TextReplacementApp-Properties-Resources-Culture'></a>
### Culture `property`

##### Summary

Overrides the current thread's CurrentUICulture property for all
  resource lookups using this strongly typed resource class.

<a name='P-TextReplacementApp-Properties-Resources-ResourceManager'></a>
### ResourceManager `property`

##### Summary

Returns the cached ResourceManager instance used by this class.
