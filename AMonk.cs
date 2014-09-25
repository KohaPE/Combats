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
    class Monk : Modules.ICombat
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
            get { return "A Monk by Koha"; }                      //This is the name displayed in SPQR's Class selection DropdownList
        }

        #region enums
        internal enum Spells : int                      //This is a convenient list of all spells used by our combat routine
        {                                               //you can have search on wowhead.com for spell name, and get the id in url
            KegSmash = 121253,
			PuriBrew = 119582,
			Jab = 108557,
			BOK = 100784,
			BoF = 115181,
			ExpelHarm = 115072,
			RJW = 116847,
			TigerPalm = 100787,
			Guard = 115295,
			DampHarm = 122278,
			ElusiveBrew = 115308,
			ZP = 124081,
			ChiBrew = 115399,
			SCK = 101546,
			PureBrew = 119582,
			ChiWave = 115098,
			CJL = 117952,
			ManaTea = 115294,
			Renew = 115151,
			Uplift = 116670,
			SurgingMist = 123273,
        }

        internal enum Auras : int                       //This is another convenient list of Auras used in our combat routine
        {												//you can have those in wowhead.com (again) and get the id in url
			ElusiveBrew = 128939,
			Elusive = 115308,
			Shuffle = 115307,
			Guard = 115295,
			ZP = 124081,
			PG = 118636,
			RJW = 116847,
			BoF = 123725,
			StagL = 12427,
			StagM = 124274,
			StagH = 124273,
			DHaze1 = 123727,
			DHaze2 = 116330,
			TankCheck = 120267,
			TP = 125359,
			HealingCheck = 116346,
			ManaTea = 115867,
			VM = 118674,
			SZ = 127722,
			RM = 119611,
			MM = 139597,
			GlyphofMT = 123763,
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
//public override void OnCombat(WoW.Classes.ObjectManager.WowUnit unit)
//{
//}

private int lastSSTick = 0;

private void castNextSpellbySinglePriority(WowUnit TARGET)
{

		var MyChi = ObjectManager.LocalPlayer.GetPower(WoW.Classes.ObjectManager.WowUnit.WowPowerType.MonkChi);
		var MyEnergy = ObjectManager.LocalPlayer.GetPower(Anthrax.WoW.Classes.ObjectManager.WowUnit.WowPowerType.Energy);
		var IsCasting = ObjectManager.LocalPlayer.IsCasting;


		
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
	
	
	////////////////////////////////////////////////////Brewmaster///////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////////////////////////
	if (ME.HasAuraById((int)Auras.TankCheck))
	{
	//Healing & Survival
	
	
	
	if (ME.HealthPercent <= 95 && MyChi >= 3 && AI.Controllers.Spell.CanCast((int)Spells.Guard) && ME.HasAuraById((int)Auras.PG) && !ME.HasAuraById((int)Auras.Guard))
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Guard);
                return;
            }
			
			
	if (ME.HealthPercent <= 99 && AI.Controllers.Spell.CanCast((int)Spells.ZP) && !ME.HasAuraById((int)Auras.ZP)) 
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ZP);
                return;
            }
			else
	if (AI.Controllers.Spell.CanCast((int)Spells.ChiWave)) 
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ChiWave);
                return;
            }	
			
			
	if (ME.HealthPercent <= 80 && MyChi < 4 && AI.Controllers.Spell.CanCast((int)Spells.ExpelHarm))
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ExpelHarm);
                return;
            }
			
			
	if (MyChi <= 1 && MyEnergy <= 30 && AI.Controllers.Spell.CanCast((int)Spells.ChiBrew))
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ChiBrew);
                return;
            }
	//Defensive
	
	if (MyChi >= 1 && ME.HasAuraById((int)Auras.StagM) || ME.HasAuraById((int)Auras.StagH) 
        || MyChi >= 1 && ME.HasAuraById((int)Auras.StagL) && ME.Auras.Where(x => x.SpellId == (int)Auras.StagL && x.TimeLeft <= 8000).Any()
		|| ME.HealthPercent <= 80 && MyChi >= 1 && ME.HasAuraById((int)Auras.StagL) && ME.Auras.Where(x => x.SpellId == (int)Auras.StagL && x.TimeLeft <= 8000).Any())
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.PureBrew);
                return;
            }
			
	if (ME.Auras.Where(x => x.SpellId == (int)Auras.ElusiveBrew && x.StackCount >= 10).Any() &&
            AI.Controllers.Spell.CanCast((int)Spells.ElusiveBrew) && !ME.HasAuraById((int)Auras.Elusive))
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ElusiveBrew);
                return;
            }
			
	if (ME.HealthPercent <= 94 && AI.Controllers.Spell.CanCast((int)Spells.Guard) && ME.HasAuraById((int)Auras.PG) && !ME.HasAuraById((int)Auras.Guard))
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Guard);
                return;
            }
	
	

	//Rotation!!!
	
	if (MyChi < 3 && AI.Controllers.Spell.CanCast((int)Spells.KegSmash))
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.KegSmash);
                return;
            }
	
	if (AI.Controllers.Spell.CanCast((int)Spells.BOK) && !ME.HasAuraById((int)Auras.Shuffle)
	|| AI.Controllers.Spell.CanCast((int)Spells.BOK) && ME.Auras.Where(x => x.SpellId == (int)Auras.Shuffle && x.TimeLeft < 12000).Any())
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BOK);
                return;
            }
			
	if (AI.Controllers.Spell.CanCast((int)Spells.TigerPalm) && ME.Auras.Where(x => x.SpellId == (int)Auras.TP && x.TimeLeft < 3000).Any()
	|| AI.Controllers.Spell.CanCast((int)Spells.TigerPalm) && !ME.HasAuraById((int)Auras.TP))
	        {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.TigerPalm);
                return;
            }
	
			
			
	if (MyChi < 4 && AI.Controllers.Spell.CanCast((int)Spells.Jab))
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Jab);
                return;
            }
	if (AI.Controllers.Spell.CanCast((int)Spells.BOK) && MyChi > 2)
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BOK);
                return;
            }
			
	if (AI.Controllers.Spell.CanCast((int)Spells.TigerPalm))
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.TigerPalm);
                return;
            }
		
	}
    
	
	//////////////////////////////////////////////////FistWeaving/////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////////
	
	if (ME.HasAuraById((int)Auras.HealingCheck))
	{
	if (!IsCasting)
	{
	//Chi Capping Code
		if (MyChi >= 4 && AI.Controllers.Spell.CanCast((int)Spells.BOK) && ME.HasAuraById((int)Auras.MM))
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BOK);
                return;
            }
		if (MyChi >= 4 && AI.Controllers.Spell.CanCast((int)Spells.Uplift) && ME.HasAuraById((int)Auras.RM) && ME.HealthPercent <= 70)
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Uplift);
                return;
            }
			
	//Mana Tea Code.
		if (ME.Auras.Where(x => x.SpellId == (int)Auras.ManaTea && x.StackCount >= 2).Any() && ME.HasAuraById((int)Auras.GlyphofMT) &&
            AI.Controllers.Spell.CanCast((int)Spells.ManaTea) && ObjectManager.LocalPlayer.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.Mana) <= 90
			|| ME.HasAuraById((int)Auras.ManaTea) && ObjectManager.LocalPlayer.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.Mana) <= 10)
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ManaTea);
                return;
            }
			
		if (ME.Auras.Where(x => x.SpellId == (int)Auras.ManaTea && x.StackCount >= 5).Any() &&
            AI.Controllers.Spell.CanCast((int)Spells.ManaTea) && ObjectManager.LocalPlayer.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.Mana) <= 80
			|| ME.HasAuraById((int)Auras.ManaTea) && ObjectManager.LocalPlayer.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.Mana) <= 10)
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ManaTea);
                return;
            }
			
		if (AI.Controllers.Spell.CanCast((int)Spells.Renew))
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Renew);
                return;
            }	
		if (AI.Controllers.Spell.CanCast((int)Spells.ChiWave))
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ChiWave);
                return;
            }	
			
		if (ME.Auras.Where(x => x.SpellId == (int)Auras.VM && x.StackCount >= 5).Any() &&
            AI.Controllers.Spell.CanCast((int)Spells.SurgingMist))
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SurgingMist);
                return;
            }	

		if (!ME.HasAuraById((int)Auras.SZ) && AI.Controllers.Spell.CanCast((int)Spells.BOK) && MyChi >= 2 && ME.HasAuraById((int)Auras.MM))
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BOK);
                return;
            }
		if (!ME.HasAuraById((int)Auras.TP) && AI.Controllers.Spell.CanCast((int)Spells.TigerPalm) && ME.HasAuraById((int)Auras.MM)
		|| ME.HasAuraById((int)Auras.MM) && AI.Controllers.Spell.CanCast((int)Spells.TigerPalm) && MyChi > 1)
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.TigerPalm);
                return;
            }		

		if (!ME.HasAuraById((int)Auras.MM) && AI.Controllers.Spell.CanCast((int)Spells.Jab))
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Jab);
                return;
            }	
		
		if (TARGET.Position.Distance3DFromPlayer > 10 && AI.Controllers.Spell.CanCast((int)Spells.CJL))
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.CJL);
                return;
            }			
		
	
	
	
	
	
	}
	}
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
}
}
#endregion

#region AOE>4 rotation
 private void castNextSpellbyAOEPriority(WowUnit TARGET)
{
		var MyChi = ObjectManager.LocalPlayer.GetPower(WoW.Classes.ObjectManager.WowUnit.WowPowerType.MonkChi);
		var MyEnergy = ObjectManager.LocalPlayer.GetPower(Anthrax.WoW.Classes.ObjectManager.WowUnit.WowPowerType.Energy);
		var IsCasting = ObjectManager.LocalPlayer.IsCasting;
		
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
	
	
	if (ME.HealthPercent <= 95 && MyChi >= 3 && AI.Controllers.Spell.CanCast((int)Spells.Guard) && ME.HasAuraById((int)Auras.PG) && !ME.HasAuraById((int)Auras.Guard))
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Guard);
                return;
            }
			
			
	if (ME.HealthPercent <= 99 && AI.Controllers.Spell.CanCast((int)Spells.ZP) && !ME.HasAuraById((int)Auras.ZP)) 
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ZP);
                return;
            }
			else
	if (AI.Controllers.Spell.CanCast((int)Spells.ChiWave)) 
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ChiWave);
                return;
            }	
			
			
	if (ME.HealthPercent <= 80 && MyChi < 4 && AI.Controllers.Spell.CanCast((int)Spells.ExpelHarm))
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ExpelHarm);
                return;
            }
			
			
	if (MyChi <= 1 && MyEnergy <= 30 && AI.Controllers.Spell.CanCast((int)Spells.ChiBrew))
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ChiBrew);
                return;
            }
	//Defensive
	
	if (MyChi >= 1 && ME.HasAuraById((int)Auras.StagM) || ME.HasAuraById((int)Auras.StagH) 
        || MyChi >= 1 && ME.HasAuraById((int)Auras.StagL) && ME.Auras.Where(x => x.SpellId == (int)Auras.StagL && x.TimeLeft <= 8000).Any()
		|| ME.HealthPercent <= 80 && MyChi >= 1 && ME.HasAuraById((int)Auras.StagL) && ME.Auras.Where(x => x.SpellId == (int)Auras.StagL && x.TimeLeft <= 8000).Any())
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.PureBrew);
                return;
            }
			
	if (ME.Auras.Where(x => x.SpellId == (int)Auras.ElusiveBrew && x.StackCount >= 10).Any() &&
            AI.Controllers.Spell.CanCast((int)Spells.ElusiveBrew) && !ME.HasAuraById((int)Auras.Elusive))
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ElusiveBrew);
                return;
            }
			
	if (ME.HealthPercent <= 94 && AI.Controllers.Spell.CanCast((int)Spells.Guard) && ME.HasAuraById((int)Auras.PG) && !ME.HasAuraById((int)Auras.Guard))
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Guard);
                return;
            }
			
	
	//ROTATION!!!!
	if (TARGET.Position.Distance3DFromPlayer < 8)
		{
	if (AI.Controllers.Spell.CanCast((int)Spells.RJW)) 
        {
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.RJW);
            return;
        }		

	//Keg Smash
	if (MyChi <= 2 && AI.Controllers.Spell.CanCast((int)Spells.KegSmash))
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.KegSmash);
                return;
            }		
	//Shuffle and Black out Kick	
	if (AI.Controllers.Spell.CanCast((int)Spells.BOK) && !ME.HasAuraById((int)Auras.Shuffle)
	|| AI.Controllers.Spell.CanCast((int)Spells.BOK) && ME.Auras.Where(x => x.SpellId == (int)Auras.Shuffle && x.TimeLeft < 8000).Any())
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BOK);
                return;
            }
	//Breath of Fire		
	if (MyChi >= 2 && AI.Controllers.Spell.CanCast((int)Spells.BoF) && TARGET.HasAuraById((int)Auras.DHaze1)
	&& ME.Auras.Where(x => x.SpellId == (int)Auras.Shuffle && x.TimeLeft > 4000).Any() && !TARGET.HasAuraById((int)Auras.BoF))
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BoF);
                return;
            }	
	//TigerPalm Buff		
	if (AI.Controllers.Spell.CanCast((int)Spells.TigerPalm) && ME.Auras.Where(x => x.SpellId == (int)Auras.TP && x.TimeLeft < 3000).Any()
	|| AI.Controllers.Spell.CanCast((int)Spells.TigerPalm) && !ME.HasAuraById((int)Auras.TP))
	        {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.TigerPalm);
                return;
            }

			
			
	if (MyChi < 3 && AI.Controllers.Spell.CanCast((int)Spells.Jab) && MyEnergy >= 60)
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Jab);
                return;
            }
	
	if (MyChi > 2 && AI.Controllers.Spell.CanCast((int)Spells.BOK))
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BOK);
                return;
            }	
		
	if (AI.Controllers.Spell.CanCast((int)Spells.TigerPalm))
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.TigerPalm);
                return;
            }
			
			
			
			
		}
    }

		//////////////////////////////////////////////////FistWeaving/////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////////
	
	if (ME.HasAuraById((int)Auras.HealingCheck))
	{
	if (!IsCasting)
	{
	//Chi Capping Code
		if (MyChi >= 4 && AI.Controllers.Spell.CanCast((int)Spells.BOK) && ME.HasAuraById((int)Auras.MM))
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BOK);
                return;
            }
		if (MyChi >= 4 && AI.Controllers.Spell.CanCast((int)Spells.Uplift) && ME.HasAuraById((int)Auras.RM) && ME.HealthPercent <= 70)
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Uplift);
                return;
            }
			
	//Mana Tea Code.
		if (ME.Auras.Where(x => x.SpellId == (int)Auras.ManaTea && x.StackCount >= 2).Any() && ME.HasAuraById((int)Auras.GlyphofMT) &&
            AI.Controllers.Spell.CanCast((int)Spells.ManaTea) && ObjectManager.LocalPlayer.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.Mana) <= 90
			|| ME.HasAuraById((int)Auras.ManaTea) && ObjectManager.LocalPlayer.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.Mana) <= 10)
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ManaTea);
                return;
            }
			
		if (ME.Auras.Where(x => x.SpellId == (int)Auras.ManaTea && x.StackCount >= 5).Any() &&
            AI.Controllers.Spell.CanCast((int)Spells.ManaTea) && ObjectManager.LocalPlayer.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.Mana) <= 80
			|| ME.HasAuraById((int)Auras.ManaTea) && ObjectManager.LocalPlayer.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.Mana) <= 10)
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ManaTea);
                return;
            }
			
		if (AI.Controllers.Spell.CanCast((int)Spells.Renew))
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Renew);
                return;
            }	
		if (AI.Controllers.Spell.CanCast((int)Spells.ChiWave))
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ChiWave);
                return;
            }	
			
		if (ME.Auras.Where(x => x.SpellId == (int)Auras.VM && x.StackCount >= 5).Any() &&
            AI.Controllers.Spell.CanCast((int)Spells.SurgingMist))
            {
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SurgingMist);
                return;
            }	

			
		if (AI.Controllers.Spell.CanCast((int)Spells.RJW)) 
        {
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.RJW);
            return;
        }	
		
		if (!ME.HasAuraById((int)Auras.SZ) && AI.Controllers.Spell.CanCast((int)Spells.BOK) && MyChi >= 2 && ME.HasAuraById((int)Auras.MM))
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BOK);
                return;
            }
		if (!ME.HasAuraById((int)Auras.TP) && AI.Controllers.Spell.CanCast((int)Spells.TigerPalm) && ME.HasAuraById((int)Auras.MM)
		|| ME.HasAuraById((int)Auras.MM) && AI.Controllers.Spell.CanCast((int)Spells.TigerPalm) && MyChi > 1)
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.TigerPalm);
                return;
            }		
		
		if (!ME.HasAuraById((int)Auras.MM) && MyChi < 4 && AI.Controllers.Spell.CanCast((int)Spells.Jab))
			{
                WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Jab);
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