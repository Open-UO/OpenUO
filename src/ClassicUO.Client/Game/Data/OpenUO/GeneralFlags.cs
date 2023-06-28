﻿namespace ClassicUO.Game.Data.OpenUO;

internal class SettingGeneralFlags
{
    /// <summary>
    /// Skip the City Select menu when creating a new character
    /// </summary>
    [OptionID(0)]
    public bool DisableCityOption;

    /// <summary>
    /// Disable all race options when creating a new character
    /// </summary>
    [OptionID(1)]
    public bool DisableRaceOptions;
		
    /// <summary>
    /// Cooldowns can be sent from the server to display remaining cooldowns
    /// </summary>
    [OptionID(2)]
    public bool CooldownGumpEnabled;

    /// <summary>
    /// Enhanced buff system which gives serial numbers to buffs rather than using the gumpid to categorize them.
    /// </summary>
    [OptionID(3)]
    public bool EnhancedBuffInformation;
}