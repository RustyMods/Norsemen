using UnityEngine;

namespace Norsemen;

public partial class Viking
{
    public uint m_lastRevision;
    public string m_lastDataString = string.Empty;
    public bool m_loading;
    public bool m_saving;
    public bool m_inUse;

    public Player m_currentPlayer;
    public GameObject m_previousFollowTarget;

    public int m_lastInventoryCount;
    
    public void OnInventoryChanged()
    {
        if (IsLoading() || !m_nview.IsOwner()) return;
        Save();
        if (!IsTamed())
        {
            int currentCount = GetInventory().NrOfItems();
            if (currentCount < m_lastInventoryCount)
            {
                m_vikingAI.SetAggravated(true, BaseAI.AggravatedReason.Theif);
                m_vikingAI.SetTarget(m_currentPlayer);
            }
        }
        UpdateEncumber();
    }
    
    public bool Save()
    {
        if (!m_nview.IsValid() || !m_nview.IsOwner()) return false;
        m_saving = true;
        NorsemenPlugin.LogDebug($"{GetName()} saving inventory");
        ZPackage pkg = new ZPackage();
        m_inventory.Save(pkg);
        string base64 = pkg.GetBase64();
        m_nview.GetZDO().Set(ZDOVars.s_items, base64);
        m_lastRevision = m_nview.GetZDO().DataRevision;
        m_lastDataString = base64;
        m_nview.GetZDO().Set(VikingVars.inventoryChanged, true);
        m_saving = false;
        return true;
    }

    public void CheckForChanges()
    {
        if (!m_nview.IsValid()) return;
        Load();
        Save();
    }
    
    public bool IsLoading() => m_loading;
    public bool IsSaving() => m_saving;

    public void ForceLoad()
    {
        string base64 = m_nview.GetZDO().GetString(ZDOVars.s_items);
        if (!string.IsNullOrEmpty(base64))
        {
            ZPackage pkg = new ZPackage(base64);
            m_loading = true;
            m_inventory.Load(pkg);
            m_loading = false;
        }
        m_lastDataString = base64;
        m_lastRevision = m_nview.GetZDO().DataRevision;
        UpdateEncumber();
    }

    public void Load()
    {
        uint revision = m_nview.GetZDO().DataRevision;
        if (revision == m_lastRevision) return;
        m_lastRevision = revision;
        string base64 = m_nview.GetZDO().GetString(ZDOVars.s_items);
        if (base64 == m_lastDataString) return;
        if (string.IsNullOrEmpty(base64))
        {
            m_lastDataString = base64;
            return;
        }
        ZPackage pkg = new ZPackage(base64);
        m_loading = true;
        m_inventory.Load(pkg);
        m_loading = false;
        m_lastDataString = base64;
        m_lastRevision = revision;
        UpdateEncumber();
    }

    public bool CheckAccess(long playerID)
    {
        if (!IsPrivate()) return true;
        long owner = m_nview.GetZDO().GetLong(ZDOVars.s_owner);
        if (owner == 0L) return true;
        return owner == playerID;
    }
    
     public void RPC_RequestOpen(long uid, long playerID)
    {
        if (!m_nview.IsOwner()) return;
        if (IsInUse())
        {
            m_nview.InvokeRPC(uid, nameof(RPC_OpenResponse), false);
        }
        else
        {
            ZDOMan.instance.ForceSendZDO(uid, m_nview.GetZDO().m_uid);
            m_nview.GetZDO().SetOwner(uid);
            Save();
            m_nview.InvokeRPC(uid, nameof(RPC_OpenResponse), true);
        }
    }

    public void RPC_OpenResponse(long uid, bool granted)
    {
        if (!Player.m_localPlayer) return;
        if (granted)
        {
            Load();
            m_lastInventoryCount = GetInventory().NrOfItems();
            InventoryGui.instance.Show(this);
        }
        else
        {
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_inuse");
        }
    }
    
    public void StackAll()
    {
        m_nview.InvokeRPC(nameof(RPC_RequestStack), Game.instance.GetPlayerProfile().GetPlayerID());
    }

    public void RPC_RequestStack(long uid, long playerID)
    {
        if (!m_nview.IsOwner()) return;
        if (IsInUse() && uid != ZNet.GetUID())
        {
            m_nview.InvokeRPC(uid, nameof(RPC_StackResponse), false);
        }
        else
        {
            ZDOMan.instance.ForceSendZDO(uid, m_nview.GetZDO().m_uid);
            m_nview.GetZDO().SetOwner(uid);
            m_nview.InvokeRPC(uid, nameof(RPC_StackResponse), true);
        }
    }

    public void RPC_StackResponse(long uid, bool granted)
    {
        if (!Player.m_localPlayer) return;
        if (granted)
        {
            if (m_inventory.StackAll(Player.m_localPlayer.GetInventory(), true) <= 0) return;
            InventoryGui.instance.m_moveItemEffects.Create(transform.position, Quaternion.identity);
        }
        else
        {
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_inuse");
        }
    }

    public bool IsInUse() => m_inUse;
    
    public void SetInUse(Player player)
    {
        if (!m_nview.IsOwner()) return;
        bool inUse = player != null;
        if (m_inUse == inUse) return;
        
        m_currentPlayer = player;
        m_inUse = inUse;
        
        if (!player)
        {
            EquipItems();

            if (m_previousFollowTarget)
            {
                Follow(m_previousFollowTarget, null);
            }
            else
            {
                UnFollow();
            }
            
            m_vikingAI.ResetWorkTargets();
        }
        else
        {
            m_previousFollowTarget = GetFollowTarget();
            Follow(player.gameObject, player.GetPlayerName());
        }
    }

    public bool IsPrivate()
    {
        if (!m_nview.IsValid()) return true;
        return m_nview.GetZDO().GetBool(VikingVars.isPrivate);
    }

    public void SetPrivate(bool isPrivate)
    {
        if (!m_nview.IsValid()) return;
        m_nview.GetZDO().Set(VikingVars.isPrivate, isPrivate);
    }
}