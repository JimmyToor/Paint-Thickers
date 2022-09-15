using System;

namespace AI
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