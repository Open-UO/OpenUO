namespace ClassicUO.Game.Data.OpenUO;

internal class SettingOptionFlags
{
    /// <summary>
    /// Rolling Text Options showing up in the option menu, with this disabled you will still be able to send rolling text
    /// Clients just won't be able to change rolling text options.
    /// </summary>
    [OptionID(0)]
    public bool ShowRollingTextOptions;
		
    /// <summary>
    /// Friend manager which will prevent targets from being selected as hostile targets
    /// </summary>
    [OptionID(1)]
    public bool FriendManagerOptions;

    /// <summary>
    /// Split Hostile and Beneficial Last Targets Options
    /// </summary>
    [OptionID(2)]
    public bool AllowSplitTargetsOptions;

    /// <summary>
    /// Option for client to enable a pointer showing where their selected mobile (last target / last beneficial target is)
    /// </summary>
    [OptionID(3)]
    public bool AllowSelectedMobileDisplay;

    /// <summary>
    /// Allow Area of Effect Target Options
    /// </summary>
    [OptionID(4)]
    public bool AreaOfEffectTargetOptions;
    
    /// <summary>
    /// Allow offscreen targeting 
    /// </summary>
    [OptionID(5)]
    public bool AllowOffscreenTargeting;

    /// <summary>
    /// Allow Players the option to disable Walking Animations
    /// </summary>
    [OptionID(6)]
    public bool NoWalkAnimationOption;
    
    
    /// <summary>
    /// Allow Players to toggle whether they are using fast turning or not.
    /// If this option is disabled players will always turn at the global limit.
    /// </summary>
    [OptionID(7)]
    public bool FastTurnOption;
}