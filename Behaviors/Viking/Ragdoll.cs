namespace Norsemen;

public partial class Viking
{
    public override void OnRagdollCreated(Ragdoll ragdoll)
    {
        base.OnRagdollCreated(ragdoll);
        if (ragdoll.TryGetComponent(out Norseman.ExtraRagdoll component))
        {
            component.SetElfEars(m_isElf, Utils.Vec3ToColor(m_skinColor));
        }
    }
}