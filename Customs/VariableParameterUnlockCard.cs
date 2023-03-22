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

        internal void ResetParameterEffect()
        {
            Effects[0] = new ParameterEffect
            {
                Parameters = new KitchenParameters
                {
                    CustomersPerHour = 0,
                    CustomersPerHourReduction = 0,
                    MaximumGroupSize = 0,
                    MinimumGroupSize = 0,
                    CurrentCourses = 0
                }
            };
        }

        internal void AddParameterEffect(KitchenParameters kitchenParameters)
        {
            ParameterEffect effect = Effects[0] as ParameterEffect;
            KitchenParameters newKitchenParameters = new KitchenParameters
            {
                CustomersPerHour = effect.Parameters.CustomersPerHour + kitchenParameters.CustomersPerHour,
                CustomersPerHourReduction = effect.Parameters.CustomersPerHourReduction + kitchenParameters.CustomersPerHourReduction,
                MaximumGroupSize = effect.Parameters.MaximumGroupSize + kitchenParameters.MaximumGroupSize,
                MinimumGroupSize = effect.Parameters.MinimumGroupSize + kitchenParameters.MinimumGroupSize,
                CurrentCourses = 0
            };
            effect.Parameters = newKitchenParameters;
            Effects[0] = effect;
        }
    }
}
