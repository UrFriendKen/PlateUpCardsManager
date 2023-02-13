using Kitchen;
using KitchenCardsManager.Helpers;
using KitchenData;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;

namespace KitchenCardsManager
{
    internal class CardsManagerController : GameSystemBase
    {
        internal enum CardAddStatus
        {
            Success,
            AlreadyObtained,
            RequirementsNotMet
        }

        internal static bool IsInKitchen { get => GameInfo.CurrentScene == SceneType.Kitchen; }

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
                FromFranchise = false
            });
            Set<CSkipShowingRecipe>(entity);
            Set<CProgressionOption.Selected>(entity);
        }

        internal static bool AddProgressionUnlock(int unlockID, out string statusMessage)
        {
            if (CanBeAddedToRun(unlockID, out statusMessage))
            {
                UnlocksToAdd.Enqueue(unlockID);
                return true;
            }
            return false;
        }

        internal static bool CanBeAddedToRun(int unlockID, out string statusMessage)
        {
            if (CurrentScene != SceneType.Kitchen)
            {
                statusMessage = "You must be in a restaurant to add cards!";
                return false;
            }
            Unlock unlock = UnlockHelpers.GetAllUnlocksEnumerable().Where(x => x.ID == unlockID).First();

            if (CurrentUnlockIDs.Contains(unlockID) || UnlocksToAdd.Contains(unlockID))
            {
                statusMessage = $"{unlock.Name} already added!";
            }
            if (!unlock.IsUnlockable)
            {
                statusMessage = $"{unlock.Name} is not unlockable!";
                return false;
            }
            if (unlock.UnlockGroup == UnlockGroup.FranchiseCard)
            {
                statusMessage = $"You can only get {unlock.Name} by franchising!";
                return false;
            }
            if (GameInfo.AllCurrentCards.Intersect(unlock.Requires).Count() != unlock.Requires.Count())
            {
                statusMessage = "Requirements not met!\nYou must have:";
                foreach (Unlock require in unlock.Requires)
                {
                    statusMessage += $"\n- {require.Name}";
                }
                return false;
            }
            if (GameInfo.AllCurrentCards.Intersect(unlock.BlockedBy).Count() > 0)
            {
                statusMessage = $"Cannot add {unlock.Name}!\nIt is blocked by:";
                foreach (Unlock blocker in unlock.BlockedBy)
                {
                    statusMessage += $"\n- {blocker.Name}";
                }
                return false;
            }
            statusMessage = $"Successfully added {unlock.Name}";
            return true;
        }

        internal static IEnumerable<ICard> GetPresentUnlockBlockers(Unlock unlock)
        {
            return GameInfo.AllCurrentCards.Intersect(unlock.BlockedBy);
        }

        internal static IEnumerable<ICard> GetMissingUnlockRequirements(Unlock unlock)
        {
            return unlock.Requires.Where(x => !GameInfo.AllCurrentCards.Contains(x));
        }
    }
}
