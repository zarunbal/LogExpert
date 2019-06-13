using System.Collections.Generic;

namespace LogExpert
{
    internal static class Parser
    {
        public static List<Section> ParseSections(string formatString, out bool syntaxError)
        {
            var tokenizer = new Tokenizer(formatString);
            var sections = new List<Section>();
            syntaxError = false;
            while (true)
            {
                var section = ParseSection(tokenizer, sections.Count, out var sectionSyntaxError);

                if (sectionSyntaxError)
                    syntaxError = true;

                if (section == null)
                    break;

                sections.Add(section);
            }

            return sections;
        }

        private static Section ParseSection(Tokenizer reader, int index, out bool syntaxError)
        {
            bool hasDateParts = false;
            string token;
            List<string> tokens = new List<string>();

            syntaxError = false;
            while ((token = ReadToken(reader, out syntaxError)) != null)
            {
                if (token == ";")
                    break;

                if (Token.IsDatePart(token))
                {
                    hasDateParts |= true;
                    tokens.Add(token);
                }
                else
                {
                    tokens.Add(token);
                }
            }

            if (syntaxError || tokens.Count == 0)
            {
                return null;
            }

            List<string> generalTextDateDuration = null;

            if (hasDateParts)
            {
                ParseMilliseconds(tokens, out generalTextDateDuration);
            }
            else
            {
                // Unable to parse format string
                syntaxError = true;
                return null;
            }

            return new Section()
            {
                SectionIndex = index,
                GeneralTextDateDurationParts = generalTextDateDuration
            };
        }

        private static void ParseMilliseconds(List<string> tokens, out List<string> result)
        {
            // if tokens form .0 through .000.., combine to single subsecond token
            result = new List<string>();
            for (var i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                if (token == ".")
                {
                    var zeros = 0;
                    while (i + 1 < tokens.Count && tokens[i + 1] == "0")
                    {
                        i++;
                        zeros++;
                    }

                    if (zeros > 0)
                        result.Add("." + new string('0', zeros));
                    else
                        result.Add(".");
                }
                else
                {
                    result.Add(token);
                }
            }
        }

        private static string ReadToken(Tokenizer reader, out bool syntaxError)
        {
            var offset = reader.Position;
            if (
                ReadLiteral(reader) ||

                // Symbols
                reader.ReadOneOf("#?,!&%+-$€£0123456789{}():;/.@ ") ||

                // Date
                reader.ReadString("tt", true) ||
                reader.ReadOneOrMore('y') ||
                reader.ReadOneOrMore('Y') ||
                reader.ReadOneOrMore('m') ||
                reader.ReadOneOrMore('M') ||
                reader.ReadOneOrMore('d') ||
                reader.ReadOneOrMore('D') ||
                reader.ReadOneOrMore('h') ||
                reader.ReadOneOrMore('H') ||
                reader.ReadOneOrMore('s') ||
                reader.ReadOneOrMore('S'))
            {
                syntaxError = false;
                var length = reader.Position - offset;
                return reader.Substring(offset, length);
            }

            syntaxError = reader.Position < reader.Length;
            return null;
        }

        private static bool ReadLiteral(Tokenizer reader)
        {
            if (reader.Peek() == '\\' || reader.Peek() == '*' || reader.Peek() == '_')
            {
                reader.Advance(2);
                return true;
            }
            else if (reader.ReadEnclosed('"', '"'))
            {
                return true;
            }
            else if (reader.ReadEnclosed('\'', '\''))
            {
                return true;
            }

            return false;
        }
    }
}