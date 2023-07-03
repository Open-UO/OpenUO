using System;

namespace ClassicUO.Game.Data
{
    internal class CooldownTimer
    {
        public CooldownTimer(int itemID, ushort itemHue, float timeInSeconds, int offsetX, int offsetY, string text,
                             ushort circleHue, ushort textHue, ushort countdownHue)
        {
            ItemID = itemID;
            ItemHue = itemHue;

            StartTime = DateTime.UtcNow;
            ExpiryTime = DateTime.UtcNow + TimeSpan.FromSeconds(timeInSeconds);
            OffsetX = offsetX;
            OffsetY = offsetY;
            Text = text;

            CircleHue = circleHue;
            TextHue = textHue;
            CountdownHue = countdownHue;
        }

        public bool IsExpired => ExpiryTime <= DateTime.UtcNow;

        public readonly int ItemID;
        public readonly int ItemHue;
        public readonly DateTime ExpiryTime;
        public readonly DateTime StartTime;
        public readonly int OffsetX;
        public readonly int OffsetY;
        public readonly string Text;
        public readonly ushort CircleHue;
        public readonly ushort TextHue;
        public readonly ushort CountdownHue;
    }
}