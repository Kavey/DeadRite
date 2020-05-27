using BattleRight.SDK.UI.Models;
using Kavey_Series.Abilities;

namespace Kavey_Series.Champions
{
    public interface IChampion
    {
        /*Order
         Champion Properties
         Menu
         Casting Ability
         GetAbilityFromIndex
         InitializeMenu
         InitializeAbilities
         */
        bool Initialized { get; set; }

        string ChampionName { get; set; }
        Ability M1 { get; set; }
        Ability M2 { get; set; }
        Ability EX1 { get; set; }
        Ability EX2 { get; set; }
        Ability Space { get; set; }
        Ability Q { get; set; }
        Ability E { get; set; }
        Ability R { get; set; }
        Ability F { get; set; }

        Menu Menu { get; set; }
        Menu Keys { get; set; }
        Menu Combo { get; set; }
        Menu AntiGapclosing { get; set; }
        Menu Drawings { get; set; }

        Ability CastingAbility { get; set; }
        Ability GetAbilityFromIndex(int index);

        void Initialize();
        void InitializeMenu();
        void InitializeAbilities();
        void Destroy();
    }
}