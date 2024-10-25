//.AddQEffect(new QEffect() {
//    ProvideMainAction = self => {
//        Item? handcrossbow = self.Owner.HeldItems.FirstOrDefault(item => item.ItemName == ItemName.HandCrossbow);

//        if (handcrossbow == null || handcrossbow.EphemeralItemProperties.NeedsReload) {
//            return null;
//        }
//        CombatAction strike = self.Owner.CreateStrike(handcrossbow);
//        strike.Traits.Add(Trait.Basic);
//        strike.Name = "Poisoned Hand Crossbow";
//        strike.WithGoodness((t, a, d) => {
//            DiceFormula trueDamageFormula = DiceFormula.FromText("1d4", "hand crossbow");
//            float expectedValue = trueDamageFormula.ExpectedValue;
//            if (!d.HasEffect(QEffectIds.lethargyPoison)) {
//                expectedValue += 1f;
//            }
//            if (a.Battle.Encounter.GetType() == typeof(S3E3TempleOfTheDawnbringer) && t.OwnerAction.HasTrait(Trait.Ranged) && (S3E3TempleOfTheDawnbringer.IsOutsideTemple(a) || S3E3TempleOfTheDawnbringer.IsOutsideTemple(d)))
//                return (float)int.MinValue;
//            foreach (QEffect qeffect in a.QEffects) {
//                if (qeffect.AdditionalGoodness != null)
//                    expectedValue += qeffect.AdditionalGoodness(qeffect, t.OwnerAction, d);
//            }
//            return expectedValue;
//        });
//        return (ActionPossibility)strike;
//    }
//})


//CombatAction poisonCA = new CombatAction(self.Owner, IllustrationName.None, "Drow Poison", new Trait[] { Trait.Poison }, "", Target.Uncastable());
//Checks.SavingThrow(Defense.Fortitude);
//CheckResult result = CommonSpellEffects.RollSavingThrow(target, poisonCA, Defense.Fortitude, self.Owner.Level + 13);
//if (result == CheckResult.Failure) {
//    target.AddQEffect(new Affliction(QEffectId.Unspecified, "Drow Poison", self.Owner.Level + 13, "", 3, ));
//}

//Affliction poison = new Affliction(QEffectId.Unspecified, "Drow Poison", attacker.Level + 13, "", 3, dmg => "", self => { });

//Option sneak = ((creature.Actions.ActionsLeft == 1 && creature.Actions.UsedQuickenedAction == true) || (creature.Actions.ActionsLeft == 0 && creature.Actions.UsedQuickenedAction == false)) && creature.Battle.AllCreatures.All<Creature>((Func<Creature, bool>)(enemy => !enemy.EnemyOf(creature) || creature.DetectionStatus.EnemiesYouAreHiddenFrom.Contains<Creature>(enemy))) && !creature.DetectionStatus.Undetected ? options.Where<Option>((Func<Option, bool>)(opt => opt.OptionKind == OptionKind.MoveHere && opt.Text == "Sneak" && opt is TileOption)).ToList<Option>().GetRandom<Option>() : (Option)null;

//// Check if 2 actions remaining
//if (((creature.Actions.ActionsLeft == 2 && creature.Actions.UsedQuickenedAction == true) || (creature.Actions.ActionsLeft == 1 && creature.Actions.UsedQuickenedAction == false)) && creature.Actions.AttackedThisManyTimesThisTurn > 1) {
//    return options.Where(opt => opt.Text == "Hide").ToList().GetRandom();
//}

//// Check if 1 action remaining
//if ((creature.Actions.ActionsLeft == 1 && creature.Actions.UsedQuickenedAction == true) || (creature.Actions.ActionsLeft == 0 && creature.Actions.UsedQuickenedAction == false)) {
//    return creature.Battle.AllCreatures.All(enemy =>
//    !enemy.EnemyOf(creature) || creature.DetectionStatus.EnemiesYouAreHiddenFrom.Contains(enemy))
//    && !creature.DetectionStatus.Undetected ?
//    options.Where(opt => opt.OptionKind == OptionKind.MoveHere && opt.Text == "Sneak" && opt is TileOption).ToList().GetRandom() : null;
//}

//.AddQEffect(new QEffect("Assassin's Action", "Can sneak or hide as a quickened action.") {
//    Innate = true,
//    QuickenedFor = action => {
//        if (new ActionId[] { ActionId.Sneak, ActionId.Hide }.Contains(action.ActionId)) {
//            return true;
//        }
//        return false;
//    }
//})

// .WithSpellProficiencyBasedOnSpellAttack(8, Ability.Intelligence).AddSpellcastingSource(SpellcastingKind.Prepared, Trait.Wizard, Ability.Intelligence, Trait.Arcane).WithSpells(new SpellId[6]
//{
//    SpellId.BurningHands,
//        SpellId.ElectricArc,
//        SpellId.NeedleDarts,
//        SpellId.MagicMissile,
//        SpellId.MagicMissile,
//        SpellId.BurningHands
//      }).Done();

// CREATURE - Unseen Guardian

//Creatures.Add(ModEnums.CreatureId.UNSEEN_GUARDIAN,
//    encounter => new Creature(IllustrationName.ElectricityMephit256, "Unseen Guardian", new List<Trait>() { Trait.Elemental, Trait.Air, Trait.Lawful }, 1, 5, 8, new Defenses(15, 4, 10, 6), 18, new Abilities(1, 3, 3, 1, 3, 1), new Skills(stealth: 6))
//    .WithAIModification(ai => {
//        ai.IsDemonHorizonwalker = true;
//        ai.OverrideDecision = (self, options) => {
//            Creature creature = self.Self;

//            if (creature.Actions.ActionsLeft == 1 && !(creature.HasEffect(QEffectId.Grabbed) || creature.HasEffect(QEffectId.Restrained) || creature.HasEffect(QEffectId.Immobilized))) {
//                return options.FirstOrDefault(o => o.Text == "Vanish");
//            }
//            return null;
//            //return creature.Actions.ActionsLeft == 1 && creature.Battle.AllCreatures.All<Creature>((Func<Creature, bool>)(enemy => !enemy.EnemyOf(creature) || creature.DetectionStatus.EnemiesYouAreHiddenFrom.Contains<Creature>(enemy))) && !creature.DetectionStatus.Undetected ? options.Where<Option>((Func<Option, bool>)(opt => opt.OptionKind == OptionKind.MoveHere && opt.Text == "Sneak" && opt is TileOption)).ToList<Option>().GetRandom<Option>() : (Option)null;
//        };
//    })
//    .WithProficiency(Trait.Weapon, Proficiency.Trained)
//    .AddQEffect(new QEffect("Obliviating Aura", "The unseen guardian feels slippery and elusive in its victim's minds, making it easy for them to lose track of its postion. It gains a +10 bonus to checks made to sneak and can hide in plain sight.") {
//        Id = QEffectId.HideInPlainSight,
//        Innate = true,
//        Illustration = IllustrationName.Blur,
//        BonusToSkillChecks = (skill, action, target) => {
//            if (action.Name == "Sneak") {
//                return new Bonus(10, BonusType.Status, "Obliviating Aura");
//            }
//            return null;
//        },
//    })
//    .AddQEffect(new QEffect() {
//        ProvideMainAction = self => {
//            return (ActionPossibility)new CombatAction(self.Owner, IllustrationName.Blur, "Vanish", new Trait[] { Trait.Magical, Trait.Mental }, $"The {self.Owner.BaseName} erases its locations from its adversary's minds, and attempts to slip away to a new postion.", Target.Self())
//            .WithActionCost(1)
//            .WithActionId(ActionId.Hide)
//            .WithEffectOnSelf(async user => {
//                //foreach (Creature enemy in user.Battle.AllCreatures.Where(cr => cr.OwningFaction != user.OwningFaction)) {
//                //    user.DetectionStatus.HiddenTo.Add(enemy);
//                //}
//                user.DetectionStatus.Undetected = true;
//                //user.DetectionStatus.RecalculateIsHiddenToAnEnemy();
//                user.AddQEffect(new QEffect() {
//                    ExpiresAt = ExpirationCondition.ExpiresAtEndOfYourTurn,
//                    QuickenedFor = action => action.ActionId == ActionId.Sneak
//                });
//                user.AddQEffect(new QEffect() {
//                    Id = QEffectId.Slowed,
//                    Value = 3,
//                    PreventTakingAction = action => action.HasTrait(Trait.Move) ? null : "Can only move.",
//                });
//                await user.Battle.GameLoop.Turn(user, false);
//                user.RemoveAllQEffects(qf => qf.Id == QEffectId.Slowed);
//            });
//        }
//    })
//    .AddQEffect(QEffect.DamageImmunity(DamageKind.Bleed))
//    .AddQEffect(QEffect.DamageImmunity(DamageKind.Poison))
//    .AddQEffect(QEffect.ImmunityToCondition(QEffectId.Paralyzed))
//    .AddQEffect(QEffect.Flying())
//    .AddQEffect(QEffect.SneakAttack("1d8"))
//    .WithUnarmedStrike(CommonItems.CreateNaturalWeapon(IllustrationName.Fist, "Fists", "2d4", DamageKind.Bludgeoning, new Trait[] { Trait.Unarmed, Trait.Magical, Trait.Finesse, Trait.Melee, Trait.Agile }))
//    .WithAdditionalUnarmedStrike(new Item(IllustrationName.FourWinds, "Slicing Wind", new Trait[] { Trait.Ranged, Trait.Electricity, Trait.Magical }).WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Slashing) {
//        VfxStyle = new VfxStyle(5, ProjectileKind.Cone, IllustrationName.FourWinds),
//        Sfx = SfxName.ElementalBlastWater
//    }.WithRangeIncrement(4)))
//);

//ModManager.RegisterNewCreature("Unseen Guardian", Creatures[ModEnums.CreatureId.UNSEEN_GUARDIAN]);

