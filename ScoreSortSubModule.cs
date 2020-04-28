using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection;

namespace ScoreSort
{
    public class ScoreSortSubModule : MBSubModuleBase 
    {
        protected override void OnSubModuleLoad() 
        {
            
        }
    }

    [HarmonyPatch(typeof(ScoreboardVM))]
    [HarmonyPatch("UpdateQuitText")]
    public static class UpdateQuitTextPatch
    {
        public static bool isOver { get; set; }

        [HarmonyPostfix]
        public static void PostFix(ScoreboardVM __instance)
        {
            if (__instance.IsOver)
            {
                foreach (var party in __instance.Attackers.Parties)
                {
                    party.Members = ScoreBoardHelper.GetOrderedScoreList(party);
                }

                foreach (var party in __instance.Defenders.Parties)
                {
                    party.Members = ScoreBoardHelper.GetOrderedScoreList(party);
                }
            }
        }
    }
}
