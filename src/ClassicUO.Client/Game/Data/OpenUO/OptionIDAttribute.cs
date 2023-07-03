using System;

namespace ClassicUO.Game.Data.OpenUO;

[AttributeUsage(AttributeTargets.Field)]
public class OptionIDAttribute : Attribute
{
    private readonly int m_ID;
    public OptionIDAttribute(int id)
    {
        m_ID = id;
    }

    public int ID => m_ID;
}