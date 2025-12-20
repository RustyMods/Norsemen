using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace Norsemen;

public partial class VikingGui
{
    public static Display armor = null!;
    public static Display health = null!;

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Awake))]
    private static class InventoryGui_Awake_Patch
    {
        private static void Prefix(InventoryGui __instance)
        {
            GameObject ButtonContainer = new GameObject("norsemen_buttons");
            RectTransform rect = ButtonContainer.AddComponent<RectTransform>();
            rect.SetParent(__instance.m_container, false);
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = Vector2.zero;
            ButtonContainer.AddComponent<NorseGui>();
        }
        private static void Postfix(InventoryGui __instance)
        {
            GameObject source = __instance.transform.Find("root/Player/Armor").gameObject;
            RectTransform? container = __instance.m_container;

            GameObject? tooltipPrefab = __instance.m_containerGrid.m_elementPrefab.GetComponent<UITooltip>().m_tooltipPrefab;
            armor = new Display(source, container, tooltipPrefab, "NorsemanArmor");
            health = new Display(source, container, tooltipPrefab, "NorsemanHealth");
            health.rect.localPosition -=  new Vector3(0, health.rect.rect.height + health.icon.rectTransform.rect.height, 0);
            health.icon.sprite = __instance.m_minStationLevelIcon.sprite;
            armor.Hide();
            health.Hide();
            
            if (NorseGui.instance == null) return;
            
            NorseGui.instance.behaviour.SetupGlow(__instance.m_repairButtonGlow);
            NorseGui.instance.patrol.SetupGlow(__instance.m_repairButtonGlow);
            NorseGui.instance.access.SetupGlow(__instance.m_repairButtonGlow);
        }
    }

    public class Display
    {
        public GameObject obj;
        public RectTransform rect;
        public Image bkg;
        public Image icon;
        public TMPro.TMP_Text text;
        public UITooltip tooltip;
        
        public Display(GameObject source, RectTransform parent, GameObject tooltipPrefab, string name)
        {
            obj = UnityEngine.Object.Instantiate(source, parent);
            obj.name = name;

            rect = obj.GetComponent<RectTransform>();
            bkg = obj.transform.Find("bkg").GetComponent<Image>();
            icon = obj.transform.Find("armor_icon").GetComponent<Image>();
            text = obj.GetComponentInChildren<TMPro.TMP_Text>();
            tooltip = obj.AddComponent<UITooltip>();
            tooltip.m_tooltipPrefab = tooltipPrefab;

            obj.transform.SetSiblingIndex(2);
        }

        public void Show(string txt)
        {
            obj.SetActive(true);
            if (text == null) return;
            text.text = txt;
        }

        public void Hide() => obj.SetActive(false);
    }
}
    

