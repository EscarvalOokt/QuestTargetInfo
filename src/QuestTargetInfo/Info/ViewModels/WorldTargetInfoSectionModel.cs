using System.Collections.Generic;

namespace QuestTargetInfo.Info.ViewModels
{
    internal sealed class WorldTargetInfoSectionModel(
        WorldTargetInfoSectionKind kind,
        string title,
        string statusText,
        WorldTargetInfoStatusKind statusKind,
        IReadOnlyList<WorldTargetInfoLineModel> lines)
    {
        public WorldTargetInfoSectionKind Kind { get; } = kind;

        public string Title { get; } = title;

        public string StatusText { get; } = statusText;

        public WorldTargetInfoStatusKind StatusKind { get; } = statusKind;

        public IReadOnlyList<WorldTargetInfoLineModel> Lines { get; } = lines;
    }
}