using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace Norsemen;

public class VikingTomb : MonoBehaviour
{
    public static readonly List<VikingTomb> instances = new();

    public ZNetView m_nview = null!;
    public TombStone m_tombstone = null!;

    public bool m_revived;
    public float m_reviveDuration = 10f;
    public float m_cancelDistance = 10f;
    public float m_revertDistance = 5f;
    public float m_checkInterval = 0.5f;
    
    public static EffectList reviveEffects = new();
    
    public Player? m_reviver;
    public Player.MinorActionData? m_action;
    public static VikingTomb? m_currentTomb;

    public void Awake()
    {
        instances.Add(this);
        m_nview = GetComponent<ZNetView>();
        m_tombstone = GetComponent<TombStone>();
    }

    public void OnDestroy()
    {
        instances.Remove(this);
    }

    public void FixedUpdate()
    {
        UpdateRevive();
    }

    public void UpdateRevive()
    {
        if (m_action == null || m_reviver == null) return;

        float distance = Vector3.Distance(transform.position, m_reviver.transform.position);
        
        if (distance > m_cancelDistance)
        {
            m_action = null;
            m_currentTomb = null;
            m_reviver = null;
        }
        else if (distance > m_revertDistance)
        {
            m_action.m_time -= Time.fixedDeltaTime;
        }
        else
        {
            m_action.m_time += Time.fixedDeltaTime;
        }
    }

    public string GetHoverText()
    {
        if (!m_nview.IsValid()) return string.Empty;
        if (m_tombstone.m_container.GetInventory().NrOfItems() == 0) return string.Empty;
        StringBuilder sb = new StringBuilder();
        sb.Append($"{m_tombstone.m_text} {m_tombstone.GetOwnerName()}");

        bool isReviving = m_nview.GetZDO().GetBool(VikingVars.reviving);
        if (!isReviving) sb.Append("\n[<color=yellow><b>$KEY_Use</b></color>] $piece_container_open");

        string actionText = isReviving ? "$norseman_cancel" : "$norseman_revive";
        sb.Append($"\n[<color=yellow><b>$KEY_AltPlace + $KEY_Use</b></color>] {actionText}");

        return Localization.instance.Localize(sb.ToString());
    }

    public void Setup(Viking viking)
    {
        ZDO? zdo = m_nview.GetZDO();
        if (zdo == null) return;
        
        zdo.Set(VikingVars.vikingPrefab, viking.name.Replace("(Clone)", ""));
        zdo.Set(ZDOVars.s_tamedName, viking.GetText());
        zdo.Set(ZDOVars.s_level, viking.GetLevel());
        zdo.Set(ZDOVars.s_hairColor, viking.m_hairColor);
        zdo.Set(ZDOVars.s_modelIndex, viking.m_visEquipment.GetModelIndex());
        zdo.Set(ZDOVars.s_hairItem, viking.m_hairItem);
        zdo.Set(ZDOVars.s_hairColor, viking.m_hairColor);
        zdo.Set(ZDOVars.s_beardItem, viking.m_beardItem);
        zdo.Set(ZDOVars.s_skinColor, viking.m_skinColor);
        zdo.Set(VikingVars.behaviour, (int)viking.m_vikingAI.m_behaviour);
        zdo.Set(VikingVars.patrol, (int)viking.m_vikingAI.m_moveType);
        zdo.Set(VikingVars.isElf, viking.m_isElf);
    }
    public bool Revive()
    {
        if (m_reviver == null) return false;
        
        string? prefabName = m_nview.GetZDO().GetString(VikingVars.vikingPrefab);
        GameObject? prefab = ZNetScene.instance.GetPrefab(prefabName);
        if (prefab == null) return false;
        
        string? tamedName = m_nview.GetZDO().GetString(ZDOVars.s_tamedName);
        int level = m_nview.GetZDO().GetInt(ZDOVars.s_level);
        Vector3 hairColor = m_nview.GetZDO().GetVec3(ZDOVars.s_hairColor, Vector3.zero);
        int model = m_nview.GetZDO().GetInt(ZDOVars.s_modelIndex);
        string? hair = m_nview.GetZDO().GetString(ZDOVars.s_hairItem);
        string? beard = m_nview.GetZDO().GetString(ZDOVars.s_beardItem);
        Vector3 skinColor = m_nview.GetZDO().GetVec3(ZDOVars.s_skinColor, Vector3.one);
        string? items = m_nview.GetZDO().GetString(ZDOVars.s_items);
        int behaviour = m_nview.GetZDO().GetInt(VikingVars.behaviour);
        int patrol = m_nview.GetZDO().GetInt(VikingVars.patrol);
        bool isElf = m_nview.GetZDO().GetBool(VikingVars.isElf);

        int hash = prefabName.GetStableHashCode();
        ZDO zdo = ZDOMan.instance.CreateNewZDO(transform.position, hash);
        zdo.SetPrefab(hash);
        zdo.Set(VikingVars.isSet, true);
        zdo.Set(ZDOVars.s_tamedName, tamedName);
        zdo.Set(ZDOVars.s_level, level);
        zdo.Set(ZDOVars.s_hairItem, hair);
        zdo.Set(ZDOVars.s_beardItem, beard);
        zdo.Set(ZDOVars.s_skinColor, skinColor);
        zdo.Set(ZDOVars.s_hairColor, hairColor);
        zdo.Set(VikingVars.behaviour, behaviour);
        zdo.Set(VikingVars.patrol, patrol);
        zdo.Set(ZDOVars.s_tamed, true);
        zdo.Set(ZDOVars.s_modelIndex, model);
        zdo.Set(VikingVars.createTombStone, true);
        zdo.Set(ZDOVars.s_items, items);
        zdo.Set(ZDOVars.s_addedDefaultItems, true);
        zdo.Set(VikingVars.lastLevelUpTime, ZNet.instance.GetTime().Ticks);
        zdo.Set(VikingVars.createTombStone, true);
        zdo.Set(VikingVars.isElf, isElf);
        zdo.Set(ZDOVars.s_follow, m_reviver.GetPlayerName());
        ZNetScene.instance.CreateObject(zdo);
        
        m_tombstone.m_container.GetInventory().RemoveAll();
        m_revived = true;
        m_reviver = null;
        return true;
    }

    public bool IsReviving() => IsInvoking(nameof(CheckReviveProgress));

    public void CheckReviveProgress()
    {
        if (m_reviver == null || m_action == null)
        {
            NorsemenPlugin.LogDebug("Reviver is null or progress action is null, canceling");
            m_nview.GetZDO().Set(VikingVars.reviving, false);
            CancelInvoke(nameof(CheckReviveProgress));
        }
        else
        {
            if (m_action.m_time >= m_action.m_duration)
            {
                CancelInvoke(nameof(CheckReviveProgress));
                Invoke(nameof(Revive), 1f);
                reviveEffects.Create(transform.position, Quaternion.identity);
                m_nview.GetZDO().Set(VikingVars.reviving, false);
                m_action = null;
                m_currentTomb = null;
            }
            else if (m_action.m_time <= 0.0f)
            {
                CancelInvoke(nameof(CheckReviveProgress));
                m_nview.GetZDO().Set(VikingVars.reviving, false);
                m_action = null;
                m_reviver = null;
                m_currentTomb = null;
            }
        }
    }
    
    public bool Interact(Humanoid character, bool hold, bool alt)
    {
        if (hold || m_tombstone.m_container.GetInventory().NrOfItems() == 0) return false;
        if (character is not Player player) return false;

        bool isReviving = m_nview.GetZDO().GetBool(VikingVars.reviving);
        if (!m_nview.IsOwner() && isReviving)
        {
            character.Message(MessageHud.MessageType.Center, "$msg_inuse");
            return false;
        }

        if (alt)
        {
            if (m_currentTomb != null)
            {
                character.Message(MessageHud.MessageType.Center, $"$norseman_revive_inuse {m_currentTomb.m_tombstone.GetOwnerName()}");
                return false;
            }
            
            if (m_revived) return false;
            if (!IsInvoking(nameof(CheckReviveProgress)))
            {
                m_nview.ClaimOwnership();
                m_reviver = player;
                m_currentTomb = this;
                m_action = new Player.MinorActionData
                {
                    m_duration = m_reviveDuration,
                    m_progressText = $"$norseman_reviving {m_tombstone.GetOwnerName()}",
                    m_type = Player.MinorActionData.ActionType.Reload
                };
                InvokeRepeating(nameof(CheckReviveProgress), 1f, m_checkInterval);
                m_nview.GetZDO().Set(VikingVars.reviving, true);
            }
            else
            {
                m_nview.GetZDO().Set(VikingVars.reviving, false);
                CancelInvoke(nameof(CheckReviveProgress));
                m_action = null;
                m_reviver = null;
                m_currentTomb = null;
            }
            return true;
        }

        if (isReviving) return false;

        return m_tombstone.m_container.Interact(character, false, false);
    }

    public static void RemoveAll()
    {
        var count = 0;
        foreach (var tomb in instances)
        {
            ZNetScene.instance.Destroy(tomb.gameObject);
            ++count;
        }

        NorsemenPlugin.LogDebug($"Removed {count} norsemen tombstones");
    }

    [HarmonyPatch(typeof(TombStone), nameof(TombStone.GetHoverText))]
    private static class TombStone_GetHoverText
    {
        private static bool Prefix(TombStone __instance, ref string __result)
        {
            if (!__instance.TryGetComponent(out VikingTomb component)) return true;
            __result = component.GetHoverText();
            return false;
        }
    }

    [HarmonyPatch(typeof(TombStone), nameof(TombStone.Interact))]
    private static class TombStone_Interact
    {
        private static bool Prefix(TombStone __instance, Humanoid character, bool hold, bool alt, ref bool __result)
        {
            if (!__instance.TryGetComponent(out VikingTomb component)) return true;
            __result = component.Interact(character, hold, alt);
            return false;
        }
    }

    [HarmonyPatch(typeof(Player))]
    private static class Player_GetActionProgress
    {
        [HarmonyPatch(nameof(Player.GetActionProgress))]
        [HarmonyPatch(new [] { typeof(string), typeof(float), typeof(Player.MinorActionData) }, 
            new [] { ArgumentType.Out, ArgumentType.Out, ArgumentType.Out })]
        private static void Postfix(Player __instance, ref string name, ref float progress, ref Player.MinorActionData? data)
        {
            if (__instance != Player.m_localPlayer || data != null || m_currentTomb == null) return;

            if (!m_currentTomb.IsReviving() || m_currentTomb.m_action == null) return;

            data = m_currentTomb.m_action;
            name = m_currentTomb.m_action.m_progressText;
            progress = Mathf.Clamp01(m_currentTomb.m_action.m_time / m_currentTomb.m_action.m_duration);
        }
    }
}