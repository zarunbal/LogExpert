using System.Runtime.Versioning;

namespace LogExpert.UI.Controls;

[method: SupportedOSPlatform("windows")]
internal class LogTextColumn () : DataGridViewColumn(new LogGridCell())
{

}
