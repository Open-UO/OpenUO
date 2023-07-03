using System;

namespace ClassicUO.Game.Data.OpenUO;

public class MovementSettings
{
    public int MoveSpeedWalkingUnmounted { get; set; } = MovementSpeed.STEP_DELAY_WALK;
    public int MoveSpeedRunningUnmounted { get; set; } = MovementSpeed.STEP_DELAY_RUN;

    //Default move speed with a mount
    public int MoveSpeedWalkingMounted { get; set; } = MovementSpeed.STEP_DELAY_MOUNT_WALK;
    public int MoveSpeedRunningMounted { get; set; } = MovementSpeed.STEP_DELAY_MOUNT_RUN;

    //Turning delay
    public ushort TurnDelay { get; set; } = 100;
}