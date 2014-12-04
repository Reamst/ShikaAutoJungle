﻿#region
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
#endregion

namespace JeonProject
{
    class Program
    {
        private static Menu baseMenu;
        private static String Champion_name;
        public static SpellSlot smiteSlot = SpellSlot.Unknown;
        public static Spell smite;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
            Game.OnGameUpdate += OnGameUpdate;
        }

        private static void OnGameLoad(EventArgs args) 
        {

            Game.PrintChat("J Project was Activited");
            Game.PrintChat("Champion : " + ObjectManager.Player.BaseSkinName);
            Game.PrintChat("Spells : " + ObjectManager.Player.SummonerSpellbook.GetSpell(SpellSlot.Summoner1).Name + " " + ObjectManager.Player.SummonerSpellbook.GetSpell(SpellSlot.Summoner2).Name);
            setSmiteSlot();
           


            //메인메뉴
            baseMenu = new Menu("ProjectJ", "ProjectJ", true);
            var menu_smite = new Menu("Jsmite", "Jsmite");
            baseMenu.AddSubMenu(menu_smite);

            /* 
             스마이트 세부 메뉴
             */

            menu_smite.AddItem(new MenuItem("AutoSmite", "AutoSmite").SetValue(true));
            menu_smite.AddItem(new MenuItem("smite_holdkey", "Key:").SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Press)));
            baseMenu.AddToMainMenu();
            Champion_name = ObjectManager.Player.BaseSkinName;
            
            //ObjectManager.Player.BaseSkinName <ㅡ 현재 사용하고 있는 챔피언의 기본 스킨 이름
   
        }
        private static void OnGameUpdate(EventArgs args)
        {
            if (baseMenu.Item("AutoSmite").GetValue<bool>() && baseMenu.Item("smite_holdkey").GetValue<KeyBind>().Active)
            {
                double smitedamage;
                bool smiteReady=false;
                smitedamage = setSmiteDamage();
                Drawing.DrawText(ObjectManager.Player.HPBarPosition.X + 10, ObjectManager.Player.HPBarPosition.Y + 36, System.Drawing.Color.Gold, "AutoSmite!");
                Drawing.DrawText(ObjectManager.Player.HPBarPosition.X + 10, ObjectManager.Player.HPBarPosition.Y + 46, System.Drawing.Color.Gold, smiteSlot.ToString());
                Obj_AI_Base mob = GetNearest(ObjectManager.Player.ServerPosition);
                testFind(ObjectManager.Player.ServerPosition);

                Game.PrintChat(smiteSlot.ToString() + "<damage>" + smitedamage);
                Game.PrintChat(ObjectManager.Player.SummonerSpellbook.GetSpell(SpellSlot.Summoner2).Name);
                
                if (ObjectManager.Player.SummonerSpellbook.CanUseSpell(smiteSlot) == SpellState.Ready && Vector3.Distance(ObjectManager.Player.ServerPosition, mob.ServerPosition) < smite.Range)
                {
                    smiteReady = true;
                }

                if (smiteReady && mob.Health < smitedamage)
                {
                    setSmiteSlot();
                    ObjectManager.Player.SummonerSpellbook.CastSpell(smiteSlot, mob);
                }


                
            }


        }

        #region 스마이트
        /* 
         * 스마이트 설정 
         * 
         */

        public static readonly int[] SmitePurple = { 3713, 3726, 3725, 3726, 3723 };
        public static readonly int[] SmiteGrey = { 3711, 3722, 3721, 3720, 3719 };
        public static readonly int[] SmiteRed = { 3715, 3718, 3717, 3716, 3714 };
        public static readonly int[] SmiteBlue = { 3706, 3710, 3709, 3708, 3707 };
        private static readonly string[] MinionNames =
        {
            "TT_Spiderboss", "TTNGolem", "TTNWolf", "TTNWraith",
            "SRU_Blue", "SRU_Gromp", "SRU_Murkwolf", "SRU_Razorbeak", "SRU_Red", "SRU_Krug", "SRU_Dragon", "SRU_Baron","SRU_Crab"
        };


        public static void setSmiteSlot()
        {
            foreach (var spell in ObjectManager.Player.SummonerSpellbook.Spells.Where(spell => String.Equals(spell.Name, smitetype(), StringComparison.CurrentCultureIgnoreCase)))
            {

                smiteSlot = spell.Slot;
                smite = new Spell(smiteSlot, 700);
                return;
            }
        }
        public static string smitetype()
        {
            if (SmiteBlue.Any(Items.HasItem))
            {
                return "s5_summonersmiteplayerganker";
            }
            if (SmiteRed.Any(Items.HasItem))
            {
                return "s5_summonersmiteduel";
            }
            if (SmiteGrey.Any(Items.HasItem))
            {
                return "s5_summonersmitequick";
            }
            if (SmitePurple.Any(Items.HasItem))
            {
                return "itemsmiteaoe";
            }
            return "summonersmite";
        }
        public static Obj_AI_Minion GetNearest(Vector3 pos)
        {
            var minions =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(minion => minion.IsValid && MinionNames.Any(name => minion.Name.StartsWith(name)) && !MinionNames.Any(name => minion.Name.Contains("Mini")));
            var objAiMinions = minions as Obj_AI_Minion[] ?? minions.ToArray();
            Obj_AI_Minion sMinion = objAiMinions.FirstOrDefault();
            double? nearest = null;
            foreach (Obj_AI_Minion minion in objAiMinions)
            {
                double distance = Vector3.Distance(pos, minion.Position);
                if (nearest == null || nearest > distance)
                {
                    nearest = distance;
                    sMinion = minion;
                }
            }
            return sMinion;
        }
        public static double setSmiteDamage()
        {
            int level = ObjectManager.Player.Level;
            int[] damage =
            {
                20*level + 370,
                30*level + 330,
                40*level + 240,
                50*level + 100
            };
            return damage.Max();
        }
        public static void testFind(Vector3 pos)
        {
            double? nearest = null;
            var minions =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(minion => minion.IsValid);
            var objAiMinions = minions as Obj_AI_Minion[] ?? minions.ToArray();
            Obj_AI_Minion sMinion = objAiMinions.FirstOrDefault();
            foreach (Obj_AI_Minion minion in minions)
            {
                double distance = Vector3.Distance(pos, minion.Position);
                if (nearest == null || nearest > distance)
                {
                    nearest = distance;
                    sMinion = minion;
                }
            }
            Game.PrintChat("Minion name is: " + sMinion.Name);
        }
        #endregion
    }
}
