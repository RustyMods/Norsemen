namespace Norsemen;

public enum Emotion
{
    Aggressive = 0, 
    Passive = 1
}

public enum Movement
{
    Patrol = 0,
    Guard = 1
}
public partial class VikingAI
{
    public Emotion m_behaviour = Emotion.Aggressive;
    public Movement m_moveType = Movement.Patrol;
    
    public float m_behaviourUpdateTimer;

    public void SetEmotion(int state)
    {
        Emotion behaviour = (Emotion)state;
        if (behaviour== m_behaviour) return;
        m_behaviour = behaviour;
        m_nview.GetZDO().Set(VikingVars.behaviour, state);
    }

    public void SetMovement(int state)
    {
        Movement patrol = (Movement)state;
        if (patrol == m_moveType) return;
        m_moveType = patrol;
        m_nview.GetZDO().Set(VikingVars.patrol, state);
    }

    public void UpdateBehaviour(float dt)
    {
        m_behaviourUpdateTimer += dt;
        if (m_behaviourUpdateTimer < 1f) return;
        m_behaviourUpdateTimer = 0.0f;
        
        SetEmotion(m_nview.GetZDO().GetInt(VikingVars.behaviour));
        SetMovement(m_nview.GetZDO().GetInt(VikingVars.patrol));
    }
}