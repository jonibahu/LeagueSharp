using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gragas
{
    internal class Program
    {
        public const string ChampionName = "Gragas";
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q, Q2, W, E, R;
        public static Menu Config;
        private static Obj_AI_Hero Player;
        private static GameObject QObject = null;
        private static float QObjectCreateTime = 0f;
        private static bool CanUseQLaunch = true;
        private static float QObjectMaxDamageTime = 0f;
        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.BaseSkinName != ChampionName) return;
            Game.PrintChat("Loading 'Roll Out The Barrel'...");

            Q = new Spell(SpellSlot.Q, 1100);
            Q.SetSkillshot(-0.5f, 110f, 1000f, false, SkillshotType.SkillshotCircle);

            Q2 = new Spell(SpellSlot.Q, 250);

            W = new Spell(SpellSlot.W, 0);

            E = new Spell(SpellSlot.E, 1100);
            E.SetSkillshot(0.3f, 50, 1000, true, SkillshotType.SkillshotLine);

            R = new Spell(SpellSlot.R, 1100);
            R.SetSkillshot(0.3f, 700, 1000, false, SkillshotType.SkillshotCircle);

            Config = new Menu("Roll Out The Barrel", ChampionName, true);

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));

            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));

            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));

            Config.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseQLaneClear", "Use Q").SetValue(true));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseWLaneClear", "Use W").SetValue(true));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseELaneClear", "Use E").SetValue(true));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseRLaneClear", "Use R").SetValue(true));

            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("UseEAntiGapcloser", "E on Gapclose (Incomplete)").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("UseRAntiGapcloser", "R on Gapclose (Incomplete)").SetValue(true));

            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;

            Game.OnGameUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            GameObject.OnCreate += OnCreateObject;
            GameObject.OnDelete += OnDeleteObject;
            Config.AddToMainMenu();
            Game.PrintChat("'Roll Out The Barrel' Loaded!");

        }

        private static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            //throw new NotImplementedException();
        }

        private static void Orbwalking_AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            //throw new NotImplementedException();
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                //Game.PrintChat("combo");
                Combo();
            }
            if (Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                //Game.PrintChat("harass");
                Harass();
            }
            if (Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                //Game.PrintChat("laneclear");
                LaneClear();
            }
        }

        private static void Harass()
        {
            var useQ = Config.Item("UseQHarass").GetValue<bool>();

            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            
        }

        private static string gragQStatus()
        {
            if (QObject == null && CanUseQLaunch)
            {
                return "NULL";
            }
            return QObject.ToString();
        }

        private static void LaneClear()
        {
            var useQ = Config.Item("UseQLaneClear").GetValue<bool>();
            var useW = Config.Item("UseWLaneClear").GetValue<bool>();
            var useE = Config.Item("UseELaneClear").GetValue<bool>();
            var useR = Config.Item("UseRLaneClear").GetValue<bool>();

            var rangedMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.Ranged);
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);

            var jungleMinions = MinionManager.GetMinions(ObjectManager.Player.Position, Q.Range, MinionTypes.All, MinionTeam.NotAlly);

            foreach (var minion in jungleMinions)
            {
                allMinions.Add(minion);
            }
            if (useQ && Q.IsReady())
            {
                bool barrelRoll = Player.HasBuff("GragasQ");
                var rangedLocation = Q.GetCircularFarmLocation(rangedMinions);
                var location = Q.GetCircularFarmLocation(allMinions);
                var bLocation = (location.MinionsHit > rangedLocation.MinionsHit + 1) ? location : rangedLocation;

                if (!barrelRoll && bLocation.MinionsHit > 0)
                {
                    Q.Cast(bLocation.Position.To3D());

                }
                if (barrelRoll)
                {
                    int minionsHit = 0;
                    foreach (var minion in allMinions)
                    {
                        if (Vector3.Distance(bLocation.Position.To3D(), minion.ServerPosition) <= Q.Width && Q.GetDamage(minion) > minion.Health)
                        {
                            minionsHit += 1;
                        }
                    }
                    if (minionsHit >= 3)
                    {
                        Q.Cast();
                    }

                }
            }
            if (useE && E.IsReady())
            {
                var rangedLocation = Q.GetCircularFarmLocation(rangedMinions);
                var location = Q.GetCircularFarmLocation(allMinions);
                var bLocation = (location.MinionsHit > rangedLocation.MinionsHit + 1) ? location : rangedLocation;
                if (bLocation.MinionsHit > 2)
                {
                    E.Cast(bLocation.Position.To3D());
                }
            }
        }

        private static void OnCreateObject(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Gragas") && sender.Name.Contains("Q_Ally"))
            {
                Game.PrintChat(sender.Name);
                Game.PrintChat("Gragas Q is out!");
                QObject = sender;
                QObjectCreateTime = Game.Time;
                QObjectMaxDamageTime = QObjectCreateTime + 2;
                CanUseQLaunch = false;
            }

        }
        private static void OnDeleteObject(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Gragas") && sender.Name.Contains("Q_Ally"))
            {
                Game.PrintChat(sender.Name);
                Game.PrintChat("Gragas Q exploded!");
                CanUseQLaunch = true;
            }
        }

        private static void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            //throw new NotImplementedException();
        }

        private static void Combo()
        {
            var useQ = Config.Item("UseQCombo").GetValue<bool>();
            var useW = Config.Item("UseWCombo").GetValue<bool>();
            var useE = Config.Item("UseECombo").GetValue<bool>();
            var useR = Config.Item("UseRCombo").GetValue<bool>();

            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);


            if (useQ) { ComboQ(target); ComboQ2(target); }
            if (useW) { ComboW(target); }
            if (useE) { ComboE(target); }
            if (useR) { ComboR(target); }
        }

        private static void ComboQ(Obj_AI_Hero target)
        {
            if (Q.IsReady() && QObject == null && target.IsValidTarget(Q.Range))
            {
                PredictionOutput pred = Prediction.GetPrediction(target, Q.Delay, Q.Width / 2, Q.Speed);
                if (pred.Hitchance == HitChance.Medium)
                {
                    Q.Cast(pred.CastPosition, true);
                }
            }
        }
        private static void ComboQ2(Obj_AI_Hero target)
        {
            if (QObject != null)
            {
                if (target.Distance(QObject.Position) < Q2.Range)
                {
                    Q.Cast();
                }
            }
        }
        private static void ComboW(Obj_AI_Hero target)
        {
            if (W.IsReady() && Player.Distance(target) < 250)
            {
                W.Cast();
            }
        }
        private static void ComboE(Obj_AI_Hero target)
        {
            if (E.IsReady() && target.IsValidTarget(E.Range))
            {
                PredictionOutput pred = Prediction.GetPrediction(target, E.Delay, E.Width / 2, E.Speed);
                if (E.WillHit(target, pred.CastPosition))
                {
                    E.Cast(pred.CastPosition, true);
                }
            }
        }
        private static void ComboR(Obj_AI_Hero target)
        {
            Game.PrintChat("R Killable: " + R.IsKillable(target));
            if (R.IsReady() && target.IsValidTarget(E.Range) && R.IsKillable(target))
            {
                PredictionOutput pred = Prediction.GetPrediction(target, R.Delay, R.Width / 2, R.Speed);
                R.Cast(pred.CastPosition, true);
            }
        }
    }
}
