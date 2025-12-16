using UnityEngine;

namespace Norsemen;

public partial class Viking
{
    public void RPC_Command(long sender, ZDOID characterID, bool message)
    {
        Player? player = GetPlayer(characterID);
        if (player == null) return;
        GameObject? followTarget = m_vikingAI.GetFollowTarget();
        if (followTarget == null)
        {
            Follow(player.gameObject, player.GetPlayerName());
            m_vikingAI.m_followPlayer = player;
            if (message)
            {
                player.Message(MessageHud.MessageType.Center, GetHoverName() + " $hud_tamefollow");
            }
        }
        else
        {
            UnFollow();
            if (message)
            {
                player.Message(MessageHud.MessageType.Center, GetHoverName() + " $hud_tamestay");
            }
        }
    }
    
    public Player? GetPlayer(ZDOID characterID)
    {
        GameObject instance = ZNetScene.instance.FindInstance(characterID);
        return instance ? instance.GetComponent<Player>() : null;
    }
    
    public void Command(Humanoid user, bool message = true)
    {
        m_nview.InvokeRPC(nameof(RPC_Command), user.GetZDOID(), message);
    }
    
    public bool IsFollowing() => GetFollowTarget() != null;
    public GameObject? GetFollowTarget() => m_vikingAI.GetFollowTarget();
    
    public void Follow(GameObject target, string? playerName)
    {
        m_vikingAI.ResetPatrolPoint();
        m_vikingAI.SetFollowTarget(target);
        
        if (string.IsNullOrEmpty(playerName)) return;
        if (m_nview.IsOwner())
        {
            m_nview.GetZDO().Set(ZDOVars.s_follow, playerName);
        }
    }

    public void UnFollow()
    {
        m_vikingAI.SetFollowTarget(null);
        m_vikingAI.SetPatrolPoint();
        m_vikingAI.m_followPlayer = null;
        if (m_nview.IsOwner())
        {
            m_nview.GetZDO().Set(ZDOVars.s_follow, "");
        }
    }
    
    public void UpdateSavedFollowTarget()
    {
        if (!m_nview.IsOwner() || m_vikingAI.GetFollowTarget() != null) return;
        string? followName = m_nview.GetZDO().GetString(ZDOVars.s_follow);
        if (string.IsNullOrEmpty(followName)) return;
        foreach (Player player in Player.GetAllPlayers())
        {
            if (player.GetPlayerName() == followName)
            {
                Command(player, false);
                return;
            }
        }
    }
}