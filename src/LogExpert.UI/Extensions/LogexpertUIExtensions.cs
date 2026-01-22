using System.Runtime.Versioning;

namespace LogExpert.UI.Extensions;

[SupportedOSPlatform("windows")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case", Justification = "Intentionally")]
internal static class LogexpertUIExtensions
{
    /// <seealso href="https://stackoverflow.com/a/4842576/1987788"/>
    extension(ComboBox comboBox)
    {
        public int GetMaxTextWidth ()
        {
            var maxTextWidth = comboBox.Width;

            foreach (var item in comboBox.Items)
            {
                var textWidthInPixels = TextRenderer.MeasureText(item.ToString(), comboBox.Font).Width;

                if (textWidthInPixels > maxTextWidth)
                {
                    maxTextWidth = textWidthInPixels;
                }
            }

            return maxTextWidth;
        }
    }

    extension(Control parent)
    {
        /// <summary>
        /// Enumerates all controls within the specified parent control, including nested child controls.
        /// </summary>
        /// <param name="parent">The parent control whose child controls are to be enumerated. Cannot be <see langword="null"/>.</param>
        /// <returns>An <see cref="IEnumerable{Control}"/> of <see cref="Control"/> objects representing all controls within the parent,
        /// including nested children.</returns>
        [SupportedOSPlatform("windows")]
        public IEnumerable<Control> ControlsRecursive ()
        {
            ArgumentNullException.ThrowIfNull(parent, nameof(parent));

            foreach (Control control in parent.Controls)
            {
                yield return control;

                // recurse into children
                foreach (var child in ControlsRecursive(control))
                {
                    yield return child;
                }
            }
        }
    }
}
