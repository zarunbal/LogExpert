# PatternWindow Localization - Resource Keys Required

## Overview
PatternWindow has been updated with an `ApplyResources()` method. The following resource keys need to be added to `LogExpert.Resources/Resources.resx`:

## Resource Keys to Add

### Dialog Title
| Key | English Value | German Value (Suggested) |
|-----|--------------|-------------------------|
| `PatternWindow_UI_Title` | `"Patterns"` | `"Muster"` |

### Labels
| Key | English Value | German Value (Suggested) |
|-----|--------------|-------------------------|
| `PatternWindow_UI_Label_NumberOfBlocks` | `"Number of blocks (pattern variants):"` | `"Anzahl der Blöcke (Mustervarianten):"` |
| `PatternWindow_UI_Label_BlockLines` | `"Block lines:"` | `"Blockzeilen:"` |
| `PatternWindow_UI_Label_FeatureDescription` | `"This feature is pre-beta and does not work :)\r\nUsage: Select a range in the log window and press \"Recalc\". \r\nThis will search for text ranges similar to the selected one."` | `"Diese Funktion ist Pre-Beta und funktioniert nicht :)\r\nVerwendung: Wählen Sie einen Bereich im Log-Fenster aus und drücken Sie \"Neu berechnen\". \r\nDies sucht nach ähnlichen Textbereichen."` |
| `PatternWindow_UI_Label_Fuzzy` | `"Fuzzy"` | `"Fuzzy"` |
| `PatternWindow_UI_Label_MaxDiff` | `"Max diff"` | `"Max. Abweichung"` |
| `PatternWindow_UI_Label_MaxMisses` | `"Max misses"` | `"Max. Fehlschläge"` |
| `PatternWindow_UI_Label_Weight` | `"Weigth"` | `"Gewicht"` |
| `PatternWindow_UI_Label_NoRangeSet` | `"(no range set)"` | `"(kein Bereich gesetzt)"` |
| `PatternWindow_UI_Label_RangeFormat` | `"Start: {0}\nEnd: {1}"` | `"Start: {0}\nEnde: {1}"` |

**Note**: "Weigth" is a typo in the original code - should be "Weight", but keeping the key name to match the existing control name.

### Buttons
| Key | English Value | German Value (Suggested) |
|-----|--------------|-------------------------|
| `PatternWindow_UI_Button_Recalc` | `"Recalc"` | `"Neu berechnen"` |
| `PatternWindow_UI_Button_SetRange` | `"Set range"` | `"Bereich festlegen"` |

### DataGridView Column Headers (dynamically created)
| Key | English Value | German Value (Suggested) |
|-----|--------------|-------------------------|
| `PatternWindow_UI_ColumnHeader_Weight` | `"Weight"` | `"Gewicht"` |
| `PatternWindow_UI_ColumnHeader_Diff` | `"Diff"` | `"Abweichung"` |

## Total Resource Keys
- **13 resource keys** in total

## Implementation Status
- ? `ApplyResources()` method added to PatternWindow.cs
- ? Constructor updated to call `ApplyResources()` after `InitializeComponent()`
- ? Dynamic column headers localized in `SetColumnizer()` method
- ? Dynamic range label localized in `OnSetRangeButtonClick()` method
- ? **PENDING**: Resource keys need to be added to Resources.resx
- ? **PENDING**: German translations need to be added to Resources.de.resx

## Next Steps
1. Add all 13 resource keys to `LogExpert.Resources/Resources.resx` with English values
2. Add German translations to `LogExpert.Resources/Resources.de.resx`
3. Build solution to verify no compilation errors
4. Test PatternWindow dialog in both English and German

## Notes
- Pattern window is described as "pre-beta" feature that doesn't work yet
- The label text acknowledges this in a humorous way
- The typo "Weigth" instead of "Weight" exists in the original code (label7 control name) but we use the correct spelling in the resource value

## Files Modified
- ? `LogExpert.UI/Controls/LogWindow/PatternWindow.cs` - Added localization methods

## Files That Need Updates
- ? `LogExpert.Resources/Resources.resx` - Add 13 English resource keys
- ? `LogExpert.Resources/Resources.de.resx` - Add 13 German translations
