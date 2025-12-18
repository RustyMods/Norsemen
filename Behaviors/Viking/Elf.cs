using UnityEngine;

namespace Norsemen;

public partial class Viking
{
    public bool m_isElf = false;
    public GameObject m_elfEars;
    public Material[] m_elfEarMats;

    public void SetElfEars()
    {
        if (m_elfEars == null) return;
        m_elfEars.SetActive(m_isElf);
        SetElfEarColor(Utils.Vec3ToColor(m_skinColor));
    }

    public void SetElfEarColor(Color color)
    {
        if (m_elfEarMats == null) return;

        foreach (Material material in m_elfEarMats)
        {
            material.color = color;
        }
    }
}