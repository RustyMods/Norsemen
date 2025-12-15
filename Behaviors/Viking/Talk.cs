using System.Collections.Generic;
using UnityEngine;

namespace Norsemen;

public partial class Viking
{
    private static readonly int EmoteSit = Animator.StringToHash("emote_sit");
    
    public float m_lastTargetUpdate;
    public float m_maxRange = 15f;
    public float m_greetRange = 10f;
    public float m_byeRange = 15f;
    public float m_offset = 2f;
    public float m_minTalkInterval = 1.5f;
    public float m_hideDialogDelay = 5f;
    public float m_randomTalkInterval = 10f;
    public float m_randomTalkChance = 1f;
    public float m_randomTalkTimer;
    
    public List<string> m_randomTalk = new List<string>()
    {
        "$norseman_random_talk_1",
        "$norseman_random_talk_2",
        "$norseman_random_talk_3",
        "$norseman_random_talk_4",
        "$norseman_random_talk_5",
        "$norseman_random_talk_6",
        "$norseman_random_talk_7",
        "$norseman_random_talk_8",
    };
    public List<string> m_randomTalkInPlayerBase = new List<string>()
    {
        "$norseman_in_base_talk_1",
        "$norseman_in_base_talk_2",
        "$norseman_in_base_talk_3",
        "$norseman_in_base_talk_4",
        "$norseman_in_base_talk_5",
        "$norseman_in_base_talk_6",
        "$norseman_in_base_talk_7",
        "$norseman_in_base_talk_8",
    };
    public List<string> m_randomGreets = new List<string>()
    {
        "$norseman_random_greet_1",
        "$norseman_random_greet_2",
        "$norseman_random_greet_3",
        "$norseman_random_greet_4",
        "$norseman_random_greet_5",
        "$norseman_random_greet_6",
        "$norseman_random_greet_7",
        "$norseman_random_greet_8",
    };
    public List<string> m_randomGoodbye = new List<string>()
    {
        "$norseman_random_bye_1",
        "$norseman_random_bye_2",
        "$norseman_random_bye_3",
        "$norseman_random_bye_4",
        "$norseman_random_bye_5",
        "$norseman_random_bye_6",
        "$norseman_random_bye_7",
        "$norseman_random_bye_8",
    };
    public List<string> m_aggravatedTalk = new List<string>()
    {
        "$norseman_aggravated_1",
        "$norseman_aggravated_2",
        "$norseman_aggravated_3",
        "$norseman_aggravated_4",
        "$norseman_aggravated_5",
        "$norseman_aggravated_6",
        "$norseman_aggravated_7",
        "$norseman_aggravated_8",
    };
    public List<string> m_thievedTalk = new ();

    public static EffectListRef m_randomTalkFX = new("sfx_dverger_vo_idle");
    public static readonly EffectListRef m_randomGreetFX = new("sfx_haldor_greet");
    public static readonly EffectListRef m_randomGoodbyeFX = new("sfx_haldor_laugh");
    public static readonly EffectListRef m_alertedFX = new("sfx_dverger_vo_attack");
    
    public bool m_didGreet;
    public bool m_didGoodbye;
    public Player? m_targetPlayer;
    public bool m_seeTarget;
    public bool m_hearTarget;
    
    public readonly Queue<QueuedSay> m_queuedTexts = new Queue<QueuedSay>();
    private readonly List<string> m_greetEmotes = new()
    {
        "emote_wave", "emote_bow"
    };
    private readonly List<string> m_randomEmote = new()
    {
        "emote_dance", "emote_despair", "emote_cry", "emote_point", "emote_flex", "emote_challenge", "emote_cheer",
        "emote_blowkiss", "emote_comehere", "emote_laugh", "emote_roar", "emote_shrug"
    };
    
    public void LookTowardsTarget()
    {
        if (!m_nview.IsOwner() || m_targetPlayer == null) return;
        if (GetVelocity().magnitude < 0.5 && !InAttack())
        {
            Vector3 lookDir = (m_targetPlayer.GetEyePoint() - GetEyePoint()).normalized;
            SetLookDir(lookDir);   
        }
    }

    public void UpdateTalk(float dt)
    {
        if (m_vikingAI.IsAlerted()) return;

        m_randomTalkTimer += dt;
        if (m_randomTalkTimer < m_randomTalkInterval) return;
        m_randomTalkTimer = 0.0f;

        float roll = UnityEngine.Random.value;
        if (roll > m_randomTalkChance) return;
        
        UpdateTarget();
        if (m_targetPlayer != null)
        {
            if (m_seeTarget)
            {
                float distance = Vector3.Distance(m_targetPlayer.transform.position, transform.position);
                if (!m_didGreet && distance < m_greetRange)
                {
                    m_didGreet = true;
                    string? emote = m_greetEmotes[UnityEngine.Random.Range(0, m_greetEmotes.Count)];
                    QueueSay(m_randomGreets, emote, m_randomGreetFX);
                }

                if (m_didGreet && !m_didGoodbye && distance > m_byeRange)
                {
                    m_didGoodbye = true;
                    string? emote = m_greetEmotes[UnityEngine.Random.Range(0, m_greetEmotes.Count)];
                    QueueSay(m_randomGoodbye, emote, m_randomGoodbyeFX);
                }
            }
        }
        
        UpdateSayQueue();
    }

    public void UpdateTarget()
    {
        if (Time.time - m_lastTargetUpdate <= 1.0) return;
        m_lastTargetUpdate = Time.time;
        m_targetPlayer = null;
        Player? closestPlayer = Player.GetClosestPlayer(transform.position, m_maxRange);
        if (closestPlayer == null || m_vikingAI.IsEnemy(closestPlayer)) return;
        m_seeTarget = m_vikingAI.CanSeeTarget(closestPlayer);
        m_hearTarget = m_vikingAI.CanHearTarget(closestPlayer);
        if (!m_seeTarget && !m_hearTarget) return;
        m_targetPlayer = closestPlayer;
    }

    public bool QueueSay(List<string> texts, string trigger = "", EffectListRef? effect = null)
    {
        if (texts.Count == 0 || m_queuedTexts.Count >= 3) return false;
        QueuedSay text = new QueuedSay()
        {
            text = texts[UnityEngine.Random.Range(0, texts.Count)],
            trigger = trigger,
            m_effect = effect
        };
        m_queuedTexts.Enqueue(text);
        return true;
    }

    public bool Say(List<string> texts, string trigger = "", EffectListRef? effect = null)
    {
        if (texts.Count == 0) return false;
        Say(texts[UnityEngine.Random.Range(0, texts.Count)], trigger);
        effect?.Create(transform.position, Quaternion.identity, variant: m_visEquipment.GetModelIndex());
        return true;
    }

    public void QueueEmote(string trigger)
    {
        QueuedSay text = new QueuedSay()
        {
            text = string.Empty,
            trigger = trigger,
        };
        m_queuedTexts.Enqueue(text);
    }

    public void UpdateSayQueue()
    {
        if (m_queuedTexts.Count <= 0 || Time.time - NpcTalk.m_lastTalkTime < m_minTalkInterval) return;
        QueuedSay text = m_queuedTexts.Dequeue();
        Say(text.text, text.trigger);
        text.m_effect?.Create(transform.position, Quaternion.identity, variant: m_visEquipment.GetModelIndex());
    }
    
    public void Say(string text, string trigger)
    {
        NpcTalk.m_lastTalkTime = Time.time;
        if (!string.IsNullOrEmpty(text))
        {
            Chat.instance.SetNpcText(gameObject, Vector3.up * m_offset, 20f, m_hideDialogDelay, "", text, false);
        }
        if (string.IsNullOrEmpty(trigger)) return;
        m_animator.SetTrigger(trigger);
    }
    
    public bool InPlayerBase() => EffectArea.IsPointInsideArea(transform.position, EffectArea.Type.PlayerBase, 30f);
    
    public class QueuedSay
    {
        public string text;
        public string trigger;
        public EffectListRef? m_effect;
    }
}