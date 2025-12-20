using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Norsemen;

public partial class Viking : Humanoid, Interactable, TextReceiver
{
    public static readonly List<Viking> instances = new();
    public VikingAI m_vikingAI = null!;
    public GameObject m_tombstone = null!;
    public BaseAI.AggravatedReason m_aggravatedReason;

    public EffectList m_spawnEffects = new();
    public EffectList m_skillLevelupEffects = new();
    public EffectList m_equipStartEffects = new();
    public EffectList m_perfectDodgeEffects = new();
    
    public override void Awake()
    {
        instances.Add(this);

        m_inventory.m_name = "NorsemanInventory";
        
        m_vikingAI = GetComponent<VikingAI>();
        SetupConfigs();
        SetupFood();
        base.Awake();
        
        if (m_nview.IsValid())
        {
            m_nview.Register<long>(nameof(RPC_RequestOpen), RPC_RequestOpen);
            m_nview.Register<bool>(nameof(RPC_OpenResponse), RPC_OpenResponse);
            m_nview.Register<long>(nameof(RPC_RequestStack), RPC_RequestStack);
            m_nview.Register<bool>(nameof(RPC_StackResponse), RPC_StackResponse);
            m_nview.Register<ZDOID, bool>(nameof(RPC_Command), RPC_Command);
            m_nview.Register<string>(nameof(RPC_SetText), RPC_SetText);
        
            SetupCustomization();
            
            InvokeRepeating(nameof(TamingUpdate), 3f, 3f);
            // InvokeRepeating(nameof(Load), 1f, 1f);
        }

        m_inventory.m_onChanged += OnInventoryChanged;
        m_vikingAI.m_onConsumedItem += OnConsumedItem;
        m_vikingAI.m_onBecameAggravated += OnAggravated;
 
        m_tamingTime = configs.TamingTime;
        if ((m_startsTamed || m_tamingTime <= 0f) && configs.Tameable)
        {
            SetTamed(true);
        }

        m_visEquipment.m_isPlayer = true;
    }

    public override void OnDeath()
    {
        if (IsTamed())
        {
            CreateTombStone();
        }
        else
        {
            DropItems();
        }
        base.OnDeath();
    }

    public override void OnDestroy()
    {
        RemoveConfigSubscriptions();
        instances.Remove(this);
        base.OnDestroy();
    }
    
    public override void Start()
    {
        Load();
        AddDefaultItems();
        m_visEquipment.ClearEquipment();
        EquipItems();
        CheckLastWork();
    }

    public void Update()
    {
        if (!m_nview.IsValid() || !m_nview.IsOwner()) return;
        float dt = Time.deltaTime;
        UpdateSavedFollowTarget();
        UpdateTalk(dt);
    }

    public override string GetHoverName()
    {
        if (!FactionManager.customFactions.TryGetValue(m_faction, out var data)) return GetName();
        return $"<color={data.hoverColor}>{GetName()}</color>";
    }

    public override string GetHoverText()
    {
        if (!Player.m_localPlayer) return string.Empty;
        
        StringBuilder sb = new();
        sb.Append(GetText());
        bool usingGamepad = ZInput.IsGamepadActive() && !ZInput.IsMouseActive();

        if (IsTamed())
        {
            sb.Append($" ( {GetStatusString()} )");
            bool isFollowing = IsFollowing();
            if (isFollowing)
            {
                sb.Append("\n[<color=yellow><b>$KEY_Use</b></color>] $norseman_stay");
            }
            else
            {
                sb.Append("\n[<color=yellow><b>$KEY_Use</b></color>] $norseman_follow");
            }
            
            if (usingGamepad)
            {
                sb.Append("\n[<color=yellow><b>$KEY_AltKeys + $KEY_Use</b></color>] $hud_rename");
                sb.Append($"\n[<color=yellow><b>{ZInput.instance.GetBoundKeyString("JoyLTrigger") + "$KEY_Use"} </b></color> $piece_container_open");
            }
            else
            {
                sb.Append("\n[<color=yellow><b>$KEY_AltPlace + $KEY_Use</b></color>] $hud_rename");
                sb.Append("\n[<color=yellow><b>L.Alt + $KEY_Use</b></color>] $piece_container_open");
                
            }
        }
        else
        {
            if (configs.Tameable)
            {
                int tameness = GetTameness();
                if (tameness <= 0)
                {
                    sb.Append($" ( $hud_wild, {GetStatusString()} )");
                }
                else
                {
                    sb.Append($" $hud_tameness {tameness}%, {GetStatusString()} )");
                }
            }

            if (!m_vikingAI.CanSeeTarget(Player.m_localPlayer) && NorsemenPlugin.CanSteal)
            {
                if (usingGamepad)
                {
                    sb.Append($"\n[<color=yellow><b>{ZInput.instance.GetBoundKeyString("JoyLTrigger")} + $KEY_Use</b></color>] $norseman_steal");
                }
                else
                {
                    sb.Append("\n[<color=yellow><b>L.Alt + $KEY_Use</b></color>] $norseman_steal");
                }
            }

        }
        
        return Localization.instance.Localize(sb.ToString());
    }
    
    public bool Interact(Humanoid user, bool hold, bool alt)
    {
        if (user is not Player player) return false;
        string owner = m_nview.GetZDO().GetString(ZDOVars.s_ownerName);
        if (string.IsNullOrEmpty(owner))
        {
            m_nview.GetZDO().Set(ZDOVars.s_owner, player.GetPlayerID());
            m_nview.GetZDO().Set(ZDOVars.s_ownerName, player.GetPlayerName());
        }
        if (IsTamed())
        {
            if (ZInput.GetKey(KeyCode.LeftAlt) || ZInput.GetButton("JoyLTrigger"))
            {
                if (!CheckAccess(player.GetPlayerID()))
                {
                    player.Message(MessageHud.MessageType.Center, "$msg_cantopen");
                }
                else
                {
                    m_nview.InvokeRPC(nameof(RPC_RequestOpen), player.GetPlayerID());
                }
            }
            else if (alt)
            {
                TextInput.instance.RequestText(this, "$hud_rename", 10);
            }
            else
            {
                Command(user);
            }
        }
        else if (!m_vikingAI.CanSeeTarget(user) && NorsemenPlugin.CanSteal)
        {
            if (ZInput.GetKey(KeyCode.LeftAlt) || ZInput.GetButton("JoyLTrigger"))
            {
                m_nview.InvokeRPC(nameof(RPC_RequestOpen), player.GetPlayerID());
            }
        }
        return true;
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item)
    {
        if (!CanConsumeItem(item)) return false;
        OnConsumedItem(item);
        user.GetInventory().RemoveItem(item, 1);
        return true;
    }

    public override void OnDamaged(HitData hit)
    {

    }

    public void OnAggravated(BaseAI.AggravatedReason reason)
    {
        m_aggravatedReason = reason;
        switch (reason)
        {
            case BaseAI.AggravatedReason.Damage:
                Say(TalkManager.GetTalk(TalkManager.TalkType.Damaged), "emote_roar", alertedFX);
                break;
            case BaseAI.AggravatedReason.Theif:
                Say(TalkManager.GetTalk(TalkManager.TalkType.Thieved), "emote_roar", alertedFX);
                break;
        }
    }

    public static List<Viking> GetAllVikings() => instances;

    public static List<Viking> GetVikings(Vector3 pos, float range)
    {
        List<Viking> result = new();
        List<Viking> allVikings = GetAllVikings();
        for (int i = 0; i < allVikings.Count; ++i)
        {
            Viking viking = allVikings[i];
            float distance = Vector3.Distance(pos, viking.transform.position);
            if (distance < range)
            {
                result.Add(viking);
            }
        }
        return result;
    }

    public static Viking? GetNearestViking(Vector3 pos, float maxRange)
    {
        float nearest = float.MaxValue;
        Viking? result = null;
        List<Viking> allVikings = GetAllVikings();
        
        for (int i = 0; i < allVikings.Count; ++i)
        {
            Viking viking = allVikings[i];
            float distance = Vector3.Distance(pos, viking.transform.position);
            if (distance < nearest && distance < maxRange)
            {
                result = viking;
                nearest = distance;
            }
        }

        return result;
    }
}