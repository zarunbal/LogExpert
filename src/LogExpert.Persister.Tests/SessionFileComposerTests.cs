using System.Collections;
using System.Reflection;
using System.Text;

using LogExpert.Core.Classes.JsonConverters;
using LogExpert.Core.Classes.Persister;
using LogExpert.Core.Config;

using Newtonsoft.Json;

namespace LogExpert.Persister.Tests;

[TestFixture]
public class SessionFileComposerTests
{
    private string _testDirectory;
    private string _sessionDirectory;
    private string _logFileName;

    [SetUp]
    public void Setup ()
    {
        _testDirectory = Path.Join(Path.GetTempPath(), "LogExpertTests", Guid.NewGuid().ToString());
        _ = Directory.CreateDirectory(_testDirectory);
        _sessionDirectory = Path.Join(_testDirectory, "sessionFiles");

        _logFileName = Path.Join(_testDirectory, "test.log");
        File.WriteAllText(_logFileName, "Test log content");
    }

    [TearDown]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Unit Test")]
    public void TearDown ()
    {
        if (Directory.Exists(_testDirectory))
        {
            try
            {
                Directory.Delete(_testDirectory, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    /// <summary>
    /// Equality is serialize-&amp;-compare: both sides are serialized with deterministic settings
    /// (reusing the Columnizer/Encoding converters) and compared as strings. Deep by
    /// construction — a new field automatically joins the comparison.
    /// </summary>
    private static readonly JsonSerializerSettings _comparisonSettings = new()
    {
        Converters =
        {
            new ColumnizerJsonConverter(),
            new EncodingJsonConverter()
        },
        Formatting = Formatting.Indented,
        ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
        PreserveReferencesHandling = PreserveReferencesHandling.Objects,
    };

    private static string SerializeForComparison (object value)
    {
        return JsonConvert.SerializeObject(value, _comparisonSettings);
    }

    /// <summary>
    /// Serialize-&amp;-compare with the excluded top-level fields removed from the JSON — used by
    /// the reverse trip, whose exclusions are named and justified, never implicit.
    /// </summary>
    private static string SerializeExcluding (object value, params string[][] exclusionLists)
    {
        var json = Newtonsoft.Json.Linq.JObject.Parse(SerializeForComparison(value));

        foreach (var exclusions in exclusionLists)
        {
            foreach (var propertyName in exclusions)
            {
                _ = json.Remove(propertyName);
            }
        }

        return json.ToString();
    }

    /// <summary>
    /// The fully-populated snapshot fixture: every property must hold a non-default value (the
    /// fixture-completeness test enforces this), so every mapped field takes part in every trip.
    /// </summary>
    private static SessionSnapshot CreateFullSnapshot ()
    {
        return new SessionSnapshot
        {
            FollowTail = true,
            Encoding = Encoding.UTF8,
            LineCount = 1234,
        };
    }

    /// <summary>
    /// The populated <see cref="PersistenceData"/> fixture. Mapped fields hold non-default
    /// values; the named-exclusion fields are populated too, so the reverse trip proves the
    /// exclusion list is load-bearing. Fields not yet carried by the snapshot stay at their
    /// defaults until Ticket 2 maps them.
    /// </summary>
    private static PersistenceData CreateFullPersistenceData ()
    {
        return new PersistenceData
        {
            FollowTail = true,
            Encoding = Encoding.UTF8,
            LineCount = 1234,
            FileName = @"C:\logs\app.log",
            SessionFileName = "legacy-never-set.lxp",
        };
    }

    /// <summary>
    /// The named exclusion list (spec, Testing Decisions): fields that legitimately do not
    /// round-trip. Each entry is justified in the spec's mapping table.
    /// </summary>
    private static readonly string[] _namedExclusions =
    [
        // Dead fields — written only by the legacy XML reader, applied nowhere:
        nameof(PersistenceData.BookmarkListPosition),
        nameof(PersistenceData.BookmarkListVisible),
        nameof(PersistenceData.ShowBookmarkCommentColumn),
        nameof(PersistenceData.ColumnizerName),
        nameof(PersistenceData.SettingsSaveLoadLocation),
        // Always null in practice — never assigned anywhere in the UI:
        nameof(PersistenceData.SessionFileName),
        // Transformed by Persister on SameDir saves after compose — documented, not asserted:
        nameof(PersistenceData.FileName),
    ];

    /// <summary>
    /// Fields the snapshot does not carry YET — this list shrinks to empty in Ticket 2, which
    /// maps them. It is not part of the spec's named exclusion list.
    /// </summary>
    private static readonly string[] _notYetMappedTicket2 =
    [
        nameof(PersistenceData.BookmarkList),
        nameof(PersistenceData.Columnizer),
        nameof(PersistenceData.CurrentLine),
        nameof(PersistenceData.FilterAdvanced),
        nameof(PersistenceData.FilterParamsList),
        nameof(PersistenceData.FilterPosition),
        nameof(PersistenceData.FilterSaveListVisible),
        nameof(PersistenceData.FilterTabDataList),
        nameof(PersistenceData.CellSelectMode),
        nameof(PersistenceData.FirstDisplayedLine),
        nameof(PersistenceData.HighlightGroupName),
        nameof(PersistenceData.FilterVisible),
        nameof(PersistenceData.MultiFile),
        nameof(PersistenceData.MultiFileMaxDays),
        nameof(PersistenceData.MultiFileNames),
        nameof(PersistenceData.MultiFilePattern),
        nameof(PersistenceData.RowHeightList),
        nameof(PersistenceData.TabName),
    ];

    /// <summary>
    /// Reflects over an instance's public properties and returns the names of those still at
    /// their "unpopulated" value: <c>default(T)</c> for value types, null or empty for strings
    /// and collections, null for other references. The fixture-completeness tests use this so
    /// that adding a field without extending the fixtures fails immediately.
    /// </summary>
    private static List<string> GetUnpopulatedProperties (object instance, params string[][] exclusionLists)
    {
        var excluded = exclusionLists.SelectMany(x => x).ToHashSet();
        List<string> unpopulated = [];

        foreach (var property in instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (excluded.Contains(property.Name))
            {
                continue;
            }

            var value = property.GetValue(instance);
            var isUnpopulated = value switch
            {
                null => true,
                string text => text.Length == 0,
                IEnumerable items => !items.Cast<object>().Any(),
                _ => property.PropertyType.IsValueType && value.Equals(Activator.CreateInstance(property.PropertyType)),
            };

            if (isUnpopulated)
            {
                unpopulated.Add(property.Name);
            }
        }

        return unpopulated;
    }

    #region Fixture completeness (the regression guard)

    // Pins that the guard itself can fire — an all-defaults instance must be reported.
    [Test]
    public void FixtureCompleteness_UnpopulatedSnapshot_IsDetected ()
    {
        Assert.That(GetUnpopulatedProperties(new SessionSnapshot()), Is.Not.Empty);
    }

    [Test]
    public void FixtureCompleteness_SnapshotFixture_LeavesNoPropertyUnpopulated ()
    {
        Assert.That(GetUnpopulatedProperties(CreateFullSnapshot()), Is.Empty,
            "Extend CreateFullSnapshot — every SessionSnapshot property must hold a populated value so the round trips exercise it");
    }

    [Test]
    public void FixtureCompleteness_PersistenceDataFixture_LeavesNoMappedPropertyUnpopulated ()
    {
        Assert.That(GetUnpopulatedProperties(CreateFullPersistenceData(), _namedExclusions, _notYetMappedTicket2), Is.Empty,
            "Extend CreateFullPersistenceData (or, if the field legitimately never round-trips, justify it on the spec's named exclusion list)");
    }

    #endregion

    #region Round trips

    [Test]
    public void ForwardTrip_ComposeThenDecompose_ReturnsEqualSnapshot ()
    {
        var snapshot = CreateFullSnapshot();

        var persistenceData = SessionFileComposer.Compose(snapshot);
        var roundTripped = SessionFileComposer.Decompose(persistenceData);

        Assert.That(SerializeForComparison(roundTripped), Is.EqualTo(SerializeForComparison(snapshot)));
    }

    // The reverse trip catches new PersistenceData fields the snapshot doesn't carry — the
    // classic "added a setting, forgot to persist it" case starts on this side.
    [Test]
    public void ReverseTrip_DecomposeThenCompose_PreservesEverythingButTheNamedExclusions ()
    {
        var original = CreateFullPersistenceData();

        var snapshot = SessionFileComposer.Decompose(original);
        var recomposed = SessionFileComposer.Compose(snapshot);

        Assert.That(
            SerializeExcluding(recomposed, _namedExclusions, _notYetMappedTicket2),
            Is.EqualTo(SerializeExcluding(original, _namedExclusions, _notYetMappedTicket2)));
    }

    // The composer is pure: composing a session can never change live window state (the
    // IsFilterTail side effect this seam removes lives at the control's call site now).
    [Test]
    public void Compose_DoesNotMutateTheSnapshot ()
    {
        var snapshot = CreateFullSnapshot();
        var before = SerializeForComparison(snapshot);

        _ = SessionFileComposer.Compose(snapshot);

        Assert.That(SerializeForComparison(snapshot), Is.EqualTo(before));
    }

    [Test]
    public void Decompose_DoesNotMutateThePersistenceData ()
    {
        var persistenceData = CreateFullPersistenceData();
        var before = SerializeForComparison(persistenceData);

        _ = SessionFileComposer.Decompose(persistenceData);

        Assert.That(SerializeForComparison(persistenceData), Is.EqualTo(before));
    }

    // The flagship trip: one test guarding the mapping AND the serialization (the
    // missing-JSON-converter class of bug). Uses a non-SameDir save location because Persister
    // rewrites FileName on SameDir saves (transformed by design, see the spec).
    [Test]
    public void DiskTrip_ComposePersistLoadDecompose_ReturnsEqualSnapshot ()
    {
        var snapshot = CreateFullSnapshot();
        var preferences = new Preferences
        {
            SaveLocation = SessionSaveLocation.ApplicationStartupDir,
        };

        var persistenceData = SessionFileComposer.Compose(snapshot);
        var savedFileName = Core.Classes.Persister.Persister.SavePersistenceData(_logFileName, persistenceData, preferences, _sessionDirectory);
        var loadedData = Core.Classes.Persister.Persister.LoadPersistenceData(_logFileName, preferences, _sessionDirectory);
        var roundTripped = SessionFileComposer.Decompose(loadedData);

        Assert.That(File.Exists(savedFileName), Is.True, "The .lxp must actually have been written");
        Assert.That(SerializeForComparison(roundTripped), Is.EqualTo(SerializeForComparison(snapshot)));
    }

    #endregion

    #region IsStale (Rollover staleness rule)

    // Saved LineCount greater than the file's current line count means the snapshot
    // belongs to a longer, since-rolled file.
    [TestCase(100, 50, ExpectedResult = true, TestName = "IsStale_SavedCountGreaterThanCurrent_IsStale")]
    [TestCase(100, 100, ExpectedResult = false, TestName = "IsStale_SavedCountEqualToCurrent_IsNotStale")]
    [TestCase(50, 100, ExpectedResult = false, TestName = "IsStale_SavedCountLessThanCurrent_IsNotStale")]
    [TestCase(0, 0, ExpectedResult = false, TestName = "IsStale_BothCountsZero_IsNotStale")]
    [TestCase(1, 0, ExpectedResult = true, TestName = "IsStale_SavedCountAgainstEmptyFile_IsStale")]
    [TestCase(0, 100, ExpectedResult = false, TestName = "IsStale_NoSavedCount_IsNotStale")]
    public bool IsStale_ComparesSavedLineCountToCurrent (int savedLineCount, int currentLineCount)
    {
        var snapshot = new SessionSnapshot { LineCount = savedLineCount };

        return SessionFileComposer.IsStale(snapshot, currentLineCount);
    }

    #endregion
}
