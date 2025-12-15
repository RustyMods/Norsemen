namespace Norsemen;

public partial class VikingAI
{
    public bool UpdateSleeping(float dt)
    {
        UpdateSleep(dt);
        return IsSleeping();
    }
}