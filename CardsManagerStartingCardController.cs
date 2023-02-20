using Kitchen;
using KitchenData;
using Unity.Entities;
using static Kitchen.CreateCardSetsFromSetting;

namespace KitchenCardsManager
{
    [UpdateInGroup(typeof(ChangeModeGroup))]
    [UpdateBefore(typeof(CreateCardSetsFromSetting))]
    internal class CardsManagerStartingCardController : GameSystemBase
    {
        public class StartingUnlockData
        {
            public RestaurantSetting RestaurantSetting;
            public Unlock StartingUnlock;
        }

        public static StartingUnlockData Output;

        protected override void Initialise()
        {
            base.Initialise();
        }

        protected override void OnUpdate()
        {
            if (!Has<CHasCreated>() && RequireEntity<SLayout>(out var comp) && Require(comp, out CSetting comp2) && GameData.Main.TryGet<RestaurantSetting>(comp2.RestaurantSetting, out var output))
            {
                if (GameData.Main.TryGet<RestaurantSetting>(comp2.RestaurantSetting, out var output2))
                {
                    Output = new StartingUnlockData()
                    {
                        RestaurantSetting = output,
                        StartingUnlock = output.StartingUnlock
                    };
                }
            }
            else
            {
                Output = null;
            }
        }
    }
}
