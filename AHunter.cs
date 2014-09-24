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
// nrgdret AOE/Single Target Rotation Used in this. (well done)                                                                                             //
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////


namespace Anthrax
{
    class Hunter : Modules.ICombat
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
            get { return "A Hunter by Koha"; }                      //This is the name displayed in SPQR's Class selection DropdownList
        }

        #region enums
        internal enum Spells : int                      //This is a convenient list of all spells used by our combat routine
        {                                               //you can have search on wowhead.com for spell name, and get the id in url
            CobraShot = 77767,
            ArcaneShot = 3044,
            SerpentSting = 1978,
            KillCommand = 34026,
            DireBeast = 120679,
			MultiShot = 2643,
            KillShot = 53351,
			GlaiveToss= 117050,
			FocusFire = 82692,
			BestialWrath = 19574,
			RapidFire = 3045,
			MendPet = 136,
            Fervor = 82726,
			AMurderOfCrows = 131894,
			BlackArrow = 3674,
			ExplosiveShot = 53301,
			Misdirect = 34477,
			Pet1 = 883,
			Pet2 = 83242,
			Pet3 = 83243,
			RevivePet = 982,
			Etrap = 82939,
			Stampede = 121818,
        }

        internal enum Auras : int                       //This is another convenient list of Auras used in our combat routine
        {												//you can have those in wowhead.com (again) and get the id in url
            SerpentSting = 118253,//1978,
			Frenzy = 19615,
			TheBeastWithin = 34471,
			ES = 53301,
			LaL = 56453,
			BlackArrow = 3674,
			TotH = 34720,
			MendPet = 136,
			Misdirection = 35079,
			FD = 5384,
			Survival = 118976,
			T162P = 144637,
			SurvivalCheck = 118976,
			BMCheck = 34954,
			FocusFire = 82692,
}

        public enum CallPetSpells : int
        {
            CallPet1 = 883,
            CallPet2 = 83242,
            CallPet3 = 43243,
            CallPet4 = 83244,
            CallPet5 = 83245,
        }
		public enum Macros : int
		{
			Misdirect = 1
			
		}
#endregion

public bool CallPet()
        {
            
            /*
             *  Attempt to use "Call Pet" spells.
             */

            List<WoW.Classes.WowActionBarSlot> callPetSpells = WoW.Internals.ActionBar.GetFilledSlots().Where(x => x.IsSpell && Enum.IsDefined(typeof(CallPetSpells), x.ActionId)).ToList();

            if (callPetSpells.Count > 0)
            {

                WoW.Internals.ActionBar.PressSlot(callPetSpells.First().BarIndex, callPetSpells.First().SlotIndex);

                int startWaitPetTick = Environment.TickCount;

                while (!WoW.Internals.ObjectManager.Pet.IsValid ||
                       !WoW.Internals.ObjectManager.Pet.IsAlive)
                {

                    if (Environment.TickCount - startWaitPetTick > 3000)
                        break;

                    System.Threading.Thread.Sleep(100);
                }

                return WoW.Internals.ObjectManager.Pet.IsValid && WoW.Internals.ObjectManager.Pet.IsAlive;
            }

            /*
             *  Attempt to use "Call Pet" flyout.
             */

            List<WoW.Classes.WowActionBarSlot> callPetFlyouts = WoW.Internals.ActionBar.GetFilledSlots().Where(x => x.IsFlyout && x.ActionId == 9).ToList();

            if (callPetFlyouts.Count > 0)
            {

                WoW.Classes.WowActionBarSlot actionSlot = callPetFlyouts.First();

                WoW.Internals.ActionBar.PressSlot(actionSlot.BarIndex, actionSlot.SlotIndex);

                System.Threading.Thread.Sleep(1000);

                WoW.Classes.Frames.WowFrame SpellFlyoutButton = WoW.Internals.UIFrame.GetFrameByName("SpellFlyoutButton1");

                if (SpellFlyoutButton.IsValid &&
                    SpellFlyoutButton.IsVisible)
                {

                    SpellFlyoutButton.LeftClick();

                    int startWaitPetTick = Environment.TickCount;

                    while (!WoW.Internals.ObjectManager.Pet.IsValid ||
                           !WoW.Internals.ObjectManager.Pet.IsAlive)
                    {

                        if (Environment.TickCount - startWaitPetTick > 3000)
                            break;

                        System.Threading.Thread.Sleep(100);
                    }

                }
            }

            return WoW.Internals.ObjectManager.Pet.IsValid && WoW.Internals.ObjectManager.Pet.IsAlive;

        }

        public void PleaseBringMyPetBack()
        {

            if (!CallPet())
            {

                Logger.WriteLine("Unable to call pet, attempt resurrect ...");

                AI.Controllers.Spell.Cast((int)Spells.RevivePet, null);

                int startWaitPetTick = Environment.TickCount;

                while (!WoW.Internals.ObjectManager.Pet.IsValid ||
                       !WoW.Internals.ObjectManager.Pet.IsAlive)
                {

                    if (Environment.TickCount - startWaitPetTick > 3000)
                        break;

                    System.Threading.Thread.Sleep(100);
                }

            }

        }
		

		
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
//public override void OnCombat(WoW.Classes.ObjectManager.WowUnit unit)
//{}

private int lastSSTick = 0;
private int lastCSTick = 0;
private int lastMDTick = 0;


private void castNextSpellbySinglePriority(WowUnit TARGET)
{

	var Focus = ObjectManager.LocalPlayer.GetPower(WoW.Classes.ObjectManager.WowUnit.WowPowerType.Focus);
	var IsCasting = ObjectManager.LocalPlayer.IsCasting;
	var Pet = ObjectManager.Pet;

if (TARGET.Health >= 1 && ME.InCombat && !IsCasting)
{ //Combat Check

if (ME.UnitsAttackingMe.Count >= 1 && AI.Controllers.Spell.CanCast((int)Spells.Misdirect) && !ME.HasAuraById((int)Auras.Misdirection) && Environment.TickCount - lastMDTick > 2000 && Pet.Health >= 1)
	{
              Anthrax.WoW.Internals.ActionBar.PressSlot(0, 0);
			  Logger.WriteLine("Casting Misdirect!!!");
			  lastMDTick = Environment.TickCount;
              return;
          }

												///////////////////////////Beast Master////////////////////
if (ME.HasAuraById((int)Auras.BMCheck))
{ //Spec Check		

///Pet Controls

            if (!WoW.Internals.ObjectManager.Pet.IsValid || 
                WoW.Internals.ObjectManager.Pet.IsDead)
            {
                PleaseBringMyPetBack();
            }
		if (DetectKeyPress.GetKeyState(DetectKeyPress.Alt) < 0)
                {
                    if (AI.Controllers.Spell.CanCast((int)Spells.Etrap)
                         && !IsCasting)
                    {
                        WoW.Internals.MouseController.RightClick();
                        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Etrap);
                        WoW.Internals.MouseController.LockCursor();
                        WoW.Internals.MouseController.MoveMouse(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                        WoW.Internals.MouseController.LeftClick();
                        WoW.Internals.MouseController.UnlockCursor();
                    }

                    return;

                }
			
	if (Pet.HealthPercent < 80 && !Pet.HasAuraById((int)Auras.MendPet) && AI.Controllers.Spell.CanCast((int)Spells.MendPet) && Pet.IsAlive)
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.MendPet);
            return;
        }
		
//	if (ME.UnitsAttackingMe >= 1 && !ME.HasAuraById((int)Auras.Misdirection) && AI.Controllers.Spell.CanCast((int)Spells.Misdirect))
//			{
//               Anthrax.WoW.Internals.Chat.SendMessage("/click BT4Button9");
//               return;
//            }

		
	if (ME.HasAuraById((int)Auras.T162P) && AI.Controllers.Spell.CanCast((int)Spells.RapidFire))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.RapidFire);
                return;
            }
	if (TARGET.MaxHealth >= 30000000 && AI.Controllers.Spell.CanCast((int)Spells.Stampede))
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Stampede);
                return;
            }
				//Arcane Shot Capp
		if(Focus >= 90 && AI.Controllers.Spell.CanCast((int)Spells.ArcaneShot))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ArcaneShot);
                return;
            }
			//Serpent Sting
		if(!TARGET.HasAuraById((int)Auras.SerpentSting) && AI.Controllers.Spell.CanCast((int)Spells.SerpentSting) && Environment.TickCount - lastSSTick > 2000)
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SerpentSting);
				lastSSTick = Environment.TickCount;
                return;
            }	
			
		//Focus Fire
		if(!ME.HasAuraById((int)Auras.FocusFire) && AI.Controllers.Spell.CanCast((int)Spells.FocusFire) && ME.Auras.Where(x => x.SpellId == (int)Auras.Frenzy && x.StackCount >= 5).Any())
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.FocusFire);
                return;
            }
		//KillCommand
		if(AI.Controllers.Spell.CanCast((int)Spells.KillCommand)&& PET.Position.Distance3DFromPlayer < 25)
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.KillCommand);
                return;
            }		
			

			
		//KillShot
		if(TARGET.HealthPercent < 20 && AI.Controllers.Spell.CanCast((int)Spells.KillShot))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.KillShot);
                return;
            }	
			
		//Glaive Toss
		if(AI.Controllers.Spell.CanCast((int)Spells.GlaiveToss))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.GlaiveToss);
                return;
            }
		//Dire Beast
		if(AI.Controllers.Spell.CanCast((int)Spells.DireBeast))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DireBeast);
                return;
            }
				
			
		//Arcane Shot
		if(AI.Controllers.Spell.CanCast((int)Spells.ArcaneShot) && !AI.Controllers.Spell.CanCast((int)Spells.AMurderOfCrows) && !AI.Controllers.Spell.CanCast((int)Spells.KillCommand)
		&& !AI.Controllers.Spell.CanCast((int)Spells.DireBeast) && !AI.Controllers.Spell.CanCast((int)Spells.GlaiveToss) && Focus > 35)
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ArcaneShot);
                return;
            }
			
				//CobraShot
		if(Focus <= 40 && AI.Controllers.Spell.CanCast((int)Spells.CobraShot) && Environment.TickCount - lastCSTick > 1800)
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.CobraShot);
				lastCSTick = Environment.TickCount;
                return;
            }

}//Spec Check		
												///////////////////////////Survival////////////////////////
if (ME.HasAuraById((int)Auras.SurvivalCheck))
{ //Spec Check


///Pet Controls

            if (!WoW.Internals.ObjectManager.Pet.IsValid || 
                WoW.Internals.ObjectManager.Pet.IsDead)
            {
                PleaseBringMyPetBack();
            }
		if (DetectKeyPress.GetKeyState(DetectKeyPress.Alt) < 0)
                {
                    if (AI.Controllers.Spell.CanCast((int)Spells.Etrap)
                         && !IsCasting)
                    {
                        WoW.Internals.MouseController.RightClick();
                        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Etrap);
                        WoW.Internals.MouseController.LockCursor();
                        WoW.Internals.MouseController.MoveMouse(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                        WoW.Internals.MouseController.LeftClick();
                        WoW.Internals.MouseController.UnlockCursor();
                    }

                    return;

                }
			
	if (Pet.HealthPercent < 80 && !Pet.HasAuraById((int)Auras.MendPet) && AI.Controllers.Spell.CanCast((int)Spells.MendPet) && Pet.IsAlive)
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.MendPet);
            return;
        }
		

			
		if (ME.HasAuraById((int)Auras.T162P) && AI.Controllers.Spell.CanCast((int)Spells.RapidFire))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.RapidFire);
                return;
            }
			
	if (TARGET.MaxHealth >= 30000000 && AI.Controllers.Spell.CanCast((int)Spells.Stampede))
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Stampede);
                return;
            }
		//A Murder Of Crows
		if(AI.Controllers.Spell.CanCast((int)Spells.AMurderOfCrows))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.AMurderOfCrows);
                return;
            }
			//Serpent Sting
		if(!TARGET.HasAuraById((int)Auras.SerpentSting) && AI.Controllers.Spell.CanCast((int)Spells.SerpentSting) && Environment.TickCount - lastSSTick > 2000)
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SerpentSting);
				lastSSTick = Environment.TickCount;
                return;
            }	
			
		//Arcane Shot Capp
		if(Focus >= 90 && AI.Controllers.Spell.CanCast((int)Spells.ArcaneShot))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ArcaneShot);
                return;
            }
			
			
		//Explosive Shot
		if(ME.HasAuraById((int)Auras.ES) && AI.Controllers.Spell.CanCast((int)Spells.ExplosiveShot)
		|| Focus >= 25 && AI.Controllers.Spell.CanCast((int)Spells.ExplosiveShot))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ExplosiveShot);
                return;
            }
		//Glaive Toss
		if(Focus >= 15 && AI.Controllers.Spell.CanCast((int)Spells.GlaiveToss))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.GlaiveToss);
                return;
            }	
		//KillShot
		if(TARGET.HealthPercent < 20 && AI.Controllers.Spell.CanCast((int)Spells.KillShot))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.KillShot);
                return;
            }
		//Dire Beast
		if(AI.Controllers.Spell.CanCast((int)Spells.DireBeast))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DireBeast);
                return;
            }
		//Black Arrow
		if(Focus >= 35 && AI.Controllers.Spell.CanCast((int)Spells.BlackArrow))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BlackArrow);
                return;
            }
		//Arcane Shot
		if(AI.Controllers.Spell.CanCast((int)Spells.ArcaneShot) && !AI.Controllers.Spell.CanCast((int)Spells.AMurderOfCrows) && !AI.Controllers.Spell.CanCast((int)Spells.BlackArrow)
		&& !AI.Controllers.Spell.CanCast((int)Spells.DireBeast) && !AI.Controllers.Spell.CanCast((int)Spells.GlaiveToss) && !AI.Controllers.Spell.CanCast((int)Spells.ExplosiveShot) && Focus > 35)
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ArcaneShot);
                return;
            }
			
		//CobraShot
		if(Focus <= 60 && AI.Controllers.Spell.CanCast((int)Spells.CobraShot) && Environment.TickCount - lastCSTick > 1700)
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.CobraShot);
				lastCSTick = Environment.TickCount;
                return;
            }		
	

	
} //End of Spec Check
} //Combat Check
} //End AoE Code
#endregion

#region AOE>4 rotation
 private void castNextSpellbyAOEPriority(WowUnit TARGET)
{
	var Focus = ObjectManager.LocalPlayer.GetPower(WoW.Classes.ObjectManager.WowUnit.WowPowerType.Focus);
	var IsCasting = ObjectManager.LocalPlayer.IsCasting;
	var Pet = ObjectManager.Pet;
		
	if (TARGET.Health >= 1 && ME.InCombat && !IsCasting)
{ //Combat Check

if (ME.UnitsAttackingMe.Count >= 1 && AI.Controllers.Spell.CanCast((int)Spells.Misdirect) && !ME.HasAuraById((int)Auras.Misdirection) && Environment.TickCount - lastMDTick > 2000 && Pet.Health >= 1)
	{
              Anthrax.WoW.Internals.ActionBar.PressSlot(0, 0);
			  Logger.WriteLine("Casting Misdirect!!!");
			  lastMDTick = Environment.TickCount;
              return;
          }

													///////////////////////////Beast Master////////////////////////
if (ME.HasAuraById((int)Auras.BMCheck))
{ //Spec Check


///Pet Controls

            if (!WoW.Internals.ObjectManager.Pet.IsValid || 
                WoW.Internals.ObjectManager.Pet.IsDead)
            {
                PleaseBringMyPetBack();
            }

				if (DetectKeyPress.GetKeyState(DetectKeyPress.Alt) < 0)
                {
                    if (AI.Controllers.Spell.CanCast((int)Spells.Etrap)
                         && !IsCasting)
                    {
                        WoW.Internals.MouseController.RightClick();
                        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Etrap);
                        WoW.Internals.MouseController.LockCursor();
                        WoW.Internals.MouseController.MoveMouse(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                        WoW.Internals.MouseController.LeftClick();
                        WoW.Internals.MouseController.UnlockCursor();
                    }

                    return;

                }
			
	if (Pet.HealthPercent < 80 && !Pet.HasAuraById((int)Auras.MendPet) && AI.Controllers.Spell.CanCast((int)Spells.MendPet) && Pet.IsAlive)
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.MendPet);
            return;
        }
			
		if (ME.HasAuraById((int)Auras.T162P) && AI.Controllers.Spell.CanCast((int)Spells.RapidFire))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.RapidFire);
                return;
            }
	
		//MultiShot
		if(AI.Controllers.Spell.CanCast((int)Spells.MultiShot))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.MultiShot);
                return;
            }
		
		//Glaive Toss
		if(Focus >= 15 && AI.Controllers.Spell.CanCast((int)Spells.GlaiveToss))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.GlaiveToss);
                return;
            }
			
					//KillShot
		if(TARGET.HealthPercent < 20 && AI.Controllers.Spell.CanCast((int)Spells.KillShot))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.KillShot);
                return;
            }
				//Dire Beast
		if(AI.Controllers.Spell.CanCast((int)Spells.DireBeast))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DireBeast);
                return;
            }
			
		//Focus Fire
		if(!ME.HasAuraById((int)Auras.FocusFire) && AI.Controllers.Spell.CanCast((int)Spells.FocusFire) && ME.Auras.Where(x => x.SpellId == (int)Auras.Frenzy && x.StackCount >= 5).Any())
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.FocusFire);
                return;
            }

			
		//CobraShot
		if(Focus <= 60 && AI.Controllers.Spell.CanCast((int)Spells.CobraShot) && Environment.TickCount - lastCSTick > 1700)
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.CobraShot);
				lastCSTick = Environment.TickCount;
                return;
            }			
	

	
} //End of Spec Check
													///////////////////////////Survival////////////////////////
if (ME.HasAuraById((int)Auras.SurvivalCheck))
{ //Spec Check


///Pet Controls

            if (!WoW.Internals.ObjectManager.Pet.IsValid || 
                WoW.Internals.ObjectManager.Pet.IsDead)
            {
                PleaseBringMyPetBack();
            }
		
		if (DetectKeyPress.GetKeyState(DetectKeyPress.Alt) < 0)
                {
                    if (AI.Controllers.Spell.CanCast((int)Spells.Etrap)
                         && !IsCasting)
                    {
                        WoW.Internals.MouseController.RightClick();
                        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Etrap);
                        WoW.Internals.MouseController.LockCursor();
                        WoW.Internals.MouseController.MoveMouse(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                        WoW.Internals.MouseController.LeftClick();
                        WoW.Internals.MouseController.UnlockCursor();
                    }

                    return;

                }
			
	if (Pet.HealthPercent < 80 && !Pet.HasAuraById((int)Auras.MendPet) && AI.Controllers.Spell.CanCast((int)Spells.MendPet) && Pet.IsAlive)
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.MendPet);
            return;
        }
			
		if (ME.HasAuraById((int)Auras.T162P) && AI.Controllers.Spell.CanCast((int)Spells.RapidFire))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.RapidFire);
                return;
            }
	
		//MultiShot
		if(AI.Controllers.Spell.CanCast((int)Spells.MultiShot))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.MultiShot);
                return;
            }
		
		//Explosive Shot
		if(ME.HasAuraById((int)Auras.ES) && AI.Controllers.Spell.CanCast((int)Spells.ExplosiveShot))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ExplosiveShot);
                return;
            }
		//Glaive Toss
		if(Focus >= 15 && AI.Controllers.Spell.CanCast((int)Spells.GlaiveToss))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.GlaiveToss);
                return;
            }
		//KillShot
		if(TARGET.HealthPercent < 20 && AI.Controllers.Spell.CanCast((int)Spells.KillShot))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.KillShot);
                return;
            }
				//Dire Beast
		if(AI.Controllers.Spell.CanCast((int)Spells.DireBeast))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DireBeast);
                return;
            }

			
		//CobraShot
		if(Focus <= 60 && AI.Controllers.Spell.CanCast((int)Spells.CobraShot) && Environment.TickCount - lastCSTick > 1700)
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.CobraShot);
				lastCSTick = Environment.TickCount;
                return;
            }			
	

	
} //End of Spec Check
	
	
	
	
	
	
} //Combat Check
} //End AoE Code
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
}}