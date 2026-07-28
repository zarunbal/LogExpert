using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace LogExpert.Core.Helpers;

/// <summary>
/// Resolves encoding names and code pages, including the legacy Windows code pages that .NET does not
/// ship with by default.
/// </summary>
/// <remarks>
/// .NET only knows Unicode, ASCII and latin1 out of the box; anything else — windows-1250,
/// windows-1252, … — requires <see cref="CodePagesEncodingProvider"/> to be registered first, and
/// <see cref="Encoding.GetEncoding(string)"/> throws until it is.
/// <para>
/// Registration used to happen as a side effect of constructing the Preferences dialog. Everything
/// that resolves an encoding name runs earlier than that or never opens the dialog at all — the
/// Preferences default encoding, the per-file encoding in a .lxp, the settings JSON — and every one of
/// those call sites swallows the exception and falls back to <see cref="Encoding.Default"/>. The result
/// was that a code page the user had picked was silently discarded on the next start.
/// </para>
/// <para>
/// Resolving through this class removes the ordering problem: every method here registers the provider
/// before it resolves, so no caller has to run after some other component. Callers should not use
/// <see cref="Encoding.GetEncoding(string)"/> or <see cref="Encoding.GetEncoding(int)"/> directly;
/// the <c>Encoding.Ascii</c>-style static properties are fine, since .NET always has those.
/// </para>
/// </remarks>
public static class EncodingRegistry
{
    /// <summary>
    /// Registers <see cref="CodePagesEncodingProvider"/> on first use.
    /// </summary>
    /// <remarks>
    /// <see cref="LazyThreadSafetyMode.ExecutionAndPublication"/> is the point: registration must have
    /// *completed* before any thread is allowed past, otherwise a second thread resolving concurrently
    /// would call <see cref="Encoding.GetEncoding(string)"/> too early, catch the
    /// <see cref="ArgumentException"/> and silently fall back — the exact bug this class exists to
    /// prevent. Files load under <c>Task.Run</c>, so concurrent first resolves do happen.
    /// </remarks>
    private static readonly Lazy<bool> _provider = new(
        () =>
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return true;
        },
        LazyThreadSafetyMode.ExecutionAndPublication);

    private static void EnsureRegistered ()
    {
        _ = _provider.Value;
    }

    /// <summary>
    /// Resolves a code page number.
    /// </summary>
    /// <param name="codePage">The code page number, e.g. 1252.</param>
    /// <returns>The <see cref="Encoding"/> for <paramref name="codePage"/>.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="codePage"/> is not a supported code page. Intended for hard-coded code pages,
    /// where an unsupported value is a programming error rather than bad user input; use
    /// <see cref="TryGetEncoding(string, out Encoding)"/> for values that come from a file.
    /// </exception>
    public static Encoding GetEncoding (int codePage)
    {
        EnsureRegistered();
        return Encoding.GetEncoding(codePage);
    }

    /// <summary>
    /// Resolves an encoding name, falling back when it cannot be resolved.
    /// </summary>
    /// <param name="name">An encoding name such as "windows-1252", possibly null or empty.</param>
    /// <param name="fallback">The encoding to return when <paramref name="name"/> is unusable.</param>
    /// <returns>The resolved encoding, or <paramref name="fallback"/>.</returns>
    public static Encoding GetEncoding (string? name, Encoding fallback)
    {
        return TryGetEncoding(name, out var encoding) ? encoding : fallback;
    }

    /// <summary>
    /// Attempts to resolve an encoding name.
    /// </summary>
    /// <param name="name">An encoding name such as "windows-1252", possibly null or empty.</param>
    /// <param name="encoding">The resolved encoding, or null when the name is unusable.</param>
    /// <returns>
    /// <c>true</c> when <paramref name="name"/> names a supported encoding; <c>false</c> when it is
    /// null, blank or unknown.
    /// </returns>
    public static bool TryGetEncoding (string? name, [NotNullWhen(true)] out Encoding? encoding)
    {
        encoding = null;

        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        EnsureRegistered();

        try
        {
            encoding = Encoding.GetEncoding(name);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            // Thrown for code pages the provider knows of but cannot instantiate.
            return false;
        }
    }
}
