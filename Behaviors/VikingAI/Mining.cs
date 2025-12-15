using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Norsemen;


public partial class VikingAI
{
    public MineRock? m_mineRock;
    public MineRock5? m_mineRock5;
    public Destructible? m_destructible;
    public Vector3 m_lastMineRock5Point;
    
    public bool UpdateMineRockMining(float dt, ItemDrop.ItemData pickaxe)
    {
        if (m_mineRock == null) return false;
        
        if (m_mineRock.gameObject == null)
        {
            ResetWorkTargets();
            return false;
        }
        if (!MoveTo(dt, m_mineRock.transform.position, pickaxe.m_shared.m_attack.m_attackRange, false)) return true;
        StopMoving();
        LookAt(m_mineRock.transform.position);
        DoPickaxe(pickaxe, m_mineRock);
        m_lastWorkActionTime = Time.time;
        m_nview.GetZDO().Set(VikingVars.lastWorkTime, ZNet.instance.GetTime().Ticks);
        return true;
    }

    public bool UpdateMineRock5Mining(float dt, ItemDrop.ItemData pickaxe)
    {
        if (m_mineRock5 == null) return false;
        
        if (m_mineRock5.gameObject == null)
        {
            ResetWorkTargets();
            return false;
        }

        Vector3 closestPoint;

        if (m_lastMineRock5Point != Vector3.zero)
        {
            float distanceFromLastPoint = Vector3.Distance(m_lastMineRock5Point, transform.position);
            if (distanceFromLastPoint < 10f)
            {
                closestPoint = m_lastMineRock5Point;
            }
            else
            {
                FindNearestPoint(m_mineRock5, out closestPoint);
            }
        }
        else
        {
            FindNearestPoint(m_mineRock5, out closestPoint);
        }
        
        m_lastMineRock5Point = closestPoint;
        
        if (!MoveTo(dt, closestPoint, pickaxe.m_shared.m_attack.m_attackRange, false)) return true;
        StopMoving();
        LookAt(closestPoint);
        DoPickaxe(pickaxe, m_mineRock5);
        m_lastWorkActionTime = Time.time;
        m_nview.GetZDO().Set(VikingVars.lastWorkTime, ZNet.instance.GetTime().Ticks);
        return true;
    }

    public void FindNearestPoint(MineRock5 mineRock5, out Vector3 closestPoint)
    {
        closestPoint = mineRock5.transform.position;
        List<Collider> colliders = mineRock5.m_hitAreas.Select(x => x.m_collider).ToList();
        if (colliders.Count > 0)
        {
            float closestDistance = float.MaxValue;
            for (int i = 0; i < colliders.Count; ++i)
            {
                Collider? collider = colliders[i];
                if (collider == null) continue;
                Vector3 point;
                if (collider is MeshCollider { convex: false })
                {
                    point = collider.ClosestPointOnBounds(transform.position);
                }
                else
                {
                    point = collider.ClosestPoint(transform.position);
                }

                float distance = Vector3.Distance(point, transform.position);
                if (distance < closestDistance)
                {
                    closestPoint = point;
                    closestDistance = distance;
                }
            }
        }
    }

    public bool UpdateDestructibleMining(float dt, ItemDrop.ItemData pickaxe)
    {
        if (m_destructible == null) return false;
        
        if (m_destructible.gameObject == null)
        {
            ResetWorkTargets();
            return false;
        }
        Vector3 point;
        if (m_destructible.m_spawnWhenDestroyed != null)
        {
            Collider? collider = m_destructible.GetComponentInChildren<Collider>();
            if (collider == null)
            {
                point = m_destructible.transform.position;
            }
            else
            {
                if (collider is MeshCollider { convex: false })
                {
                    point = collider.ClosestPointOnBounds(transform.position);
                }
                else
                {
                    point = collider.ClosestPoint(transform.position);
                }
            }
        }
        else
        {
            point = m_destructible.transform.position;
        }
        if (!MoveTo(dt, point, pickaxe.m_shared.m_attack.m_attackRange, false)) return true;
        StopMoving();
        LookAt(m_destructible.transform.position);
        DoPickaxe(pickaxe, m_destructible);
        m_lastWorkActionTime = Time.time;
        m_nview.GetZDO().Set(VikingVars.lastWorkTime, ZNet.instance.GetTime().Ticks);
        return true;
    }
    
    public void DoPickaxe(ItemDrop.ItemData pickaxe, MineRock mineRock)
    {
        if (m_viking.StartWork(pickaxe))
        {
            List<DropTable.DropData> resources = mineRock.m_dropItems.m_drops;
            AddResources(resources);
        }
    }

    public void DoPickaxe(ItemDrop.ItemData pickaxe, MineRock5 mineRock5)
    {
        if (m_viking.StartWork(pickaxe))
        {
            List<DropTable.DropData> resources = mineRock5.m_dropItems.m_drops;
            AddResources(resources);
        }
    }
    
    public void DoPickaxe(ItemDrop.ItemData pickaxe, Destructible destructible)
    {
        if (m_viking.StartWork(pickaxe))
        {
            List<DropTable.DropData>? resources = null;
            if (destructible.m_spawnWhenDestroyed != null)
            {
                MineRock5? mineRock5 = destructible.m_spawnWhenDestroyed.GetComponent<MineRock5>();
                if (mineRock5 != null)
                {
                    resources =  mineRock5.m_dropItems.m_drops;
                }
            }
            else
            {
                DropOnDestroyed? dropOnDestroyed = destructible.GetComponent<DropOnDestroyed>();
                if (dropOnDestroyed != null)
                {
                    resources = dropOnDestroyed.m_dropWhenDestroyed.m_drops;
                }
            }

            if (resources != null)
            {
                AddResources(resources);
            }
        }
    }
}