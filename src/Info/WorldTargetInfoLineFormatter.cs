using System.Collections.Generic;

namespace QuestTargetInfo
{
    internal static class WorldTargetInfoLineFormatter
    {
        public static IEnumerable<string> Format(WorldTargetInfoModel model)
        {
            if(model == null)
                yield break;

            foreach(WorldTargetInfoSectionModel section in model.Sections)
            {
                if(section == null)
                    continue;

                if(section.Kind != WorldTargetInfoSectionKind.Route)
                    yield return "";

                string header = FormatSectionHeader(section);
                if(!string.IsNullOrEmpty(header))
                    yield return header;

                foreach(WorldTargetInfoLineModel line in section.Lines)
                {
                    string text = FormatLine(line);

                    if(!string.IsNullOrEmpty(text))
                        yield return text;
                }
            }
        }

        internal static string FormatLine(WorldTargetInfoLineModel line)
        {
            if(line == null)
                return null;

            if(line.HasText)
                return line.Text;

            if(!line.HasLabelValue)
                return null;

            if(string.IsNullOrEmpty(line.Label))
                return line.Value;

            if(string.IsNullOrEmpty(line.Value))
                return line.Label;

            return line.Label + ": " + line.Value;
        }

        private static string FormatSectionHeader(
            WorldTargetInfoSectionModel section)
        {
            if(section.Kind == WorldTargetInfoSectionKind.Route)
                return section.Title;

            if(string.IsNullOrEmpty(section.Title))
                return section.StatusText;

            if(string.IsNullOrEmpty(section.StatusText))
                return section.Title;

            return section.Title + ": " + section.StatusText;
        }
    }
}