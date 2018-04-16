using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Windows.Forms;

namespace LogExpert
{
	public class Column : IColumn
	{
		#region Fields

		private static readonly int _maxLength = 4678 - 3;
		private static readonly string replacement = "...";

		private static IEnumerable<Func<string, string>> replacements = new List<Func<string, string>>(
			new Func<string, string>[]
			{
				//replace tab with 3 spaces, from old coding. Needed???
				input => input.Replace("\t", "  "),

				//shorten string if it exceeds maxLength
				input =>
				{
					if (input.Length > _maxLength)
					{
						return input.Substring(0, _maxLength) + replacement;
					}

					return input;
				}
			});
		private string _fullValue;

		#endregion

		#region Properties

		public static IColumn EmptyColumn { get; } = new Column {FullValue = string.Empty};

		public IColumnizedLogLine Parent { get; set; }

		public string FullValue
		{
			get { return _fullValue; }
			set
			{
				_fullValue = value;

				string temp = FullValue;

				foreach (var replacement in replacements)
				{
					temp = replacement(temp);
				}
			}
		}

		public string DisplayValue { get; private set; }

		string ITextValue.Text => FullValue;

		#endregion

		#region Public methods

		public static Column[] CreateColumns(int count, IColumnizedLogLine parent)
		{
			return CreateColumns(count, parent, string.Empty);
		}

		public static Column[] CreateColumns(int count, IColumnizedLogLine parent, string defaultValue)
		{
			Column[] output = new Column[count];

			for (int i = 0; i < count; i++)
			{
				output[i] = new Column {FullValue = defaultValue, Parent = parent};
			}

			return output;
		}

		public override string ToString()
		{
			return DisplayValue ?? "";
		}

		#endregion
	}
}