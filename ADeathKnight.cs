using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using Anthrax.WoW.Classes.ObjectManager;
using Anthrax.WoW.Internals;
using Anthrax.AI.Controllers;
using Anthrax.WoW;
using Anthrax;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections;
using System.ComponentModel;
using System.Threading;
using System.Xml.Serialization;
using System.IO;


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Notes:                                                                                                       //
//                                                                                                              //
// - Using NRGDRET's AOE modifier code. Awsome work he has done i've just gone through and edited it to suit my //
//   needs.																										//                      	
//                                                                                                              //
// - You can switch between single/aoe rotations by pressing 'z' key. Once again this is not perfect because    //
//   we can't hook to keypresses but instead have to use pooling, so you keypress can be missed. I've added     //
//   OSD so you can know which rotation is active.                                                              //
//                                                                                                              //
// - Rotation is based on icy-veins:                                                                            //
//   http://wow.icy-veins.com/frost-death-knight-pve-dps-guide													//
//   http://wow.icy-veins.com/blood-death-knight-pve-tank-guide													//
//   http://wow.icy-veins.com/unholy-death-knight-pve-dps-guide                                                 //
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////



namespace Anthrax
{
    class Death_Knight : Modules.ICombat
    {
        #region private vars
        bool isAOE;
        WowLocalPlayer ME;
        //Stopwatch stopwatch;
        //List<long> averageScanTimes;
        #endregion

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        public override string Name
        {
            get { return "Kohas Death Knight"; }
        }

        #region enums
        internal enum Spells : int                      //This is a convenient list of all spells used by our combat routine
        {                                               //you can have search on wowhead.com for spell name, and get the id in url
            BloodStrike = 45902,
            ScourgeStrike = 55090,
            FesteringStrike = 85948,
            FrostStrike = 49143,
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

        internal enum Auras : int                       //This is another convenient list of Auras used in our combat routine
        {												//you can have those in wowhead.com (again) and get the id in url
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
        #endregion

		
		
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
        #region singleRotation
        private void castNextSpellbySinglePriority(WowUnit TARGET)
		{
		if (TARGET.Health >= 1 && ME.InCombat)
		{
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
			
						                // Rune Strike
                if (ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) >= 89 &&
                    AI.Controllers.Spell.CanCast((int)Spells.RuneStrike))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.RuneStrike);
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

			if (TARGET.Position.Distance3DFromPlayer > 10 && TARGET.Position.Distance3DFromPlayer < 30
			&& ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Unholy) > 1 
			&& ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Frost) > 1)
			{
				if (!TARGET.HasAuraById((int)Auras.FrostFever) && AI.Controllers.Spell.CanCast((int)Spells.Outbreak) || !TARGET.HasAuraById((int)Auras.BloodPlague) && AI.Controllers.Spell.CanCast((int)Spells.Outbreak))
				{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Outbreak);
                return;
				}
			}
			
			
            if (TARGET.Position.Distance3DFromPlayer < 7)
            {

                // Deseases
				
				if (ME.HasAuraById((int)Auras.CS))
				{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BloodBoil);
                    return;
				}

                if ((TARGET.Auras.Where(x => x.SpellId == (int)Auras.FrostFever && x.TimeLeft < 8000).Any() ||
                   TARGET.Auras.Where(x => x.SpellId == (int)Auras.BloodPlague && x.TimeLeft < 8000).Any()) &&
                    AI.Controllers.Spell.CanCast((int)Spells.BloodBoil)
				|| ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Blood) >= 2 && ME.UnitsAttackingMe.Count > 1 && TARGET.HasAuraById((int)Auras.BloodPlague))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BloodBoil);
                    return;
                }
				
								// Heart Strike
                if (AI.Controllers.Spell.CanCast((int)Spells.HS) && ME.HasAuraById((int)Auras.BloodShield) 
				&& ME.Auras.Where(x => x.SpellId == (int)Auras.BloodShield && x.TimeLeft >= 6).Any() 
				&&  ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Death) >= 1
				|| AI.Controllers.Spell.CanCast((int)Spells.HS) && ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Blood) >= 1)
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HS);
                    return;
                }
				
				if (!ME.HasAuraById((int)Auras.BloodShield) && TARGET.HasAuraById((int)Auras.BloodPlague) 
				|| (ME.HasAuraById((int)Auras.BloodShield) && ME.Auras.Where(x => x.SpellId == (int)Auras.BloodShield && x.TimeLeft <= 6000).Any()))
				{
					WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DeathStrike);
					return;
				}

                if (!TARGET.HasAuraById((int)Auras.FrostFever) ||
                    !TARGET.HasAuraById((int)Auras.BloodPlague))
                {
                        if (!TARGET.HasAuraById((int)Auras.FrostFever) &&
                            AI.Controllers.Spell.CanCast((int)Spells.IcyTouch))
                        {
                            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.IcyTouch);
                            return;
                        }
                        if (!TARGET.HasAuraById((int)Auras.BloodPlague) &&
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


                // Soul Reaper
                if(TARGET.HealthPercent < 35 &&
                    AI.Controllers.Spell.CanCast((int)Spells.SoulReaper))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SoulReaper);
                    return;
                }
				
				// Horn of Winter
            if (AI.Controllers.Spell.CanCast((int)Spells.HoW))
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HoW);
                return;
            }
                
			                // Rune Strike
                if (ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) >= 30 &&
                    AI.Controllers.Spell.CanCast((int)Spells.RuneStrike))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.RuneStrike);
                    return;
                }

            }
            else
            {
                // Death Coil
                if (TARGET.HealthPercent < 10 &&
                    AI.Controllers.Spell.CanCast((int)Spells.DeathCoil))
                {
                    AI.Controllers.Spell.UseShapeshiftForm((int)Spells.DeathCoil);
                    return;
                }

               // AI.Controllers.Mover.MoveToObject(TARGET);
            }

            AI.Controllers.Spell.AttackTarget();
            // No cast processed
            // We should do a "Right Click" on the TARGET here if we are not in combat
            // in order to auto attack it.

        }
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////FROST SPEC//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		if (ME.HasAuraById((int)Auras.FrostCheck) && !TARGET.HasAuraById((int)Auras.DWCheck))
		{
            // Horn of Winter
            if (!ME.HasAuraById((int)Auras.HoW) &&
            AI.Controllers.Spell.CanCast((int)Spells.HoW))
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HoW);
                return;
            }

            // We always want to face the target
            //WoW.Internals.Movements.Face(TARGET.Position);

            if (ME.HealthPercent < 30)
            {
                // Death Strike
                if (AI.Controllers.Spell.CanCast((int)Spells.DeathStrike))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DeathStrike);
                    return;
                }

            }

			if (TARGET.Position.Distance3DFromPlayer > 10 && TARGET.Position.Distance3DFromPlayer < 30)
			{
				if (!TARGET.HasAuraById((int)Auras.FrostFever) && AI.Controllers.Spell.CanCast((int)Spells.Outbreak) || !TARGET.HasAuraById((int)Auras.BloodPlague) && AI.Controllers.Spell.CanCast((int)Spells.Outbreak))
				{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Outbreak);
                return;
				}
			}
			
			                // Frost Strike
                if (ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) >= 89 &&
                    AI.Controllers.Spell.CanCast((int)Spells.FrostStrike))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.FrostStrike);
                    return;
                }
            if (TARGET.Position.Distance3DFromPlayer < 7)
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

                if (!TARGET.HasAuraById((int)Auras.FrostFever) ||
                    !TARGET.HasAuraById((int)Auras.BloodPlague))
                {
                    if (!TARGET.HasAuraById((int)Auras.FrostFever) &&
                    AI.Controllers.Spell.CanCast((int)Spells.HowlingBlast))
                    {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HowlingBlast);
                    return;
                    }
                    if (!TARGET.HasAuraById((int)Auras.BloodPlague) &&
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
			if (AI.Controllers.Spell.CanCast((int)Spells.SoulReaperFrost) && TARGET.HealthPercent < 35)
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
			
			if (ME.Auras.Where(x => x.SpellId == (int)Auras.BloodTap && x.StackCount >= 5).Any() && AI.Controllers.Spell.CanCast((int)Spells.BloodTap))
			{
			//AI.Controllers.Spell.Cast((int)Spells.BloodTap, TARGET);
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BloodTap);
			return;
			}
                

            }
            else
            {
                // Death Coil
                if (TARGET.HealthPercent < 10 &&
                    AI.Controllers.Spell.CanCast((int)Spells.DeathCoil))
                {
                    AI.Controllers.Spell.UseShapeshiftForm((int)Spells.DeathCoil);
                    return;
                }

               // AI.Controllers.Mover.MoveToObject(TARGET);
            }

            AI.Controllers.Spell.AttackTarget();
            // No cast processed
            // We should do a "Right Click" on the TARGET here if we are not in combat
            // in order to auto attack it.

        }
		////////////////////////////////DW FROST  Written By schmiddi/////////////////////////////////////////////////////
		if (ME.HasAuraById((int)Auras.FrostCheck) && TARGET.HasAuraById((int)Auras.DWCheck))
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
			if (AI.Controllers.Spell.CanCast((int)Spells.SoulReaperFrost) && TARGET.HealthPercent < 35)
				{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SoulReaperFrost);
				return;
				}
			//Blood Plague Check
			if (AI.Controllers.Spell.CanCast((int)Spells.PlagueStrike) && !TARGET.HasAuraById((int)Auras.BloodPlague))
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
			//AI.Controllers.Spell.Cast((int)Spells.BloodTap, TARGET);
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BloodTap);
			return;
			}
		  }
        }
		}
        #endregion

        #region AOE>4 rotation
        private void castNextSpellbyAOEPriority(WowUnit TARGET)
        {
		if (TARGET.Health >= 1 && ME.InCombat)
		{
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
			
						                // Rune Strike
                if (ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) >= 89 &&
                    AI.Controllers.Spell.CanCast((int)Spells.RuneStrike))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.RuneStrike);
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

			if (TARGET.Position.Distance3DFromPlayer > 10 && TARGET.Position.Distance3DFromPlayer < 30
			&& ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Unholy) > 1 
			&& ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Frost) > 1)
			{
				if (!TARGET.HasAuraById((int)Auras.FrostFever) && AI.Controllers.Spell.CanCast((int)Spells.Outbreak) || !TARGET.HasAuraById((int)Auras.BloodPlague) && AI.Controllers.Spell.CanCast((int)Spells.Outbreak))
				{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Outbreak);
                return;
				}
			}
			
			
            if (TARGET.Position.Distance3DFromPlayer < 7)
            {

                // Deseases
				
				if (ME.HasAuraById((int)Auras.CS))
				{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BloodBoil);
                    return;
				}

                if ((TARGET.Auras.Where(x => x.SpellId == (int)Auras.FrostFever && x.TimeLeft < 8000).Any() ||
                   TARGET.Auras.Where(x => x.SpellId == (int)Auras.BloodPlague && x.TimeLeft < 8000).Any()) &&
                    AI.Controllers.Spell.CanCast((int)Spells.BloodBoil)
				|| ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Blood) >= 2 && ME.UnitsAttackingMe.Count > 1 && TARGET.HasAuraById((int)Auras.BloodPlague))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BloodBoil);
                    return;
                }
				
				// Heart Strike
                if (AI.Controllers.Spell.CanCast((int)Spells.BloodBoil) && ME.HasAuraById((int)Auras.BloodShield) 
				&& ME.Auras.Where(x => x.SpellId == (int)Auras.BloodShield && x.TimeLeft >= 6).Any() 
				&&  ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Death) >= 1
				|| AI.Controllers.Spell.CanCast((int)Spells.BloodBoil) && ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Blood) >= 1)
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BloodBoil);
                    return;
                }
				
				if (!ME.HasAuraById((int)Auras.BloodShield) && TARGET.HasAuraById((int)Auras.BloodPlague) 
				|| (ME.HasAuraById((int)Auras.BloodShield) && ME.Auras.Where(x => x.SpellId == (int)Auras.BloodShield && x.TimeLeft <= 6000).Any()))
				{
					WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DeathStrike);
					return;
				}

                if (!TARGET.HasAuraById((int)Auras.FrostFever) ||
                    !TARGET.HasAuraById((int)Auras.BloodPlague))
                {
                        if (!TARGET.HasAuraById((int)Auras.FrostFever) &&
                            AI.Controllers.Spell.CanCast((int)Spells.IcyTouch))
                        {
                            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.IcyTouch);
                            return;
                        }
                        if (!TARGET.HasAuraById((int)Auras.BloodPlague) &&
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


                // Soul Reaper
                if(TARGET.HealthPercent < 35 &&
                    AI.Controllers.Spell.CanCast((int)Spells.SoulReaper))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SoulReaper);
                    return;
                }


				
				// Horn of Winter
            if (AI.Controllers.Spell.CanCast((int)Spells.HoW))
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HoW);
                return;
            }
                
			                // Rune Strike
                if (ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) >= 30 &&
                    AI.Controllers.Spell.CanCast((int)Spells.RuneStrike))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.RuneStrike);
                    return;
                }

            }
            else
            {
                // Death Coil
                if (TARGET.HealthPercent < 10 &&
                    AI.Controllers.Spell.CanCast((int)Spells.DeathCoil))
                {
                    AI.Controllers.Spell.UseShapeshiftForm((int)Spells.DeathCoil);
                    return;
                }

               // AI.Controllers.Mover.MoveToObject(TARGET);
            }

            AI.Controllers.Spell.AttackTarget();
            // No cast processed
            // We should do a "Right Click" on the TARGET here if we are not in combat
            // in order to auto attack it.

        }
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////FROST SPEC//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		
		
		if (ME.HasAuraById((int)Auras.FrostCheck))
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
			
			if (TARGET.Position.Distance3DFromPlayer < 10 && AI.Controllers.Spell.CanCast((int)Spells.UnholyBlight))
				{
					WoW.Internals.ActionBar.ExecuteSpell((int)Spells.UnholyBlight);
					return;
				}
			//Howling Blast on Cooldown
			if (AI.Controllers.Spell.CanCast((int)Spells.HowlingBlast)
			|| ME.HasAuraById((int)Auras.Rime))
				{
					WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HowlingBlast);
					return;
				}
				
			//Blood Plague Check
			if (AI.Controllers.Spell.CanCast((int)Spells.PlagueStrike) && !TARGET.HasAuraById((int)Auras.BloodPlague) 
			|| AI.Controllers.Spell.CanCast((int)Spells.PlagueStrike) && ME.GetReadyRuneCountByType (WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Unholy) > 1)
				{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.PlagueStrike);
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
			//AI.Controllers.Spell.Cast((int)Spells.BloodTap, TARGET);
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BloodTap);
			return;
			}
		  
        }
		}
		

        } /////////////////END AOE ROTATION
        #endregion

        #region auxFunctions
        public void changeRotation()
        {
            if (isAOE)
            {
                Console.Beep(5000, 100);
                isAOE = false;
                Logger.WriteLine("Rotation Single!!");
            }
            else
            {
                Console.Beep(5000, 100);
                Console.Beep(5000, 100);
                Console.Beep(5000, 100);
                isAOE = true;
                Logger.WriteLine("Rotation AOE!!");
            }
        }
        #endregion

        public override void OnCombat(WowUnit TARGET)
        {
            /* Performance tests
            stopwatch.Stop();
            averageScanTimes.Add(stopwatch.ElapsedMilliseconds);
            SPQR.Logger.WriteLine("Elapsed:  " + stopwatch.ElapsedMilliseconds.ToString() + " miliseconds, average:" + (averageScanTimes.Sum() / averageScanTimes.Count()).ToString() + ",Max:" + averageScanTimes.Max());
            stopwatch.Restart();
             */
            if (!Cooldown.IsGlobalCooldownActive && TARGET.IsValid)
            {
                if (isAOE) { castNextSpellbyAOEPriority(TARGET); } else { castNextSpellbySinglePriority(TARGET); }
            }
            if ((GetAsyncKeyState(90) == -32767))
            {
                changeRotation();
            }
        }

        public override void OnLoad()   //This is called when the Customclass is loaded in SPQR
        {
            Logger.WriteLine("CustomClass " + Name + " Loaded");
        }

        public override void OnUnload() //This is called when the Customclass is unloaded in SPQR
        {
 
            Logger.WriteLine("CustomClass " + Name + " Unloaded, Goodbye !");
        }

        public override void OnBotStart() //This is called once, when you hit CTRL+X to start SPQR combat routine
        {
            ME = ObjectManager.LocalPlayer;
            Logger.WriteLine("Launching " + Name + " routine... enjoy! Press z to switch between single/aoe");
            /* Performance tests
            stopwatch=new Stopwatch();
            averageScanTimes = new List<long>();
             */
        }

        public override void OnBotStop() //This is called once, when you hit CTRL+X to stop SPQR combat routine
        {
            Logger.WriteLine("Stopping " + Name + " routine... gl smashing keys.");
        }


    }
}