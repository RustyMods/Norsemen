using UnityEngine;

namespace Norsemen;

public partial class Viking
{
    public void Dodge(Vector3 dir)
    {
        transform.rotation = Quaternion.LookRotation(dir);
        m_body.rotation = transform.rotation;
        m_zanim.SetTrigger("dodge");
        AddNoise(5f);
        m_dodgeEffects.Create(transform.position, Quaternion.identity, transform);
    }

    public bool CanDodge() => !IsBlocking() && !IsRunning() && !IsSwimming();
}