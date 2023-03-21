using KitchenData;
using KitchenLib.Customs;
using System.Collections.Generic;

namespace KitchenCardsManager.Customs
{
    internal class VariableParameterUnlockCard : CustomUnlockCard
    {
        public override string UniqueNameID => Main.CARDS_MANAGER_VARIABLE_PARAMETER_UNLOCK_CARD_UNIQUENAMEID;

        public override List<UnlockEffect> Effects { get; protected set; } = new List<UnlockEffect>()
        {
            new ParameterEffect
            {
                Parameters = new KitchenParameters
                {

                }
            }
        };

        public override bool IsUnlockable => false;

        public override List<(Locale, UnlockInfo)> InfoList => new List<(Locale, UnlockInfo)>
        {
            (Locale.English, new UnlockInfo
            {
                Name = "",
                Description = "",
                Locale = Locale.English,
                FlavourText = ""
            })
        };

        internal void UpdateParameterEffect(KitchenParameters kitchenParameters)
        {
            Effects[0] = new ParameterEffect
            {
                Parameters = kitchenParameters
            };
        }
    }
}
