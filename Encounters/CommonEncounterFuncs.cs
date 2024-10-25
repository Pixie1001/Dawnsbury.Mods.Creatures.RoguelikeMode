using Dawnsbury.Auxiliary;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Core;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Treasure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters {

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class CommonEncounterFuncs {
        public static void ApplyEliteAdjustments(TBattle battle) {
            foreach (Creature enemy in battle.AllCreatures.Where(cr => cr.OwningFaction.IsEnemy)) {
                enemy.ApplyEliteAdjustments(false);
            }
        }

        public static void ApplyWeakAdjustments(TBattle battle) {
            foreach (Creature enemy in battle.AllCreatures.Where(cr => cr.OwningFaction.IsEnemy)) {
                enemy.ApplyWeakAdjustments(false);
            }
        }

        public static int GetGoldReward(int level, ModEnums.EncounterType type) {
            float gold = 50 * (level * 0.7f);
            if (type == ModEnums.EncounterType.ELITE) {
                gold *= 1.5f;
            }
            if (type == ModEnums.EncounterType.BOSS) {
                gold *= 2f;
            }

            return (int) gold;
        }

        public static async Task PresentEliteRewardChoice(TBattle battle) {
            if (battle.CampaignState == null || battle.CampaignState.AdventurePath == null || battle.CampaignState.AdventurePath.Id != "RoguelikeMode") {
                return;
            }

            // TODO: Hook up loot tables
            Item option1 = Items.CreateNew(ItemName.NecklaceOfFireballsI);
            Item option2 = Items.CreateNew(ItemName.BarbariansGloves);

            string[] options = { option1.Name, option2.Name };
            string text = "Select your reward:\n\n";
            text += "{b}" + option1.Name + ".{/b} " + option1.Description;
            text += "\n\n{b}" + option2.Name + ".{/b} " + option2.Description;

            //text = "Regular length question.";

            Creature looter = battle.AllCreatures.FirstOrDefault(cr => cr.OwningFaction.IsHumanControlled);

            //ChoiceButtonOption choice = await looter.AskForChoiceAmongButtons(IllustrationName.GoldPouch, text, options);

            //if (option1.Name == choice.Caption) {
            //    battle.Encounter.Rewards.Add(option1);
            //} else if (option1.Name == choice.Caption) {
            //    battle.Encounter.Rewards.Add(option2);
            //}

            if (await looter.AskForConfirmation(IllustrationName.GoldPouch, text, "Test 1", "Test 2")) {
                battle.Encounter.Rewards.Add(option1);
            } else {
                battle.Encounter.Rewards.Add(option2);
            }
        }

    }
}