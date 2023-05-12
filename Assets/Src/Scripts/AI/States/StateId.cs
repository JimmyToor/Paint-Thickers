using System;

namespace Src.Scripts.AI.States
{
    [Serializable]
    public enum StateId
    {
        Idle,
        Attacking,
        Wander,
        Sunk,
        Standing,
        SunkStruggle,
        SunkEscapePaint,
        Scanning,
        TargetSighted,
        TargetLost,
        Patrol
    }
}