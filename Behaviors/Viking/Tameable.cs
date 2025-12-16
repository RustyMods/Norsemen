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

    public int m_maxLevel = 3;
    public float m_secondsToLevelUp = 1800f;
    public void OnTameTimeConfigChanged(object sender, EventArgs args)
    {
        if (!m_nview.IsValid()) return;
        m_tamingTime = configs.TamingTime;
        m_nview.GetZDO().Set(ZDOVars.s_tameTimeLeft, m_tamingTime);
    }
    
    public void TamingUpdate()
    {
        if (!configs.Tameable) return;
        if (!m_nview.IsValid() || !m_nview.IsOwner() || IsHungry() || m_baseAI.IsAlerted()) return;
        
        if (IsTamed())
        {
            int currentLevel = GetLevel();
            if (currentLevel >= m_maxLevel)
            {
                CancelInvoke(nameof(TamingUpdate));
                return;
            }
            
            long time = ZNet.instance.GetTime().Ticks;
            long tameTime = m_nview.GetZDO().GetLong(VikingVars.lastLevelUpTime);
            if (tameTime == 0L) return;
            long difference = time - tameTime;
            TimeSpan span = new TimeSpan(difference);
            double totalSeconds = span.TotalSeconds;
            float timeToLevelUp = m_secondsToLevelUp * currentLevel;
            if (totalSeconds < timeToLevelUp) return;
            SetLevel(currentLevel + 1);
            m_skillLevelupEffects.Create(transform.position, Quaternion.identity);
            m_nview.GetZDO().Set(VikingVars.lastLevelUpTime, time);

            Player? nearestPlayer = Player.GetClosestPlayer(transform.position, 20f);
            if (nearestPlayer != null)
            {
                string message = $"{GetText()} $msg_leveledup";
                nearestPlayer.Message(MessageHud.MessageType.Center, message);
            }
        }
        else
        {
            m_vikingAI.SetDespawnInDay(false);
            m_vikingAI.SetEventCreature(false);
            DecreaseRemainingTime(3f);
            
            float remainingTime = GetRemainingTime();
            if (remainingTime <= 0.0f)
            {
                Tame();
            }
            else
            {
                m_sootheEffect.Create(transform.position, transform.rotation);
            }
        }
    }

    public void Tame()
    {
        if (!configs.Tameable) return;
        
        Game.instance.IncrementPlayerStat(PlayerStatType.CreatureTamed);
        if (!m_nview.IsValid() || !m_nview.IsOwner() || IsTamed()) return;
        m_vikingAI.MakeTame();
        m_tamedEffect.Create(transform.position, transform.rotation);
        Player closestPlayer = Player.GetClosestPlayer(transform.position, 30f);
        if (closestPlayer != null)
        {
            closestPlayer.Message(MessageHud.MessageType.Center, m_name + " $hud_tamedone");
        }

        m_nview.GetZDO().Set(VikingVars.lastLevelUpTime, ZNet.instance.GetTime().Ticks);
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

        float timeLeft = remainingTime - time;
        if (timeLeft < 0.0)
        {
            timeLeft = 0.0f;
        }

        m_nview.GetZDO().Set(ZDOVars.s_tameTimeLeft, timeLeft);
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

        if (!configs.Tameable) return false;

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