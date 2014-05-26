using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml.Serialization;
using System.IO;
using Anthrax.WoW.Internals;

namespace Anthrax
{
    public class Death_Knight : Modules.ICombat
    {
        public enum Spells : int
        {
            BloodStrike = 45902,
            ScourgeStrike = 55090,
            FesteringStrike = 85948,
            FrostStrike = 45902,
            PlagueStrike = 45462,
            IcyTouch = 45477,
            DeathStrike = 49998,
            RuneStrike = 56815,
            DeathCoil = 47541,
            Obliterate = 49020,
            SoulReaper = 130735,
			Outbreak = 77575,
			BoneShield = 49222,
			BloodTap = 45529,
			BloodBoil = 48721,
			HeartStrike = 45902,
			HoW = 57330,
			SoulReaperBlood = 114866,
			HS = 55050,
			SummonPet = 46584,
			FS = 85948,
			SoulReaperUnholy = 130736,
			Pest = 50842,
			DarkT = 63560,
			Conversion = 119975,
			SoulReaperFrost = 130735,
			HowlingBlast = 49184,
			PoF = 51271,
			UnholyBlight = 115989,
			BloodPresence = 48263,
        }

        public enum Auras : int
        {
		FrostFever = 55095,
        BloodPlague = 55078,
		HoW = 57330,
		BloodTap = 114851,
		Conversion = 119975,
		CS = 81141,
		BoneShield = 49222,
		BloodPresence = 48263,
		BloodShield = 77535,		
		UHP = 56835,
		SF = 81132,
		BloodCheck = 50371,
		SRB = 114866,
		Shadow = 91342,
		SDoom = 81340,
		PetDarkT = 63560,
		SoulReaperU = 130736,
		FP = 48266,
		KillingM = 51124,
		Rime = 59052,
		SRF = 130735,
		PoF = 51271,
		UHS = 53365,
		UHB = 115989,
		FrostCheck = 51128,
		DWCheck = 51714,
        }

        public override string Name
        {
            get { return "Kohas DeathKnight"; }
        }

        // /!\ WARNING /!\
        // The OnCombat function should NOT be blocking function !
        // The bot will handle calling it in loop until the combat is over.
        // Blocking function may lead to slow behavior.

        public override void OnPatrol()
        {

        }

        // /!\ WARNING /!\
        // The OnPull function should NOT be blocking function !
        // The bot will handle calling it in loop until the combat is over.
        // Blocking function may lead to slow behavior.
        public override void OnPull(WoW.Classes.ObjectManager.WowUnit unit)
        {

        }

        // /!\ WARNING /!\
        // The OnCombat function should NOT be blocking function !
        // The bot will handle calling it in loop until the combat is over.
        // Blocking function may lead to slow behavior.
        public override void OnCombat(WoW.Classes.ObjectManager.WowUnit unit)
        {
		var PET = ObjectManager.Pet;
		var TARGET = unit;
        var ME = ObjectManager.LocalPlayer;
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////BLOOD SPEC//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		if (ME.HasAuraById((int)Auras.BloodCheck))
		{
            // Bone Shield
            if ((!ME.HasAuraById((int)Auras.BoneShield) ||
                ME.Auras.Where(x => x.SpellId == (int)Auras.BoneShield && x.StackCount <= 1).Any()) &&
            AI.Controllers.Spell.CanCast((int)Spells.BoneShield))
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BoneShield);
                return;
            }

            // Horn of Winter
            if (!ME.HasAuraById((int)Auras.HoW) &&
            AI.Controllers.Spell.CanCast((int)Spells.HoW))
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HoW);
                return;
            }

            // We always want to face the target
            //WoW.Internals.Movements.Face(unit.Position);

            if (ME.HealthPercent < 30)
            {
                // Death Strike
                if (AI.Controllers.Spell.CanCast((int)Spells.DeathStrike))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DeathStrike);
                    return;
                }

            }

			if (unit.Position.Distance3DFromPlayer > 10 && unit.Position.Distance3DFromPlayer < 30)
			{
				if (!unit.HasAuraById((int)Auras.FrostFever) && AI.Controllers.Spell.CanCast((int)Spells.Outbreak) || !unit.HasAuraById((int)Auras.BloodPlague) && AI.Controllers.Spell.CanCast((int)Spells.Outbreak))
				{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Outbreak);
                return;
				}
			}
			
			
            if (unit.Position.Distance3DFromPlayer < 7)
            {

                // Deseases
				
				if (ME.HasAuraById((int)Auras.CS))
				{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BloodBoil);
                    return;
				}

                if ((unit.Auras.Where(x => x.SpellId == (int)Auras.FrostFever && x.TimeLeft < 1000).Any() ||
                   unit.Auras.Where(x => x.SpellId == (int)Auras.BloodPlague && x.TimeLeft < 1000).Any()) &&
                    AI.Controllers.Spell.CanCast((int)Spells.BloodBoil))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BloodBoil);
                    return;
                }

                if (!unit.HasAuraById((int)Auras.FrostFever) ||
                    !unit.HasAuraById((int)Auras.BloodPlague))
                {
                        if (!unit.HasAuraById((int)Auras.FrostFever) &&
                            AI.Controllers.Spell.CanCast((int)Spells.IcyTouch))
                        {
                            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.IcyTouch);
                            return;
                        }
                        if (!unit.HasAuraById((int)Auras.BloodPlague) &&
                            AI.Controllers.Spell.CanCast((int)Spells.PlagueStrike))
                        {
                            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.PlagueStrike);
                            return;
                        }
                                     
                }
                                
                // Death Strike
                if (AI.Controllers.Spell.CanCast((int)Spells.DeathStrike))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DeathStrike);
                    return;
                }

                // Rune Strike
                if (ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) >= 30 &&
                    AI.Controllers.Spell.CanCast((int)Spells.RuneStrike))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.RuneStrike);
                    return;
                }

                // Soul Reaper
                if(unit.HealthPercent < 35 &&
                    AI.Controllers.Spell.CanCast((int)Spells.SoulReaper))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SoulReaper);
                    return;
                }

                // Heart Strike
                if (AI.Controllers.Spell.CanCast((int)Spells.HeartStrike))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HeartStrike);
                    return;
                }
				
				// Horn of Winter
            if (AI.Controllers.Spell.CanCast((int)Spells.HoW))
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HoW);
                return;
            }
                

            }
            else
            {
                // Death Coil
                if (unit.HealthPercent < 10 &&
                    AI.Controllers.Spell.CanCast((int)Spells.DeathCoil))
                {
                    AI.Controllers.Spell.UseShapeshiftForm((int)Spells.DeathCoil);
                    return;
                }

               // AI.Controllers.Mover.MoveToObject(unit);
            }

            AI.Controllers.Spell.AttackTarget();
            // No cast processed
            // We should do a "Right Click" on the unit here if we are not in combat
            // in order to auto attack it.

        }
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////FROST SPEC//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		if (ME.HasAuraById((int)Auras.FrostCheck) && !unit.HasAuraById((int)Auras.DWCheck))
		{
            // Horn of Winter
            if (!ME.HasAuraById((int)Auras.HoW) &&
            AI.Controllers.Spell.CanCast((int)Spells.HoW))
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HoW);
                return;
            }

            // We always want to face the target
            //WoW.Internals.Movements.Face(unit.Position);

            if (ME.HealthPercent < 30)
            {
                // Death Strike
                if (AI.Controllers.Spell.CanCast((int)Spells.DeathStrike))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DeathStrike);
                    return;
                }

            }

			if (unit.Position.Distance3DFromPlayer > 10 && unit.Position.Distance3DFromPlayer < 30)
			{
				if (!unit.HasAuraById((int)Auras.FrostFever) && AI.Controllers.Spell.CanCast((int)Spells.Outbreak) || !unit.HasAuraById((int)Auras.BloodPlague) && AI.Controllers.Spell.CanCast((int)Spells.Outbreak))
				{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Outbreak);
                return;
				}
			}
			
			
            if (unit.Position.Distance3DFromPlayer < 7)
            {
				//Pillar Of Frost
				if (!ME.HasAuraById((int)Auras.PoF) && AI.Controllers.Spell.CanCast((int)Spells.PoF))
				{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.PoF);
				}
                // Deseases
				
				if (ME.HasAuraById((int)Auras.Rime))
				{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HowlingBlast);
                    return;
				}

                if (!unit.HasAuraById((int)Auras.FrostFever) ||
                    !unit.HasAuraById((int)Auras.BloodPlague))
                {
                        if (!unit.HasAuraById((int)Auras.FrostFever) &&
                            AI.Controllers.Spell.CanCast((int)Spells.HowlingBlast))
                        {
                            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HowlingBlast);
                            return;
                        }
                        if (!unit.HasAuraById((int)Auras.BloodPlague) &&
                            AI.Controllers.Spell.CanCast((int)Spells.PlagueStrike))
                        {
                            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.PlagueStrike);
                            return;
                        }
                                     
                }
				
				 // Obliterate
                if (AI.Controllers.Spell.CanCast((int)Spells.Obliterate))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Obliterate);
                    return;
                }
				
			//Soul Reaper if Target Health below 35%
			if (AI.Controllers.Spell.CanCast((int)Spells.SoulReaperFrost) && unit.HealthPercent < 35)
				{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SoulReaperFrost);
				return;
				}

                // Frost Strike
                if (ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) >= 20 &&
                    AI.Controllers.Spell.CanCast((int)Spells.FrostStrike))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.FrostStrike);
                    return;
                }
				
				// Horn of Winter
            if (AI.Controllers.Spell.CanCast((int)Spells.HoW))
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HoW);
                return;
            }
			
			if (ME.Auras.Where(x => x.SpellId == (int)Auras.BloodTap && x.StackCount >= 5).Any())
			{
			//AI.Controllers.Spell.Cast((int)Spells.BloodTap, unit);
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BloodTap);
			return;
			}
                

            }
            else
            {
                // Death Coil
                if (unit.HealthPercent < 10 &&
                    AI.Controllers.Spell.CanCast((int)Spells.DeathCoil))
                {
                    AI.Controllers.Spell.UseShapeshiftForm((int)Spells.DeathCoil);
                    return;
                }

               // AI.Controllers.Mover.MoveToObject(unit);
            }

            AI.Controllers.Spell.AttackTarget();
            // No cast processed
            // We should do a "Right Click" on the unit here if we are not in combat
            // in order to auto attack it.

        }
		////////////////////////////////DW FROST  Written By schmiddi/////////////////////////////////////////////////////
		if (ME.HasAuraById((int)Auras.FrostCheck) && unit.HasAuraById((int)Auras.DWCheck))
		{
			//Frost Strike if over 89 Runic Power
			if (ME.GetPower(Anthrax.WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) >= 89 && AI.Controllers.Spell.CanCast((int)Spells.FrostStrike) ||
			ME.GetPower(Anthrax.WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) >= 20 && AI.Controllers.Spell.CanCast((int)Spells.FrostStrike) && ME.HasAuraById((int)Auras.KillingM))
				{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.FrostStrike);
				return;
				}
			//Pillar Of Frost
				if (!ME.HasAuraById((int)Auras.PoF) && AI.Controllers.Spell.CanCast((int)Spells.PoF))
				{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.PoF);
				}
			
			//Howling Blast on Cooldown
			if (AI.Controllers.Spell.CanCast((int)Spells.HowlingBlast) && ME.GetReadyRuneCountByType (WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Frost) > 1 
			|| AI.Controllers.Spell.CanCast((int)Spells.HowlingBlast) && ME.GetReadyRuneCountByType (WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Death) > 1
			|| ME.HasAuraById((int)Auras.Rime))
				{
					WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HowlingBlast);
					return;
				}
			//Soul Reaper if Target Health below 35%
			if (AI.Controllers.Spell.CanCast((int)Spells.SoulReaperFrost) && unit.HealthPercent < 35)
				{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SoulReaperFrost);
				return;
				}
			//Blood Plague Check
			if (AI.Controllers.Spell.CanCast((int)Spells.PlagueStrike) && !unit.HasAuraById((int)Auras.BloodPlague))
				{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.PlagueStrike);
				return;
				}
				
			//Obliterate if you have 1 Unholy Rune
			if (AI.Controllers.Spell.CanCast((int)Spells.Obliterate) && ME.GetReadyRuneCountByType (WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Unholy) >= 1)
				{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Obliterate);
				return;
				}
			//Howling Blast
			if (AI.Controllers.Spell.CanCast((int)Spells.HowlingBlast))
				{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HowlingBlast);
                    return;
				}
			//Frost Strike
				if (ME.GetPower(Anthrax.WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) >= 20 && AI.Controllers.Spell.CanCast((int)Spells.FrostStrike))
				{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.FrostStrike);
				return;
				}
			//Horn of Winter
				if (AI.Controllers.Spell.CanCast((int)Spells.HoW))
				{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HoW);
				return;
				}
			if (ME.Auras.Where(x => x.SpellId == (int)Auras.BloodTap && x.StackCount >= 5).Any())
			{
			//AI.Controllers.Spell.Cast((int)Spells.BloodTap, unit);
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BloodTap);
			return;
			}
 
        }
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////UNHOLY SPEC//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		}
    }
}