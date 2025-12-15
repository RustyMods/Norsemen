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

    public float m_shipAttachTimer;
    
    public void UpdateAttachShip(float dt)
    {
        if (!m_nview.IsOwner() || !IsTamed()) return;
        
        m_shipAttachTimer += dt;
        if (m_shipAttachTimer < 10f) return;
        m_shipAttachTimer = 0.0f;

        if (IsAttachedToShip())
        {
            GameObject? followTarget = m_vikingAI.GetFollowTarget();
            if (followTarget == null || !followTarget.TryGetComponent(out Player player))
            {
                AttachStop();
            }
            else
            {
                Ship? localShip = GetStandingOnShip();
                if (localShip == null || !localShip.IsPlayerInBoat(player))
                {
                    AttachStop();
                    TeleportTo(player.transform.position + player.transform.forward * 2f, transform.rotation, false);
                }
            }
        }
        else
        {
            GameObject? followTarget = m_vikingAI.GetFollowTarget();
            if (followTarget == null) return;

            if (!followTarget.TryGetComponent(out Player player)) return;

            Ship? localShip = player.GetStandingOnShip();
            if (localShip == null) return;
            
            if (!localShip.IsPlayerInBoat(player)) return;
            
            Chair[]? seats = localShip.GetComponentsInChildren<Chair>();
            if (seats == null) return;

            foreach (Chair seat in seats)
            {
                Player? closestPlayer = Player.GetClosestPlayer(seat.transform.position, 0.1f);
                Viking? closestViking = GetNearestViking(seat.transform.position, 0.1f);

                if (closestPlayer != null || closestViking != null) continue;

                AttachStart(seat.m_attachPoint, null, false, false, seat.m_inShip, seat.m_attachAnimation, seat.m_detachOffset, null);
                break;
            }
        }
    }

    public override void AttachStart(
        Transform attachPoint,
        GameObject colliderRoot,
        bool hideWeapons,
        bool isBed,
        bool onShip,
        string attachAnimation,
        Vector3 detachOffset,
        Transform cameraPos = null)
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
        UpdateAttach();
        ResetCloth();
    }

    public override bool IsAttachedToShip()
    {
        return m_attached && m_attachedToShip;
    }


    public void UpdateAttach()
    {
        if (!m_attached)
        {
            return;
        }
        if (m_attachPoint != null)
        {
            transform.position = m_attachPoint.position;
            transform.rotation = m_attachPoint.rotation;
            Rigidbody? componentInParent = m_attachPoint.GetComponentInParent<Rigidbody>();
            m_body.useGravity = false;
            m_body.linearVelocity = (bool)(Object)componentInParent
                ? componentInParent.GetPointVelocity(transform.position)
                : Vector3.zero;
            m_body.angularVelocity = Vector3.zero;
            m_maxAirAltitude = transform.position.y;
        }
        else
        {
            AttachStop();
        }
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