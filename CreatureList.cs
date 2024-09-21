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

namespace Dawnsbury.Mods.Creatures.RoguelikeMode {

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class CreatureList {
        internal static Dictionary<ModEnums.CreatureId, Func<Encounter?, Creature>> Creatures = new Dictionary<ModEnums.CreatureId, Func<Encounter?, Creature>>();

        internal static void LoadCreatures() {
            Creatures.Add(ModEnums.CreatureId.UNSEEN_GUARDIAN,
                encounter => new Creature(IllustrationName.ElectricityMephit256, "Unseen Guardian", new List<Trait>() { Trait.Elemental, Trait.Air, Trait.Lawful }, 1, 5, 8, new Defenses(15, 4, 10, 6), 18, new Abilities(1, 3, 3, 1, 3, 1), new Skills(stealth: 6))
                .WithAIModification(ai => {
                    ai.IsDemonHorizonwalker = true;
                    ai.OverrideDecision = (self, options) => {
                        Creature creature = self.Self;
                        return creature.Actions.ActionsLeft == 1 && creature.Battle.AllCreatures.All<Creature>((Func<Creature, bool>)(enemy => !enemy.EnemyOf(creature) || creature.DetectionStatus.EnemiesYouAreHiddenFrom.Contains<Creature>(enemy))) && !creature.DetectionStatus.Undetected ? options.Where<Option>((Func<Option, bool>)(opt => opt.OptionKind == OptionKind.MoveHere && opt.Text == "Sneak" && opt is TileOption)).ToList<Option>().GetRandom<Option>() : (Option)null;
                    };
                })

                .WithProficiency(Trait.Weapon, Proficiency.Trained)
                .AddQEffect(new QEffect("Indistinct Form", "Target gains a +20 bonus to checks made to sneak or hide and can hide in plain sight, due to their apt ability to slip out of sight.") {
                    Id = QEffectId.HideInPlainSight,
                    Innate = true,
                    Illustration = IllustrationName.Blur,
                    BonusToSkillChecks = (skill, action, target) => {
                        if (action.Name == "Sneak" || action.Name == "Hide") {
                            return new Bonus(20, BonusType.Status, "Indistinct Form");
                        }
                        return null;
                    },
                })
                .AddQEffect(QEffect.DamageImmunity(DamageKind.Bleed))
                .AddQEffect(QEffect.DamageImmunity(DamageKind.Poison))
                .AddQEffect(QEffect.ImmunityToCondition(QEffectId.Paralyzed))
                .AddQEffect(QEffect.Flying())
                .AddQEffect(QEffect.SneakAttack("1d8"))
                .WithUnarmedStrike(CommonItems.CreateNaturalWeapon(IllustrationName.Fist, "Fists", "2d4", DamageKind.Bludgeoning, new Trait[] { Trait.Unarmed, Trait.Magical, Trait.Finesse, Trait.Melee, Trait.Agile }))
                .WithAdditionalUnarmedStrike(new Item(IllustrationName.FourWinds, "Slicing Wind", new Trait[] { Trait.Ranged, Trait.Electricity, Trait.Magical }).WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Slashing) {
                    VfxStyle = new VfxStyle(5, ProjectileKind.Cone, IllustrationName.FourWinds),
                    Sfx = SfxName.AirSpell
                }.WithRangeIncrement(4)))
            );

            ModManager.RegisterNewCreature("Unseen Guardian", Creatures[ModEnums.CreatureId.UNSEEN_GUARDIAN]);


            // Add new creature here

        }

        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
        internal static class CommonMonsterActions {
            public static CombatAction CreateHide(Creature self) {
                return new CombatAction(self, (Illustration)IllustrationName.Hide, "Hide", new Trait[2] { Trait.Basic, Trait.AttackDoesNotTargetAC },
                    "Make one Stealth check against the Perception DCs of each enemy creature that can see you but that you have cover or concealment from. On a success, you become Hidden to that creature.",
                    (Target)Target.Self(((cr, ai) => ai.HideSelf())).WithAdditionalRestriction((Func<Creature, string>)(innerSelf => {
                        if (HiddenRules.IsHiddenFromAllEnemies(innerSelf, innerSelf.Occupies))
                            return "You're already hidden from all enemies.";
                        return !innerSelf.Battle.AllCreatures.Any<Creature>((Func<Creature, bool>)(cr => cr.EnemyOf(innerSelf) && HiddenRules.HasCoverOrConcealment(innerSelf, cr))) ? "You don't have cover or concealment from any enemy." : (string)null;
                    })))
                .WithActionId(ActionId.Hide)
                .WithSoundEffect(SfxName.Hide)
                .WithEffectOnSelf((innerSelf => {
                    int roll = R.NextD20();
                    foreach (Creature creature in innerSelf.Battle.AllCreatures.Where<Creature>((Func<Creature, bool>)(cr => cr.EnemyOf(innerSelf)))) {
                        if (!innerSelf.DetectionStatus.HiddenTo.Contains(creature) && HiddenRules.HasCoverOrConcealment(innerSelf, creature)) {
                            CheckBreakdown breakdown = CombatActionExecution.BreakdownAttack(new CombatAction(innerSelf, (Illustration)IllustrationName.Hide, "Hide", new Trait[1]
                            {
                    Trait.Basic
                            }, "[this condition has no description]", (Target)Target.Self()).WithActiveRollSpecification(new ActiveRollSpecification(Checks.SkillCheck(Skill.Stealth), Checks.DefenseDC(Defense.Perception))), creature);
                            CheckBreakdownResult breakdownResult = new CheckBreakdownResult(breakdown, roll);
                            string str8 = breakdown.DescribeWithFinalRollTotal(breakdownResult);
                            DefaultInterpolatedStringHandler interpolatedStringHandler;
                            if (breakdownResult.CheckResult >= CheckResult.Success) {
                                innerSelf.DetectionStatus.HiddenTo.Add(creature);
                                Tile occupies = creature.Occupies;
                                Color lightBlue = Color.LightBlue;
                                string str9 = innerSelf?.ToString();
                                string str10 = creature?.ToString();
                                interpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 3);
                                interpolatedStringHandler.AppendLiteral(" (");
                                interpolatedStringHandler.AppendFormatted(breakdownResult.D20Roll.ToString() + breakdown.TotalCheckBonus.WithPlus());
                                interpolatedStringHandler.AppendLiteral("=");
                                interpolatedStringHandler.AppendFormatted<int>(breakdownResult.D20Roll + breakdown.TotalCheckBonus);
                                interpolatedStringHandler.AppendLiteral(" vs. ");
                                interpolatedStringHandler.AppendFormatted<int>(breakdown.TotalDC);
                                interpolatedStringHandler.AppendLiteral(").");
                                string stringAndClear = interpolatedStringHandler.ToStringAndClear();
                                string log = str9 + " successfully hid from " + str10 + stringAndClear;
                                string logDetails = str8;
                                occupies.Overhead("hidden from", lightBlue, log, "Hide", logDetails);
                            } else {
                                Tile occupies = creature.Occupies;
                                Color red = Color.Red;
                                string str11 = innerSelf?.ToString();
                                string str12 = creature?.ToString();
                                interpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 3);
                                interpolatedStringHandler.AppendLiteral(" (");
                                interpolatedStringHandler.AppendFormatted(breakdownResult.D20Roll.ToString() + breakdown.TotalCheckBonus.WithPlus());
                                interpolatedStringHandler.AppendLiteral("=");
                                interpolatedStringHandler.AppendFormatted<int>(breakdownResult.D20Roll + breakdown.TotalCheckBonus);
                                interpolatedStringHandler.AppendLiteral(" vs. ");
                                interpolatedStringHandler.AppendFormatted<int>(breakdown.TotalDC);
                                interpolatedStringHandler.AppendLiteral(").");
                                string stringAndClear = interpolatedStringHandler.ToStringAndClear();
                                string log = str11 + " failed to hide from " + str12 + stringAndClear;
                                string logDetails = str8;
                                occupies.Overhead("hide failed", red, log, "Hide", logDetails);
                            }
                        }
                    }
                }));
            }

            // Insert new actions here
        }
    }
}
