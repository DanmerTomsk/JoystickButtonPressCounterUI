﻿namespace JoystickButtonPressCounterUI.Observer
{
    [Flags]
    internal enum JoyFlags
    {
        JOY_RETURNX = 0x00000001,
        JOY_RETURNY = 0x00000002,
        JOY_RETURNZ = 0x00000004,
        JOY_RETURNR = 0x00000008,
        JOY_RETURNU = 0x00000010,
        JOY_RETURNV = 0x00000020,
        JOY_RETURNPOV = 0x00000040,
        JOY_RETURNBUTTONS = 0x00000080,
        JOY_RETURNCENTERED = 0x00000400,
        JOY_RETURNALL = JOY_RETURNX | JOY_RETURNY | JOY_RETURNZ | JOY_RETURNR | JOY_RETURNU | JOY_RETURNV | JOY_RETURNPOV | JOY_RETURNBUTTONS,
    }
}
