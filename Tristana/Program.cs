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
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q, W, E, R;
        private static Obj_AI_Hero Player;

        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;

        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            Q = new Spell(SpellSlot.Q, 550);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, 550);
            R = new Spell(SpellSlot.R, 550);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.Level > 1)
            {
                Q.Range = 541 + 9 * (Player.Level - 1);
                E.Range = 541 + 9 * (Player.Level - 1);
                R.Range = 541 + 9 * (Player.Level - 1);
            }
        }
    }
}
