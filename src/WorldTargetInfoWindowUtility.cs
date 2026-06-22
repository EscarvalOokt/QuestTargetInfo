using System.Linq;
using Verse;

namespace QuestTargetInfo
{
    internal static class WorldTargetInfoWindowUtility
    {
        public static void OpenOrReplace(WorldTargetInfoRequest request)
        {
            CloseOpenedWindows();

            if(request == null || request.Source != WorldTargetInfoSource.Quest)
                return;

            Find.WindowStack.Add(new Window_QuestTargetInfo(request));
        }

        public static void CloseOpenedWindows()
        {
            var questInfoWindows = Find.WindowStack.Windows
                .OfType<Window_QuestTargetInfo>()
                .ToList();

            foreach(Window_QuestTargetInfo window in questInfoWindows)
                Find.WindowStack.TryRemove(window);
        }
    }
}