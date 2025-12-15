using UnityEngine;

namespace Norsemen;

public partial class Viking
{
    public Vector3 m_hairColor = Vector3.one;
    public Vector3 m_skinColor = Vector3.one;
    
    public void SetupCustomization()
    {
        m_hairColor = m_nview.GetZDO().GetVec3(ZDOVars.s_hairColor, Vector3.zero);
        m_skinColor = m_nview.GetZDO().GetVec3(ZDOVars.s_skinColor, Vector3.one);
        
        bool isSet = m_nview.GetZDO().GetBool(VikingVars.isSet);
        if (isSet) return;
        
        SetRandomModel(out bool isFemale);
        SetRandomName();
        SetRandomHair();
        SetRandomBeard(isFemale);
        SetRandomHairColor();
        SetRandomSkinColor();
        m_nview.GetZDO().Set(VikingVars.isSet, true);
    }

    public void SetRandomSkinColor()
    {
        Color color = CustomizationManager.GetRandomSkinColor();
        Vector3 vec = Utils.ColorToVec3(color);
        SetSkinColor(vec);
    }

    public void SetSkinColor(Vector3 color)
    {
        if (m_skinColor == color) return;
        m_skinColor = color;
        m_visEquipment.SetSkinColor(color);
    }
    
    public void SetRandomModel(out bool isFemale)
    {
        int index = UnityEngine.Random.Range(0, 2);
        m_visEquipment.SetModel(index);
        isFemale = index != 0;
    }

    public void SetRandomBeard(bool isFemale)
    {
        if (isFemale) return;
        if (CustomizationManager.beards.Count <= 0) return;
        string? beard = CustomizationManager.beards[UnityEngine.Random.Range(0, CustomizationManager.beards.Count)];
        SetBeard(beard);
    }

    public void SetRandomHair()
    {
        if (CustomizationManager.hairs.Count <= 0) return;
        string? hair = CustomizationManager.hairs[UnityEngine.Random.Range(0, CustomizationManager.hairs.Count)];
        SetHair(hair);
    }

    public void SetRandomHairColor()
    {
        Color color = CustomizationManager.hairColors[UnityEngine.Random.Range(0, CustomizationManager.hairColors.Count)];
        SetHairColor(Utils.ColorToVec3(color));
    }

    public void SetHairColor(Vector3 color)
    {
        if (m_hairColor == color) return;
        m_hairColor = color;
        m_visEquipment.SetHairColor(m_hairColor);
    }
}