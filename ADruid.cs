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
    class Druid : Modules.ICombat
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
            get { return "A Druid by Koha"; }                      //This is the name displayed in SPQR's Class selection DropdownList
        }

        #region enums
        internal enum Spells : int                      //This is a convenient list of all spells used by our combat routine
        {                                               //you can have search on wowhead.com for spell name, and get the id in url
            Wrath = 5176,
			Starfire = 2912,
            Moonfire = 8921,
			Sunfire = 93402,
			Starfall = 48505,
            Typhoon = 132469,
			Starsurge = 78674,
            MightyBlast = 5211,
            ForceOfNature = 106737,
            Innervate = 29166,
            HealingTouch = 5185,
            Rejuvenation = 774,
            Regrowth = 8936,
            Swiftmend = 18562,
            Tranquility = 740,
            NaturesVigil = 124974,
            Barkskin = 22812,
            CenarionWard = 102351,
            Renewal = 108238,
            MightOfUrsoc = 106922,
			Thrash = 77758,
			Shred = 5221,
			SavageRoar = 52610,
			Rip = 1079,
			Rake = 1822,
			FeroBite = 22568,
			Mangle = 33917,
			MangleBear = 33878,
			Mangle3 = 33876,
			FF = 770,
			TigerFury = 5217,
			Ravage = 6785,
			Pounce = 9005,
			CatForm = 768,
			BearForm = 5487,
			SavageD = 62606,
			FrenzyRegen = 22842,
			Lacerate = 33745,
			Maul = 6807,
			Swipe = 779,
			SwipeBear = 106785,
			FoN = 106737,
        }

        internal enum Auras : int                       //This is another convenient list of Auras used in our combat routine
        {												//you can have those in wowhead.com (again) and get the id in url
			Moonfire = 8921,
			Sunfire = 93402,
			ShootingStars = 93400,
            Rejuvenation = 774,
			FF = 770,
            Regrowth = 8936,
            Barkskin = 22812,
			Rake = 1822,
			Rip = 1079,
			SavageRoar = 127538,
			PredSwiftness = 69369,
			Thrash = 106830,
			Prowl = 5215,
			CatForm = 768,
			BearForm = 5487,
			TAC = 135286,
			DoC = 145162,
			SavageD = 132402,
			Solar = 48517,
			Lunar = 48518,
			Boomkin = 24858,
			EMSolar = 67484,
			EMLunar = 67483,
			DreamoC = 145152,
			ClearCast = 15700,
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
//{
//}
private void castNextSpellbySinglePriority(WowUnit TARGET)
{
	if (TARGET.Health >= 1 && ME.InCombat)
	{
	
	//Healing & Survival
	{
	if (ME.HealthPercent <= 60 && AI.Controllers.Spell.CanCast((int)Spells.FrenzyRegen))
			{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.FrenzyRegen);
				return;
			}
	if (ObjectManager.LocalPlayer.GetPower(Anthrax.WoW.Classes.ObjectManager.WowUnit.WowPowerType.Rage) >= 60 && AI.Controllers.Spell.CanCast((int)Spells.SavageD) && !ME.HasAuraById((int)Auras.SavageD))
			{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SavageD);
				return;
			}
	if (ME.HasAuraById((int)Auras.DoC))
			{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HealingTouch);
				return;
			}

			
		if (TARGET.Position.Distance3DFromPlayer < 7)
		{
		//Mangle!!!
			if (AI.Controllers.Spell.CanCast((int)Spells.MangleBear))
			{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.MangleBear);
				return;
			}
		//Maul With Procs
			if (AI.Controllers.Spell.CanCast((int)Spells.Maul))
			{
			if (ObjectManager.LocalPlayer.GetPower(Anthrax.WoW.Classes.ObjectManager.WowUnit.WowPowerType.Rage) >= 90 || ME.HasAuraById((int)Auras.TAC) && ME.HasAuraById((int)Auras.SavageD))
			{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Maul);
				return;
			}
			}
			
		//Thrash Debuff
			if (AI.Controllers.Spell.CanCast((int)Spells.Thrash))
			{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Thrash);
				return;
			}
		//FaerieFire
			if (!TARGET.HasAuraById((int)Auras.FF) && AI.Controllers.Spell.CanCast((int)Spells.FF) || AI.Controllers.Spell.CanCast((int)Spells.FF))
			{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.FF);
				return;
			}
			
		//Lacerate
			if (AI.Controllers.Spell.CanCast((int)Spells.Lacerate))
			{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Lacerate);
				return;
			}			
			
			
			
			
		}
    }
}}
#endregion

#region AOE>4 rotation
 private void castNextSpellbyAOEPriority(WowUnit TARGET)
{
	if (TARGET.Health >= 1 && ME.InCombat)
	{
	
	//Healing & Survival
	{
	if (ME.HealthPercent <= 60 && AI.Controllers.Spell.CanCast((int)Spells.FrenzyRegen))
			{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.FrenzyRegen);
				return;
			}
	if (ObjectManager.LocalPlayer.GetPower(Anthrax.WoW.Classes.ObjectManager.WowUnit.WowPowerType.Rage) >= 60 && AI.Controllers.Spell.CanCast((int)Spells.SavageD) && !ME.HasAuraById((int)Auras.SavageD))
			{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SavageD);
				return;
			}
	if (ME.HasAuraById((int)Auras.DoC))
			{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HealingTouch);
				return;
			}
	
		if (TARGET.Position.Distance3DFromPlayer < 8)
		{
			if (AI.Controllers.Spell.CanCast((int)Spells.Thrash) && !TARGET.HasAuraById((int)Auras.Thrash))
			{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Thrash);
				return;
			}
		
			//Swipe
			if (AI.Controllers.Spell.CanCast((int)Spells.Swipe))
			{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Swipe);
				return;
			}
		
		//Mangle!!!
			if (AI.Controllers.Spell.CanCast((int)Spells.MangleBear))
			{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.MangleBear);
				return;
			}
			
			
		//Maul With Procs
			if (AI.Controllers.Spell.CanCast((int)Spells.Maul))
			{
			if (ObjectManager.LocalPlayer.GetPower(Anthrax.WoW.Classes.ObjectManager.WowUnit.WowPowerType.Rage) >= 90 || ME.HasAuraById((int)Auras.TAC) && ME.HasAuraById((int)Auras.SavageD))
			{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Maul);
				return;
			}
			}
			
		//Thrash Debuff
			if (AI.Controllers.Spell.CanCast((int)Spells.Thrash))
			{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Thrash);
				return;
			}
			
			
			
			
		}
    }
}}
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