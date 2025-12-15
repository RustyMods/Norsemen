using UnityEngine;

namespace Norsemen;

public partial class Viking
{
    public override bool TeleportTo(Vector3 pos, Quaternion rot, bool distantTeleport)
    {
        if (distantTeleport) return false;
        transform.position = pos;
        transform.rotation = rot;
        return true;
    }
}