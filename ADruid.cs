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
		Settings CCSettings = new Settings();
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
			StarFire = 2912,
            Moonfire = 8921,
			Sunfire = 93402,
			AstralC = 127663,
			StarFall = 48505,
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
			ThrashFeral = 106830,
			Shred = 5221,
			SavageRoar = 127538,
			Rip = 1079,
			Rake = 1822,
			FeroBite = 22568,
			Mangle = 33917,
			MangleBear = 33878,
			MangleFeral = 33876,
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
			SwipeFeral = 62078,
			SwipeBear = 106785,
			FoN = 106737,
			Prowl = 5215,
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
			SavageRoar = 62071,
			PredSwiftness = 69369,
			Thrash = 106830,
			ThrashFeralBear = 77758,
			FearlBearForm = 17057,
			CatForm = 768,
			Prowl = 5215,
			GlyphofS = 127540,
			BearForm = 5487,
			TAC = 135286,
			DoC = 145162,
			SavageD = 132402,
			EclipseSolar = 48517,
			EclipseLunar = 48518,
			Boomkin = 24858,
			EMSolar = 67484,
			EMLunar = 67483,
			DreamoC = 145152,
			ClearCast = 15700,
			AstralInsight = 145138,
			BoomkinCheck = 106732,
			TankCheck = 106734,
			FeralCheck = 106733,
			CC = 135700,
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

		var MyRage = ObjectManager.LocalPlayer.GetPower(WoW.Classes.ObjectManager.WowUnit.WowPowerType.Rage);
		var MyEnergy = ObjectManager.LocalPlayer.GetPower(Anthrax.WoW.Classes.ObjectManager.WowUnit.WowPowerType.Energy);
		
	if (TARGET.Health >= 1 && ME.InCombat)
	{
	
	
	/////////////////////////////////////////////////////////////////////////////TANK SPEC////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	if (ME.HasAuraById((int)Auras.TankCheck))
	{
	//Healing & Survival
	{
	if (ME.HealthPercent < CCSettings.FrenzyRegen && AI.Controllers.Spell.CanCast((int)Spells.FrenzyRegen))
			{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.FrenzyRegen);
				return;
			}
			
		if (ME.HealthPercent < CCSettings.Barkskin && AI.Controllers.Spell.CanCast((int)Spells.Barkskin))
			{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Barkskin);
				return;
			}
	if (MyRage >= 60 && AI.Controllers.Spell.CanCast((int)Spells.SavageD) && !ME.HasAuraById((int)Auras.SavageD))
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
    }
	
/////////////////////////////////////////////////////////////////////////////////Boomkin///////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////			
			
	if (ME.HasAuraById((int)Auras.BoomkinCheck))
	{
	//Healing & Survival
	if(TARGET.Position.Distance3DFromPlayer <= 40)
            {
                // Rejuvenation
                if (ME.HealthPercent <= 50 &&
                    !ME.HasAuraById((int)Auras.Rejuvenation) &&
                    AI.Controllers.Spell.CanCast((int)Spells.Rejuvenation))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Rejuvenation);
                    return;
                }
				
				if (ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.Mana) <= 80 && AI.Controllers.Spell.CanCast((int)Spells.Innervate))
					{
						WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Innervate);
						return;
					}
					
								                // Moonfire
                if ((!TARGET.HasAuraById((int)Auras.Moonfire) ||
                    TARGET.Auras.Where(x => x.SpellId == (int)Auras.Moonfire && x.TimeLeft <= 6000).Any()) &&
                    AI.Controllers.Spell.CanCast((int)Spells.Moonfire))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Moonfire);
                    return;
                }

                // Sunfire
                if ((!TARGET.HasAuraById((int)Auras.Sunfire) ||
                    TARGET.Auras.Where(x => x.SpellId == (int)Auras.Sunfire && x.TimeLeft <= 6000).Any()) &&
                    AI.Controllers.Spell.CanCast((int)Spells.Sunfire))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Sunfire);
                    return;
                }
				
				
                // StarFall
                if (AI.Controllers.Spell.CanCast((int)Spells.StarFall))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.StarFall);
                    return;
                }

                // Starsurge
                if (AI.Controllers.Spell.CanCast((int)Spells.Starsurge))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Starsurge);
                    return;
                }

				
				if (ME.HasAuraById((int)Auras.AstralInsight) && AI.Controllers.Spell.CanCast((int)Spells.AstralC))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.AstralC);
                    return;
                }
				
				 // Solar
            if (ME.HasAuraById((int)Auras.EclipseSolar))
			{
			
			
				if ((!TARGET.HasAuraById((int)Auras.Sunfire) ||
                    TARGET.Auras.Where(x => x.SpellId == (int)Auras.Sunfire && x.TimeLeft <= 6000).Any()) &&
                    AI.Controllers.Spell.CanCast((int)Spells.Sunfire))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Sunfire);
                    return;
                }
				
				if ((!TARGET.HasAuraById((int)Auras.Moonfire) ||
                    TARGET.Auras.Where(x => x.SpellId == (int)Auras.Moonfire && x.TimeLeft <= 6000).Any()) &&
                    AI.Controllers.Spell.CanCast((int)Spells.Moonfire))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Moonfire);
                    return;
                }
				
				
                if (AI.Controllers.Spell.CanCast((int)Spells.Wrath))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Wrath);
                    return;
                }
			}
                // Lunar
            if (ME.HasAuraById((int)Auras.EclipseLunar))
			{
				if ((!TARGET.HasAuraById((int)Auras.Moonfire) ||
                    TARGET.Auras.Where(x => x.SpellId == (int)Auras.Moonfire && x.TimeLeft <= 6000).Any()) &&
                    AI.Controllers.Spell.CanCast((int)Spells.Moonfire))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Moonfire);
                    return;
                }
				
				if ((!TARGET.HasAuraById((int)Auras.Sunfire) ||
                    TARGET.Auras.Where(x => x.SpellId == (int)Auras.Sunfire && x.TimeLeft <= 6000).Any()) &&
                    AI.Controllers.Spell.CanCast((int)Spells.Sunfire))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Sunfire);
                    return;
                }
				
				
                if (AI.Controllers.Spell.CanCast((int)Spells.StarFire))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.StarFire);
                    return;
                }
			}
                // Wrath
                if(AI.Controllers.Spell.CanCast((int)Spells.Wrath) && ME.HasAuraById((int)Auras.EMSolar))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Wrath);
                    return;
                }
				// Starfire
                if(AI.Controllers.Spell.CanCast((int)Spells.StarFire) && ME.HasAuraById((int)Auras.EMLunar))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.StarFire);
                    return;
                }
				
				if(AI.Controllers.Spell.CanCast((int)Spells.StarFire) && !ME.HasAuraById((int)Auras.EMLunar) && !ME.HasAuraById((int)Auras.EMLunar))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.StarFire);
                    return;
                }
                
            }
    }
	
	////////////////////////////////////////////////////////////////Feral/////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	if (ME.HasAuraById((int)Auras.FeralCheck))
	{
	
//	if (!ME.InCombat && ME.UnitsAttackingMeOrPet.Count == 0 && AI.Controllers.Spell.CanCast((int)Spells.Prowl) && !ME.HasAuraById((int)Auras.Prowl))
  //          {
 //               Logger.WriteLine("Enter stealth ...");
 //               WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Prowl);
 //           }
			
	if (ME.HasAuraById((int)Auras.GlyphofS) && !ME.HasAuraById((int)Auras.SavageRoar) && AI.Controllers.Spell.CanCast((int)Spells.SavageRoar))
			{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SavageRoar);
                    return;
            }
			
	if (ME.HasAuraById((int)Auras.PredSwiftness) && AI.Controllers.Spell.CanCast((int)Spells.HealingTouch))
			{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HealingTouch);
                    return;
            }
			
	if (TARGET.Auras.Where(x => x.SpellId == (int)Auras.Rip && x.TimeLeft > 6000).Any() && AI.Controllers.Spell.CanCast((int)Spells.FeroBite) && ME.ComboPoints > 4
	|| TARGET.HealthPercent < 25 && ME.ComboPoints > 4 && TARGET.HasAuraById((int)Auras.Rip) && AI.Controllers.Spell.CanCast((int)Spells.FeroBite))
		{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.FeroBite);
                    return;
        }
		
		
	if (!TARGET.HasAuraById((int)Auras.Rip) && AI.Controllers.Spell.CanCast((int)Spells.Rip) && ME.ComboPoints > 4
	|| TARGET.Auras.Where(x => x.SpellId == (int)Auras.Rip && x.TimeLeft < 3000).Any() && AI.Controllers.Spell.CanCast((int)Spells.Rip) && ME.ComboPoints > 4 && AI.Controllers.Spell.CanCast((int)Spells.Rip)
	|| ME.HasAuraById((int)Auras.DoC) && ME.Auras.Where(x => x.SpellId == (int)Auras.DoC && x.StackCount <= 2).Any() && ME.ComboPoints > 4 && AI.Controllers.Spell.CanCast((int)Spells.Rip)
	)
		{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Rip);
                    return;
        }
	
	if (!TARGET.HasAuraById((int)Auras.Rake) && AI.Controllers.Spell.CanCast((int)Spells.Rake) && ME.ComboPoints <= 4
	|| TARGET.Auras.Where(x => x.SpellId == (int)Auras.Rake && x.TimeLeft < 4000).Any() && AI.Controllers.Spell.CanCast((int)Spells.Rake)
	|| ME.HasAuraById((int)Auras.DoC) && ME.Auras.Where(x => x.SpellId == (int)Auras.DoC && x.StackCount >= 2).Any() && ME.ComboPoints < 5 && AI.Controllers.Spell.CanCast((int)Spells.Rake))
		{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Rake);
                    return;
        }
		
	if (!TARGET.HasAuraById((int)Auras.Thrash) && AI.Controllers.Spell.CanCast((int)Spells.ThrashFeral) && TARGET.HasAuraById((int)Auras.Rip) && TARGET.HasAuraById((int)Auras.Rake) && ME.ComboPoints > 3)
		{
		WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ThrashFeral);
		}
	
	if (MyEnergy <= 30 && AI.Controllers.Spell.CanCast((int)Spells.TigerFury))
		{
		WoW.Internals.ActionBar.ExecuteSpell((int)Spells.TigerFury);
		}
		
	if (!TARGET.HasAuraById((int)Auras.FF) && AI.Controllers.Spell.CanCast((int)Spells.FF))
		{
		WoW.Internals.ActionBar.ExecuteSpell((int)Spells.FF);
		}
		
		
	if (ME.ComboPoints < 5 && AI.Controllers.Spell.CanCast((int)Spells.MangleFeral))
		{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.MangleFeral);
                    return;
        }
	
	}
	

//////////////////////////////////////////////////////////////////////3	
}////////////////////////End Single Target Rotation///////////////////
}/////////////////////////////////////////////////////////////////////
#endregion

#region AOE>4 rotation
 private void castNextSpellbyAOEPriority(WowUnit TARGET)
{

		var MyRage = ObjectManager.LocalPlayer.GetPower(WoW.Classes.ObjectManager.WowUnit.WowPowerType.Rage);
		var MyEnergy = ObjectManager.LocalPlayer.GetPower(Anthrax.WoW.Classes.ObjectManager.WowUnit.WowPowerType.Energy);
		
	if (TARGET.Health >= 1 && ME.InCombat)
	{
	if (ME.HasAuraById((int)Auras.TankCheck))
	{
	//Healing & Survival
	{
	if (ME.HealthPercent < CCSettings.FrenzyRegen && AI.Controllers.Spell.CanCast((int)Spells.FrenzyRegen))
			{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.FrenzyRegen);
				return;
			}
			
		if (ME.HealthPercent < CCSettings.Barkskin && AI.Controllers.Spell.CanCast((int)Spells.Barkskin))
			{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Barkskin);
				return;
			}
	if (MyRage >= 60 && AI.Controllers.Spell.CanCast((int)Spells.SavageD) && !ME.HasAuraById((int)Auras.SavageD))
			{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SavageD);
				return;
			}
	if (ME.HasAuraById((int)Auras.DoC))
			{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HealingTouch);
				return;
			}

	
		if (TARGET.Position.Distance3DFromPlayer < 5)
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
    }
	
	
	////////////////////////////////////////////////////////////////FERAL//////////////////////////////////////////////
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	if (ME.HasAuraById((int)Auras.FeralCheck))
	{
	
	if (ME.HasAuraById((int)Auras.GlyphofS) && !ME.HasAuraById((int)Auras.SavageRoar) && AI.Controllers.Spell.CanCast((int)Spells.SavageRoar))
			{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SavageRoar);
                    return;
            }
			
	if (ME.HasAuraById((int)Auras.PredSwiftness) && AI.Controllers.Spell.CanCast((int)Spells.HealingTouch))
			{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HealingTouch);
                    return;
            }
			
	if (!TARGET.HasAuraById((int)Auras.Thrash) && AI.Controllers.Spell.CanCast((int)Spells.ThrashFeral)
	|| ME.HasAuraById((int)Auras.DoC) && AI.Controllers.Spell.CanCast((int)Spells.ThrashFeral) && ME.Auras.Where(x => x.SpellId == (int)Auras.DoC && x.StackCount >= 2).Any())
		{
		WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ThrashFeral);
		}
	
	if (MyEnergy <= 30 && AI.Controllers.Spell.CanCast((int)Spells.TigerFury))
		{
		WoW.Internals.ActionBar.ExecuteSpell((int)Spells.TigerFury);
		}
	
	
	if (ME.ComboPoints < 5 && AI.Controllers.Spell.CanCast((int)Spells.SwipeFeral) && MyEnergy > 50
	|| ME.HasAuraById((int)Auras.CC) && AI.Controllers.Spell.CanCast((int)Spells.SwipeFeral))
		{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SwipeFeral);
                    return;
        }
	
	if (!TARGET.HasAuraById((int)Auras.Rip) && AI.Controllers.Spell.CanCast((int)Spells.Rip) && ME.ComboPoints > 4)
		{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Rip);
                    return;
        }
	
	if (!TARGET.HasAuraById((int)Auras.Rake) && AI.Controllers.Spell.CanCast((int)Spells.Rake) && ME.ComboPoints <= 4
	|| TARGET.Auras.Where(x => x.SpellId == (int)Auras.Rake && x.TimeLeft < 4000).Any() && AI.Controllers.Spell.CanCast((int)Spells.Rake))
		{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Rake);
                    return;
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
        public int FrenzyRegen = 80;
        public int Barkskin = 70;

        [XmlIgnore]
        [CategoryAttribute("Survival Settings"),
        DisplayName("Frenzy Regen?"), DefaultValueAttribute(80)]
        public int _FrenzyRegen
        {
            get
            {
                return FrenzyRegen;
            }
            set
            {
                FrenzyRegen = value;
            }
        }
       [XmlIgnore]
        [CategoryAttribute("Survival Settings"),
        DisplayName("Barkskin"), DefaultValueAttribute(80)]
        public int _Barkskin
        {
            get
            {
                return Barkskin;
            }
            set
            {
                Barkskin = value;
            }
        }

	}
}}