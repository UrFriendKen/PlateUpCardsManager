using Kitchen;

namespace KitchenCardsManager.Helpers
{
    internal class NetworkHelper
    {
        internal static bool IsHost()
        {
            return Session.CurrentGameNetworkMode == GameNetworkMode.Host;
        }
    }
}
