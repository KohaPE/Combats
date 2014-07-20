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
    class Rogue : Modules.ICombat
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
            get { return "A Rogue by Koha"; }                      //This is the name displayed in SPQR's Class selection DropdownList
        }

        #region enums
        internal enum Spells : int                      //This is a convenient list of all spells used by our combat routine
        {                                               //you can have search on wowhead.com for spell name, and get the id in url
		Stealth = 1784,
		Ambush = 8676,
		RS = 84617,
		DeadlyPoison = 2823,
		LeechingPoison = 108211,
		Rup = 1943,
		SnD = 5171,
		Evis = 2098,
		SS = 1752,
		FoK = 51723,
		CT = 121411,
		BF = 13877,
		Recup = 73651,
		Redirect = 73981,
        }

        internal enum Auras : int                       //This is another convenient list of Auras used in our combat routine
        {												//you can have those in wowhead.com (again) and get the id in url
		Stealth = 1784,
		CombatCheck = 35551,
		RS = 84617,
		DeadlyPoison = 2823,
		LeechingPoison = 108211,
		Rup = 1943,
		SnD = 5171,
		Recup = 73651,
		BF = 13877,
		
		
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

private void castNextSpellbySinglePriority(WowUnit TARGET)
{

		var MyEnergy = ObjectManager.LocalPlayer.GetPower(Anthrax.WoW.Classes.ObjectManager.WowUnit.WowPowerType.Energy);
		
	if (!ME.InCombat && !ME.HasAuraById((int)Auras.DeadlyPoison))
	        {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DeadlyPoison);
			return;
        }
		if (!ME.InCombat && !ME.HasAuraById((int)Auras.LeechingPoison))
	        {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.LeechingPoison);
			return;
        }
		
	 if (!ME.InCombat && !ME.HasAuraById((int)Auras.Stealth) && Environment.TickCount - lastStealthTick > 2000 && !ObjectManager.LocalPlayer.IsMounted)
         {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Stealth);
			lastStealthTick = Environment.TickCount;
			return;
        }
		
		if (DetectKeyPress.GetKeyState(DetectKeyPress.Alt) < 0 && AI.Controllers.Spell.CanCast((int)Spells.BF))
			{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BF);
                    return;
            }
		
//	if (ME.HasAuraById((int)Auras.Stealth) && TARGET.Position.Distance3DFromPlayer < 8)
//			{
//                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Ambush);
//                    return;
//            }	

	if (ME.HealthPercent <= CCSettings.Recup && !ME.HasAuraById((int)Auras.Recup) && AI.Controllers.Spell.CanCast((int)Spells.Recup))
			{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Recup);
                    return;
            }
			
			
	if (TARGET.Health >= 1 && ME.InCombat)
	{
	
	
	/////////////////////////////////////////////////////////////////////////////Combat SPEC////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	if (ME.HasAuraById((int)Auras.CombatCheck))
	{

	if (!ME.HasAuraById((int)Auras.Stealth))
	{
	
		if (ME.HealthPercent <= CCSettings.Recup && !ME.HasAuraById((int)Auras.Recup) && AI.Controllers.Spell.CanCast((int)Spells.Recup))
			{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Recup);
                    return;
            }
			
			
	if (DetectKeyPress.GetKeyState(DetectKeyPress.Alt) < 0 && AI.Controllers.Spell.CanCast((int)Spells.BF))
			{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.BF);
                    return;
            }
	
			
	if (!ME.HasAuraById((int)Auras.SnD) && AI.Controllers.Spell.CanCast((int)Spells.SnD)
	|| ME.HasAuraById((int)Auras.SnD) && AI.Controllers.Spell.CanCast((int)Spells.SnD) && ME.Auras.Where(x => x.SpellId == (int)Auras.SnD && x.TimeLeft < 3000).Any())
			{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SnD);
                    return;
            }
			
	if (!TARGET.HasAuraById((int)Auras.Rup) && AI.Controllers.Spell.CanCast((int)Spells.Rup) && ME.ComboPoints >= 5 && !ME.HasAuraById((int)Auras.BF)
	|| TARGET.HasAuraById((int)Auras.Rup) && AI.Controllers.Spell.CanCast((int)Spells.Rup) && TARGET.Auras.Where(x => x.SpellId == (int)Auras.Rup && x.TimeLeft < 3000).Any() && !ME.HasAuraById((int)Auras.BF))
			{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Rup);
                    return;
            }
			
	if (TARGET.HasAuraById((int)Auras.Rup) && ME.ComboPoints >= 5 && AI.Controllers.Spell.CanCast((int)Spells.Evis) && TARGET.Auras.Where(x => x.SpellId == (int)Auras.Rup && x.TimeLeft > 8000).Any() && ME.Auras.Where(x => x.SpellId == (int)Auras.SnD && x.TimeLeft > 8000).Any()
	|| ME.ComboPoints >= 5 && AI.Controllers.Spell.CanCast((int)Spells.Evis) && ME.Auras.Where(x => x.SpellId == (int)Auras.SnD && x.TimeLeft > 8000).Any() && ME.HasAuraById((int)Auras.BF))
			{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Evis);
                    return;
            }
	
	if (!TARGET.HasAuraById((int)Auras.RS) && AI.Controllers.Spell.CanCast((int)Spells.RS)
	|| TARGET.HasAuraById((int)Auras.RS) && AI.Controllers.Spell.CanCast((int)Spells.RS) && TARGET.Auras.Where(x => x.SpellId == (int)Auras.RS && x.TimeLeft < 3000).Any())
			{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.RS);
                    return;
            }
	
	if (ME.ComboPoints < 5 && AI.Controllers.Spell.CanCast((int)Spells.SS))
				{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SS);
                    return;
            }
	}
} //Combat Check




//////////////////////////////////////////////////////////////////////3	
}////////////////////////End Single Target Rotation///////////////////
}/////////////////////////////////////////////////////////////////////
#endregion

#region AOE>4 rotation
 private void castNextSpellbyAOEPriority(WowUnit TARGET)
{
		var MyEnergy = ObjectManager.LocalPlayer.GetPower(Anthrax.WoW.Classes.ObjectManager.WowUnit.WowPowerType.Energy);
		
	 if (!ME.InCombat && !ME.HasAuraById((int)Auras.Stealth) && Environment.TickCount - lastStealthTick > 2000 && !ObjectManager.LocalPlayer.IsMounted)
         {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Stealth);
			lastStealthTick = Environment.TickCount;
			return;
        }
		
	if (ME.HasAuraById((int)Auras.Stealth) && TARGET.Position.Distance3DFromPlayer < 8)
			{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Ambush);
                    return;
            }	

		
		
	if (TARGET.Health >= 1 && ME.InCombat)
	{
	
	
	/////////////////////////////////////////////////////////////////////////////Combat SPEC////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	if (ME.HasAuraById((int)Auras.CombatCheck))
	{

	if (!ME.HasAuraById((int)Auras.Stealth))
	{
	
	if (ME.ComboPoints >= 5 && AI.Controllers.Spell.CanCast((int)Spells.CT))
				{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.CT);
                    return;
            }
	
	if (ME.ComboPoints < 5 && AI.Controllers.Spell.CanCast((int)Spells.FoK))
				{
                    WoW.Internals.ActionBar.ExecuteSpell((int)Spells.FoK);
                    return;
            }
	
	
	
	}
} //Combat Check
}
}
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

                using (StreamReader rd = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "\\Combats\\ARogue.xml"))
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
	
	public int Recup = 90;
		
	//Healing Settings
		[XmlIgnore]
        [CategoryAttribute("Survival Settings"),
        DisplayName("Recuperate Hp Limit"), DefaultValueAttribute(90)]
        public int _Recup
        {
            get
            {
                return Recup;
            }
            set
            {
                Recup = value;
            }
        }

	}
}}