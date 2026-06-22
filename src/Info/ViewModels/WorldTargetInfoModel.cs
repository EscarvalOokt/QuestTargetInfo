using System.Collections.Generic;

namespace QuestTargetInfo
{
    internal sealed class WorldTargetInfoModel
    {
        public WorldTargetInfoModel(
            string title,
            IReadOnlyList<WorldTargetInfoSectionModel> sections)
        {
            Title = title;
            Sections = sections;
        }

        public string Title { get; }

        public IReadOnlyList<WorldTargetInfoSectionModel> Sections { get; }
    }
}