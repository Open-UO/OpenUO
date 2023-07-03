using System;
using System.Collections.Generic;

namespace ClassicUO.Game.UI.Gumps
{
    internal class ActiveAbility
    {
        public string Name;
        public string Description;
        public int IconLarge;
        public TimeSpan Cooldown;
        public DateTime CooldownStart;
        public DateTime CooldownEnd;
        public short Hue;
        public short Charges;
        public bool UseNextMove;
        public DateTime InUseUntil;
        public DateTime InUseStart;
    }

    internal class ActiveAbilityObject
    {
        public string Name;
        public int Serial;
        public List<ActiveAbility> Abilities;
    }
}