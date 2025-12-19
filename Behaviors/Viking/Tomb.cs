using UnityEngine;

namespace Norsemen;

public partial class Viking
{
    public void CreateTombStone()
    {
        if (!m_nview.IsOwner() || GetInventory().NrOfItems() <= 0) return;
        GameObject? tombstone = Instantiate(m_tombstone, transform.position, transform.rotation);
        tombstone.GetComponent<Container>().GetInventory().MoveAll(m_inventory);
        tombstone.GetComponent<TombStone>().Setup(GetName(), 0L);
        tombstone.GetComponent<VikingTomb>().Setup(this);
    }

    public void DropItems()
    {
        Vector3 center = transform.position;
        float range = 0.5f;
        
        foreach (ItemDrop.ItemData? item in GetInventory().GetAllItems())
        {
            if (item.IsEquipable()) continue;
            Quaternion rotation = Quaternion.Euler(0.0f, Random.Range(0, 360), 0.0f);
            Vector3 area = UnityEngine.Random.insideUnitSphere * range;
            Vector3 pos = center + Vector3.up * 0.5f + area;
            ItemDrop drop = ItemDrop.DropItem(item, item.m_stack, pos, rotation);
            
            Rigidbody body = drop.GetComponent<Rigidbody>();
            if (body == null) continue;
            Vector3 force = Random.insideUnitSphere;
            if (force.y < 0.0) force.y = -force.y;
            body.AddForce(force * 5f,  ForceMode.VelocityChange);
        }
    }
}