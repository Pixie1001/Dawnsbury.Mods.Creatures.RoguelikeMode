using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Dawnsbury;
using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Core.CharacterBuilder.AbilityScores;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Coroutines.Requests;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Core.StatBlocks.Description;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Display.Text;
using Dawnsbury.IO;
using Dawnsbury.Modding;
using Dawnsbury.ThirdParty.SteamApi;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static System.Collections.Specialized.BitVector32;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.Intrinsics.Arm;
using System.Xml;
using Dawnsbury.Core.Mechanics.Damage;
using System.Runtime.CompilerServices;
using System.ComponentModel.Design;
using System.Text;
using static Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb.BarbarianFeatsDb.AnimalInstinctFeat;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics.Metrics;
using Microsoft.Xna.Framework.Audio;
using static System.Reflection.Metadata.BlobBuilder;
using Dawnsbury.Core.CharacterBuilder.FeatsDb;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Core.Animations.Movement;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.ModEnums;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.ModEnums;
using Dawnsbury.Campaign.Encounters.Tutorial;
using HarmonyLib;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level1;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level2;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level3;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.BossFights;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode
{

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class Loader {
        internal static Dictionary<ModEnums.CreatureId, Func<Encounter?, Creature>> Creatures = new Dictionary<ModEnums.CreatureId, Func<Encounter?, Creature>>();

        [DawnsburyDaysModMainMethod]
        public static void LoadMod() {
            var harmony = new Harmony("Dawnsbury.Mods.GameModes.RoguelikeMode");
            Harmony.DEBUG = true;
            harmony.PatchAll();

            CreatureList.LoadCreatures();
            ScriptHooks.LoadHooks();
            LoadEncounters();
        }

        private static void LoadEncounters() {
            //ModManager.RegisterEncounter<HallOfBeginnings>("HallOfBeginnings.tmx");
            //ModManager.RegisterEncounter<DrowAmbushLv1>("RoguelikeEncounter/DrowAmbushLv1.tmx");
            //ModManager.RegisterEncounter<DrowAmbushLv2>("RoguelikeEncounter/DrowAmbushLv2.tmx");
            //ModManager.RegisterEncounter<DrowAmbushLv3>("RoguelikeEncounter/DrowAmbushLv3.tmx");
            //ModManager.RegisterEncounter<InquisitrixTrapLv1>("RoguelikeEncounter/InquisitrixTrapLv1.tmx");
            //ModManager.RegisterEncounter<InquisitrixTrapLv2>("RoguelikeEncounter/InquisitrixTrapLv2.tmx");
            //ModManager.RegisterEncounter<InquisitrixTrapLv3>("RoguelikeEncounter/InquisitrixTrapLv3.tmx");
            //ModManager.RegisterEncounter<RatSwarmLv1>("RoguelikeEncounter/RatSwarmLv1.tmx");
            //ModManager.RegisterEncounter<RatSwarmLv2>("RoguelikeEncounter/RatSwarmLv2.tmx");
            //ModManager.RegisterEncounter<RatSwarmLv3>("RoguelikeEncounter/RatSwarmLv3.tmx");

            //// Elite Fights
            //ModManager.RegisterEncounter<HallOfSmokeLv1>("RoguelikeEncounters/Elite_HallOfSmokeLv1.tmx");
            //ModManager.RegisterEncounter<HallOfSmokeLv2>("RoguelikeEncounters/Elite_HallOfSmokeLv2.tmx");
            //ModManager.RegisterEncounter<HallOfSmokeLv3>("RoguelikeEncounters/Elite_HallOfSmokeLv3.tmx");

            //// Boss fights
            //ModManager.RegisterEncounter<Boss_DriderFight>("RoguelikeEncounters/Boss_DriderFight.tmx");

            ModManager.RegisterEncounter<HallOfBeginnings>("HallOfBeginnings.tmx");
            ModManager.RegisterEncounter<DrowAmbushLv1>("DrowAmbushLv1.tmx");
            ModManager.RegisterEncounter<DrowAmbushLv2>("DrowAmbushLv2.tmx");
            ModManager.RegisterEncounter<DrowAmbushLv3>("DrowAmbushLv3.tmx");
            ModManager.RegisterEncounter<InquisitrixTrapLv1>("InquisitrixTrapLv1.tmx");
            ModManager.RegisterEncounter<InquisitrixTrapLv2>("InquisitrixTrapLv2.tmx");
            ModManager.RegisterEncounter<InquisitrixTrapLv3>("InquisitrixTrapLv3.tmx");
            ModManager.RegisterEncounter<RatSwarmLv1>("RatSwarmLv1.tmx");
            ModManager.RegisterEncounter<RatSwarmLv2>("RatSwarmLv2.tmx");
            ModManager.RegisterEncounter<RatSwarmLv3>("RatSwarmLv3.tmx");

            // Elite Fights
            ModManager.RegisterEncounter<HallOfSmokeLv1>("Elite_HallOfSmokeLv1.tmx");
            ModManager.RegisterEncounter<HallOfSmokeLv2>("Elite_HallOfSmokeLv2.tmx");
            ModManager.RegisterEncounter<HallOfSmokeLv3>("Elite_HallOfSmokeLv3.tmx");

            // Boss fights
            ModManager.RegisterEncounter<Boss_DriderFight>("Boss_DriderFight.tmx");
        }
    }
}
