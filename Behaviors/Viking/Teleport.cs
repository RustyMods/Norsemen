using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Norsemen;

public partial class Viking
{
    public override bool TeleportTo(Vector3 pos, Quaternion rot, bool distantTeleport)
    {
        float y = ZoneSystem.instance.GetSolidHeight(pos);
        pos.y = y;
        
        if (distantTeleport)
        {
            ZDO? zdo = m_nview.GetZDO();
            Vector2i sector = ZoneSystem.GetZone(pos);
            zdo.SetPosition(pos);
            zdo.SetRotation(rot);
            zdo.m_sector = sector.ClampToShort();
            ZDOMan.instance.ForceSendZDO(zdo.m_uid);
            transform.position = pos;
            transform.rotation = rot;
            return true;
        }

        if (!ZoneSystem.instance.IsZoneLoaded(pos))
        {
            NorsemenPlugin.LogDebug($"[{GetName()}] tried to teleport, but zone is not loaded");
            return false;
        }
        
        transform.position = pos;
        transform.rotation = rot;
        return true;
    }

    [HarmonyPatch(typeof(Player), nameof(Player.TeleportTo))]
    private static class Player_Teleport_To
    {
        private static void Postfix(Player __instance, Vector3 pos, Quaternion rot, bool __result)
        {
            if (!__result || __instance != Player.m_localPlayer) return;
            
            List<Viking> allVikings = GetVikings(__instance.transform.position, 20f);
            for (int i = 0; i < allVikings.Count; ++i)
            {
                Viking? viking = allVikings[i];
                GameObject? follow = viking.GetFollowTarget();
                if (follow == null || follow != __instance.gameObject) continue;
                
                if (!viking.IsTeleportable())
                {
                    string? localized = Localization.instance.Localize("$norsemen_cannot_tp");
                    string msg = string.Format(localized, viking.GetText());
                    __instance.Message(MessageHud.MessageType.Center, msg);
                    continue;
                }
                
                viking.TeleportTo(pos, rot, true);
            }
        }
    }
}