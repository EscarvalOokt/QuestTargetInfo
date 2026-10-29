using System.Collections.Generic;

namespace QuestTargetInfo.Info.ViewModels
{
    internal sealed class WorldTargetInfoModel(string title, IReadOnlyList<WorldTargetInfoSectionModel> sections)
    {
        public string Title { get; } = title;

        public IReadOnlyList<WorldTargetInfoSectionModel> Sections { get; } = sections;
    }
}