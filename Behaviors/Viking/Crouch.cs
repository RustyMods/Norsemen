using UnityEngine;

namespace Norsemen;

public partial class Viking
{
    public bool m_crouchToggled;

    public override void SetCrouch(bool crouch)
    {
        m_crouchToggled = crouch;
    }

    public bool CanCrouch()
    {
        if (IsSwimming()) return false;
        if (IsRunning()) return false;
        if (IsBlocking()) return false;
        if (InAttack()) return false;
        if (IsDrawingBow()) return false;
        if (IsDead()) return false;
        if (IsStaggering()) return false;
        return true;
    }
}