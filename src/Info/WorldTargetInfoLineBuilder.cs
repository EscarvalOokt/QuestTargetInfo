using System.Collections.Generic;

namespace QuestTargetInfo
{
    internal static class WorldTargetInfoLineBuilder
    {
        public static IEnumerable<string> BuildLines(WorldTargetInfoRequest request)
        {
            WorldTargetInfoModel model = WorldTargetInfoModelBuilder.Build(request);
            return WorldTargetInfoLineFormatter.Format(model);
        }
    }
}