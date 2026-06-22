using System.Collections.Generic;

namespace QuestTargetInfo
{
    internal sealed class WorldTargetInfoSectionModel
    {
        public WorldTargetInfoSectionModel(
            WorldTargetInfoSectionKind kind,
            string title,
            string statusText,
            WorldTargetInfoStatusKind statusKind,
            IReadOnlyList<WorldTargetInfoLineModel> lines)
        {
            Kind = kind;
            Title = title;
            StatusText = statusText;
            StatusKind = statusKind;
            Lines = lines;
        }

        public WorldTargetInfoSectionKind Kind { get; }

        public string Title { get; }

        public string StatusText { get; }

        public WorldTargetInfoStatusKind StatusKind { get; }

        public IReadOnlyList<WorldTargetInfoLineModel> Lines { get; }
    }
}