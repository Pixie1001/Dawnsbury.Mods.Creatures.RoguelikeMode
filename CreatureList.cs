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
using Dawnsbury.Campaign.Encounters.A_Crisis_in_Dawnsbury;
using System.Buffers;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode {

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class CreatureList {
        internal static Dictionary<ModEnums.CreatureId, Func<Encounter?, Creature>> Creatures = new Dictionary<ModEnums.CreatureId, Func<Encounter?, Creature>>();

        internal static void LoadCreatures() {
            // CREATURE - Unseen Guardian
            Creatures.Add(ModEnums.CreatureId.UNSEEN_GUARDIAN,
                encounter => new Creature(IllustrationName.ElectricityMephit256, "Unseen Guardian", new List<Trait>() { Trait.Elemental, Trait.Air, Trait.Lawful }, 1, 5, 8, new Defenses(16, 4, 10, 6), 22, new Abilities(1, 3, 3, 1, 3, 1), new Skills(stealth: 5))
                .WithAIModification(ai => {
                    ai.IsDemonHorizonwalker = true;
                    ai.OverrideDecision = (self, options) => {
                        Creature creature = self.Self;

                        return creature.Actions.ActionsLeft == 1 && creature.Battle.AllCreatures.All<Creature>((Func<Creature, bool>)(enemy => !enemy.EnemyOf(creature) || creature.DetectionStatus.EnemiesYouAreHiddenFrom.Contains<Creature>(enemy))) && !creature.DetectionStatus.Undetected ? options.Where<Option>((Func<Option, bool>)(opt => opt.OptionKind == OptionKind.MoveHere && opt.Text == "Sneak" && opt is TileOption)).ToList<Option>().GetRandom<Option>() : (Option)null;
                    };
                })
                .WithProficiency(Trait.Weapon, Proficiency.Trained)
                .AddQEffect(new QEffect("Obliviating Aura", "The unseen guardian feels slippery and elusive in its victim's minds, making it easy for them to lose track of it's postion. It gains a +20 bonus to checks made to sneak or hide and can hide in plain sight.") {
                    Id = QEffectId.HideInPlainSight,
                    Innate = true,
                    Illustration = IllustrationName.Blur,
                    BonusToSkillChecks = (skill, action, target) => {
                        if (action.Name == "Sneak" || action.Name == "Hide") {
                            return new Bonus(20, BonusType.Status, "Indistinct Form");
                        }
                        return null;
                    }
                })
                .AddQEffect(new QEffect("Seek Vulnerability", "The Unseen Guardian's obliviating aura quickly falls apart as soon as a creatre's attention begins to settle on it, distrupting the magic. Succsessful seek attempts count as critical successes, and critical successes are upgraded to fully reveal the Unseen Guardian to all of the seeker's allies.") {
                    Innate = true,
                    AfterYouAreTargeted = async (self, action) => {
                        action.ChosenTargets.CheckResults.TryGetValue(self.Owner, out var result);
                        if (action.ActionId == ActionId.Seek && result == CheckResult.Success) {
                            self.Owner.DetectionStatus.HiddenTo.Remove(action.Owner);
                            self.Owner.DetectionStatus.RecalculateIsHiddenToAnEnemy();
                        } else if (action.ActionId == ActionId.Seek && result == CheckResult.CriticalSuccess) {
                            self.Owner.DetectionStatus.HiddenTo.Clear();
                            self.Owner.DetectionStatus.RecalculateIsHiddenToAnEnemy();
                        }
                    }
                })
                .AddQEffect(QEffect.DamageImmunity(DamageKind.Bleed))
                .AddQEffect(QEffect.DamageImmunity(DamageKind.Poison))
                .AddQEffect(QEffect.ImmunityToCondition(QEffectId.Paralyzed))
                .AddQEffect(QEffect.Flying())
                .AddQEffect(QEffect.SneakAttack("1d8"))
                .WithUnarmedStrike(CommonItems.CreateNaturalWeapon(IllustrationName.Fist, "Fists", "2d4", DamageKind.Bludgeoning, new Trait[] { Trait.Unarmed, Trait.Magical, Trait.Finesse, Trait.Melee, Trait.Agile }))
                .WithAdditionalUnarmedStrike(new Item(IllustrationName.FourWinds, "Slicing Wind", new Trait[] { Trait.Ranged, Trait.Electricity, Trait.Magical }).WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Slashing) {
                    VfxStyle = new VfxStyle(5, ProjectileKind.Cone, IllustrationName.FourWinds),
                    Sfx = SfxName.ElementalBlastWater
                }.WithRangeIncrement(4)))
            );

            ModManager.RegisterNewCreature("Unseen Guardian", Creatures[ModEnums.CreatureId.UNSEEN_GUARDIAN]);


            // CREATURE - Drow Assassin
            Creatures.Add(ModEnums.CreatureId.DROW_ASSASSIN,
                encounter => new Creature(IllustrationName.Shadow, "Drow Assassin", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, Trait.Humanoid }, 1, 7, 6, new Defenses(18, 4, 10, 7), 18, new Abilities(-1, 4, 1, 2, 2, 1), new Skills(stealth: 10, acrobatics: 7))
                .WithAIModification(ai => {
                    ai.OverrideDecision = (self, options) => {

                        Creature creature = self.Self;

                        if (creature.HasEffect(QEffectIds.Lurking)) {
                            Creature stalkTarget = creature.Battle.AllCreatures.FirstOrDefault(c => c.QEffects.FirstOrDefault(qf => qf.Id == QEffectIds.Stalked && qf.Source == creature) != null);
                            foreach (Option option in options.Where(o => o.OptionKind == OptionKind.MoveHere && o.AiUsefulness.ObjectiveAction != null && o.AiUsefulness.ObjectiveAction.Action.ActionId == ActionId.Sneak)) {
                                TileOption? option2 = option as TileOption;
                                if (option2 != null && option2.Tile.DistanceTo(stalkTarget.Occupies) <= 5 && option2.Tile.HasLineOfEffectTo(stalkTarget.Occupies) <= CoverKind.Standard) {
                                    option2.AiUsefulness.MainActionUsefulness += 10;
                                }
                                //option2.AiUsefulness.MainActionUsefulness += 20 - option2.Tile.DistanceTo(stalkTarget.Occupies);
                            }
                            foreach (Option option in options.Where(o => o.OptionKind != OptionKind.MoveHere)) {
                                if (creature.Occupies.DistanceTo(stalkTarget.Occupies) <= 5 && creature.HasLineOfEffectTo(stalkTarget.Occupies) <= CoverKind.Standard) {
                                    option.AiUsefulness.MainActionUsefulness += 15;
                                }

                                //option2.AiUsefulness.MainActionUsefulness += 20 - option2.Tile.DistanceTo(stalkTarget.Occupies);
                            }
                        }

                        return null;
                    };
                })
                .WithProficiency(Trait.Weapon, Proficiency.Expert)
                .AddQEffect(new QEffect() {
                    Id = QEffectId.SwiftSneak
                })
                .AddQEffect(new QEffect("Shadowsilk Cloak", "Target can always attempt to sneak or hide, even when unobstructed.") {
                    Id = QEffectId.HideInPlainSight,
                    Innate = true,
                    StartOfCombat = async self => {
                        List<Creature> party = self.Owner.Battle.AllCreatures.Where(c => c.OwningFaction.IsHumanControlled).ToList();
                        Creature target = party.OrderBy(c => c.HP / 100 * (c.Defenses.GetBaseValue(Defense.AC) * 5)).ToList()[0];

                        self.Owner.AddQEffect(new QEffect {
                            Id = QEffectIds.Lurking,
                            Value = 2,
                            PreventTakingAction = action => action.ActionId != ActionId.Sneak ? "Stalking prey, cannot act." : null,
                            StateCheck = self => {
                                if (!self.Owner.DetectionStatus.Undetected) {
                                    QEffect startled = QEffect.Stunned(2);
                                    startled.Illustration = IllustrationName.Dazzled;
                                    startled.Name = "Startled";
                                    startled.Description = "The assassin is startled by their premature discovery.\nAt the beginning of their next turn, they will lose 2 actions.\n\nThey can't take reactions.";
                                    self.Owner.Occupies.Overhead("*startled!*", Color.Black);
                                    self.Owner.AddQEffect(startled);
                                    Sfxs.Play(SfxName.DazzlingFlash);
                                    self.ExpiresAt = ExpirationCondition.Immediately;
                                }
                            },
                            BonusToSkillChecks = (skill, action, target) => {
                                if (skill == Skill.Stealth && action.Name == "Sneak") {
                                    return new Bonus(7, BonusType.Status, "Lurking");
                                }
                                return null;
                            },
                            ExpiresAt = ExpirationCondition.CountsDownAtEndOfYourTurn,
                        });

                        foreach (Creature player in party) {
                            self.Owner.DetectionStatus.HiddenTo.Add(player);
                        }
                        self.Owner.DetectionStatus.Undetected = true;
                        target.AddQEffect(CommonQEffects.Stalked(self.Owner));

                        self.Owner.AddQEffect(new QEffect() {
                            Id = QEffectId.Slowed,
                            Value = 1
                        });
                        await self.Owner.Battle.GameLoop.Turn(self.Owner, false);
                        self.Owner.RemoveAllQEffects(qf => qf.Id == QEffectId.Slowed);
                    }

                })
                .AddQEffect(new QEffect("Nimble Dodge {icon:Reaction}", "{b}Trigger{/b} The drow assassin is hit or critically hit by an attack. {b}Effect{/b} The drow assassin gains a +2 bonus to their Armor Class against the triggering attack.") {
                    YouAreTargetedByARoll = async (self, action, result) => {
                        if ((result.CheckResult == CheckResult.Success || result.CheckResult == CheckResult.CriticalSuccess) && result.ThresholdToDowngrade <= 2) {
                            if (self.UseReaction()) {
                                self.Owner.AddQEffect(new QEffect() {
                                    ExpiresAt = ExpirationCondition.Ephemeral,
                                    BonusToDefenses = (self, action, defence) => defence == Defense.AC ? new Bonus(2, BonusType.Untyped, "Nimble Dodge") : null
                                });
                                return true;
                            }
                        }
                        return false;
                    }
                })
                .AddQEffect(new QEffect() {
                    AdditionalGoodness = (self, action, target) => {
                        if (target.QEffects.FirstOrDefault(qf => qf.Id == QEffectIds.Stalked && qf.Source == self.Owner) != null) {
                            return 10f;
                        }
                        return 0f;
                    }
                })
                .AddQEffect(CommonQEffects.Drow())
                .AddQEffect(QEffect.SneakAttack("2d6"))
                .WithBasicCharacteristics()
                .WithProficiency(Trait.Dagger, Proficiency.Expert)
                .AddHeldItem(Items.CreateNew(ItemName.Dagger))
                .AddQEffect(CommonQEffects.SpiderVenomAttack(16, "dagger"))
                );

            ModManager.RegisterNewCreature("Drow Assassin", Creatures[ModEnums.CreatureId.DROW_ASSASSIN]);


            // CREATURE - Drow Fighter
            int poisonDC = 17;
            Creatures.Add(ModEnums.CreatureId.DROW_FIGHTER,
            encounter => new Creature(IllustrationName.MerfolkShopkeeper, "Drow Fighter", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, Trait.Humanoid }, 1, 5, 6, new Defenses(15, 4, 10, 7), 18,
            new Abilities(2, 4, 2, 0, 1, 0), new Skills(acrobatics: 7, athletics: 5, stealth: 7, intimidation: 5))
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature creature = self.Self;
                    // Check if has crossbow
                    Item? handcrossbow = creature.HeldItems.FirstOrDefault(item => item.ItemName == ItemName.HandCrossbow);

                    if (handcrossbow == null) {
                        return null;
                    }

                    // Check if crossbow is loaded
                    if (handcrossbow.EphemeralItemProperties.NeedsReload) {
                        // foreach (Option option in options.Where(opt => opt.AiUsefulness.ObjectiveAction != null && opt.AiUsefulness.ObjectiveAction.Action.Name.StartsWith("Reload") || opt.Text == "Reload")) {
                        foreach (Option option in options.Where(opt => opt.Text == "Reload" || (opt.AiUsefulness.ObjectiveAction != null && opt.AiUsefulness.ObjectiveAction.Action.Name == "Reload"))) {
                            option.AiUsefulness.MainActionUsefulness = 1f;
                        }
                    }
                    return null;
                };
            })
            .WithProficiency(Trait.Weapon, Proficiency.Trained)
            .AddQEffect(CommonQEffects.Drow())
            .AddQEffect(QEffect.AttackOfOpportunity(false))
            .WithBasicCharacteristics()
            .WithProficiency(Trait.Weapon, Proficiency.Expert)
            .AddHeldItem(Items.CreateNew(ItemName.Rapier))
            .AddHeldItem(Items.CreateNew(ItemName.HandCrossbow))
            .AddQEffect(new QEffect() {
                ProvideMainAction = self => {
                    Item? rapier = self.Owner.HeldItems.FirstOrDefault(item => item.ItemName == ItemName.Rapier);
                    if (rapier == null) {
                        return null;
                    }

                    StrikeModifiers strikeModifiers = new StrikeModifiers() {
                        AdditionalBonusesToAttackRoll = new List<Bonus>() { new Bonus(1, BonusType.Circumstance, "Skewer") },
                        OnEachTarget = async (a, d, result) => {
                            d.AddQEffect(QEffect.PersistentDamage("1d6", DamageKind.Bleed));
                        }
                    };
                    CombatAction action = self.Owner.CreateStrike(rapier, -1, strikeModifiers);
                    action.ActionCost = 2;
                    action.Name = "Skewer";
                    action.Description = StrikeRules.CreateBasicStrikeDescription2(action.StrikeModifiers, additionalSuccessText: "If you dealt damage, inflict 1d6 persistent bleed damage.");
                    action.ShortDescription += " and inflict 1d6 persistent bleed damage.";
                    action.Illustration = new SideBySideIllustration(action.Illustration, IllustrationName.PersistentBleed);
                    action.WithGoodnessAgainstEnemy((target, attacker, defender) => {
                        return defender.QEffects.FirstOrDefault(qf => qf.Name.Contains(" persistent " + DamageKind.Bleed.ToString().ToLower() + " damage")) != null ? (8 + attacker.Abilities.Strength) * 1.1f : (4.5f + attacker.Abilities.Strength) * 1.1f;
                    });

                    return (ActionPossibility)action;
                }
            })
            .AddQEffect(new QEffect("Lethargy Poison", "Enemies damaged by the drow fighter's hand crossbow attack, are afflicted by lethargy poison. {b}Stage 1{/b} slowed 1; {b}Stage 2{/b} slowed 1 for rest of encounter") {
                StartOfCombat = async self => {
                    self.Name += $" (DC {poisonDC + self.Owner.Level})";
                },
                AfterYouDealDamage = async (attacker, action, target) => {
                    if (action.Item != null && action.Item.ItemName == ItemName.HandCrossbow) {
                        Affliction poison = new Affliction(QEffectIds.LethargyPoison, "Lethargy Poison", attacker.Level + poisonDC, "{b}Stage 1{/b} slowed 1; {b}Stage 2{/b} slowed 1 for rest of encounter", 2, dmg => null, qf => {
                            if (qf.Value == 1) {
                                qf.Owner.AddQEffect(QEffect.Slowed(1).WithExpirationEphemeral());
                            }

                            if (qf.Value == 2) {
                                QEffect nEffect = QEffect.Slowed(1).WithExpirationNever();
                                nEffect.CounteractLevel = qf.CounteractLevel;
                                qf.Owner.AddQEffect(nEffect);
                                qf.Owner.RemoveAllQEffects(qf2 => qf2.Id == QEffectIds.LethargyPoison);
                                qf.Owner.Occupies.Overhead("*lethargy poison converted to slowed 1*", Color.Black);
                            }
                        });

                        await Affliction.ExposeToInjury(poison, attacker, target);
                    }
                },
                AdditionalGoodness = (self, action, target) => {
                    Item? handcrossbow = self.Owner.HeldItems.FirstOrDefault(item => item.ItemName == ItemName.HandCrossbow);
                    int dc = poisonDC + self.Owner.Level;

                    if (handcrossbow == null) {
                        return 0f;
                    }

                    if (action == null || action.Item != handcrossbow) {
                        return 0f;
                    }

                    if (self.Owner.Battle.AllCreatures.Where(cr => cr.OwningFaction.EnemyFactionOf(self.Owner.OwningFaction) && cr.Threatens(self.Owner.Occupies)).ToArray().Length > 0) {
                        return 0f;
                    }

                    if (target != null && !target.HasEffect(QEffectIds.LethargyPoison) && !target.HasEffect(QEffectId.Slowed)) {
                        float start = 15f;
                        float percentage = (float) dc - ((float)target.Defenses.GetBaseValue(Defense.Fortitude) + 10.5f);
                        percentage *= 5f;
                        percentage += 50f;
                        start = start / 100 * percentage;
                        return start;
                    }

                    return 0f;
                }
            }));

            ModManager.RegisterNewCreature("Drow Fighter", Creatures[ModEnums.CreatureId.DROW_FIGHTER]);


            // CREATURE - Drow Priestess
            Creatures.Add(ModEnums.CreatureId.DROW_PRIESTESS,
            encounter => new Creature(IllustrationName.Succubus, "Drow Priestess", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, Trait.Humanoid }, 3, 9, 6, new Defenses(20, 8, 7, 11), 39,
            new Abilities(1, 2, 1, 0, 4, 2), new Skills(deception: 9, stealth: 7, intimidation: 9))
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature creature = self.Self;

                    // Bane AI
                    foreach (Option option in options.Where(o => o.Text == "Bane")) {
                        option.AiUsefulness.MainActionUsefulness = 30;
                    }
                    Option? expandBane = options.FirstOrDefault(o => o.Text == "Increase Bane radius");
                    if (expandBane != null) {
                        QEffect bane = creature.QEffects.FirstOrDefault(qf => qf.Name == "Bane");
                        (int, bool) temp = ((int, bool)) bane.Tag;
                        int radius = temp.Item1;

                        expandBane.AiUsefulness.MainActionUsefulness = 0f;
                        foreach (Creature enemy in creature.Battle.AllCreatures.Where(cr => cr.OwningFaction.EnemyFactionOf(creature.OwningFaction) && creature.DistanceTo(cr.Occupies) == radius + 1)) {
                            expandBane.AiUsefulness.MainActionUsefulness += 4;
                        }
                    }

                    // Demoralize AI
                    foreach (Option option in options.Where(o => o.Text == "Demoralize" || (o.AiUsefulness.ObjectiveAction != null && o.AiUsefulness.ObjectiveAction.Action.ActionId == ActionId.Demoralize))) {
                        option.AiUsefulness.MainActionUsefulness = 0f;
                    }

                    // Ally and enemy proximity AI
                    foreach (Option option in options.Where(o => o.OptionKind == OptionKind.MoveHere)) {
                        TileOption? option2 = option as TileOption;
                        if (option2 != null) {
                            //option2.AiUsefulness.MainActionUsefulness += creature.Battle.AllCreatures.Where(c => c != creature && c.OwningFaction == creature.OwningFaction && !c.HasTrait(Trait.Mindless) && c.DistanceTo(option2.Tile) <= 2 && c.HasLineOfEffectTo(option2.Tile) != CoverKind.Blocked).ToArray().Length;
                            //option2.AiUsefulness.MainActionUsefulness += creature.Battle.AllCreatures.Where(c => c.OwningFaction.EnemyFactionOf(creature.OwningFaction) && c.DistanceTo(option2.Tile) <= 2).ToArray().Length * 0.2f;
                            float mod1 = creature.Battle.AllCreatures.Where(c => c != creature && c.OwningFaction == creature.OwningFaction && !c.HasTrait(Trait.Mindless) && c.DistanceTo(option2.Tile) <= 2 && c.HasLineOfEffectTo(option2.Tile) != CoverKind.Blocked).ToArray().Length;
                            float mod2 = creature.Battle.AllCreatures.Where(c => c.OwningFaction.EnemyFactionOf(creature.OwningFaction) && c.DistanceTo(option2.Tile) <= 2).ToArray().Length * 0.2f;
                            option2.AiUsefulness.MainActionUsefulness += mod1 + mod2;
                        }
                    }
                    foreach (Option option in options.Where(o => o.OptionKind != OptionKind.MoveHere && o.AiUsefulness.MainActionUsefulness != 0)) {
                        float mod1 = creature.Battle.AllCreatures.Where(c => c != creature && c.OwningFaction == creature.OwningFaction && !c.HasTrait(Trait.Mindless) && c.DistanceTo(creature) <= 2 && creature.HasLineOfEffectTo(c.Occupies) != CoverKind.Blocked).ToArray().Length;
                        float mod2 = creature.Battle.AllCreatures.Where(c => c.OwningFaction.EnemyFactionOf(creature.OwningFaction) && c.DistanceTo(creature) <= 2).ToArray().Length * 0.2f;
                        option.AiUsefulness.MainActionUsefulness += mod1 + mod2;
                        //option.AiUsefulness.MainActionUsefulness += creature.Battle.AllCreatures.Where(c => c != creature && c.OwningFaction == creature.OwningFaction && !c.HasTrait(Trait.Mindless) && c.DistanceTo(creature) <= 2 && creature.HasLineOfEffectTo(c.Occupies) != CoverKind.Blocked).ToArray().Length;
                        //option.AiUsefulness.MainActionUsefulness += creature.Battle.AllCreatures.Where(c => c.OwningFaction.EnemyFactionOf(creature.OwningFaction) && c.DistanceTo(creature) <= 2).ToArray().Length * 0.2f;

                    }

                    return null;
                };
            })
            .AddQEffect(CommonQEffects.CruelTaskmistress("1d6"))
            .AddQEffect(CommonQEffects.Drow())
            .WithBasicCharacteristics()
            .WithProficiency(Trait.Weapon, Proficiency.Expert)
            .WithProficiency(Trait.Divine, Proficiency.Expert)
            .AddHeldItem(Items.CreateNew(CustomItems.ScourgeOfFangs))
            .WithSpellProficiencyBasedOnSpellAttack(11, Ability.Wisdom)
            .AddSpellcastingSource(SpellcastingKind.Prepared, Trait.Cleric, Ability.Wisdom, Trait.Divine).WithSpells(
                new SpellId[] { SpellId.Bane, SpellId.Fear, SpellId.Fear, SpellId.Fear, SpellId.ChillTouch },
                new SpellId[] { SpellId.Harm, SpellId.Harm, SpellId.Harm }).Done()
            );
            ModManager.RegisterNewCreature("Drow Priestess", Creatures[ModEnums.CreatureId.DROW_PRIESTESS]);

            
            // CREATURE - Hunting spider
            Creatures.Add(ModEnums.CreatureId.HUNTING_SPIDER,
            encounter => new Creature(IllustrationName.VenomousSnake256, "Hunting Spider", new List<Trait>() { Trait.Animal }, 1, 7, 5, new Defenses(17, 6, 9, 5), 16,
            new Abilities(2, 4, 1, -5, 2, -2), new Skills(acrobatics: 7, stealth: 7, athletics: 5))
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature creature = self.Self;
                    Option best = options.MaxBy(o => o.AiUsefulness.MainActionUsefulness);
                    return null;
                };
            })
            .WithProficiency(Trait.Melee, Proficiency.Expert)
            .WithProficiency(Trait.Ranged, Proficiency.Trained)
            .WithCharacteristics(false, true)
            .WithUnarmedStrike(new Item(IllustrationName.Jaws, "fangs", new Trait[] { Trait.Melee, Trait.Finesse, Trait.Unarmed, Trait.Brawling }).WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Piercing)))
            .AddQEffect(new QEffect("Webwalk", "This creature moves through webs unimpeded.") {Id = QEffectId.IgnoresWeb})
            .AddQEffect(CommonQEffects.SpiderVenomAttack(16, "fangs"))
            .AddQEffect(CommonQEffects.WebAttack(16))
            );
            ModManager.RegisterNewCreature("Hunting Spider", Creatures[ModEnums.CreatureId.HUNTING_SPIDER]);


            // CREATURE - Drider
            Creatures.Add(ModEnums.CreatureId.DRIDER,
            encounter => new Creature(IllustrationName.DemonWebspinner256, "Drider", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, Trait.Aberration }, 3, 6, 6, new Defenses(17, 12, 7, 6), 56,
            new Abilities(5, 3, 3, 1, 3, 2), new Skills(athletics: 10, intimidation: 8))
            .WithProficiency(Trait.Melee, Proficiency.Expert)
            .WithProficiency(Trait.Ranged, Proficiency.Expert)
            .WithBasicCharacteristics()
            .AddHeldItem(Items.CreateNew(ItemName.Glaive))
            .WithUnarmedStrike(new Item(IllustrationName.Jaws, "fangs", new Trait[] { Trait.Melee, Trait.Finesse, Trait.Unarmed, Trait.Brawling }).WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Piercing)))
            .AddQEffect(CommonQEffects.Drow())
            .AddQEffect(QEffect.AttackOfOpportunity())
            .AddQEffect(CommonQEffects.SpiderVenomAttack(16, "fangs")) // Change to drider venom?
            .AddQEffect(CommonQEffects.WebAttack(16))
            .AddQEffect(CommonQEffects.MiniBoss())
            .AddQEffect(new QEffect("Webwalk", "This creature moves through webs unimpeded.") { Id = QEffectId.IgnoresWeb })
            );
            ModManager.RegisterNewCreature("Drider", Creatures[ModEnums.CreatureId.DRIDER]);


            // CREATURE - Drow Arcanist
            Creatures.Add(ModEnums.CreatureId.DROW_ARCANIST,
            encounter => new Creature(IllustrationName.DarkPoet256, "Drow Arcanist", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, Trait.Humanoid }, 1, 7, 6, new Defenses(15, 4, 7, 10), 14,
            new Abilities(1, 3, 0, 5, 1, 1), new Skills(acrobatics: 10, intimidation: 6, arcana: 8))
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature creature = self.Self;

                    // Get current proximity score
                    float currScore = 0f;
                    foreach (Creature enemy in creature.Battle.AllCreatures.Where(cr => cr.OwningFaction.EnemyFactionOf(creature.OwningFaction))) {
                        currScore += creature.DistanceTo(enemy);
                    }

                    // Find how close allies are to the party
                    List<Creature> allies = creature.Battle.AllCreatures.Where(cr => cr.Alive && cr.OwningFaction == creature.OwningFaction).ToList();
                    float allyScore = 0;
                    foreach (Creature ally in allies) {
                        foreach (Creature enemy in creature.Battle.AllCreatures.Where(cr => cr.OwningFaction.EnemyFactionOf(creature.OwningFaction))) {
                           allyScore += ally.DistanceTo(enemy);
                        }
                    }
                    allyScore /= allies.Count;

                    foreach (Option option in options.Where(o => o.OptionKind == OptionKind.MoveHere && o is TileOption)) {
                        TileOption option2 = option as TileOption;
                        float personalScore = 0f;
                        foreach (Creature enemy in creature.Battle.AllCreatures.Where(cr => cr.OwningFaction.EnemyFactionOf(creature.OwningFaction))) {
                            personalScore += option2.Tile.DistanceTo(enemy.Occupies);
                        }

                        if (option2.Text == "Stride" && creature.Battle.AllCreatures.Where(cr => cr.OwningFaction.EnemyFactionOf(creature.OwningFaction) && cr.Threatens(creature.Occupies)).ToArray().Length == 0) {
                            if (personalScore > allyScore && currScore < allyScore) {
                                option2.AiUsefulness.MainActionUsefulness += 5f;
                            } else if (personalScore < allyScore) {
                                option2.AiUsefulness.MainActionUsefulness -= 15f;
                            }
                        } else if (option2.Text == "Step") {
                            if (personalScore > allyScore && currScore < allyScore) {
                                option2.AiUsefulness.MainActionUsefulness += 5f;
                            } else if (personalScore < allyScore) {
                                option2.AiUsefulness.MainActionUsefulness -= 15f;
                            }
                        }
                    }

                    return null;
                };
            })
            .WithProficiency(Trait.Melee, Proficiency.Trained)
            .WithProficiency(Trait.Arcane, Proficiency.Expert)
            .WithBasicCharacteristics()
            .AddHeldItem(Items.CreateNew(ItemName.RepeatingHandCrossbow))
            .AddQEffect(CommonQEffects.Drow())
            .AddQEffect(new QEffect("Slip Away {icon:Reaction}", "{b}Trigger{/b} The drow arcanist is damaged by an attack. {b}Effect{/b} The drow arcanist makes a free step action and gains +1 AC until the end of their attacker's turn.") {
                AfterYouTakeDamage = async (self, amount, kind, action, critical) => {
                    if (!(action.HasTrait(Trait.Melee) || (action.Owner != null && action.Owner.IsAdjacentTo(self.Owner)))) {
                        return;
                    }

                    if (self.UseReaction()) {
                        self.Owner.AddQEffect(new QEffect("Slip Away", "+1 circumstance bonus to AC.") {
                            Illustration = IllustrationName.Shield,
                            BonusToDefenses = (self, action, defence) => defence == Defense.AC ? new Bonus(1, BonusType.Circumstance, "Slip Away") : null,
                            ExpiresAt = ExpirationCondition.ExpiresAtEndOfAnyTurn
                        });
                        await self.Owner.StepAsync("Choose tile for Slip Away");
                    }
                }
            })
            .AddQEffect(new QEffect("Dark Arts", "The drow arcanist excels at causing pain with their black practice. Their non-cantrip spells gain a +2 status bonus to damage.") {
                BonusToDamage = (qfSelf, spell, target) => { 
                    return spell.HasTrait(Trait.Spell) && !spell.HasTrait(Trait.Cantrip) && !spell.HasTrait(Trait.Focus) && spell.CastFromScroll == null ? new Bonus(2, BonusType.Status, "Dark Arts") : null;
                }
            })
            .AddSpellcastingSource(SpellcastingKind.Prepared, Trait.Wizard, Ability.Intelligence, Trait.Arcane).WithSpells(
                new SpellId[] { SpellId.MagicMissile, SpellId.MagicMissile, SpellId.GrimTendrils, SpellId.ChillTouch, SpellId.ProduceFlame, SpellId.Shield }).Done()
            );
            ModManager.RegisterNewCreature("Drow Arcanist", Creatures[ModEnums.CreatureId.DROW_ARCANIST]);


            // CREATURE - Drow Arcanist
            Creatures.Add(ModEnums.CreatureId.DROW_INQUISITRIX,
            encounter => new Creature(IllustrationName.DarkPoet256, "Drow Inquisitrix", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, Trait.Humanoid }, 2, 8, 6, new Defenses(17, 5, 8, 11), 30,
            new Abilities(2, 4, 1, 2, 2, 4), new Skills(acrobatics: 8, intimidation: 11, religion: 7))
            .WithProficiency(Trait.Martial, Proficiency.Expert)
            .WithProficiency(Trait.Spell, Proficiency.Trained)
            .WithBasicCharacteristics()
            .AddHeldItem(Items.CreateNew(CustomItems.ScourgeOfFangs))
            .AddQEffect(CommonQEffects.Drow())
            .AddQEffect(QEffect.SneakAttack("1d6"))
            .AddQEffect(new QEffect("Iron Command {icon:Reaction}", "{b}Trigger{/b} An enemy within 15 feet damages you. {b}Effect{/b} Your attacker must choose either to fall prone or suffer 1d6 mental damage. You then deal +1 evil or negative damage against them with your strikes until the end of your next turn.") {
                AfterYouTakeDamage = async (self, amount, kind, action, critical) => {
                    if (action == null || action.Owner == null || action.Owner == action.Owner.Battle.Pseudocreature) {
                        return;
                    }

                    if (action.Owner.OwningFaction == self.Owner.OwningFaction) {
                        return;
                    }

                    if (self.UseReaction()) {
                        if (await action.Owner.Battle.AskForConfirmation(action.Owner, self.Owner.Illustration, $"{self.Owner.Name} uses Iron Command, urging you to kneel before your betters. Do you wish to drop prone in supplication, or refuse and suffer 1d6 mental damage?", "Submit", "Defy")) {
                            action.Owner.AddQEffect(QEffect.Prone());
                        } else {
                            await CommonSpellEffects.DealDirectDamage(null, DiceFormula.FromText("1d6"), action.Owner, CheckResult.Success, DamageKind.Mental);
                        }

                        DamageKind type = DamageKind.Evil;
                        if (!action.Owner.HasTrait(Trait.Good) && !action.Owner.HasTrait(Trait.Undead)) {
                            type = DamageKind.Negative;
                        }

                        self.Owner.AddQEffect(new QEffect("Inquisitrix Mandate", $"You deal +1 {type.HumanizeTitleCase2()} damage against {action.Owner.Name} for daring to strike against you.") {
                            Illustration = IllustrationName.BestowCurse,
                            AddExtraKindedDamageOnStrike = (strike, target) => {
                                if (strike.HasTrait(Trait.Strike) && target == action.Owner) {
                                    return new KindedDamage(DiceFormula.FromText("1", "Inquisitrix Mandate"), type);
                                }
                                return null;
                            },
                            AdditionalGoodness = (self, action, target) => {
                                if (action.HasTrait(Trait.Strike) && target == action.Owner) {
                                    return 1f;
                                }
                                return 0f;
                            },
                            ExpiresAt = ExpirationCondition.ExpiresAtEndOfYourTurn
                        });
                    }
                }
            })
            .AddQEffect(new QEffect() {
                ProvideMainAction = self => {
                    if (self.Owner.Spellcasting.PrimarySpellcastingSource.Spells.FirstOrDefault(spell => spell.SpellId == SpellId.Harm) == null) {
                        return null;
                    }

                    Item weapon = self.Owner.PrimaryWeapon;

                    StrikeModifiers strikeModifiers = new StrikeModifiers() {
                        OnEachTarget = async (a, d, result) => {
                            await CommonSpellEffects.DealDirectDamage(null, DiceFormula.FromText("1d8"), d, result, DamageKind.Negative);
                            a.Spellcasting.PrimarySpellcastingSource.Spells.RemoveFirst(spell => spell.SpellId == SpellId.Harm);
                        }
                    };
                    CombatAction action = self.Owner.CreateStrike(weapon, -1, strikeModifiers);
                    action.ActionCost = 2;
                    action.Name = "Channel Smite";
                    action.Description = "You siphon the destructive energies of positive or negative energy through a melee attack and into your foe. Make a melee Strike and add the spell’s damage to the Strike’s damage. This is negative damage if you expended a harm spell or positive damage if you expended a heal spell. The spell is expended with no effect if your Strike fails or hits a creature that isn’t damaged by that energy type (such as if you hit a non-undead creature with a heal spell).";
                    action.ShortDescription += " and expends a casting of harm to inflict 1d8 negative damage.";
                    action.Illustration = new SideBySideIllustration(action.Illustration, IllustrationName.Harm);
                    action.WithGoodnessAgainstEnemy((target, attacker, defender) => {
                        return defender.HasTrait(Trait.Undead) ? -100f : 4.5f + action.TrueDamageFormula.ExpectedValue;
                    });

                    return (ActionPossibility)action;
                }
            })
            .AddSpellcastingSource(SpellcastingKind.Prepared, Trait.Cleric, Ability.Charisma, Trait.Divine).WithSpells(
                new SpellId[] { SpellId.Harm, SpellId.Harm, SpellId.Harm }).Done()
            );
            ModManager.RegisterNewCreature("Drow Inquisitrix", Creatures[ModEnums.CreatureId.DROW_INQUISITRIX]);

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
