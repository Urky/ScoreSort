using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection;
using TaleWorlds.MountAndBlade.ViewModelCollection.Scoreboard;

namespace ScoreSort
{
    public class ScoreSortSubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            new Harmony("ScoreSort").PatchAll();
        }
    }

    [HarmonyPatch(typeof(ScoreboardVM))]
    [HarmonyPatch("UpdateQuitText")]
    public static class UpdateQuitTextPatch
    {
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

    public static class ScoreBoardHelper
    {
        public static MBBindingList<SPScoreboardUnitVM> GetOrderedScoreList(SPScoreboardPartyVM party)
        {
            List<SPScoreboardUnitVM> list = party.Members.OrderByDescending(vm => vm.IsHero)
                .ThenByDescending(vm => vm.Score.Kill)
                .ToList();

            MBBindingList<SPScoreboardUnitVM> mbList = new MBBindingList<SPScoreboardUnitVM>();
            foreach (var item in GetScoreboardUnits(party))
            {
                mbList.Add(item);
            }

            mbList.Add(new SPScoreboardUnitVM(new BasicCharacterObject {Name = TextObject.Empty}));

            foreach (SPScoreboardUnitVM member in list)
            {
                mbList.Add(member);
            }

            return mbList;
        }

        private static IEnumerable<SPScoreboardUnitVM> GetScoreboardUnits(SPScoreboardPartyVM party)
        {
            foreach (var formationClass in new[]
            {
                FormationClass.Infantry, FormationClass.Ranged, FormationClass.Cavalry, FormationClass.HorseArcher,
                FormationClass.Skirmisher, FormationClass.HeavyInfantry, FormationClass.LightCavalry,
                FormationClass.HeavyCavalry, FormationClass.General
            })
            {
                var entry = CreateScoreEntry(party, formationClass);

                if (entry != null)
                    yield return entry;
            }
        }

        private static SPScoreboardUnitVM CreateScoreEntry(SPScoreboardPartyVM party, FormationClass formationClass)
        {
            BasicCharacterObject character = new BasicCharacterObject
            {
                Name = new TextObject(formationClass.ToString())
            };

            int kills = party.Members.Where(vm => vm.Character.CurrentFormationClass == formationClass)
                .Sum(vm => vm.Score.Kill);
            int deaths = party.Members.Where(vm => vm.Character.CurrentFormationClass == formationClass)
                .Sum(vm => vm.Score.Dead);
            int wounded = party.Members.Where(vm => vm.Character.CurrentFormationClass == formationClass)
                .Sum(vm => vm.Score.Wounded);
            int routed = party.Members.Where(vm => vm.Character.CurrentFormationClass == formationClass)
                .Sum(vm => vm.Score.Routed);
            int readyToUpgrade = party.Members.Where(vm => vm.Character.CurrentFormationClass == formationClass)
                .Sum(vm => vm.Score.ReadyToUpgrade);
            int remaining = party.Members.Where(vm => vm.Character.CurrentFormationClass == formationClass)
                .Sum(vm => vm.Score.Remaining);

            if (remaining + routed + wounded + deaths == 0)
            {
                return null;
            }

            SPScoreboardUnitVM score = new SPScoreboardUnitVM(character)
            {
                Score = new SPScoreboardStatsVM(character.Name)
                {
                    Kill = kills,
                    Dead = deaths,
                    Wounded = wounded,
                    Routed = routed,
                    ReadyToUpgrade = readyToUpgrade,
                    Remaining = remaining + routed + wounded + deaths,
                }
            };

            return score;
        }
    }
}