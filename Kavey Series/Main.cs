using System;
using System.Collections.Generic;
using BattleRight.Core;
using BattleRight.Sandbox;
using BattleRight.SDK.UI;
using BattleRight.SDK.UI.Models;
using Kavey_Series.AntiGapclose;
using Kavey_Series.Champions;
using Kavey_Series.Utilities;

namespace Kavey_Series
{
    public class Main : IAddon
    {
        private static Dictionary<string, IChampion> LoadedChampions;
        private static IChampion Champion;
        private static Menu Menu;

        public void OnInit()
        {
            Menu = new Menu("kavey_series", "Kavey Series");
            MainMenu.AddMenu(Menu);
            LoadedChampions = new Dictionary<string, IChampion>();
            Game.OnMatchStart += Game_OnMatchStart;
            Game.OnMatchEnd += Game_OnMatchEnd;
            AntiGapcloser.Initialize();
        }

        private void Game_OnMatchStart(EventArgs args)
        {
            try
            {
                var champName = Utility.Player.CharName;
                if (champName == "Shen Rao")
                {
                    champName = "Shen";
                }
                if (LoadedChampions.ContainsKey(champName))
                {
                    LoadedChampions[champName].Initialize();
                    Champion = LoadedChampions[champName];
                    return;
                }
                if (Champion != null)
                    return;
                var addonName = "Kavey_Series.Champions." + champName;
                var type = Type.GetType(addonName, true);
                Champion = (IChampion)Activator.CreateInstance(type);
                LoadedChampions.Add(champName, Champion);
            }
            catch (Exception ex)
            {
                Utility.Log("[Kavey Series] Champion {0} not supported.", ConsoleColor.Red, Utility.Player.CharName);
                Utility.Log("[Exception] - {0} - {1}", ConsoleColor.Red, ex.Message, ex.StackTrace);
            }
        }

        private void Game_OnMatchEnd(EventArgs args)
        {
            if (Champion == null)
                return;
            Utility.Log($"[Kavey Series] {Champion.ChampionName} unloaded.", ConsoleColor.Red);
            Champion.Destroy();
            Champion = null;
        }

        public void OnUnload()
        {
            //foreach (var champion in LoadedChampions)
            //{
            //    Utility.Log($"[Kavey Series] {Champion.ChampionName} unloaded.", ConsoleColor.Red);
            //    champion.Value.Destroy();
            //}
            //Champion = null;
            //LoadedChampions = null;
        }
    }
}
