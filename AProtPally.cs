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
    class Paladin : Modules.ICombat
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
            get { return "A Prot Pally by Koha"; }                      //This is the name displayed in SPQR's Class selection DropdownList
        }

        #region enums
        internal enum Spells : int                      //This is a convenient list of all spells used by our combat routine
        {		//you can have search on wowhead.com for spell name, and get the id in url
		AS = 31935,
		Con = 26573,
		CS = 35395,
		Jud = 20271,
		HoR = 53595,
		HoW = 24275,
		HP = 114165,
		EFlame = 114163,
		SoR = 53600,
		HW = 119072,
		RF = 25780,
		SealT = 31801,
		SealI = 20165,
		HolyP = 114165,
        }

        internal enum Auras : int                       //This is another convenient list of Auras used in our combat routine
        {												//you can have those in wowhead.com (again) and get the id in url
		ProtCheck = 53592,
		RF = 25780,
		SealI = 20165,
		SealT = 31801,
		EFlame = 114163,
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
//public override void OnCombat(WoW.Classes.ObjectManager.WowUnit unit)
//{}

private void castNextSpellbySinglePriority(WowUnit TARGET)
{

	var HolyPower = ObjectManager.LocalPlayer.GetPower(WoW.Classes.ObjectManager.WowUnit.WowPowerType.HolyPower);
	var IsCasting = ObjectManager.LocalPlayer.IsCasting;

if (TARGET.Health >= 1 && ME.InCombat)
{ //Combat Check
												///////////////////////////Survival////////////////////////
if (ME.HasAuraById((int)Auras.ProtCheck))
{ //Spec Check


			
		if (!ME.HasAuraById((int)Auras.RF) && AI.Controllers.Spell.CanCast((int)Spells.RF))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.RF);
                return;
            }
			
		if (!ME.HasAuraById((int)Auras.SealI) && AI.Controllers.Spell.CanCast((int)Spells.SealI))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SealI);
                return;
            }
			
		if (HolyPower >= 4 && AI.Controllers.Spell.CanCast((int)Spells.SoR) && ME.HasAuraById((int)Auras.EFlame))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SoR);
                return;
            }
		if (HolyPower >= 3 && AI.Controllers.Spell.CanCast((int)Spells.EFlame) && !ME.HasAuraById((int)Auras.EFlame))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.EFlame);
                return;
            }
		
		if (HolyPower < 5 && AI.Controllers.Spell.CanCast((int)Spells.CS))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.CS);
                return;
            }
		if (HolyPower < 5 && AI.Controllers.Spell.CanCast((int)Spells.Jud))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Jud);
                return;
            }			
		if (AI.Controllers.Spell.CanCast((int)Spells.AS))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.AS);
                return;
            }	
		if (AI.Controllers.Spell.CanCast((int)Spells.HW))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HW);
                return;
            }
		if (AI.Controllers.Spell.CanCast((int)Spells.HoW) && TARGET.HealthPercent < 20)
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HoW);
                return;
            }
		if (AI.Controllers.Spell.CanCast((int)Spells.Con))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Con);
                return;
            }
		if (AI.Controllers.Spell.CanCast((int)Spells.HolyP))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HolyP);
                return;
            }
	
} //End of Spec Check
} //Combat Check
} //End AoE Code
#endregion

#region AOE>4 rotation
 private void castNextSpellbyAOEPriority(WowUnit TARGET)
{
	var HolyPower = ObjectManager.LocalPlayer.GetPower(WoW.Classes.ObjectManager.WowUnit.WowPowerType.HolyPower);
	var IsCasting = ObjectManager.LocalPlayer.IsCasting;
		
	if (TARGET.Health >= 1 && ME.InCombat)
{ //Combat Check
													///////////////////////////Survival////////////////////////
if (ME.HasAuraById((int)Auras.ProtCheck))
{ //Spec Check
		if (!ME.HasAuraById((int)Auras.RF) && AI.Controllers.Spell.CanCast((int)Spells.RF))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.RF);
                return;
            }
			
		if (!ME.HasAuraById((int)Auras.SealI) && AI.Controllers.Spell.CanCast((int)Spells.SealI))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SealI);
                return;
            }
			
		if (HolyPower >= 4 && AI.Controllers.Spell.CanCast((int)Spells.SoR) && ME.HasAuraById((int)Auras.EFlame))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SoR);
                return;
            }
		if (HolyPower >= 3 && AI.Controllers.Spell.CanCast((int)Spells.EFlame) && !ME.HasAuraById((int)Auras.EFlame))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.EFlame);
                return;
            }
		
		if (HolyPower < 5 && AI.Controllers.Spell.CanCast((int)Spells.HoR))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HoR);
                return;
            }
		if (AI.Controllers.Spell.CanCast((int)Spells.Con))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Con);
                return;
            }
		if (AI.Controllers.Spell.CanCast((int)Spells.AS))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.AS);
                return;
            }	
		if (AI.Controllers.Spell.CanCast((int)Spells.HW))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HW);
                return;
            }
			
		if (HolyPower < 5 && AI.Controllers.Spell.CanCast((int)Spells.Jud))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Jud);
                return;
            }
			

		if (AI.Controllers.Spell.CanCast((int)Spells.HoW) && TARGET.HealthPercent < 20)
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HoW);
                return;
            }

		if (AI.Controllers.Spell.CanCast((int)Spells.HolyP))
		    {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HolyP);
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