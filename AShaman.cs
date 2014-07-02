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
    class Shamen : Modules.ICombat
    {
        #region private vars
        bool isAOE;
        WowLocalPlayer ME;
		Settings CCSettings = new Settings();
		private System.Timers.Timer wndCloser = new System.Timers.Timer(2000);
        //Stopwatch stopwatch;
        //List<long> averageScanTimes;
        #endregion

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        public override string Name
        {
            get { return "A Shaman by Koha"; }                      //This is the name displayed in SPQR's Class selection DropdownList
        }

        #region enums
        internal enum Spells : int                      //This is a convenient list of all spells used by our combat routine
        {		//you can have search on wowhead.com for spell name, and get the id in url
            //Enchance
			FlameShock = 8050,
            StormStrike = 17364,
            LavaLash = 60103,
            LightingBolt = 403,
            LightningShield = 324,
            WaterShield = 52127,
            HealingSurge = 8004,
            PrimalStrike = 73899,
            HearthShock = 8042,
			FTW = 8024,
			WFW = 8232,
			SearingTotem = 3599,
			MagmaTotem = 8190,
			ElementalBlast = 117014,
			UnleashE = 73680,
			StormBlast = 115356,
			ChainL = 421,
			FireNova = 1535,
			ManaTideTotem = 16190,
			HealingTideTotem = 108280,
			HealingStreamTotem = 5394,
			
			//Resto
			WaterS = 52127,
			ELW = 51730,
			ChainHeal = 1064,
			Riptide = 61295,
			Hrain = 73920,
			ES = 974,
			HW = 331,
			GHW = 77472,
			
		
        }

        public enum Auras : int
        {
            FlameShock = 8050,
            WeakenedBlows = 115798,
            LightningShield = 324,
			EnhanceCheck = 30809,
			FTW = 10400,
			WFW = 33757,
			MWeapon = 53817,
			UnleashF = 73683,
			SearingFlame = 77661,
			RestoCheck = 16196,
			ELW = 52007,
			WaterS = 52127,
			ES = 974,
			Riptide = 61295,
        }

        public enum FireTotems : int
        {
            SearingTotem = 3599,
            MagmaTotem = 8190,
            FireElementalTotem = 2894,

        }


#endregion

		DateTime start_time = DateTime.Now; 
		bool flag = false;
		
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

	var IsCasting = ObjectManager.LocalPlayer.IsCasting;
	Random rnd = new Random();
	int seed = rnd.Next(250,1000);
	TimeSpan timeDiff = DateTime.Now - start_time;

if (ME.HasAuraById((int)Auras.EnhanceCheck))
{	
	if (!ME.HasAuraById((int)Auras.FTW))
	    {
        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.FTW);
        return;
        }
		
		if (!ME.HasAuraById((int)Auras.WFW))
	    {
        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.WFW);
        return;
        }
		
		if (!ME.HasAuraById((int)Auras.LightningShield))
	    {
        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.LightningShield);
        return;
        }	
}


if (ME.HasAuraById((int)Auras.RestoCheck))
{	
	if (!ME.HasAuraById((int)Auras.ELW))
	    {
        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ELW);
        return;
        }
		
		if (!ME.HasAuraById((int)Auras.WaterS))
	    {
        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.WaterS);
        return;
        }	
}


if (TARGET.Health >= 1 && ME.InCombat)
{ //Combat Check
												///////////////////////////Enhancement////////////////////////
if (ME.HasAuraById((int)Auras.EnhanceCheck))
{ //Spec Check

//Totem Codes
					
					
		if (TARGET.Position.Distance3DFromPlayer < 7)
		{
		if( ((int)timeDiff.TotalMilliseconds > 60000+seed)  || !flag)
			{
				//SPQR.Logger.WriteLine("Casting searing totem, seconds= "+(int)timeDiff.TotalMilliseconds);
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SearingTotem);
				
				while(!AI.Controllers.Spell.CanCast((int)Spells.SearingTotem))
					System.Threading.Thread.Sleep(30);
					
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SearingTotem); 
				
				start_time = DateTime.Now; 
				flag=true;
			}
		}
		
				
			if (ME.Auras.Where(x => x.SpellId == (int)Auras.MWeapon && x.StackCount >= 5).Any() && AI.Controllers.Spell.CanCast((int)Spells.ElementalBlast))
                        {
                            
                            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ElementalBlast);
                            return;
                        }
						
			if (AI.Controllers.Spell.CanCast((int)Spells.UnleashE))
                        {
                            
                            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.UnleashE);
                            return;
                        }
				
			if (ME.Auras.Where(x => x.SpellId == (int)Auras.MWeapon && x.StackCount >= 5).Any() && AI.Controllers.Spell.CanCast((int)Spells.LightingBolt))
                        {
                            
                            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.LightingBolt);
                            return;
                        }
			
			            if (AI.Controllers.Spell.CanCast((int)Spells.LavaLash) && ME.Auras.Where(x => x.SpellId == (int)Auras.SearingFlame && x.StackCount >= 5).Any())
                        {
                            
                            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.LavaLash);
                            return;
                        }

            if (AI.Controllers.Spell.CanCast((int)Spells.StormStrike))
                        {
                           
                            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.StormStrike);
                            return;
                        }
            if (AI.Controllers.Spell.CanCast((int)Spells.StormBlast))
                        {
                           
                            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.StormBlast);
                            return;
                        }
				
             if (!TARGET.HasAuraById((int)Auras.FlameShock) && AI.Controllers.Spell.CanCast((int)Spells.FlameShock)
				|| AI.Controllers.Spell.CanCast((int)Spells.FlameShock) && !TARGET.HasAuraById((int)Auras.FlameShock) && ME.HasAuraById((int)Auras.UnleashF)) 
                        {

                            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.FlameShock);
                            return;
                        }

            if (AI.Controllers.Spell.CanCast((int)Spells.LavaLash))
                        {
                            
                            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.LavaLash);
                            return;
                        }

            if (AI.Controllers.Spell.CanCast((int)Spells.HearthShock) && TARGET.Auras.Where(x => x.SpellId == (int)Auras.FlameShock && x.TimeLeft >= 6000).Any())
                        {
                           
                            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HearthShock);
                            return;
                        }

						

} //End of Spec Check



/////////////////////////////////////////////////////Resto//////////////////////////////////////////////////////

if (ME.HasAuraById((int)Auras.RestoCheck))
{
//Mouseover Light's Hammer while Pressing Alt
	if (DetectKeyPress.GetKeyState(DetectKeyPress.Alt) < 0)
                {
                    if (AI.Controllers.Spell.CanCast((int)Spells.Hrain)
                         && !IsCasting)
                    {
                        WoW.Internals.MouseController.RightClick();
                        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Hrain);
                        WoW.Internals.MouseController.LockCursor();
                        WoW.Internals.MouseController.MoveMouse(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                        WoW.Internals.MouseController.LeftClick();
                        WoW.Internals.MouseController.UnlockCursor();
                    }

                    return;

                }

	if (ME.HealthPercent <= 95 && TARGET.HealthPercent <= 95 && AI.Controllers.Spell.CanCast((int)Spells.HealingStreamTotem))
               {          
               WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HealingStreamTotem);
               return;
               }
			   
	if (DetectKeyPress.GetKeyState(DetectKeyPress.Shift) < 0)
                {
                    if (AI.Controllers.Spell.CanCast((int)Spells.ES)
                         && !IsCasting)
                    {
                        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ES);
                    }

                    return;

                }
				
					if (TARGET.HealthPercent <= CCSettings.GHW)
                    {
                        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.GHW);
                    }
				
	if (TARGET.HealthPercent <= CCSettings.Riptide && AI.Controllers.Spell.CanCast((int)Spells.Riptide) && !TARGET.HasAuraById((int)Auras.Riptide))
                    {
                        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Riptide);
                    }
	if (TARGET.HealthPercent <= CCSettings.ChainHeal && ME.HealthPercent <= CCSettings.ChainHeal && AI.Controllers.Spell.CanCast((int)Spells.ChainHeal))
                    {
                        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ChainHeal);
                    }
	if (TARGET.HealthPercent <= CCSettings.HW && AI.Controllers.Spell.CanCast((int)Spells.HW))
                    {
                        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HW);
                    }
					
					
					
} //Spec Check Resto
} //Combat Check
} //End Single Code
#endregion

#region AOE>4 rotation
 private void castNextSpellbyAOEPriority(WowUnit TARGET)
{
	
	
	var IsCasting = ObjectManager.LocalPlayer.IsCasting;
	Random rnd = new Random();
	int seed = rnd.Next(250,1000);
	TimeSpan timeDiff = DateTime.Now - start_time;
	
	if (!ME.HasAuraById((int)Auras.FTW))
	    {
        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.FTW);
        return;
        }
		
		if (!ME.HasAuraById((int)Auras.WFW))
	    {
        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.WFW);
        return;
        }
		
		if (!ME.HasAuraById((int)Auras.LightningShield))
	    {
        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.LightningShield);
        return;
        }	
		
if (TARGET.Health >= 1 && ME.InCombat)
{ //Combat Check

//////////////////////////////////////////////////////////Enhance AoE/////////////////////////////////////////////

//Totem Codes
if (ME.HasAuraById((int)Auras.EnhanceCheck))					
{					
		if (TARGET.Position.Distance3DFromPlayer < 7)
		{
		if( ((int)timeDiff.TotalMilliseconds > 60000+seed)  || !flag)
			{
				//SPQR.Logger.WriteLine("Casting searing totem, seconds= "+(int)timeDiff.TotalMilliseconds);
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.MagmaTotem);
				
				while(!AI.Controllers.Spell.CanCast((int)Spells.MagmaTotem))
					System.Threading.Thread.Sleep(30);
					
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.MagmaTotem); 
				
				start_time = DateTime.Now; 
				flag=true;
			}
		}
		
		
				
			if (ME.Auras.Where(x => x.SpellId == (int)Auras.MWeapon && x.StackCount >= 5).Any() && AI.Controllers.Spell.CanCast((int)Spells.ElementalBlast))
                        {
                            
                            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ElementalBlast);
                            return;
                        }
						
			if (AI.Controllers.Spell.CanCast((int)Spells.UnleashE))
                        {
                            
                            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.UnleashE);
                            return;
                        }
				
			if (ME.Auras.Where(x => x.SpellId == (int)Auras.MWeapon && x.StackCount >= 5).Any() && AI.Controllers.Spell.CanCast((int)Spells.ChainL))
                        {
                            
                            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ChainL);
                            return;
                        }
			
			            if (AI.Controllers.Spell.CanCast((int)Spells.LavaLash) && ME.Auras.Where(x => x.SpellId == (int)Auras.SearingFlame && x.StackCount >= 5).Any() ||
						TARGET.HasAuraById((int)Auras.FlameShock) && AI.Controllers.Spell.CanCast((int)Spells.LavaLash))
                        {
                            
                            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.LavaLash);
                            return;
                        }
						
						if (AI.Controllers.Spell.CanCast((int)Spells.FireNova) && TARGET.HasAuraById((int)Auras.FlameShock))
                        {
                           
                            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.FireNova);
                            return;
                        }
						
						
             if (!TARGET.HasAuraById((int)Auras.FlameShock) && AI.Controllers.Spell.CanCast((int)Spells.FlameShock)
				|| AI.Controllers.Spell.CanCast((int)Spells.FlameShock) && !TARGET.HasAuraById((int)Auras.FlameShock) && ME.HasAuraById((int)Auras.UnleashF)) 
                        {

                            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.FlameShock);
                            return;
                        }
						


            if (AI.Controllers.Spell.CanCast((int)Spells.StormStrike))
                        {
                           
                            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.StormStrike);
                            return;
                        }
            if (AI.Controllers.Spell.CanCast((int)Spells.StormBlast))
                        {
                           
                            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.StormBlast);
                            return;
                        }
				


            if (AI.Controllers.Spell.CanCast((int)Spells.LavaLash))
                        {
                            
                            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.LavaLash);
                            return;
                        }

            if (AI.Controllers.Spell.CanCast((int)Spells.HearthShock) && TARGET.Auras.Where(x => x.SpellId == (int)Auras.FlameShock && x.TimeLeft >= 6000).Any())
                        {
                           
                            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HearthShock);
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

	
	        public bool FireTotemActive()
        {
            WoW.Classes.Frames.WowFrame totemFrame = WoW.Internals.UIFrame.GetFrameByName("TotemFrameTotem1");

            if (totemFrame.IsValid && totemFrame.IsVisible)
                return true;
            return false;
        }

        public bool PlaceFireTotem()
        {
            WoW.Classes.Frames.WowFrame totemFrame = new WoW.Classes.Frames.WowFrame(0);

            List<WoW.Classes.WowActionBarSlot> fireTotemsSpells = WoW.Internals.ActionBar.GetFilledSlots().Where(x => x.IsSpell && Enum.IsDefined(typeof(FireTotems), x.ActionId)).ToList();

            if (fireTotemsSpells.Count > 0)
            {
                WoW.Internals.ActionBar.PressSlot(fireTotemsSpells.First().BarIndex, fireTotemsSpells.First().SlotIndex);

                totemFrame = WoW.Internals.UIFrame.GetFrameByName("TotemFrameTotem1");
                
                if (totemFrame.IsValid && totemFrame.IsVisible)
                    return true;
            }

            //actionId ?
            List<WoW.Classes.WowActionBarSlot> fireTotemsFlyouts = WoW.Internals.ActionBar.GetFilledSlots().Where(x => x.IsFlyout && x.ActionId == 80 ).ToList();

            if (fireTotemsFlyouts.Count > 0)
            {
                WoW.Classes.WowActionBarSlot actionSlot = fireTotemsFlyouts.First();

                WoW.Internals.ActionBar.PressSlot(actionSlot.BarIndex, actionSlot.SlotIndex);
                System.Threading.Thread.Sleep(500);

                WoW.Classes.Frames.WowFrame FireElementalTotemButton = WoW.Internals.UIFrame.GetFrameByName("SpellFlyoutButton3");
                WoW.Classes.Frames.WowFrame MagmaTotemButton = WoW.Internals.UIFrame.GetFrameByName("SpellFlyoutButton2");
                WoW.Classes.Frames.WowFrame SearingTotemButton = WoW.Internals.UIFrame.GetFrameByName("SpellFlyoutButton1");

                if (FireElementalTotemButton.IsValid &&
                   FireElementalTotemButton.IsVisible)
                {
                    FireElementalTotemButton.LeftClick();
                }
                else if (MagmaTotemButton.IsValid &&
                   MagmaTotemButton.IsVisible)
                {
                    MagmaTotemButton.LeftClick();
                }
                else if (SearingTotemButton.IsValid &&
                   SearingTotemButton.IsVisible)
                {
                    SearingTotemButton.LeftClick();
                }

                totemFrame = WoW.Internals.UIFrame.GetFrameByName("TotemFrameTotem1");
            }

            return totemFrame.IsVisible && totemFrame.IsValid;
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
        public int GHW = 60;
		public int Riptide = 95;
		 public int ChainHeal = 93;
		public int HW = 90;
	//Healing Settings
	//Greater Healing Wave
		[XmlIgnore]
        [CategoryAttribute("Restoration Settings"),
        DisplayName("Greater Healing Wave Hp Limit"), DefaultValueAttribute(60)]
        public int _GHW
        {
            get
            {
                return GHW;
            }
            set
            {
                GHW = value;
            }
        }
	//RipTide
		[XmlIgnore]
        [CategoryAttribute("Restoration Settings"),
        DisplayName("Riptide Hp Limit"), DefaultValueAttribute(95)]
        public int _Riptide
        {
            get
            {
                return Riptide;
            }
            set
            {
                Riptide = value;
            }
        }
		//ChainHeal
		[XmlIgnore]
        [CategoryAttribute("Restoration Settings"),
        DisplayName("Chain Heal Hp Limit"), DefaultValueAttribute(93)]
        public int _ChainHeal
        {
            get
            {
                return ChainHeal;
            }
            set
            {
                ChainHeal = value;
            }
        }	
		//Healing Wave
		[XmlIgnore]
        [CategoryAttribute("Restoration Settings"),
        DisplayName("Healing Wave Hp Limit"), DefaultValueAttribute(90)]
        public int _HW
        {
            get
            {
                return HW;
            }
            set
            {
                HW = value;
            }
        }	

	}
		
}}