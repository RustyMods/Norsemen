using UnityEngine;

namespace Norsemen;

public partial class Viking
{
    public override bool TeleportTo(Vector3 pos, Quaternion rot, bool distantTeleport)
    {
        if (!ZoneSystem.instance.IsZoneLoaded(pos)) return false;

        float y = ZoneSystem.instance.GetSolidHeight(pos);
        pos.y = y;
        
        transform.position = pos;
        transform.rotation = rot;
        return true;
    }
}