using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Norsemen;

public class BehaviourButtons : MonoBehaviour
{
    public static BehaviourButtons? instance;
    public ButtonElement behaviour = null!;
    public ButtonElement patrol = null!;

    public void Awake()
    {
        instance = this;
        InventoryGui? gui = GetComponentInParent<InventoryGui>();
        Button? stackAll = gui.m_stackAllButton;
        HorizontalLayoutGroup? layout = gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;
        layout.childControlHeight = false;
        layout.childControlWidth = false;
        layout.spacing = 2f;
        layout.padding.top = 10;
        
        behaviour = new ButtonElement(Instantiate(stackAll.gameObject, layout.transform), "Norseman_Behaviour");
        behaviour.AddListener(OnBehaviourChange);
        behaviour.SetLabel("$norseman_aggressive");
        behaviour.SetGamePadKey("JoyLTrigger");
        
        patrol = new ButtonElement(Instantiate(stackAll.gameObject, layout.transform), "Norseman_Patrol");
        patrol.AddListener(OnPatrolChange);
        patrol.SetLabel("$norseman_patrol");
        patrol.SetGamePadKey("JoyRTrigger");
    }
    
    public void OnDestroy()
    {
        instance = null;
    }

    public void Show()
    {
        if (VikingGui.m_currentViking == null) return;
        gameObject.SetActive(true);
        Emotion currentBehaviour = VikingGui.m_currentViking.m_vikingAI.m_behaviour;
        switch (currentBehaviour)
        {
            case Emotion.Passive:
                behaviour.SetLabel("$norseman_passive");
                break;
            case Emotion.Aggressive:
                behaviour.SetLabel("$norseman_aggressive");
                break;
        }
        
        Movement currentMovement = VikingGui.m_currentViking.m_vikingAI.m_moveType;
        switch (currentMovement)
        {
            case Movement.Patrol:
                patrol.SetLabel("$norseman_patrol");
                break;
            case Movement.Guard:
                patrol.SetLabel("$norseman_guard");
                break;
        }
    }
    
    public void Hide() => gameObject.SetActive(false);

    public void OnBehaviourChange()
    {
        if (VikingGui.m_currentViking == null || !Player.m_localPlayer) return;

        Emotion currentBehaviour = VikingGui.m_currentViking.m_vikingAI.m_behaviour;
        string text;
        string msg;
        switch (currentBehaviour)
        {
            default:
                VikingGui.m_currentViking.m_vikingAI.SetEmotion((int)Emotion.Passive);
                behaviour.SetLabel("$norseman_passive");
                text = Localization.instance.Localize("$norseman_behaviour_msg");
                msg = string.Format(text, VikingGui.m_currentViking.GetText(), "$norseman_passive");
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, msg);
                break;
            case Emotion.Passive:
                VikingGui.m_currentViking.m_vikingAI.SetEmotion((int)Emotion.Aggressive);
                behaviour.SetLabel("$norseman_aggressive");
                text = Localization.instance.Localize("$norseman_behaviour_msg");
                msg = string.Format(text, VikingGui.m_currentViking.GetText(), "$norseman_aggressive");
                break;
        }
        Player.m_localPlayer.Message(MessageHud.MessageType.Center, msg);
    }

    public void OnPatrolChange()
    {
        if (VikingGui.m_currentViking == null || !Player.m_localPlayer) return;

        Movement currentPatrol = VikingGui.m_currentViking.m_vikingAI.m_moveType;
        string text;
        string msg;

        switch (currentPatrol)
        {
            default:
                VikingGui.m_currentViking.m_vikingAI.SetMovement((int)Movement.Patrol);
                patrol.SetLabel("$norseman_patrol");
                text = Localization.instance.Localize("$norseman_behaviour_msg");
                msg = string.Format(text, VikingGui.m_currentViking.GetText(), "$norseman_patrol");
                break;
            case Movement.Patrol:
                VikingGui.m_currentViking.m_vikingAI.SetMovement((int)Movement.Guard);
                patrol.SetLabel("$norseman_guard");
                text = Localization.instance.Localize("$norseman_behaviour_msg");
                msg = string.Format(text, VikingGui.m_currentViking.GetText(), "$norseman_guard");
                break;
        }
        Player.m_localPlayer.Message(MessageHud.MessageType.Center, msg);
    }
    
    public class ButtonElement
    {
        public readonly GameObject go;
        
        public RectTransform rect;
        public readonly Button button;
        public ButtonSfx buttonSfx;
        public readonly UIGamePad uiGamePad;
        public Image glow;

        public readonly TMPro.TMP_Text text;
        public UIInputHint inputHint;
        public readonly TMPro.TMP_Text inputText;
        
        public ButtonElement(GameObject source, string name)
        {
            go = source;
            go.name = name;
            rect = source.GetComponent<RectTransform>();
            button = source.GetComponent<Button>();
            buttonSfx = source.GetComponent<ButtonSfx>();
            uiGamePad = source.GetComponent<UIGamePad>();
            text = source.transform.Find("Text").GetComponent<TMPro.TMP_Text>();
            inputHint = source.transform.Find("gamepad_hint (1)").GetComponent<UIInputHint>();
            inputText = inputHint.transform.Find("Text").GetComponent<TMPro.TMP_Text>();
            glow = new GameObject("glow").AddComponent<Image>();
            glow.rectTransform.SetParent(rect);
            glow.rectTransform.sizeDelta = rect.sizeDelta;
            glow.rectTransform.anchorMax = Vector2.zero;
            glow.rectTransform.anchorMin = Vector2.zero;
            glow.rectTransform.pivot = Vector2.zero;
            glow.rectTransform.anchoredPosition = Vector2.zero;
            SetGlow(false);
        }

        public void SetGlow(bool enabled)
        {
            glow.enabled = enabled;
        }

        public void SetupGlow(Image source)
        {
            glow.sprite = source.sprite;
            glow.material = source.material;
            glow.color = source.color;
        }

        public void AddListener(UnityAction action) => button.onClick.AddListener(action);

        public void SetLabel(string label) => text.text = Localization.instance.Localize(label);

        public void SetGamePadKey(string key)
        {
            uiGamePad.m_zinputKey = key;
            inputText.text = Localization.instance.Localize(ZInput.instance.GetBoundKeyString(key, true));
        }
    }
}