using ClassicUO.Game.GameObjects;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.Data;

internal class SpecialHealthBarData
{
    public Entity Entity;
    public Point Offset;
    public double ScaleWidth;
    public double ScaleHeight;
    public int BackgroundHue;
    public SpecialStatusBarID ForegroundID;
    public int ForegroundHue;
    public SpecialStatusLocation Location;
    public bool HideWhenFull;

    public SpecialHealthBarData(Entity entity, Point offset, double scaleWidth, double scaleHeight, int backgroundHue, SpecialStatusBarID foregroundId, int foregroundHue, SpecialStatusLocation location, bool hideWhenFull)
    {
        Entity = entity;
        Offset = offset;
        ScaleWidth = scaleWidth;
        ScaleHeight = scaleHeight;
        BackgroundHue = backgroundHue;
        ForegroundID = foregroundId;
        ForegroundHue = foregroundHue;
        Location = location;
        HideWhenFull = hideWhenFull;
    }

    public enum SpecialStatusLocation
    {
        Bottom = 0,
        Left,
        Top,
        Right
    }

    public enum SpecialStatusBarID
    {
        Grey = 0,
        Blue,
        Green,
        Yellow
    }
}