using UnityEngine;

namespace Norsemen;

public partial class VikingAI
{
    public float m_shipAttachTimer;
    
    public bool UpdateAttach()
    {
        if (!m_viking.m_attached)
        {
            return false;
        }
        if (m_viking.m_attachPoint != null)
        {
            transform.position =m_viking. m_attachPoint.position;
            transform.rotation = m_viking.m_attachPoint.rotation;
            Rigidbody? componentInParent = m_viking.m_attachPoint.GetComponentInParent<Rigidbody>();
            m_body.useGravity = false;
            m_body.linearVelocity = (bool)(Object)componentInParent
                ? componentInParent.GetPointVelocity(transform.position)
                : Vector3.zero;
            m_body.angularVelocity = Vector3.zero;
            m_viking.m_maxAirAltitude = transform.position.y;
            return true;
        }
        
        m_viking.AttachStop();
        return false;
    }

    public void UpdateAttachShip(float dt)
    {
        m_shipAttachTimer += dt;
        if (m_shipAttachTimer < 10f) return ;
        m_shipAttachTimer = 0.0f;

        if (m_viking.IsAttachedToShip())
        {
            if (m_follow == null || !m_follow.TryGetComponent(out Player player))
            {
                m_viking.AttachStop();
                return ;
            }
            
            Ship? localShip = m_viking.GetStandingOnShip();
            if (localShip == null || !localShip.IsPlayerInBoat(player))
            {
                m_viking.AttachStop();
                m_viking.TeleportTo(player.transform.position + player.transform.forward * 2f, transform.rotation, false);
            }
        }
        else
        {
            if (m_follow == null) return ;

            if (!m_follow.TryGetComponent(out Player player)) return;

            Ship? localShip = player.GetStandingOnShip();
            if (localShip == null) return;
            
            if (!localShip.IsPlayerInBoat(player)) return;
            
            Chair[]? seats = localShip.GetComponentsInChildren<Chair>();
            if (seats == null) return;

            foreach (Chair seat in seats)
            {
                Player? closestPlayer = Player.GetClosestPlayer(seat.transform.position, 0.1f);
                Viking? closestViking = Viking.GetNearestViking(seat.transform.position, 0.1f);

                if (closestPlayer != null || closestViking != null) continue;

                m_viking.AttachStart(seat.m_attachPoint, null, false, false, seat.m_inShip, seat.m_attachAnimation, seat.m_detachOffset, null);
                return;
            }
        }
    }
}