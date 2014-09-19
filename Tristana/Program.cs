using System;
using LeagueSharp;
using SharpDX;
using LeagueSharp.Common;
using System.Collections.Generic;

namespace Tristana
{
    internal class Program
    {
        public const string ChampionName = "Tristana";
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q, W, E, R;
        public static Menu Config;
        private static Obj_AI_Hero Player;

        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.BaseSkinName != ChampionName) return;
            Game.PrintChat("Loading 'Rocket Girl Tristana'...");
            Q = new Spell(SpellSlot.Q, 550);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, 550);
            R = new Spell(SpellSlot.R, 550);

            Config = new Menu("RocketGirl Tristana", ChampionName, true);

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));

            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.AddSubMenu(new Menu("Combo", "Combo"));

            
            Game.OnGameUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            Config.AddToMainMenu();
            Game.PrintChat("'Rocket Girl Tristana' Loaded!");
            
        }

        

        private static void Game_OnGameUpdate(EventArgs args)
        {
            Q.Range = 541 + 9 * (Player.Level - 1);
            E.Range = 541 + 9 * (Player.Level - 1);
            R.Range = 541 + 9 * (Player.Level - 1);
        }

        private static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
        }

        private static void Orbwalking_AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
        }


    }
}
