using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LogExpert.Classes.Columnizer
{
    public class ColumnizerPicker
    {
        public static ILogLineColumnizer FindColumnizerByName(string name, IList<ILogLineColumnizer> list)
        {
            foreach (ILogLineColumnizer columnizer in list)
            {
                if (columnizer.GetName().Equals(name))
                {
                    return columnizer;
                }
            }
            return null;
        }

        public static ILogLineColumnizer DecideColumnizerByName(string name, IList<ILogLineColumnizer> list)
        {
            foreach (ILogLineColumnizer columnizer in list)
            {
                if (columnizer.GetName().Equals(name))
                {
                    return columnizer;
                }
            }

            return FindColumnizer(null, null);
        }

        public static ILogLineColumnizer CloneColumnizer(ILogLineColumnizer columnizer)
        {
            if (columnizer == null)
            {
                return null;
            }
            ConstructorInfo cti = columnizer.GetType().GetConstructor(Type.EmptyTypes);
            if (cti != null)
            {
                object o = cti.Invoke(new object[] { });
                if (o is IColumnizerConfigurator)
                {
                    ((IColumnizerConfigurator)o).LoadConfig(ConfigManager.ConfigDir);
                }
                return (ILogLineColumnizer)o;
            }
            return null;
        }

        /// <summary>
        /// This method implemented the "auto columnizer" feature.
        /// This method should be called after each columnizer is changed to update the columizer.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="logFileReader"></param>
        /// <param name="logLineColumnizer"></param>
        /// <returns></returns>
        public static ILogLineColumnizer FindReplacementForAutoColumnizer(string fileName,
            IAutoLogLineColumnizerCallback logFileReader,
            ILogLineColumnizer logLineColumnizer)
        {
            if (logLineColumnizer == null || logLineColumnizer.GetName() == "Auto Columnizer")
            {
                return FindColumnizer(fileName, logFileReader);
            }
            return logLineColumnizer;
        }

        public static ILogLineColumnizer FindBetterColumnizer(string fileName,
            IAutoLogLineColumnizerCallback logFileReader,
            ILogLineColumnizer logLineColumnizer)
        {
            var newColumnizer = FindColumnizer(fileName, logFileReader);
            if (newColumnizer.GetType().Equals(logLineColumnizer.GetType()))
            {
                return null;
            }
            return newColumnizer;
        }

        /// <summary>
        /// This method will search all registered columnizer and return one according to the priority that returned
        /// by the each columnizer.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="logFileReader"></param>
        /// <returns></returns>
        public static ILogLineColumnizer FindColumnizer(string fileName, IAutoLogLineColumnizerCallback logFileReader)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return new DefaultLogfileColumnizer();
            }

            List<ILogLine> loglines = new List<ILogLine>();

            if (logFileReader != null)
            {
                loglines = new List<ILogLine>()
                {
                    // Sampling a few lines to select the correct columnizer
                    logFileReader.GetLogLine(0),
                    logFileReader.GetLogLine(1),
                    logFileReader.GetLogLine(2),
                    logFileReader.GetLogLine(3),
                    logFileReader.GetLogLine(4),
                    logFileReader.GetLogLine(5),
                    logFileReader.GetLogLine(25),
                    logFileReader.GetLogLine(100),
                    logFileReader.GetLogLine(200),
                    logFileReader.GetLogLine(400)
                };
            }

            var registeredColumnizer = PluginRegistry.GetInstance().RegisteredColumnizers;

            List<Tuple<Priority, ILogLineColumnizer>> priorityListOfColumnizers = new List<Tuple<Priority, ILogLineColumnizer>>();

            foreach (ILogLineColumnizer logLineColumnizer in registeredColumnizer)
            {
                Priority priority = default(Priority);
                if (logLineColumnizer is IColumnizerPriority columnizerPriority)
                {
                    priority = columnizerPriority.GetPriority(fileName, loglines);
                }

                priorityListOfColumnizers.Add(new Tuple<Priority, ILogLineColumnizer>(priority, logLineColumnizer));
            }

            ILogLineColumnizer lineColumnizer = priorityListOfColumnizers.OrderByDescending(a => a.Item1).Select(a => a.Item2).First();

            return lineColumnizer;
        }
    }
}
