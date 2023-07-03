namespace ClassicUO.Game.Data.OpenUO;

internal class SettingsMacrosFlags
{
    /// <summary>
    /// Sallos Style Targeting
    /// </summary>
    [OptionID(0)]
    public bool AllowSallosTargeting;
		
    /// <summary>
    /// Allow for macros to be set to use potion type on the server side so the server will search for potions.
    /// No longer restricted by having to have a container open, and faster than any string command like [usetype
    /// </summary>
    [OptionID(1)]
    public bool EnhancedPotionMacros;

    /// <summary>
    /// Allow for spell targets to be sent in one packet, bypassing the targeting process
    /// </summary>
    [OptionID(2)]
    public bool EnhancedSpellMacros;
    
    /// <summary>
    /// Allows setting macros for enhanced abilities
    /// </summary>
    [OptionID(3)]
    public bool EnhancedAbilities;
}