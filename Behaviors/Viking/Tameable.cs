using System;
using System.Collections.Generic;
using UnityEngine;

namespace Norsemen;

public partial class Viking
{
    public static readonly List<Player> s_nearbyPlayers = new List<Player>();
    
    public static readonly EffectListRef m_tamedEffect = new ("fx_creature_tamed");
    public static readonly EffectListRef m_sootheEffect = new ("vfx_creature_soothed");
    
    public float m_fedDuration = 300f;
    public float m_tamingTime = 1800f;
    public bool m_startsTamed;
    
    public float m_tamingSpeedMultiplierRange = 60f;
    public float m_tamingBoostMultiplier = 2f;
    public Skills.SkillType m_levelUpOwnerSkill;
    public float m_levelUpFactor = 1f;

    public void OnTameTimeConfigChanged(object sender, EventArgs args)
    {
        if (!m_nview.IsValid()) return;
        m_tamingTime = configs.TamingTime;
        m_nview.GetZDO().Set(ZDOVars.s_tameTimeLeft, m_tamingTime);
    }
    
    public void SetRandomName()
    {
        bool isSet = m_nview.GetZDO().GetBool(VikingVars.isSet);
        if (!isSet)
        {
            bool isFemale = m_nview.GetZDO().GetInt(ZDOVars.s_modelIndex) != 0;
            string randomName;
            if (isFemale)
            {
                randomName = NameGenerator.GenerateFemaleName();
            }
            else
            {
                randomName = NameGenerator.GenerateMaleName();
            }
            m_nview.GetZDO().Set(ZDOVars.s_tamedName, randomName);
        }
    }

    public void TamingUpdate()
    {
        if (!m_nview.IsValid() || !m_nview.IsOwner() || IsTamed() || IsHungry() || m_baseAI.IsAlerted()) return;
        m_vikingAI.SetDespawnInDay(false);
        m_vikingAI.SetEventCreature(false);
        DecreaseRemainingTime(3f);
        if (GetRemainingTime() <= 0.0f)
        {
            Tame();
        }
        else
        {
            m_sootheEffect.Create(transform.position, transform.rotation);
        }
    }

    public void Tame()
    {
        Game.instance.IncrementPlayerStat(PlayerStatType.CreatureTamed);
        if (!m_nview.IsValid() || !m_nview.IsOwner() || IsTamed()) return;
        m_vikingAI.MakeTame();
        m_tamedEffect.Create(transform.position, transform.rotation);
        Player closestPlayer = Player.GetClosestPlayer(transform.position, 30f);
        if (closestPlayer != null)
        {
            closestPlayer.Message(MessageHud.MessageType.Center, m_name + " $hud_tamedone");
        }
    }

    public void DecreaseRemainingTime(float time)
    {
        if (!m_nview.IsValid()) return;
        float remainingTime = GetRemainingTime();
        s_nearbyPlayers.Clear();
        Player.GetPlayersInRange(transform.position, m_tamingSpeedMultiplierRange, s_nearbyPlayers);
        foreach (Player nearbyPlayer in s_nearbyPlayers)
        {
            if (nearbyPlayer.GetSEMan().HaveStatusAttribute(StatusEffect.StatusAttribute.TamingBoost))
            {
                time *= m_tamingBoostMultiplier;
            }
        }

        float num = remainingTime - time;
        if (num < 0.0)
        {
            num = 0.0f;
        }

        m_nview.GetZDO().Set(ZDOVars.s_tameTimeLeft, num);
    }
    
    public int GetTameness()
    {
        return (int)((1.0 - Mathf.Clamp01(GetRemainingTime() / m_tamingTime)) * 100.0);
    }

    public float GetRemainingTime()
    {
        if (!m_nview.IsValid()) return 0.0f;
        return m_nview.GetZDO().GetFloat(ZDOVars.s_tameTimeLeft, m_tamingTime);
    }

    public void Command(Humanoid user, bool message = true)
    {
        m_nview.InvokeRPC(nameof(RPC_Command), user.GetZDOID(), message);
    }

    public Player? GetPlayer(ZDOID characterID)
    {
        GameObject instance = ZNetScene.instance.FindInstance(characterID);
        return instance ? instance.GetComponent<Player>() : null;
    }

    public GameObject? GetFollowTarget() => ((MonsterAI)m_baseAI).GetFollowTarget();
    public bool IsFollowing() => GetFollowTarget() != null;

    public void RPC_Command(long sender, ZDOID characterID, bool message)
    {
        Player? player = GetPlayer(characterID);
        if (player == null) return;
        GameObject? followTarget = m_vikingAI.GetFollowTarget();
        if (followTarget == null)
        {
            Follow(player.gameObject, player.GetPlayerName());
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
    
    public void RPC_SetText(long sender, string text)
    {
        if (!m_nview.IsValid() || !m_nview.IsOwner() || !IsTamed()) return;
        m_nview.GetZDO().Set(ZDOVars.s_tamedName, text);
    }

    public string GetName() => Localization.instance.Localize(GetText());
    
    public string GetText()
    {
        if (!m_nview.IsValid()) return string.Empty;
        string text = m_nview.GetZDO().GetString(ZDOVars.s_tamedName);
        return string.IsNullOrEmpty(text) ? m_name : text;
    }

    public void SetText(string text)
    {
        if (!m_nview.IsValid()) return;
        m_nview.InvokeRPC(nameof(RPC_SetText), text);
    }
    
    public string GetStatusString()
    {
        if (m_vikingAI.IsAlerted())
        {
            return "$hud_tamefrightened";
        }
        if (IsHungry())
        {
            return "$hud_tamehungry";
        }

        return IsTamed() ? "$hud_tamehappy" : "$hud_tameinprogress";
    }

    public override void RaiseSkill(Skills.SkillType skill, float value = 1f)
    {
        if (!IsTamed()) return;

        if (m_levelUpOwnerSkill == Skills.SkillType.None) return;

        GameObject? followTarget = GetFollowTarget();
        if (followTarget == null) return;
        
        Character character = followTarget.GetComponent<Character>();
        if (character == null) return;

        Skills skills = character.GetSkills();
        if (skills == null) return;
        
        skills.RaiseSkill(m_levelUpOwnerSkill, value * m_levelUpFactor);
    }
    
    public bool IsHungry()
    {
        if (m_nview == null) return false;

        ZDO zdo = m_nview.GetZDO();
        if (zdo == null) return false;

        DateTime dateTime = new DateTime(zdo.GetLong(ZDOVars.s_tameLastFeeding));
        
        return (ZNet.instance.GetTime() - dateTime).TotalSeconds > m_fedDuration;
    }

    public void OnConsumedItem(ItemDrop? item)
    {
        if (IsHungry())
        {
            m_sootheEffect.Create(GetCenterPoint(), Quaternion.identity);
        }
        ResetFeedingTimer();

        if (item != null)
        {
            List<string> consumedTalk = GetConsumeTalk(item.m_itemData.m_shared.m_name);
            m_queuedTexts.Clear();
            QueueSay(consumedTalk);
        }
    }

    public void ResetFeedingTimer()
    {
        m_nview.GetZDO().Set(ZDOVars.s_tameLastFeeding, ZNet.instance.GetTime().Ticks);
    }
}