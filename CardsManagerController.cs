using Kitchen;
using KitchenCardsManager.Helpers;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;

namespace KitchenCardsManager
{
    internal class CardsManagerController : GameSystemBase
    {
        internal static SceneType CurrentScene { get => GameInfo.CurrentScene; }

        internal static HashSet<int> CurrentUnlockIDs { get => GameInfo.AllCurrentCards.Select(x => x.CardID).ToHashSet(); }

        protected static Queue<int> UnlocksToAdd = new Queue<int>();

        protected override void Initialise()
        {
            base.Initialise();
        }

        protected override void OnUpdate()
        {
            if (UnlocksToAdd.Count < 1)
            {
                return;
            }
            int unlockID = UnlocksToAdd.Dequeue();
            if (!UnlockHelpers.IsValidUnlock(unlockID))
            {
                return;
            }
            Entity entity = EntityManager.CreateEntity();
            Set(entity, new CProgressionOption()
            {
                ID = unlockID,
                FromFranchise = true
            });
            Set<CSkipShowingRecipe>(entity);
            Set<CProgressionOption.Selected>(entity);
        }

        internal static bool AddProgressionUnlock(int unlockID)
        {
            if (UnlocksToAdd.Contains(unlockID) || CurrentUnlockIDs.Contains(unlockID) || CurrentScene != SceneType.Kitchen)
            {
                return false;
            }
            UnlocksToAdd.Enqueue(unlockID);
            return true;
        }
    }
}
