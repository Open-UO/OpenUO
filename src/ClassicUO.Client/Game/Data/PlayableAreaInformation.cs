using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.Data;

public class PlayableAreaInformation
{
    private bool m_Blocking;
    private ushort m_Hue;
    public ushort Hue => m_Hue;
    private List<Rectangle> m_Area;
    public bool Blocking => m_Blocking;

    public PlayableAreaInformation(bool blocking, ushort hue, List<Rectangle> area)
    {
        m_Blocking = blocking;
        m_Hue = hue;
        m_Area = area;
    }

    public bool HighLightObject(int x, int y, out ushort hue)
    {
        hue = m_Hue;
        return !Contains(x, y);
    }
    
    public bool Contains(int x, int y)
    {
        for (int i = 0; i < m_Area.Count; i++)
        {
            var rect = m_Area[i];

            if (rect.Contains(x, y))
            {
                return true;
            }
        }
        return false;
    }
}