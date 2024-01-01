using Kitchen;
using KitchenCardsManager.Helpers;
using KitchenData;
using TMPro;
using UnityEngine;

namespace KitchenCardsManager
{
    public class UnlockCardState : MonoBehaviour
    {
        private TextMeshPro AutoAddIcon;

        public UnlockCardState(UnlockCardElement attachTo)
        {
            if (attachTo == null)
                return;
            Transform body = attachTo.transform.Find("Body");
            GameObject iconGO = body?.Find("Icon")?.gameObject;
            if (iconGO != null)
            {
                GameObject autoAddIconGO = GameObject.Instantiate(iconGO);
                autoAddIconGO.name = "Auto Add Icon";
                autoAddIconGO.transform.SetParent(body);
                autoAddIconGO.transform.localScale = iconGO.transform.localScale * 0.8f;
                autoAddIconGO.transform.localRotation = iconGO.transform.localRotation;
                autoAddIconGO.transform.localPosition = new Vector3(-1.15f, 0.5f, -0.13f);
                autoAddIconGO.SetActive(false);
                AutoAddIcon = autoAddIconGO.GetComponent<TextMeshPro>();
            }
        }

        public void UpdateAutoAdd(Unlock unlock)
        {
            if (AutoAddIcon == null)
                return;
            bool isAutoAdd = UnlockHelpers.GetAutoAddState(unlock);
            if (isAutoAdd)
            {
                string colorHex;
                if (!Main.PrefManager.Get<bool>(Main.CARDS_MANAGER_ADD_REMOVE_VALIDITY_CHECKING))
                    colorHex = "#00ff00";
                else if (!UnlockHelpers.GetDefaultEnabledState(unlock) || UnlockHelpers.GetBlockedBysAutoAddState(unlock))
                    colorHex = "#ff0000";
                else if (!UnlockHelpers.GetRequirementsAutoAddState(unlock))
                    colorHex = "#ffff00";
                else
                    colorHex = "#00ff00";
                AutoAddIcon.SetText($"<color={colorHex}><sprite name=\"upgrade\" tint=1>");
            }
            AutoAddIcon.gameObject.SetActive(isAutoAdd);
        }
    }
}
