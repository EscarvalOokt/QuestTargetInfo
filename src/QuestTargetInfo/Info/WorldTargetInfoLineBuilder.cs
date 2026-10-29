using System.Collections.Generic;
using QuestTargetInfo.Info.ViewModels;

namespace QuestTargetInfo.Info
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