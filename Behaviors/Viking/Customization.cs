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
        m_hairItem = m_nview.GetZDO().GetString(ZDOVars.s_hairItem);
        m_beardItem = m_nview.GetZDO().GetString(ZDOVars.s_beardItem);
        m_isElf = m_nview.GetZDO().GetBool(VikingVars.isElf);
        
        bool isSet = m_nview.GetZDO().GetBool(VikingVars.isSet);
        if (!isSet)
        {
            SetRandomModel(out bool isFemale);
            SetRandomName();
            SetRandomHair();
            SetRandomBeard(isFemale);
            SetRandomHairColor();
            SetRandomSkinColor();
            
            m_isElf = UnityEngine.Random.value > 0.5f;
            m_nview.GetZDO().Set(VikingVars.isElf, m_isElf);
            m_nview.GetZDO().Set(VikingVars.isSet, true);
        }
        
        SetElfEars();
        m_visEquipment.SetHairEquipped(string.IsNullOrEmpty(m_hairItem) ? 0 : m_hairItem.GetStableHashCode());
        m_visEquipment.SetBeardEquipped(string.IsNullOrEmpty(m_beardItem) ? 0 : m_beardItem.GetStableHashCode());
    }
    
    public void SetRandomName()
    {
        bool isFemale = m_nview.GetZDO().GetInt(ZDOVars.s_modelIndex) != 0;
        string randomName;
        if (isFemale)
        {
            randomName = NameGenerator.names.GenerateFemaleName();
        }
        else
        {
            randomName = NameGenerator.names.GenerateMaleName();
        }
        m_nview.GetZDO().Set(ZDOVars.s_tamedName, randomName);
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
        if (isFemale)
        {
            SetBeard("");
        }
        else
        {
            if (CustomizationManager.beards.Count <= 0) return;
            string? beard = CustomizationManager.beards[UnityEngine.Random.Range(0, CustomizationManager.beards.Count)];
            SetBeard(beard);
        }
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