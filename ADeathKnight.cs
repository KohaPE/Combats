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
		Settings CCSettings = new Settings();
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
			RuneTap = 48982,
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
			DnD = 43265,
			UnholyFrenzy = 49016,
			Garg = 49206,
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
		UnholyCheck = 56835,
		DeathS = 144901,
		SGarg = 49206,
		UnholyF = 49016,
		Str1 = 138702,
		Str2 = 148899,
		
		//Gloves Snapsys Springs
		SSprings = 96228,
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
		
		private int lastDeathSTick = 0;
		private int lastConversionTick = 0;
		private int lastBTTick = 0;
		private int lastRuneTapTick = 0;
		private int lastSSTick = 0;
		
        private void castNextSpellbySinglePriority(WowUnit TARGET)
		{
		
		/////////////////////////////////////////////////////////////////////////////////////Unholy////////////////////////////////////////////////////////////////////////////////////////
		
		//Out Of Combat Pet Revive
		
		if (!WoW.Internals.ObjectManager.Pet.IsAlive && ME.HasAuraById((int)Auras.UnholyCheck) && !ME.IsMounted)
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SummonPet);
                return;
            }
			
		if(ME.HealthPercent <= CCSettings.Conversion && AI.Controllers.Spell.CanCast((int)Spells.Conversion) && ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) >= 10 
		&& Environment.TickCount - lastConversionTick > 2000 && !ME.HasAuraById((int)Auras.Conversion))
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Conversion);
				lastConversionTick = Environment.TickCount;
                return;
            }
			
				///Death and Decay on Alt Press
		if (DetectKeyPress.GetKeyState(DetectKeyPress.Alt) < 0)
                {
                    if (AI.Controllers.Spell.CanCast((int)Spells.DnD))
                    {
                        WoW.Internals.MouseController.RightClick();
                        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DnD);
                        WoW.Internals.MouseController.LockCursor();
                        WoW.Internals.MouseController.MoveMouse(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                        WoW.Internals.MouseController.LeftClick();
                        WoW.Internals.MouseController.UnlockCursor();
                    }

                    return;

                }


		//Rotation
		if (TARGET.Health >= 1 && ME.InCombat)
		{
		
		//Engineering Gloves
			if (!ME.HasAuraById((int)Auras.SSprings) && Environment.TickCount - lastSSTick > 20000 )
		{
              Anthrax.WoW.Internals.ActionBar.PressSlot(0, 0);
			  Logger.WriteLine("Synapse Srpings Used!!!");
			  lastSSTick = Environment.TickCount;
              return;
          }
        if (ME.HasAuraById((int)Auras.UnholyCheck))
		{
		
		
		///Death and Decay on Alt Press
		if (DetectKeyPress.GetKeyState(DetectKeyPress.Alt) < 0)
                {
                    if (AI.Controllers.Spell.CanCast((int)Spells.DnD))
                    {
                        WoW.Internals.MouseController.RightClick();
                        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DnD);
                        WoW.Internals.MouseController.LockCursor();
                        WoW.Internals.MouseController.MoveMouse(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                        WoW.Internals.MouseController.LeftClick();
                        WoW.Internals.MouseController.UnlockCursor();
                    }

                    return;

                }
				
		if(ME.HealthPercent <= CCSettings.Conversion && AI.Controllers.Spell.CanCast((int)Spells.Conversion) && ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) >= 10 
		&& Environment.TickCount - lastConversionTick > 2000 && !ME.HasAuraById((int)Auras.Conversion))
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Conversion);
				lastConversionTick = Environment.TickCount;
                return;
            }
				
		//Unholy Strengh for Massive Dot Dps!
		if (ME.HasAuraById((int)Auras.Str1) && ME.HasAuraById((int)Auras.Str2) && AI.Controllers.Spell.CanCast((int)Spells.UnholyFrenzy))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.UnholyFrenzy);
                    return;
                }
				
		//GARGOYLE!!!
		if (ME.HasAuraById((int)Auras.Str1) && ME.HasAuraById((int)Auras.Str2) && AI.Controllers.Spell.CanCast((int)Spells.Garg) && ME.HasAuraById((int)Auras.UnholyF))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Garg);
                    return;
                }
				
		//Dots//
		if (TARGET.Auras.Where(x => x.SpellId == ((int)Auras.BloodPlague) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any() 
		&& AI.Controllers.Spell.CanCast((int)Spells.Outbreak) && TARGET.Auras.Where(x => x.SpellId == (int)Auras.BloodPlague && x.TimeLeft <= 6000).Any())
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Outbreak);
                return;
            }		
		
		if (!TARGET.HasAuraById((int)Auras.BloodPlague) && AI.Controllers.Spell.CanCast((int)Spells.PlagueStrike) 
		&& !TARGET.Auras.Where(x => x.SpellId == ((int)Auras.BloodPlague) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any()
		|| TARGET.HasAuraById((int)Auras.BloodPlague) && AI.Controllers.Spell.CanCast((int)Spells.PlagueStrike) && TARGET.Auras.Where(x => x.SpellId == (int)Auras.BloodPlague && x.TimeLeft <= 3000).Any() 
		&& TARGET.Auras.Where(x => x.SpellId == ((int)Auras.BloodPlague) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any())
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.PlagueStrike);
                return;
            }

		//Rebuffing Dots if Higher DPS
		if (TARGET.HasAuraById((int)Auras.BloodPlague) && AI.Controllers.Spell.CanCast((int)Spells.PlagueStrike) && ME.HasAuraById((int)Auras.UnholyF) && Environment.TickCount - lastDeathSTick > 8000
		&& ME.HasAuraById((int)Auras.Str1) && ME.HasAuraById((int)Auras.Str2)
		|| ME.HasAuraById((int)Auras.Str1) && AI.Controllers.Spell.CanCast((int)Spells.PlagueStrike) && Environment.TickCount - lastDeathSTick > 8000
		|| ME.HasAuraById((int)Auras.Str2) && AI.Controllers.Spell.CanCast((int)Spells.PlagueStrike) && Environment.TickCount - lastDeathSTick > 8000
		|| ME.HasAuraById((int)Auras.Str1) && ME.HasAuraById((int)Auras.Str2) && AI.Controllers.Spell.CanCast((int)Spells.PlagueStrike) && Environment.TickCount - lastDeathSTick > 8000
		|| ME.Auras.Where(x => x.SpellId == (int)Auras.DeathS && x.StackCount >= 8).Any() && AI.Controllers.Spell.CanCast((int)Spells.PlagueStrike) && Environment.TickCount - lastDeathSTick > 8000)
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.PlagueStrike);
				lastDeathSTick = Environment.TickCount;
                return;
            }			
		//Dark Transformation
		if (AI.Controllers.Spell.CanCast((int)Spells.DarkT) && ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Unholy) >= 1)
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DarkT);
                    return;
                }		
		
		
		//Runic Power Cap
		if (ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) >= 89 &&
                    AI.Controllers.Spell.CanCast((int)Spells.DeathCoil))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DeathCoil);
                    return;
                }
		
		//Soul Reaper if target health at or below 35%		
		if (TARGET.HealthPercent <= 35 && AI.Controllers.Spell.CanCast((int)Spells.SoulReaperUnholy) && ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Unholy) >= 1 )
		        {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SoulReaperUnholy);
                    return;
                }
		
		//Death Coil
		if (ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) >= 32 &&
                    AI.Controllers.Spell.CanCast((int)Spells.DeathCoil))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DeathCoil);
                    return;
                }

		//Scourge Strike if less then 90 runic power
		if (ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) < 90 &&
                    AI.Controllers.Spell.CanCast((int)Spells.ScourgeStrike) && TARGET.Auras.Where(x => x.SpellId == (int)Auras.FrostFever && x.TimeLeft >= 3000).Any()
		|| ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Unholy) >= 2 && AI.Controllers.Spell.CanCast((int)Spells.ScourgeStrike)
		|| ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Death) >= 1)
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ScourgeStrike);
                    return;
                }

		//Festering Strike if less the 90 Runic Power
		if (ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) < 90 && AI.Controllers.Spell.CanCast((int)Spells.FesteringStrike) 
		&& TARGET.Auras.Where(x => x.SpellId == ((int)Auras.BloodPlague) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any() 
		&& ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Blood) >= 1
		&& ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Frost) >= 1)
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.FesteringStrike);
                    return;
                }
		//Festering Strike if less the 90 Runic Power
		if (ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) < 90 &&
                    AI.Controllers.Spell.CanCast((int)Spells.HoW))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HoW);
                    return;
                }		
		
		//BloodTap
		if (ME.Auras.Where(x => x.SpellId == (int)Auras.BloodTap && x.StackCount >= 5).Any() && AI.Controllers.Spell.CanCast((int)Spells.BloodTap))
		{
			if (ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Unholy) <= 1
			|| ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Frost) <= 1
			|| ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Blood) <= 1
			|| ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Death) < 1)
			{
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BloodTap);
			return;
			}
		}
		//Death Coil
		if (AI.Controllers.Spell.CanCast((int)Spells.DeathCoil))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DeathCoil);
                    return;
                }		
		
		//end Combat
		}//end spec check
		////////////////////////////////////////////////////////////////////////////////////Blood Spec//////////////////////////////////////////////////////////////////////////////////////
        if (ME.HasAuraById((int)Auras.BloodCheck))
		{
		//Conversion
		if(ME.HealthPercent <= CCSettings.Conversion && AI.Controllers.Spell.CanCast((int)Spells.Conversion) && ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) >= 10 
		&& Environment.TickCount - lastConversionTick > 2000 && !ME.HasAuraById((int)Auras.Conversion))
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Conversion);
				lastConversionTick = Environment.TickCount;
                return;
            }
		
		//RuneTap
		if (ME.HealthPercent <= CCSettings.RuneTap && AI.Controllers.Spell.CanCast((int)Spells.RuneTap) && ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Blood) >= 1 
		&& Environment.TickCount - lastRuneTapTick > 2000
		|| ME.HealthPercent <= CCSettings.RuneTap && AI.Controllers.Spell.CanCast((int)Spells.RuneTap) && ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Death) >= 3 
		&& Environment.TickCount - lastRuneTapTick > 2000)
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.RuneTap);
				lastRuneTapTick = Environment.TickCount;
                return;
            }
	
			
            // Bone Shield
            if ((!ME.HasAuraById((int)Auras.BoneShield) ||
                ME.Auras.Where(x => x.SpellId == (int)Auras.BoneShield && x.StackCount <= 1).Any()) &&
            AI.Controllers.Spell.CanCast((int)Spells.BoneShield))
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BoneShield);
                return;
            }
			
			if (DetectKeyPress.GetKeyState(DetectKeyPress.Alt) < 0)
                {
                    if (AI.Controllers.Spell.CanCast((int)Spells.DnD))
                    {
                        WoW.Internals.MouseController.RightClick();
                        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DnD);
                        WoW.Internals.MouseController.LockCursor();
                        WoW.Internals.MouseController.MoveMouse(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                        WoW.Internals.MouseController.LeftClick();
                        WoW.Internals.MouseController.UnlockCursor();
                    }

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

             // Deseases
			if (!TARGET.HasAuraById((int)Auras.FrostFever) && AI.Controllers.Spell.CanCast((int)Spells.Outbreak) || !TARGET.HasAuraById((int)Auras.BloodPlague) && AI.Controllers.Spell.CanCast((int)Spells.Outbreak))
				{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Outbreak);
                return;
				}
			//Icy Touch
				if (!TARGET.HasAuraById((int)Auras.FrostFever) && AI.Controllers.Spell.CanCast((int)Spells.IcyTouch) 
				&& !TARGET.Auras.Where(x => x.SpellId == ((int)Auras.FrostFever) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any())
                        {
                            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.IcyTouch);
                            return;
                        }
			//Plague Strike
                 if (!TARGET.HasAuraById((int)Auras.BloodPlague) 
				 && AI.Controllers.Spell.CanCast((int)Spells.PlagueStrike)
				 && !TARGET.Auras.Where(x => x.SpellId == ((int)Auras.BloodPlague) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any())
                        {
                            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.PlagueStrike);
                            return;
                        }

				
				if (!ME.HasAuraById((int)Auras.BloodShield) && TARGET.HasAuraById((int)Auras.BloodPlague) && AI.Controllers.Spell.CanCast((int)Spells.DeathStrike) 
				|| ME.HasAuraById((int)Auras.BloodShield) && ME.Auras.Where(x => x.SpellId == (int)Auras.BloodShield && x.TimeLeft <= 7000).Any() && AI.Controllers.Spell.CanCast((int)Spells.DeathStrike))
				{
					WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DeathStrike);
					return;
				}
				
			//Bloodtap
			if (ME.Auras.Where(x => x.SpellId == (int)Auras.BloodTap && x.StackCount >= 5).Any() && AI.Controllers.Spell.CanCast((int)Spells.BloodTap) && Environment.TickCount - lastBTTick > 2000)
			{
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BloodTap);
			lastBTTick = Environment.TickCount;
			return;
			}
			//Blood Boil
                if ((TARGET.Auras.Where(x => x.SpellId == (int)Auras.FrostFever && x.TimeLeft < 8000).Any() && AI.Controllers.Spell.CanCast((int)Spells.BloodBoil)
				&& TARGET.Auras.Where(x => x.SpellId == ((int)Auras.FrostFever) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any()
				|| TARGET.Auras.Where(x => x.SpellId == (int)Auras.BloodPlague && x.TimeLeft < 8000).Any()) && AI.Controllers.Spell.CanCast((int)Spells.BloodBoil)
				&& TARGET.Auras.Where(x => x.SpellId == ((int)Auras.BloodPlague) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any()
				|| ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Blood) >= 2 && TARGET.HasAuraById((int)Auras.BloodPlague)
				|| ME.HasAuraById((int)Auras.CS) && TARGET.Position.Distance3DFromPlayer < 13) //CS Proc
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BloodBoil);
                    return;
                }
				
			//Blood Boil
				if (ME.UnitsAttackingMe.Count >= 4)
				{
					if ((TARGET.Auras.Where(x => x.SpellId == (int)Auras.FrostFever && x.TimeLeft < 8000).Any() && AI.Controllers.Spell.CanCast((int)Spells.BloodBoil)
					&& TARGET.Auras.Where(x => x.SpellId == ((int)Auras.FrostFever) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any()
					|| TARGET.Auras.Where(x => x.SpellId == (int)Auras.BloodPlague && x.TimeLeft < 8000).Any()) && AI.Controllers.Spell.CanCast((int)Spells.BloodBoil)
					&& TARGET.Auras.Where(x => x.SpellId == ((int)Auras.BloodPlague) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any()
					|| ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Blood) >= 2 && TARGET.HasAuraById((int)Auras.BloodPlague)
					|| ME.HasAuraById((int)Auras.CS) && TARGET.Position.Distance3DFromPlayer < 13
					&& TARGET.Auras.Where(x => x.SpellId == ((int)Auras.BloodPlague) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any()//CS Proc
					|| AI.Controllers.Spell.CanCast((int)Spells.BloodBoil) && ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Blood) >= 1
					&& TARGET.Auras.Where(x => x.SpellId == ((int)Auras.BloodPlague) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any())
					{
						WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BloodBoil);
						return;
					}
				}
				//SoulReaper
				if (TARGET.HealthPercent <= 35 && AI.Controllers.Spell.CanCast((int)Spells.SoulReaperBlood) 
				&& ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Blood) >= 1
				&& !TARGET.HasAuraById((int)Auras.SRB))
				{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SoulReaperBlood);
                    return;
                }
				// Heart Strike
				if (ME.UnitsAttackingMe.Count <= 3)
				{
					if (AI.Controllers.Spell.CanCast((int)Spells.HS) && ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Blood) >= 1)
					{
						WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HS);
						return;
					}
				}

			// Rune Strike
                if (ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) > 29 &&
                    AI.Controllers.Spell.CanCast((int)Spells.RuneStrike))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.RuneStrike);
                    return;
                }
				
			// Horn of Winter
            if (AI.Controllers.Spell.CanCast((int)Spells.HoW))
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HoW);
                return;
            }


        }
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////FROST 2Hn SPEC//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
			
			if (DetectKeyPress.GetKeyState(DetectKeyPress.Alt) < 0)
                {
                    if (AI.Controllers.Spell.CanCast((int)Spells.DnD))
                    {
                        WoW.Internals.MouseController.RightClick();
                        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DnD);
                        WoW.Internals.MouseController.LockCursor();
                        WoW.Internals.MouseController.MoveMouse(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                        WoW.Internals.MouseController.LeftClick();
                        WoW.Internals.MouseController.UnlockCursor();
                    }

                    return;

                }
				
		if(ME.HealthPercent <= CCSettings.Conversion && AI.Controllers.Spell.CanCast((int)Spells.Conversion) && ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) >= 10 
		&& Environment.TickCount - lastConversionTick > 2000 && !ME.HasAuraById((int)Auras.Conversion))
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Conversion);
				lastConversionTick = Environment.TickCount;
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

			if (TARGET.Position.Distance3DFromPlayer > 10)
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

                if (!TARGET.HasAuraById((int)Auras.FrostFever) && AI.Controllers.Spell.CanCast((int)Spells.HowlingBlast)
				|| TARGET.HasAuraById((int)Auras.FrostFever) && TARGET.Auras.Where(x => x.SpellId == (int)Auras.FrostFever && x.TimeLeft <= 5000).Any())
					{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HowlingBlast);
                    return;
                    }
					
                    if (!TARGET.HasAuraById((int)Auras.BloodPlague) && AI.Controllers.Spell.CanCast((int)Spells.PlagueStrike)
					||  TARGET.HasAuraById((int)Auras.BloodPlague) && TARGET.Auras.Where(x => x.SpellId == (int)Auras.BloodPlague && x.TimeLeft <= 5000).Any() )
                    {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.PlagueStrike);
                    return;
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
		
		if (DetectKeyPress.GetKeyState(DetectKeyPress.Alt) < 0)
                {
                    if (AI.Controllers.Spell.CanCast((int)Spells.DnD))
                    {
                        WoW.Internals.MouseController.RightClick();
                        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DnD);
                        WoW.Internals.MouseController.LockCursor();
                        WoW.Internals.MouseController.MoveMouse(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                        WoW.Internals.MouseController.LeftClick();
                        WoW.Internals.MouseController.UnlockCursor();
                    }

                    return;

                }
				
						if(ME.HealthPercent <= CCSettings.Conversion && AI.Controllers.Spell.CanCast((int)Spells.Conversion) && ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) >= 10 
		&& Environment.TickCount - lastConversionTick > 2000 && !ME.HasAuraById((int)Auras.Conversion))
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Conversion);
				lastConversionTick = Environment.TickCount;
                return;
            }
			//Frost Strike if over 89 Runic Power
			if (ME.GetPower(Anthrax.WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) >= 89 && AI.Controllers.Spell.CanCast((int)Spells.FrostStrike) ||
			ME.GetPower(Anthrax.WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) >= 20 && AI.Controllers.Spell.CanCast((int)Spells.FrostStrike) && ME.HasAuraById((int)Auras.KillingM))
				{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.FrostStrike);
				return;
				}
			//Pillar Of Frost
				if (!ME.HasAuraById((int)Auras.PoF) && AI.Controllers.Spell.CanCast((int)Spells.PoF) 
				&& ME.GetReadyRuneCountByType (WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Frost) >= 1
				|| !ME.HasAuraById((int)Auras.PoF) && AI.Controllers.Spell.CanCast((int)Spells.PoF) 
				&& ME.GetPower(Anthrax.WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) > 15)
				{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.PoF);
				}

				//Engineering Gloves
			if (!ME.HasAuraById((int)Auras.SSprings) && Environment.TickCount - lastSSTick > 20000 )
		{
              Anthrax.WoW.Internals.ActionBar.PressSlot(0, 0);
			  Logger.WriteLine("Synapse Srpings Used!!!");
			  lastSSTick = Environment.TickCount;
              return;
          }
			
			//Howling Blast on Cooldown
			if (AI.Controllers.Spell.CanCast((int)Spells.HowlingBlast))
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
			if (AI.Controllers.Spell.CanCast((int)Spells.PlagueStrike) && !TARGET.HasAuraById((int)Auras.BloodPlague)
			|| TARGET.HasAuraById((int)Auras.BloodPlague) && TARGET.Auras.Where(x => x.SpellId == (int)Auras.BloodPlague && x.TimeLeft <= 5000).Any() 
			|| ME.GetReadyRuneCountByType (WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Unholy) >= 1)
				{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.PlagueStrike);
				return;
				}
				
			if (AI.Controllers.Spell.CanCast((int)Spells.HowlingBlast) && !TARGET.HasAuraById((int)Auras.FrostFever)
			|| TARGET.HasAuraById((int)Auras.FrostFever) && TARGET.Auras.Where(x => x.SpellId == (int)Auras.FrostFever && x.TimeLeft <= 5000).Any() )
				{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HowlingBlast);
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

		
		
		////////////////////////////////////////////////////////////////////////////////////////////////////////AOE/////////////////////////////////////////////////////////////////////////////////////////////////////////
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region AOE>4 rotation
        private void castNextSpellbyAOEPriority(WowUnit TARGET)
        {
		
					if (DetectKeyPress.GetKeyState(DetectKeyPress.Alt) < 0)
                {
                    if (AI.Controllers.Spell.CanCast((int)Spells.DnD))
                    {
                        WoW.Internals.MouseController.RightClick();
                        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DnD);
                        WoW.Internals.MouseController.LockCursor();
                        WoW.Internals.MouseController.MoveMouse(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                        WoW.Internals.MouseController.LeftClick();
                        WoW.Internals.MouseController.UnlockCursor();
                    }

                    return;

                }
				
		if(ME.HealthPercent <= CCSettings.Conversion && AI.Controllers.Spell.CanCast((int)Spells.Conversion) && ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) >= 10 
		&& Environment.TickCount - lastConversionTick > 2000 && !ME.HasAuraById((int)Auras.Conversion))
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Conversion);
				lastConversionTick = Environment.TickCount;
                return;
            }
			
			
		if (TARGET.Health >= 1 && ME.InCombat)
		{
		
					if (DetectKeyPress.GetKeyState(DetectKeyPress.Alt) < 0)
                {
                    if (AI.Controllers.Spell.CanCast((int)Spells.DnD))
                    {
                        WoW.Internals.MouseController.RightClick();
                        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DnD);
                        WoW.Internals.MouseController.LockCursor();
                        WoW.Internals.MouseController.MoveMouse(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                        WoW.Internals.MouseController.LeftClick();
                        WoW.Internals.MouseController.UnlockCursor();
                    }

                    return;

                }
							//Engineering Gloves
			if (!ME.HasAuraById((int)Auras.SSprings) && Environment.TickCount - lastSSTick > 20000 )
		{
              Anthrax.WoW.Internals.ActionBar.PressSlot(0, 0);
			  Logger.WriteLine("Synapse Srpings Used!!!");
			  lastSSTick = Environment.TickCount;
              return;
          }	
				
		/////////////////////////////////////////////////////////////////////////////////////Unholy////////////////////////////////////////////////////////////////////////////////////////
		
		//Out Of Combat Pet Revive
		
		if (!WoW.Internals.ObjectManager.Pet.IsAlive && ME.HasAuraById((int)Auras.UnholyCheck) && !ME.IsMounted)
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SummonPet);
                return;
            }
			
							///Death and Decay on Alt Press
		if (DetectKeyPress.GetKeyState(DetectKeyPress.Alt) < 0)
                {
                    if (AI.Controllers.Spell.CanCast((int)Spells.DnD))
                    {
                        WoW.Internals.MouseController.RightClick();
                        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DnD);
                        WoW.Internals.MouseController.LockCursor();
                        WoW.Internals.MouseController.MoveMouse(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                        WoW.Internals.MouseController.LeftClick();
                        WoW.Internals.MouseController.UnlockCursor();
                    }

                    return;

                }

		//Rotation
		if (TARGET.Health >= 1 && ME.InCombat)
		{
        if (ME.HasAuraById((int)Auras.UnholyCheck))
		{
		
		
		///Death and Decay on Alt Press
		if (DetectKeyPress.GetKeyState(DetectKeyPress.Alt) < 0)
                {
                    if (AI.Controllers.Spell.CanCast((int)Spells.DnD))
                    {
                        WoW.Internals.MouseController.RightClick();
                        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DnD);
                        WoW.Internals.MouseController.LockCursor();
                        WoW.Internals.MouseController.MoveMouse(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                        WoW.Internals.MouseController.LeftClick();
                        WoW.Internals.MouseController.UnlockCursor();
                    }

                    return;

                }
				if(ME.HealthPercent <= CCSettings.Conversion && AI.Controllers.Spell.CanCast((int)Spells.Conversion) && ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) >= 10 
		&& Environment.TickCount - lastConversionTick > 2000 && !ME.HasAuraById((int)Auras.Conversion))
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Conversion);
				lastConversionTick = Environment.TickCount;
                return;
            }
		//Dots//
		
		if (!TARGET.HasAuraById((int)Auras.BloodPlague) && AI.Controllers.Spell.CanCast((int)Spells.PlagueStrike)
		|| TARGET.HasAuraById((int)Auras.BloodPlague) && AI.Controllers.Spell.CanCast((int)Spells.PlagueStrike) && ME.Auras.Where(x => x.SpellId == (int)Auras.BloodPlague && x.TimeLeft <= 3000).Any() )
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.PlagueStrike);
                return;
            }	
			
		//Dark Transformation
		if (AI.Controllers.Spell.CanCast((int)Spells.DarkT) && ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Unholy) >= 1)
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DarkT);
                    return;
                }		
		
		//BloodBoil Spam
		
		if (AI.Controllers.Spell.CanCast((int)Spells.BloodBoil))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BloodBoil);
                    return;
                }

				
		//Runic Power Cap
		if (ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) >= 89 &&
                    AI.Controllers.Spell.CanCast((int)Spells.DeathCoil))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DeathCoil);
                    return;
                }
		
		//Death Coil
		if (ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) >= 32 &&
                    AI.Controllers.Spell.CanCast((int)Spells.DeathCoil))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DeathCoil);
                    return;
                }
				
		//Horn of Winter
		if (ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) < 90 &&
                    AI.Controllers.Spell.CanCast((int)Spells.HoW))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HoW);
                    return;
                }		
		
		//BloodTap
		if (ME.Auras.Where(x => x.SpellId == (int)Auras.BloodTap && x.StackCount >= 5).Any() && AI.Controllers.Spell.CanCast((int)Spells.BloodTap))
		{
			if (ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Unholy) <= 1
			|| ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Frost) <= 1
			|| ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Blood) <= 1
			|| ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Death) < 1)
			{
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BloodTap);
			return;
			}
		}
		//Death Coil
		if (AI.Controllers.Spell.CanCast((int)Spells.DeathCoil))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DeathCoil);
                    return;
                }	

		//Scourge Strike if less then 90 runic power
		if (ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) < 90 &&
                    AI.Controllers.Spell.CanCast((int)Spells.IcyTouch) && ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Frost) >= 1)
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.IcyTouch);
                    return;
                }	
				
		//Scourge Strike if less then 90 runic power
		if (ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) < 90 
		&& AI.Controllers.Spell.CanCast((int)Spells.ScourgeStrike) && ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Unholy) > 1
		|| Anthrax.WoW.Internals.Cooldown.GetCooldowns().Where(x => x.SpellId == (int)Spells.DnD).First().TimeLeft >= 7000 
		&& ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) < 90 
		&& AI.Controllers.Spell.CanCast((int)Spells.ScourgeStrike) 
		&& ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Unholy) >= 1)
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ScourgeStrike);
                    return;
                }				
		
		}//end Combat
		}//end spec check

		////////////////////////////////////////////////////////////////////////////////////Blood Spec//////////////////////////////////////////////////////////////////////////////////////
		if (TARGET.Health >= 1 && ME.InCombat)
		{
        if (ME.HasAuraById((int)Auras.BloodCheck))
		{
		
		if(ME.HealthPercent <= CCSettings.Conversion && AI.Controllers.Spell.CanCast((int)Spells.Conversion) && ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) >= 10 
		&& Environment.TickCount - lastConversionTick > 2000 && !ME.HasAuraById((int)Auras.Conversion))
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Conversion);
				lastConversionTick = Environment.TickCount;
                return;
            }
			
            // Bone Shield
            if ((!ME.HasAuraById((int)Auras.BoneShield) ||
                ME.Auras.Where(x => x.SpellId == (int)Auras.BoneShield && x.StackCount <= 1).Any()) &&
            AI.Controllers.Spell.CanCast((int)Spells.BoneShield))
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BoneShield);
                return;
            }
			
			if (DetectKeyPress.GetKeyState(DetectKeyPress.Alt) < 0)
                {
                    if (AI.Controllers.Spell.CanCast((int)Spells.DnD))
                    {
                        WoW.Internals.MouseController.RightClick();
                        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DnD);
                        WoW.Internals.MouseController.LockCursor();
                        WoW.Internals.MouseController.MoveMouse(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                        WoW.Internals.MouseController.LeftClick();
                        WoW.Internals.MouseController.UnlockCursor();
                    }

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

             // Deseases
			if (!TARGET.HasAuraById((int)Auras.FrostFever) && AI.Controllers.Spell.CanCast((int)Spells.Outbreak) || !TARGET.HasAuraById((int)Auras.BloodPlague) && AI.Controllers.Spell.CanCast((int)Spells.Outbreak))
				{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Outbreak);
                return;
				}
			//Icy Touch
				if (!TARGET.HasAuraById((int)Auras.FrostFever) && AI.Controllers.Spell.CanCast((int)Spells.IcyTouch) 
				&& !TARGET.Auras.Where(x => x.SpellId == ((int)Auras.FrostFever) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any())
                        {
                            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.IcyTouch);
                            return;
                        }
			//Plague Strike
                 if (!TARGET.HasAuraById((int)Auras.BloodPlague) 
				 && AI.Controllers.Spell.CanCast((int)Spells.PlagueStrike)
				 && !TARGET.Auras.Where(x => x.SpellId == ((int)Auras.BloodPlague) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any())
                        {
                            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.PlagueStrike);
                            return;
                        }

			//DeathStrike	
				if (!ME.HasAuraById((int)Auras.BloodShield) && TARGET.HasAuraById((int)Auras.BloodPlague) && AI.Controllers.Spell.CanCast((int)Spells.DeathStrike) 
				|| ME.HasAuraById((int)Auras.BloodShield) && ME.Auras.Where(x => x.SpellId == (int)Auras.BloodShield && x.TimeLeft <= 7000).Any() && AI.Controllers.Spell.CanCast((int)Spells.DeathStrike))
				{
					WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DeathStrike);
					return;
				}
				
			//Blood Boil	
                if ((TARGET.Auras.Where(x => x.SpellId == (int)Auras.FrostFever && x.TimeLeft < 8000).Any() && AI.Controllers.Spell.CanCast((int)Spells.BloodBoil)
				&& TARGET.Auras.Where(x => x.SpellId == ((int)Auras.FrostFever) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any()
				|| TARGET.Auras.Where(x => x.SpellId == (int)Auras.BloodPlague && x.TimeLeft < 8000).Any()) && AI.Controllers.Spell.CanCast((int)Spells.BloodBoil)
				&& TARGET.Auras.Where(x => x.SpellId == ((int)Auras.BloodPlague) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any()
				|| ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Blood) >= 2 && TARGET.HasAuraById((int)Auras.BloodPlague)
				|| ME.HasAuraById((int)Auras.CS) && TARGET.Position.Distance3DFromPlayer < 13
				&& TARGET.Auras.Where(x => x.SpellId == ((int)Auras.BloodPlague) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any()//CS Proc
				|| AI.Controllers.Spell.CanCast((int)Spells.BloodBoil) && ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Blood) >= 1
				&& TARGET.Auras.Where(x => x.SpellId == ((int)Auras.BloodPlague) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any())
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BloodBoil);
                    return;
                }
				
			//SoulReaper
				if (TARGET.HealthPercent <= 35 && AI.Controllers.Spell.CanCast((int)Spells.SoulReaperBlood) 
				&& ME.GetReadyRuneCountByType(WoW.Classes.ObjectManager.WowLocalPlayer.WowRuneType.Blood) >= 1
				&& !TARGET.HasAuraById((int)Auras.SRB))
				{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SoulReaperBlood);
                    return;
                }
				
			//Bloodtap
			if (ME.Auras.Where(x => x.SpellId == (int)Auras.BloodTap && x.StackCount >= 5).Any() && AI.Controllers.Spell.CanCast((int)Spells.BloodTap))
			{
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BloodTap);
			return;
			}

			// Rune Strike
                if (ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) > 29 &&
                    AI.Controllers.Spell.CanCast((int)Spells.RuneStrike))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.RuneStrike);
                    return;
                }
				
			// Horn of Winter
            if (AI.Controllers.Spell.CanCast((int)Spells.HoW))
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HoW);
                return;
            }
		}
        }
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////FROST SPEC//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		
		
		if (ME.HasAuraById((int)Auras.FrostCheck))
		{
		
				///Death and Decay on Alt Press
		if (DetectKeyPress.GetKeyState(DetectKeyPress.Alt) < 0)
                {
                    if (AI.Controllers.Spell.CanCast((int)Spells.DnD))
                    {
                        WoW.Internals.MouseController.RightClick();
                        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DnD);
                        WoW.Internals.MouseController.LockCursor();
                        WoW.Internals.MouseController.MoveMouse(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                        WoW.Internals.MouseController.LeftClick();
                        WoW.Internals.MouseController.UnlockCursor();
                    }

                    return;

                }
				if(ME.HealthPercent <= CCSettings.Conversion && AI.Controllers.Spell.CanCast((int)Spells.Conversion) && ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.RunicPower) >= 10 
		&& Environment.TickCount - lastConversionTick > 2000 && !ME.HasAuraById((int)Auras.Conversion))
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Conversion);
				lastConversionTick = Environment.TickCount;
                return;
            }
			
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
			
			if (TARGET.Position.Distance3DFromPlayer < 15 && AI.Controllers.Spell.CanCast((int)Spells.UnholyBlight))
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
 public class DetectKeyPress
    {
        public static int Shift = 0x10;
        public static int Ctrl = 0x11;
        public static int Alt = 0x12;

        public static int Z = 0x5A;
        public static int X = 0x58;
        public static int C = 0x43;
 
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern short GetKeyState(int virtualKeyCode);

    }
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
            if (!Cooldown.IsGlobalCooldownActive)
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
		try
            {
                XmlSerializer xs = new XmlSerializer(typeof(Settings));

                using (StreamReader rd = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "\\Combats\\ADruid.xml"))
                {
                    CCSettings = xs.Deserialize(rd) as Settings;
                }
            }
            catch
            {

            }
            finally
            {
                if (CCSettings == null)
                    CCSettings = new Settings();
            }
            Logger.WriteLine("CustomClass " + Name + " Loaded");
        }

        public override void OnUnload() //This is called when the Customclass is unloaded in SPQR
        {
		            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(Settings));
                using (StreamWriter wr = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\Combats\\nrgdRET.xml"))
                {
                    xs.Serialize(wr, CCSettings);
                }
            }
            catch { }
 
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
		
		public override object SettingsProperty
        {
            get
            {
                return CCSettings;
            }
        }

		[Serializable]
    public class Settings
    {
        public int Conversion = 80;
		public int RuneTap = 70;
		
	//Healing Settings
		[XmlIgnore]
        [CategoryAttribute("All Specs Survival Settings"),
        DisplayName("Conversion Hp Limit"), DefaultValueAttribute(80)]
        public int _Conversion
        {
            get
            {
                return Conversion;
            }
            set
            {
                Conversion = value;
            }
        }
		[XmlIgnore]
        [CategoryAttribute("Blood Spec Survival Settings"),
        DisplayName("RuneTap Hp Limit"), DefaultValueAttribute(60)]
        public int _RuneTap
        {
            get
            {
                return RuneTap;
            }
            set
            {
                RuneTap = value;
            }
        }

	}
		
		
		

    }
}