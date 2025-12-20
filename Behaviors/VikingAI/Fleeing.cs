using UnityEngine;

namespace Norsemen;

public partial class VikingAI
{
    public Heightmap.Biome m_currentBiome;
    public float m_biomeTimer;
    
    public float m_lastMoveAwayFromMountainTimer;
    public bool m_haveNonMountainPosition;
    public Vector3 m_moveAwayFromMountainPosition;

    public float m_lastMoveAwayFromTar;
    public bool m_haveNonTarPosition;
    public Vector3 m_moveAwayFromTarPosition;

    public bool UpdateFlee(float dt, bool isTamed, bool isAlerted)
    {
        if (isTamed && m_moveType is Movement.Guard)
        {
            return false;
        }

        if (UpdateFleeNotAlerted(dt, isAlerted))
        {
            return true;
        }

        if (UpdateFleeLowHealth(dt))
        {
            return true;
        }
        if (UpdateFleeLava(dt))
        {
            return true;
        }
        
        UpdateBiome(dt);

        if (UpdateFleeMountain(dt, 50f))
        {
            return true;
        }

        if (UpdateFleeTar(dt, 50f))
        {
            return true;
        }

        return false;
    }

    public bool UpdateFleeNotAlerted(float dt, bool isAlerted)
    {
        if (m_fleeIfNotAlerted && !HuntPlayer() && m_targetCreature && !isAlerted &&
            Vector3.Distance(m_targetCreature.transform.position, transform.position) - m_targetCreature.GetRadius() >
            m_alertRange)
        {
            Flee(dt, m_targetCreature.transform.position);
            return true;
        }

        return false;
    }

    public bool UpdateFleeLowHealth(float dt)
    {
        if (m_fleeIfLowHealth > 0.0 && m_timeSinceHurt < m_fleeTimeSinceHurt && m_targetCreature != null &&
            m_character.GetHealthPercentage() < (double)m_fleeIfLowHealth)
        {
            Flee(dt, m_targetCreature.transform.position);
            return true;
        }

        return false;
    }

    public bool UpdateFleeLava(float dt)
    {
        if (m_fleeInLava && m_character.InLava() && (m_targetCreature == null || m_targetCreature.AboveOrInLava()))
        {
            Flee(dt, m_character.transform.position - m_character.transform.forward);
            return true;
        }

        return false;
    }

    public void UpdateBiome(float dt)
    {
        m_biomeTimer += dt;
        if (m_biomeTimer <= 1.0f) return;
        m_biomeTimer = 0.0f;

        m_currentBiome = Heightmap.FindBiome(transform.position);
    }

    public bool UpdateFleeMountain(float dt, float maxRange)
    {
        if (m_currentBiome != Heightmap.Biome.Mountain) return false;

        HitData.DamageModifiers modifiers = m_viking.GetDamageModifiers();
        HitData.DamageModifier frostMod = modifiers.GetModifier(HitData.DamageType.Frost);

        bool shouldFlee = frostMod is not
            (HitData.DamageModifier.Resistant or
            HitData.DamageModifier.VeryResistant or
            HitData.DamageModifier.Immune or
            HitData.DamageModifier.Ignore);

        if (!shouldFlee) return false;
        
        float interval = m_haveNonMountainPosition ? 2.0f : 0.5f;
        float time = Time.time - m_lastMoveAwayFromMountainTimer;
        if (time > interval)
        {
            m_lastMoveAwayFromMountainTimer = Time.time;
            m_haveNonMountainPosition = false;
            for (int i = 0; i < 10; ++i)
            {
                Vector3 randomPos = transform.position + Quaternion.Euler(0.0f, Random.Range(0, 360), 0.0f) * Vector3.forward * Random.Range(4f, maxRange);
                Heightmap.Biome biome = Heightmap.FindBiome(randomPos);
                if (biome is Heightmap.Biome.Mountain) continue;
                randomPos.y = ZoneSystem.instance.GetSolidHeight(randomPos);
                m_moveAwayFromMountainPosition = randomPos;
                m_haveNonMountainPosition = true;
            }
        }
        
        if (!m_haveNonMountainPosition)
        {
            return false;
        }
        
        MoveTowards(m_moveAwayFromMountainPosition - transform.position, true);
        return true;
    }

    public bool UpdateFleeTar(float dt, float maxRange)
    {
        bool isTarred = m_viking.GetSEMan().HaveStatusEffect(SEMan.s_statusEffectTared);
        if (!isTarred) return false;
        
        float interval = m_haveNonTarPosition ? 2.0f : 0.5f;
        float time = Time.time - m_lastMoveAwayFromTar;
        if (time > interval)
        {
            m_lastMoveAwayFromTar = Time.time;
            m_haveNonTarPosition = false;
            for (int i = 0; i < 10; ++i)
            {
                Vector3 randomPos = transform.position + Quaternion.Euler(0.0f, Random.Range(0, 360), 0.0f) * Vector3.forward * Random.Range(4f, maxRange);
                float tarLevel = Floating.GetLiquidLevel(randomPos, type: LiquidType.Tar);
                if (randomPos.y < tarLevel) continue;
                m_moveAwayFromTarPosition = randomPos;
                m_haveNonTarPosition = true;
            }
        }

        if (!m_haveNonTarPosition)
        {
            return false;
        }
        
        MoveTowards(m_moveAwayFromTarPosition - transform.position, true);
        return true;
    }
}