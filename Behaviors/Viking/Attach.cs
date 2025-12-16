using UnityEngine;

namespace Norsemen;

public partial class Viking
{
    public bool m_attached;
    public bool m_attachedToShip;
    public Transform? m_attachPoint;
    public Vector3 m_detachOffset;
    public string? m_attachAnimation;
    public Collider[]? m_attachColliders;
    
    public override void AttachStart(
        Transform attachPoint,
        GameObject colliderRoot,
        bool hideWeapons,
        bool isBed,
        bool onShip,
        string attachAnimation,
        Vector3 detachOffset,
        Transform? cameraPos = null)
    {
        if (m_attached)
        {
            return;
        }
        m_attached = true;
        m_attachedToShip = onShip;
        m_attachPoint = attachPoint;
        m_detachOffset = detachOffset;
        m_attachAnimation = attachAnimation;
        m_zanim.SetBool(attachAnimation, true);
        m_nview.GetZDO().Set(ZDOVars.s_inBed, isBed);
        if (colliderRoot != null)
        {
            m_attachColliders = colliderRoot.GetComponentsInChildren<Collider>();
            ZLog.Log($"Ignoring {m_attachColliders.Length.ToString()} colliders");
            foreach (Collider attachCollider in m_attachColliders)
            {
                Physics.IgnoreCollision(m_collider, attachCollider, true);
            }
        }

        if (hideWeapons)
        {
            HideHandItems();
        }

        m_vikingAI.UpdateAttach();
        ResetCloth();
    }

    public override bool IsAttachedToShip()
    {
        return m_attached && m_attachedToShip;
    }
    
    public override void AttachStop()
    {
        if (!m_attached)
        {
            return;
        }

        if (m_attachPoint != null)
        {
            transform.position = m_attachPoint.TransformPoint(m_detachOffset);
        }
        if (m_attachColliders != null)
        {
            foreach (Collider? attachCollider in m_attachColliders)
            {
                if (attachCollider)
                {
                    Physics.IgnoreCollision(m_collider, attachCollider, false);
                }
            }
            m_attachColliders = null;
        }

        m_body.useGravity = true;
        m_attached = false;
        m_attachPoint = null;
        m_zanim.SetBool(m_attachAnimation, false);
        m_nview.GetZDO().Set(ZDOVars.s_inBed, false);
        ResetCloth();
    }
}