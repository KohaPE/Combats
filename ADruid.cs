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
            Innervate = 29166,
            HealingTouch = 5185,
            Rejuvenation = 774,
            Regrowth = 8936,
            Swiftmend = 18562,
            Tranquility = 740,
			WildGrowth = 48438,
			Nourish = 50464,
			Rebirth = 20484,
			Lifebloom = 33763,
			WildMushroom = 145205,
			IncarnationTree = 33891,
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
			Prowl = 5215,
			Enrage = 5229,
			FoN = 33831,
			Hurricane = 106996,
			AstralStorm = 16914,
			MotW = 1126,
			CWard = 102351,
			Berserk = 106951,
			Growl = 6795,
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
			ThrashFeralBear = 77758,
			Lacerate = 33745,
			FearlBearForm = 17057,
			CatForm = 3025,
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
			CCResto = 16870,
			RestoCheck = 106735,
			Lifebloom = 33763,
			Harmony = 100977,
			FForm = 40120,
			MotW = 1126,
			CWardtallent = 102351,
			CWard = 102352,
			Berserk = 106951,
			TigerFury = 5217,
			NaturesVigil = 124974,
			GlyphofShred = 114235,
			Vis = 148903,
			ReOrign = 139120,
			WA = 113746,
			
		//Gloves Snapsys Springs
		SSprings = 96228,
			
		//5.3 Raids
		FengTheAccursed = 131792,
		BladeLord = 123474,
		GrandEmpress = 123707,
		Tsulong = 122752,
		LeiShi = 123121,
		
		//5.4 Raids
		Jinrokh = 138349,
		JiKun = 134366,
		Durumu = 133767,
		Primordius = 136050,
		IronQon = 134691,
		TwinConsorts = 137408,
		LeiShen = 134912,
		LeiShen2 = 134916,
		LeiShen3 = 136478,
		LieShen4 = 136913,
		
		//SoO Raids
		Immerseus = 71543,
		Norushen = 146124,
		ShaofPride = 144358,
		IronJuggernaut = 144467,
		DarkShamens = 144215,
		Nazgrim = 143494,
		Malkorok = 142990,
		BlackFuse = 143385,
		Thok = 143426,
		Thok2 = 143780,
		Thok3 = 143773,
		Garrosh = 145183,
		Garrosh2 = 145195,
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

private int lastStealthTick = 0;
private int lastSDTick = 0;
private int lastSSTick = 0;

private void castNextSpellbySinglePriority(WowUnit TARGET)
{

		var MyRage = ObjectManager.LocalPlayer.GetPower(WoW.Classes.ObjectManager.WowUnit.WowPowerType.Rage);
		var MyEnergy = ObjectManager.LocalPlayer.GetPower(Anthrax.WoW.Classes.ObjectManager.WowUnit.WowPowerType.Energy);
		
	 if (!ME.InCombat && !ME.HasAuraById((int)Auras.Prowl) && Environment.TickCount - lastStealthTick > 2000 && ME.HasAuraById((int)Auras.FeralCheck) && !ObjectManager.LocalPlayer.IsMounted && !ME.HasAuraById((int)Auras.FForm))
         {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Prowl);
			lastStealthTick = Environment.TickCount;
			return;
        }
		
	if (ME.HasAuraById((int)Auras.GlyphofS) && TARGET.Position.Distance3DFromPlayer < 20 && AI.Controllers.Spell.CanCast((int)Spells.SavageRoar) && !ME.HasAuraById((int)Auras.SavageRoar)
	&& ME.HasAuraById((int)Auras.CatForm))
			{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SavageRoar);
                    return;
			}
	if (AI.Controllers.Spell.CanCast((int)Spells.Pounce) && !WoW.Internals.ObjectManager.LocalPlayer.IsBehindUnit(WoW.Internals.ObjectManager.Target) && TARGET.Position.Distance3DFromPlayer < 8)
			{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Pounce);
                    return;
			}	
	
	if (AI.Controllers.Spell.CanCast((int)Spells.Ravage) && WoW.Internals.ObjectManager.LocalPlayer.IsBehindUnit(WoW.Internals.ObjectManager.Target) && TARGET.Position.Distance3DFromPlayer < 8)
			{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Ravage);
                    return;
			}
		
		
	if (TARGET.Health >= 1 && ME.InCombat)
	{
	
	
	/////////////////////////////////////////////////////////////////////////////TANK SPEC////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
						///////////Cat Form In Tank Spec//////////////
	if (ME.HasAuraById((int)Auras.CatForm) && ME.HasAuraById((int)Auras.TankCheck))
	{
	if (ME.HasAuraById((int)Auras.Prowl))
	{
	if (AI.Controllers.Spell.CanCast((int)Spells.Pounce) && Anthrax.WoW.Internals.Movements.IsFacingHeading(ObjectManager.Target.Position))
			{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Pounce);
                    return;
			}	
	else
	if (AI.Controllers.Spell.CanCast((int)Spells.Ravage))
			{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Ravage);
                    return;
			}
	
	
	
	}
	

		

	if (!ME.HasAuraById((int)Auras.Prowl))
	{
	if (ME.HasAuraById((int)Auras.PredSwiftness) && AI.Controllers.Spell.CanCast((int)Spells.HealingTouch))
			{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HealingTouch);
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
}

					/////////////////////////////Bear Form///////////////////////////////
	if (ME.HasAuraById((int)Auras.TankCheck))
	{
	
				//Auto Taunt Code
		//5.3 Raids

		
		if (AI.Controllers.Spell.CanCast((int)Spells.Growl) && ME.HasAuraById((int)Auras.TankCheck))
		{
			if (!ME.HasAuraById((int)Auras.FengTheAccursed) && TARGET.Name == "Feng the Accursed"
			|| TARGET.Name == "Raider's Training Dummy"
			|| !ME.HasAuraById((int)Auras.BladeLord) && TARGET.Name == "Blade Lord Ta'yak"
			|| !ME.HasAuraById((int)Auras.GrandEmpress) && TARGET.Name == "Grand Empress Shek'zeer"
			|| !ME.HasAuraById((int)Auras.Tsulong) && TARGET.Name == "Tsulong"
			|| !ME.HasAuraById((int)Auras.LeiShi) && TARGET.Name == "Lei Shi"
		//5.4
			|| !ME.HasAuraById((int)Auras.Jinrokh) && TARGET.Name == "Jin'rokh the Breaker"
			|| !ME.HasAuraById((int)Auras.JiKun) && TARGET.Name == "Ji-Kun"
			|| !ME.HasAuraById((int)Auras.Durumu) && TARGET.Name == "Durumu the Forgotten"
			|| !ME.HasAuraById((int)Auras.Primordius) && TARGET.Name == "Primordius"
			|| !ME.HasAuraById((int)Auras.IronQon) && TARGET.Name == "Iron Qon"
			|| !ME.HasAuraById((int)Auras.TwinConsorts) && TARGET.Name == "Suen"
			|| !ME.HasAuraById((int)Auras.LeiShen) && TARGET.Name == "Lei Shen"
			|| !ME.HasAuraById((int)Auras.LeiShen2) && TARGET.Name == "Lei Shen"
			|| !ME.HasAuraById((int)Auras.LeiShen3) && TARGET.Name == "Lei Shen"
			|| !ME.HasAuraById((int)Auras.LieShen4) && TARGET.Name == "Lei Shen"
		//SoO
			|| !ME.HasAuraById((int)Auras.Immerseus) && TARGET.Name == "Immerseus"
			|| !ME.HasAuraById((int)Auras.Norushen) && TARGET.Name == "Norushen"
			|| !ME.HasAuraById((int)Auras.ShaofPride) && TARGET.Name == "Sha of Pride"
			|| !ME.HasAuraById((int)Auras.IronJuggernaut) && TARGET.Name == "Iron Juggernaut"
			|| !ME.HasAuraById((int)Auras.DarkShamens) && TARGET.Name == "Earthbreaker Haromm"
			|| !ME.HasAuraById((int)Auras.DarkShamens) && TARGET.Name == "Wavebinder Kardris"
			|| !ME.HasAuraById((int)Auras.Nazgrim) && TARGET.Name == "General Nazgrim"
			|| !ME.HasAuraById((int)Auras.Malkorok) && TARGET.Name == "Malkorok"
			|| !ME.HasAuraById((int)Auras.BlackFuse) && TARGET.Name == "Siegecrafter Blackfuse"
			|| !ME.HasAuraById((int)Auras.Thok) && TARGET.Name == "Thok the Bloodthirsty"
			|| !ME.HasAuraById((int)Auras.Thok2) && TARGET.Name == "Thok the Bloodthirsty"
			|| !ME.HasAuraById((int)Auras.Thok3) && TARGET.Name == "Thok the Bloodthirsty"
			|| !ME.HasAuraById((int)Auras.Garrosh) && TARGET.Name == "Garrosh Hellscream"
			|| !ME.HasAuraById((int)Auras.Garrosh2) && TARGET.Name == "Garrosh Hellscream"
			)
			{
			  WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Growl);
			  Logger.WriteLine("Auto Taunting!!!");
			}
		}
	//Healing & Survival
	{
		if (ME.HealthPercent < CCSettings.CWard && AI.Controllers.Spell.CanCast((int)Spells.CWard) && !ME.HasAuraById((int)Auras.CWard))
			{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.CWard);
				return;
			}
	
	if (ME.HealthPercent < CCSettings.FrenzyRegen && AI.Controllers.Spell.CanCast((int)Spells.FrenzyRegen) && MyRage >= 20)
			{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.FrenzyRegen);
				return;
			}
			
		if (ME.HealthPercent < CCSettings.Barkskin && AI.Controllers.Spell.CanCast((int)Spells.Barkskin))
			{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Barkskin);
				return;
			}

	if (ME.HasAuraById((int)Auras.DoC) && ME.HealthPercent < CCSettings.HealingTouch && AI.Controllers.Spell.CanCast((int)Spells.HealingTouch)
		|| ME.Auras.Where(x => x.SpellId == (int)Auras.DoC && x.TimeLeft <= 5000).Any() && AI.Controllers.Spell.CanCast((int)Spells.HealingTouch))
			{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HealingTouch);
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
	
		if (MyRage < CCSettings.Enrage && AI.Controllers.Spell.CanCast((int)Spells.Enrage) && !ME.HasAuraById((int)Auras.CatForm) && ME.HasAuraById((int)Auras.BearForm))
			{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Enrage);
				return;
			}
			
			if (MyRage >= 60 && AI.Controllers.Spell.CanCast((int)Spells.SavageD) && !ME.HasAuraById((int)Auras.SavageD) && Environment.TickCount - lastSDTick > 3000)
			{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SavageD);
				lastSDTick = Environment.TickCount;
				return;
			}
			
				//Maul With Procs
			if (MyRage > 90 && AI.Controllers.Spell.CanCast((int)Spells.Maul) || ME.HasAuraById((int)Auras.TAC) && AI.Controllers.Spell.CanCast((int)Spells.Maul) && ME.HasAuraById((int)Auras.SavageD))
			{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Maul);
				return;
			}
			
			

		
		//Mangle!!!
			if (AI.Controllers.Spell.CanCast((int)Spells.MangleBear))
			{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.MangleBear);
				return;
			}
			
		// Keep Lacerate Debuff on so it doesnt drop off
			if (AI.Controllers.Spell.CanCast((int)Spells.Lacerate) && !TARGET.HasAuraById((int)Auras.Lacerate) 
			|| AI.Controllers.Spell.CanCast((int)Spells.Lacerate) && TARGET.Auras.Where(x => x.SpellId == (int)Auras.Lacerate && x.TimeLeft <= 4000).Any() )
			{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Lacerate);
				return;
			}		
		

			
		//Thrash Debuff
			if (AI.Controllers.Spell.CanCast((int)Spells.Thrash) && !AI.Controllers.Spell.CanCast((int)Spells.Lacerate) && !AI.Controllers.Spell.CanCast((int)Spells.MangleBear))
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
	
/////////////////////////////////////////////////////////////////////////////////Boomkin///////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////			
			
	if (ME.HasAuraById((int)Auras.BoomkinCheck))
	{
	//Healing & Survival
	if(TARGET.Position.Distance3DFromPlayer <= 40 && !ME.IsCasting)
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
								//Engineering Gloves
			if (!ME.HasAuraById((int)Auras.SSprings) && Environment.TickCount - lastSSTick > 20000 )
		{
              Anthrax.WoW.Internals.ActionBar.PressSlot(0, 0);
			  Logger.WriteLine("Synapse Srpings Used!!!");
			  lastSSTick = Environment.TickCount;
              return;
          }
					
				if (ME.HasAuraById((int)Auras.ShootingStars) && AI.Controllers.Spell.CanCast((int)Spells.Starsurge))
				{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Starsurge);
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
				
				if (AI.Controllers.Spell.CanCast((int)Spells.FoN))
				{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.FoN);
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
				
				if (AI.Controllers.Spell.CanCast((int)Spells.FoN))
				{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.FoN);
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
				
				if (AI.Controllers.Spell.CanCast((int)Spells.FoN))
				{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.FoN);
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

 
 //New Combat simcraft 5.4.8
 

		
	if (ME.HasAuraById((int)Auras.Prowl))
	{


	if (AI.Controllers.Spell.CanCast((int)Spells.Ravage) && WoW.Internals.ObjectManager.LocalPlayer.IsBehindUnit(WoW.Internals.ObjectManager.Target))
			{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Ravage);
                    return;
			}	
			
		if (AI.Controllers.Spell.CanCast((int)Spells.Pounce))
			{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Pounce);
                    return;
			}	
	}
	
//Boss Cooldowns Auto.

	if(ObjectManager.Target.CreatureCache.Classification == WoW.Classes.WowCreatureCache.WowUnitClassification.WorldBoss && TARGET.Position.Distance3DFromPlayer < 8 && !ME.HasAuraById((int)Auras.TigerFury) && AI.Controllers.Spell.CanCast((int)Spells.Berserk)
	|| ObjectManager.Target.CreatureCache.Classification == WoW.Classes.WowCreatureCache.WowUnitClassification.RareElite && TARGET.Position.Distance3DFromPlayer < 8 && !ME.HasAuraById((int)Auras.TigerFury) && AI.Controllers.Spell.CanCast((int)Spells.Berserk))
			{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Berserk);
                    return;
			}
			
	if(ObjectManager.Target.CreatureCache.Classification == WoW.Classes.WowCreatureCache.WowUnitClassification.WorldBoss && TARGET.Position.Distance3DFromPlayer < 8 && AI.Controllers.Spell.CanCast((int)Spells.NaturesVigil))
			{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.NaturesVigil);
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
			
 //Keep Rip From Falling Off
 //if rip timeleft Less then 3 secs and target's health is below 25%
 	if (TARGET.HasAuraById((int)Auras.Rip) && TARGET.Auras.Where(x => x.SpellId == (int)Auras.Rip && x.TimeLeft < 5000).Any() && AI.Controllers.Spell.CanCast((int)Spells.FeroBite) && TARGET.HealthPercent <= 25
	&&(TARGET.Auras.Where(x => x.SpellId == 1079 && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any() ))
			{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.FeroBite);
                    return;
			}
 
//Proc Dearm of Cenarious @ 4+ Combo Points or when Preadatory Swiftness is about to expire.

 		if (ME.HasAuraById((int)Auras.PredSwiftness) && AI.Controllers.Spell.CanCast((int)Spells.HealingTouch) && ME.ComboPoints >= 4
		|| ME.HasAuraById((int)Auras.PredSwiftness) && AI.Controllers.Spell.CanCast((int)Spells.HealingTouch) && ME.Auras.Where(x => x.SpellId == (int)Auras.PredSwiftness && x.TimeLeft < 2000).Any())
		{
		WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HealingTouch);
		}
		
	
	//Faerie Fire
	
	if (!TARGET.HasAuraById((int)Auras.WA) && AI.Controllers.Spell.CanCast((int)Spells.FF) && !ME.HasAuraById((int)Auras.Prowl))
		{
		WoW.Internals.ActionBar.ExecuteSpell((int)Spells.FF);
		}
 
//Savage Roar if buff less then 3secs and Dosent Exist
 
	if (ME.HasAuraById((int)Auras.GlyphofS) && !ME.HasAuraById((int)Auras.SavageRoar) && AI.Controllers.Spell.CanCast((int)Spells.SavageRoar)
	|| !ME.HasAuraById((int)Auras.SavageRoar) && AI.Controllers.Spell.CanCast((int)Spells.SavageRoar) && ME.ComboPoints < 3
	|| ME.HasAuraById((int)Auras.SavageRoar) && AI.Controllers.Spell.CanCast((int)Spells.SavageRoar) && ME.ComboPoints >=5 && ME.Auras.Where(x => x.SpellId == (int)Auras.SavageRoar && x.TimeLeft < 3000).Any() )
			{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SavageRoar);
                    return;
            }
			
		
	if (TARGET.Auras.Where(x => x.SpellId == (int)Auras.Rip && x.TimeLeft > 6000).Any() && AI.Controllers.Spell.CanCast((int)Spells.FeroBite) && ME.ComboPoints >= 5 && MyEnergy >= 40
	&& (TARGET.Auras.Where(x => x.SpellId == ((int)Auras.Rip) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any() )
	|| TARGET.HealthPercent < 25 && ME.ComboPoints >= 4 && TARGET.HasAuraById((int)Auras.Rip) && AI.Controllers.Spell.CanCast((int)Spells.FeroBite) && MyEnergy >= 40
	&& (TARGET.Auras.Where(x => x.SpellId == ((int)Auras.Rip) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any() ))
		{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.FeroBite);
                    return;
        }
		
		
 //overwire rip during Execute Range
 
 	if (AI.Controllers.Spell.CanCast((int)Spells.Rip) && ME.ComboPoints >= 5
	&& !(TARGET.Auras.Where(x => x.SpellId == ((int)Auras.Rip) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any() )
	|| TARGET.Auras.Where(x => x.SpellId == (int)Auras.Rip && x.TimeLeft < 6000).Any() && AI.Controllers.Spell.CanCast((int)Spells.Rip) && ME.ComboPoints >= 5
	&& (TARGET.Auras.Where(x => x.SpellId == ((int)Auras.Rip) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any() )
	|| ME.HasAuraById((int)Auras.DoC) && ME.Auras.Where(x => x.SpellId == (int)Auras.DoC && x.StackCount <= 1).Any() && ME.ComboPoints >= 4 && AI.Controllers.Spell.CanCast((int)Spells.Rip))
		{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Rip);
                    return;
        }
	
 //Thrash with Omen of Clarity
		if (!TARGET.HasAuraById((int)Auras.Thrash) && AI.Controllers.Spell.CanCast((int)Spells.ThrashFeral) && ME.HasAuraById((int)Auras.CC) && TARGET.Position.Distance3DFromPlayer < 8)
		{
		WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ThrashFeral);
		}
	
	if (!(TARGET.Auras.Where(x => x.SpellId == ((int)Auras.Rake) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any() ) && AI.Controllers.Spell.CanCast((int)Spells.Rake) && ME.ComboPoints < 4
	|| TARGET.Auras.Where(x => x.SpellId == (int)Auras.Rake && x.TimeLeft < 5000).Any() && AI.Controllers.Spell.CanCast((int)Spells.Rake) && ME.ComboPoints < 4
	&& (TARGET.Auras.Where(x => x.SpellId == ((int)Auras.Rake) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any() )
	|| ME.HasAuraById((int)Auras.DoC) && ME.Auras.Where(x => x.SpellId == (int)Auras.DoC && x.StackCount <= 2).Any() && ME.ComboPoints < 5 && AI.Controllers.Spell.CanCast((int)Spells.Rake)
	|| ME.HasAuraById((int)Auras.Vis) && AI.Controllers.Spell.CanCast((int)Spells.Rake) && ME.ComboPoints < 4)
		{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Rake);
                    return;
        }

	
	if (!TARGET.HasAuraById((int)Auras.Thrash) && AI.Controllers.Spell.CanCast((int)Spells.ThrashFeral) && ME.HasAuraById((int)Auras.CC))
		{
		WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ThrashFeral);
		}

	
 //Tigers Fury if Less then or equal to 35 energy
 		if (!ME.HasAuraById((int)Auras.CC) && AI.Controllers.Spell.CanCast((int)Spells.TigerFury) && MyEnergy <= 35 && !ME.HasAuraById((int)Auras.Berserk))
		{
		WoW.Internals.ActionBar.ExecuteSpell((int)Spells.TigerFury);
		}
	
	if (WoW.Internals.ObjectManager.LocalPlayer.IsBehindUnit(WoW.Internals.ObjectManager.Target) && ME.ComboPoints < 5 && AI.Controllers.Spell.CanCast((int)Spells.Shred) && MyEnergy >= 40 && TARGET.Auras.Where(x => x.SpellId == (int)Auras.Rake && x.TimeLeft >= 5000).Any())
		{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Shred);
                    return;
        }	
		
	if (!WoW.Internals.ObjectManager.LocalPlayer.IsBehindUnit(WoW.Internals.ObjectManager.Target) && ME.ComboPoints < 5 && AI.Controllers.Spell.CanCast((int)Spells.MangleFeral) && MyEnergy >= 40 && TARGET.Auras.Where(x => x.SpellId == (int)Auras.Rake && x.TimeLeft >= 5000).Any())
		{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.MangleFeral);
                    return;
        }
	
	}
	//Anthrax.WoW.Classes.ObjectManager.WowUnitAura.IsPlayerCasted((int)Aura.Rip
	//Anthrax.WoW.Classes.ObjectManager.WowUnitAura.WowAuraFlags.PlayerCasted
	//Anthrax.WoW.Classes.ObjectManager.WowUnitAura.TimeLeft
	
	
		////////////////////////////////////////////////////////////////RESTO/////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	if (ME.HasAuraById((int)Auras.RestoCheck))
	{
	if (!ME.IsCasting)
	{
	//Lifebloom Code
	if (TARGET.HealthPercent <= CCSettings.Lifebloom && TARGET.Auras.Where(x => x.SpellId == (int)Auras.Lifebloom && x.StackCount < 3).Any() && AI.Controllers.Spell.CanCast((int)Spells.Lifebloom)
	|| TARGET.HealthPercent <= CCSettings.Lifebloom && !TARGET.HasAuraById((int)Auras.Lifebloom) && AI.Controllers.Spell.CanCast((int)Spells.Lifebloom)
	|| TARGET.Auras.Where(x => x.SpellId == (int)Auras.Lifebloom && x.TimeLeft <= 4000).Any() && AI.Controllers.Spell.CanCast((int)Spells.Lifebloom))
		{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Lifebloom);
                    return;
        }
	// innervate
		if (ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.Mana) <= 80 && AI.Controllers.Spell.CanCast((int)Spells.Innervate))
			{
				Anthrax.WoW.Internals.Chat.SendMessage("/target Player");
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Innervate);
				return;
			}
	//Wild Growth
	if (TARGET.HealthPercent <= CCSettings.WildGrowth && AI.Controllers.Spell.CanCast((int)Spells.WildGrowth))
		{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.WildGrowth);
                    return;
        }
	
	//Regrowth with proc	
		if (TARGET.HealthPercent <= 50 && AI.Controllers.Spell.CanCast((int)Spells.Regrowth) && !TARGET.HasAuraById((int)Auras.Regrowth)
		|| ME.HasAuraById((int)Auras.CCResto) && AI.Controllers.Spell.CanCast((int)Spells.Regrowth) && !TARGET.HasAuraById((int)Auras.Regrowth))
		{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Regrowth);
                    return;
        }
	//rejuvenation	
	if (TARGET.HealthPercent <= CCSettings.Rejuvenation && AI.Controllers.Spell.CanCast((int)Spells.Rejuvenation) && !TARGET.HasAuraById((int)Auras.Rejuvenation))
		{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Rejuvenation);
                    return;
        }
	//Wild Mushroom
	if (TARGET.HealthPercent <= CCSettings.WildMushroom && AI.Controllers.Spell.CanCast((int)Spells.WildMushroom))
		{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.WildMushroom);
                    return;
        }
	
	//Wrath	
	if (AI.Controllers.Spell.CanCast((int)Spells.Wrath))
		{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Wrath);
                    return;
        }
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
		
		if (!ME.InCombat && !ME.HasAuraById((int)Auras.Prowl) && Environment.TickCount - lastStealthTick > 2000 && ME.HasAuraById((int)Auras.FeralCheck) && !ObjectManager.LocalPlayer.IsMounted && !ME.HasAuraById((int)Auras.FForm))
         {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Prowl);
			lastStealthTick = Environment.TickCount;
			return;
        }
		
	if (ME.HasAuraById((int)Auras.GlyphofS) && TARGET.Position.Distance3DFromPlayer < 20 && AI.Controllers.Spell.CanCast((int)Spells.SavageRoar) && !ME.HasAuraById((int)Auras.SavageRoar)
	&& ME.HasAuraById((int)Auras.CatForm))
			{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SavageRoar);
                    return;
			}
	if (AI.Controllers.Spell.CanCast((int)Spells.Pounce) && !WoW.Internals.ObjectManager.LocalPlayer.IsBehindUnit(WoW.Internals.ObjectManager.Target) && TARGET.Position.Distance3DFromPlayer < 8)
			{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Pounce);
                    return;
			}	
	
	if (AI.Controllers.Spell.CanCast((int)Spells.Ravage) && WoW.Internals.ObjectManager.LocalPlayer.IsBehindUnit(WoW.Internals.ObjectManager.Target) && TARGET.Position.Distance3DFromPlayer < 8)
			{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Ravage);
                    return;
			}
		
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
	/////////////////////////////////////////////////////////////////////////////////Boomkin///////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////			
			
	if (ME.HasAuraById((int)Auras.BoomkinCheck))
	{
	//Healing & Survival
	if(TARGET.Position.Distance3DFromPlayer <= 40 && !ME.IsCasting)
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
					
				if (ME.HasAuraById((int)Auras.ShootingStars) && AI.Controllers.Spell.CanCast((int)Spells.Starsurge))
				{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Starsurge);
                    return;
                }
					
								                // Moonfire
                if ((!TARGET.HasAuraById((int)Auras.Moonfire) ||
                    TARGET.Auras.Where(x => x.SpellId == (int)Auras.Moonfire && x.TimeLeft <= 9000).Any()) &&
                    AI.Controllers.Spell.CanCast((int)Spells.Moonfire))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Moonfire);
                    return;
                }

                // Sunfire
                if ((!TARGET.HasAuraById((int)Auras.Sunfire) ||
                    TARGET.Auras.Where(x => x.SpellId == (int)Auras.Sunfire && x.TimeLeft <= 9000).Any()) &&
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

				
				if (ME.HasAuraById((int)Auras.AstralInsight) && AI.Controllers.Spell.CanCast((int)Spells.AstralC))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.AstralC);
                    return;
                }

                // Starsurge
                if (AI.Controllers.Spell.CanCast((int)Spells.Starsurge))
                {

                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Starsurge);

					return;
                }
				


				 // Solar
            if (ME.HasAuraById((int)Auras.EclipseSolar))
			{
			
			
				if ((!TARGET.HasAuraById((int)Auras.Sunfire) ||
                    TARGET.Auras.Where(x => x.SpellId == (int)Auras.Sunfire && x.TimeLeft <= 9000).Any()) &&
                    AI.Controllers.Spell.CanCast((int)Spells.Sunfire))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Sunfire);
                    return;
                }
				
				if ((!TARGET.HasAuraById((int)Auras.Moonfire) ||
                    TARGET.Auras.Where(x => x.SpellId == (int)Auras.Moonfire && x.TimeLeft <= 9000).Any()) &&
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
                    TARGET.Auras.Where(x => x.SpellId == (int)Auras.Moonfire && x.TimeLeft <= 9000).Any()) &&
                    AI.Controllers.Spell.CanCast((int)Spells.Moonfire))
                {
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Moonfire);
                    return;
                }
				
				if ((!TARGET.HasAuraById((int)Auras.Sunfire) ||
                    TARGET.Auras.Where(x => x.SpellId == (int)Auras.Sunfire && x.TimeLeft <= 9000).Any()) &&
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
	
	////////////////////////////////////////////////////////////////FERAL//////////////////////////////////////////////
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	if (ME.HasAuraById((int)Auras.FeralCheck))
	{
	
if (ME.HasAuraById((int)Auras.Prowl))
	{


	if (AI.Controllers.Spell.CanCast((int)Spells.Ravage) && WoW.Internals.ObjectManager.LocalPlayer.IsBehindUnit(WoW.Internals.ObjectManager.Target))
			{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Ravage);
                    return;
			}	
			
		if (AI.Controllers.Spell.CanCast((int)Spells.Pounce))
			{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Pounce);
                    return;
			}	
	}
	
	if (ME.HasAuraById((int)Auras.GlyphofS) && !ME.HasAuraById((int)Auras.SavageRoar) && AI.Controllers.Spell.CanCast((int)Spells.SavageRoar))
			{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SavageRoar);
                    return;
            }
			
		if (!ME.HasAuraById((int)Auras.SavageRoar) && AI.Controllers.Spell.CanCast((int)Spells.SavageRoar) && ME.ComboPoints > 1)
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
	|| ME.HasAuraById((int)Auras.DoC) && AI.Controllers.Spell.CanCast((int)Spells.ThrashFeral) && ME.Auras.Where(x => x.SpellId == (int)Auras.DoC && x.StackCount >= 4).Any())
		{
		WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ThrashFeral);
		}
	
	if (MyEnergy <= 30 && AI.Controllers.Spell.CanCast((int)Spells.TigerFury))
		{
		WoW.Internals.ActionBar.ExecuteSpell((int)Spells.TigerFury);
		}
	
	
	if (ME.ComboPoints < 5 && AI.Controllers.Spell.CanCast((int)Spells.SwipeFeral) && MyEnergy > 45
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
	//Complex Healing Code

//	public bool	NeedsHeals()
//	{
	
//	}
		
		
		[Serializable]
    public class Settings
    {
        public int FrenzyRegen = 80;
        public int Barkskin = 70;
		public int Enrage = 30;
		public int CWard = 70;
		public int Rejuvenation = 90;
		public int HealingTouch = 90;
		public int Swiftmend = 80;
		public int WildGrowth = 96;
		public int Lifebloom = 99;
		public int WildMushroom = 99;
		
	//Healing Settings
		[XmlIgnore]
        [CategoryAttribute("Restoration Settings"),
        DisplayName("Wild Mushroom Hp Limit"), DefaultValueAttribute(99)]
        public int _WildMushroom
        {
            get
            {
                return WildMushroom;
            }
            set
            {
                WildMushroom = value;
            }
        }

	
		[XmlIgnore]
        [CategoryAttribute("Restoration Settings"),
        DisplayName("Lifebloom Hp Limit"), DefaultValueAttribute(99)]
        public int _Lifebloom
        {
            get
            {
                return Lifebloom;
            }
            set
            {
                Lifebloom = value;
            }
        }
		
		[XmlIgnore]
        [CategoryAttribute("Restoration Settings"),
        DisplayName("Wild Growth Hp Limit"), DefaultValueAttribute(90)]
        public int _WildGrowth
        {
            get
            {
                return WildGrowth;
            }
            set
            {
                WildGrowth = value;
            }
        }
		
		[XmlIgnore]
        [CategoryAttribute("Restoration Settings"),
        DisplayName("Swift Mend Hp Limit"), DefaultValueAttribute(80)]
        public int _Swiftmend
        {
            get
            {
                return Swiftmend;
            }
            set
            {
                Swiftmend = value;
            }
        }	

		[XmlIgnore]
        [CategoryAttribute("Restoration Settings"),
        DisplayName("Rejuvenation Hp Limit"), DefaultValueAttribute(90)]
        public int _Rejuvenation
        {
            get
            {
                return Rejuvenation;
            }
            set
            {
                Rejuvenation = value;
            }
        }
		//Tank Settings
        [XmlIgnore]
        [CategoryAttribute("Tank Settings"),
        DisplayName("Frenzy Regen?"), DefaultValueAttribute(60)]
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
        [CategoryAttribute("Tank Settings"),
        DisplayName("Healing Touch Limit"), DefaultValueAttribute(90)]
        public int _HealingTouch
        {
            get
            {
                return HealingTouch;
            }
            set
            {
                HealingTouch = value;
            }
        }
		
		[XmlIgnore]
        [CategoryAttribute("Tank Settings"),
        DisplayName("Cenarion Ward Hp Limit"), DefaultValueAttribute(85)]
        public int _CWard
        {
            get
            {
                return CWard;
            }
            set
            {
                CWard = value;
            }
        }
		
		[XmlIgnore]
        [CategoryAttribute("Tank Settings"),
        DisplayName("Enrage Limit"), DefaultValueAttribute(30)]
        public int _Enrage
        {
            get
            {
                return Enrage;
            }
            set
            {
                Enrage = value;
            }
        }
		//Survival Settings
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