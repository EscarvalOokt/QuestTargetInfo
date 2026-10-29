using System.Linq;
using QuestTargetInfo.Runtime;
using RimWorld;
using Verse;

namespace QuestTargetInfo
{
    internal static class WorldTargetInfoWindowUtility
    {
        public static void Synchronize(MainTabWindow_Quests owner, Quest quest)
        {
            if (!QuestTargetInfoMod.Settings.ShowQuestTravelWindow ||
                !WorldTargetInfoContext.TryCreateForQuest(quest, out _))
            {
                CloseOpenedWindows();
                return;
            }

            OpenOrReplace(owner, quest);
        }

        private static void OpenOrReplace(MainTabWindow_Quests owner, Quest quest)
        {
            Window_QuestTargetInfo existing =
                Find.WindowStack.Windows.OfType<Window_QuestTargetInfo>().FirstOrDefault();
            if (existing != null && existing.IsFor(owner, quest))
                return;

            CloseOpenedWindows();
            Find.WindowStack.Add(new Window_QuestTargetInfo(owner, quest));
        }

        public static void CloseOpenedWindows()
        {
            if (Find.WindowStack == null)
                return;

            foreach (Window_QuestTargetInfo window in
                     Find.WindowStack.Windows.OfType<Window_QuestTargetInfo>().ToList())
                Find.WindowStack.TryRemove(window);
        }
    }
}