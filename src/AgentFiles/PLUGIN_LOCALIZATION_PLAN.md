# Plugin Localization Plan - Phase 3 (Optional)

## ?? Overview

**Status**: Planning Phase  
**Priority**: Optional / Future Enhancement  
**Estimated Effort**: 15-20 hours total  
**Scope**: 7 plugin projects with dialogs

---

## ?? What Needs to Be Done First

### Step 1: Understand Plugin Architecture

**Key Points**:
- ? Plugins are **separate assemblies** (different `.csproj` files)
- ? Each plugin needs its **own `Resources.resx`** file
- ? Cannot use `LogExpert.Resources` (different assembly)
- ? Each plugin is independently loaded at runtime

**Affected Plugins**:
1. **SftpFileSystemx64** - 4 dialogs
2. **CsvColumnizer** - 1 dialog
3. **Log4jXmlColumnizer** - 1 dialog
4. **RegexColumnizer** - 1 dialog

---

## ?? Plugin Analysis

### Plugin 1: CsvColumnizer ? START HERE (Simplest)

**Dialog**: `CsvColumnizerConfigDlg`  
**Elements to Localize**: ~15 elements  
**Complexity**: LOW (good starting point)

**UI Elements**:
- Dialog title: "CSV Columnizer Configuration"
- 5 labels (Delimiter char, Quote char, Escape char, Comment char, Min columns)
- 2 checkboxes (use escape chars, First line contains field names)
- 1 info label (0 = no minimum check)
- 2 buttons (OK, Cancel)

**Resource Keys Needed**:
```
CsvColumnizerConfigDlg_UI_Title
CsvColumnizerConfigDlg_UI_Label_DelimiterChar
CsvColumnizerConfigDlg_UI_Label_QuoteChar
CsvColumnizerConfigDlg_UI_Label_EscapeChar
CsvColumnizerConfigDlg_UI_Label_CommentChar
CsvColumnizerConfigDlg_UI_Label_MinColumns
CsvColumnizerConfigDlg_UI_Label_MinColumnsInfo
CsvColumnizerConfigDlg_UI_CheckBox_UseEscapeChars
CsvColumnizerConfigDlg_UI_CheckBox_FirstLineFieldNames
CsvColumnizerConfigDlg_UI_Button_OK
CsvColumnizerConfigDlg_UI_Button_Cancel
```

---

### Plugin 2: Log4jXmlColumnizer ?? MEDIUM

**Dialog**: `Log4jXmlColumnizerConfigDlg`  
**Elements to Localize**: ~10 elements  
**Complexity**: MEDIUM (has DataGridView)

**UI Elements**:
- Dialog title
- DataGridView column headers (Visible, Column name, Max length)
- Checkbox (Show local time)
- 2 buttons (OK, Cancel)

---

### Plugin 3: RegexColumnizer ?? MEDIUM

**Dialog**: `RegexColumnizerConfigDialog`  
**Elements to Localize**: ~20 elements  
**Complexity**: MEDIUM (multiple tabs/sections)

**Needs Investigation**: File needs to be examined for exact UI elements

---

### Plugin 4: SftpFileSystemx64 ??? COMPLEX

**Dialogs**: 4 dialogs  
**Elements to Localize**: ~50+ elements  
**Complexity**: HIGH (multiple complex forms)

**Dialogs**:
1. `ConfigDialog` - SFTP connection settings
2. `LoginDialog` - Login credentials
3. `FailedKeyDialog` - Key authentication failure
4. `PrivateKeyPasswordDialog` - Private key password

---

## ??? Implementation Steps (Per Plugin)

### Phase 3.1: Setup Resource Infrastructure

**For EACH Plugin Project**:

#### 1. Create Resources Folder Structure
```
PluginName/
??? Resources/
?   ??? Resources.resx          (English - base)
?   ??? Resources.de.resx       (German)
?   ??? Resources.Designer.cs   (auto-generated)
??? Dialogs/
    ??? DialogName.cs
```

#### 2. Create Resources.resx File

**Steps**:
1. Right-click plugin project in Solution Explorer
2. Add ? New Item ? Resources File
3. Name: `Resources.resx`
4. Set properties:
   - **Build Action**: Embedded Resource
   - **Custom Tool**: PublicResXFileCodeGenerator
   - **Access Modifier**: Public

#### 3. Create Resources.de.resx File

**Steps**:
1. Copy `Resources.resx`
2. Rename to `Resources.de.resx`
3. Set properties same as above

#### 4. Update Project File (.csproj)

Add to `.csproj`:
```xml
<ItemGroup>
  <EmbeddedResource Update="Resources\Resources.resx">
    <Generator>PublicResXFileCodeGenerator</Generator>
    <LastGenOutput>Resources.Designer.cs</LastGenOutput>
  </EmbeddedResource>
  
  <EmbeddedResource Update="Resources\Resources.de.resx">
    <DependentUpon>Resources.resx</DependentUpon>
  </EmbeddedResource>
</ItemGroup>

<ItemGroup>
  <Compile Update="Resources\Resources.Designer.cs">
    <DesignTime>True</DesignTime>
    <AutoGen>True</AutoGen>
    <DependentUpon>Resources.resx</DependentUpon>
  </Compile>
</ItemGroup>
```

---

### Phase 3.2: Localize Each Dialog

**For EACH Dialog in Plugin**:

#### 1. Add Resource Keys

Add to `Resources.resx`:
```xml
<data name="DialogName_UI_Title" xml:space="preserve">
  <value>Dialog Title</value>
</data>
<data name="DialogName_UI_Label_Field" xml:space="preserve">
  <value>Field Label</value>
</data>
<!-- ... more keys -->
```

#### 2. Add German Translations

Add to `Resources.de.resx`:
```xml
<data name="DialogName_UI_Title" xml:space="preserve">
  <value>Dialog-Titel</value>
</data>
<!-- ... more translations -->
```

#### 3. Update Dialog Code

Modify dialog `.cs` file:
```csharp
public DialogName()
{
    SuspendLayout();
    InitializeComponent();
    
    AutoScaleDimensions = new SizeF(96F, 96F);
    AutoScaleMode = AutoScaleMode.Dpi;
    
    ApplyResources();  // ADD THIS
    
    ResumeLayout();
}

// ADD THIS METHOD
private void ApplyResources()
{
    Text = Resources.DialogName_UI_Title;
    labelField.Text = Resources.DialogName_UI_Label_Field;
    buttonOk.Text = Resources.DialogName_UI_Button_OK;
    buttonCancel.Text = Resources.DialogName_UI_Button_Cancel;
    // ... more assignments
}
```

#### 4. Test Compilation

```bash
dotnet build PluginName/PluginName.csproj
```

---

### Phase 3.3: Testing

**For EACH Plugin**:

1. ? Build plugin successfully
2. ? Copy plugin to LogExpert plugins folder
3. ? Launch LogExpert
4. ? Test English UI
5. ? Switch to German
6. ? Test German UI
7. ? Verify fallback values work

---

## ?? Implementation Order (Recommended)

### Recommended Sequence:

1. **CsvColumnizer** (2-3 hours)
   - Simplest plugin
   - Only 1 dialog
   - Good learning example
   - Tests the process

2. **Log4jXmlColumnizer** (2-3 hours)
   - Medium complexity
   - 1 dialog with DataGridView
   - Tests column header localization

3. **RegexColumnizer** (3-4 hours)
   - Medium-high complexity
   - More complex dialog
   - Multiple controls

4. **SftpFileSystemx64** (8-10 hours)
   - Most complex
   - 4 separate dialogs
   - Connection/authentication UI
   - Critical functionality

### Time Estimates:

| Plugin | Dialogs | Estimated Time | Priority |
|--------|---------|---------------|----------|
| CsvColumnizer | 1 | 2-3 hours | ? HIGH |
| Log4jXmlColumnizer | 1 | 2-3 hours | ? MEDIUM |
| RegexColumnizer | 1 | 3-4 hours | ? MEDIUM |
| SftpFileSystemx64 | 4 | 8-10 hours | ?? LOW |
| **TOTAL** | **7** | **15-20 hours** | |

---

## ?? Quick Start Guide

### To Start Plugin Localization Today:

#### 1. Choose CsvColumnizer (Simplest)

**Why**: 
- Only 15 UI elements
- Single dialog
- No complex controls
- Good proof of concept

#### 2. Create Basic Infrastructure

**Commands**:
```bash
cd src/CsvColumnizer
mkdir Resources
# Create Resources.resx manually or via Visual Studio
```

#### 3. Add First Resource Keys

Start with just the dialog title:
```xml
<data name="CsvColumnizerConfigDlg_UI_Title" xml:space="preserve">
  <value>CSV Columnizer Configuration</value>
</data>
```

#### 4. Test the Process

Verify:
- ? Resources.Designer.cs generates
- ? Can access `Resources.CsvColumnizerConfigDlg_UI_Title`
- ? Dialog compiles
- ? Plugin loads in LogExpert

---

## ?? Challenges & Considerations

### Challenge 1: Separate Assemblies

**Problem**: Each plugin is a separate assembly  
**Solution**: Each needs its own Resources.resx  
**Impact**: Cannot reuse LogExpert.Resources

### Challenge 2: Common Resources

**Problem**: Buttons like "OK", "Cancel" repeated across plugins  
**Solution**: Either:
- Option A: Duplicate in each plugin
- Option B: Create shared "PluginResources" assembly

**Recommendation**: Start with Option A (duplication), consider Option B if needed

### Challenge 3: Plugin Loading

**Problem**: Plugins are dynamically loaded  
**Solution**: Ensure satellite assemblies (.resources.dll) are deployed with plugins  
**Impact**: Need to verify build output includes German resources

### Challenge 4: Testing

**Problem**: Need to test each plugin independently  
**Solution**: 
- Build plugin
- Copy to LogExpert plugins folder
- Test in running application

---

## ?? Prerequisites

Before starting plugin localization:

### ? Completed Prerequisites:
- Core LogExpert localization (75% complete)
- Understanding of resource file structure
- Knowledge of `ApplyResources()` pattern

### ?? Tools Needed:
- Visual Studio or VS Code
- ResX Resource Manager (optional, helpful)
- German language knowledge (or translation tool)

---

## ?? Decision Point

### Should You Start Plugin Localization?

**YES, if**:
- ? Core LogExpert localization is complete (IT IS!)
- ? You want 100% application localization
- ? Plugins are actively used by German users
- ? You have 15-20 hours available

**NO, if**:
- ?? Core features more important
- ?? Plugin usage is low
- ?? Time constraints exist
- ?? Can wait for future release

---

## ?? Expected Outcome

### After Plugin Localization:

**Before**: 75% complete (33/44 components)  
**After**: ~93% complete (40/44 components)  

**Remaining**: Only 4 out-of-scope components (Eminus, ToolArgs, ParamRequester, misc)

---

## ?? Recommendation

### My Recommendation:

**START with CsvColumnizer** as a proof of concept:

1. **Why**: 
   - Simplest plugin (2-3 hours)
   - Tests the entire process
   - Low risk, high learning value
   - Can stop after this if desired

2. **Next Steps if Successful**:
   - Continue with Log4jXmlColumnizer
   - Then RegexColumnizer
   - Finally SftpFileSystemx64

3. **If Issues Arise**:
   - Easy to pause
   - Limited scope
   - Can document challenges

---

## ?? First Steps (Next Actions)

### Immediate Next Steps:

1. **Create CsvColumnizer Resources Folder**
   ```bash
   cd src/CsvColumnizer
   mkdir Resources
   ```

2. **Create Resources.resx File**
   - Via Visual Studio: Add ? Resources File
   - Name: `Resources.resx`

3. **Add First Resource Key**
   - Just the dialog title to test

4. **Update CsvColumnizerConfigDlg.cs**
   - Add `ApplyResources()` method
   - Test compilation

5. **Verify It Works**
   - Build plugin
   - Test in LogExpert

---

## ?? Summary

**What needs to be done first**:
1. ? Decide if plugin localization is worth the effort
2. ? Choose starting plugin (recommend: CsvColumnizer)
3. ? Create Resources.resx infrastructure
4. ? Add resource keys for one dialog
5. ? Update dialog code with `ApplyResources()`
6. ? Test and verify

**Estimated Time to First Plugin**: 2-3 hours  
**Estimated Time to All Plugins**: 15-20 hours  
**Complexity**: Medium (requires separate resource files per plugin)  
**Priority**: Optional/Enhancement

---

**Ready to start?** I can help you with CsvColumnizer first! ??
