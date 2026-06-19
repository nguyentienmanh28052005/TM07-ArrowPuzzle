using UnityEngine;

public enum GameplayLockReason
{
    CameraIntro = 0,
    LevelLoading = 1,
    MemoryModeHold = 2,
    GameOverSequence = 3,
    BoosterActive = 4,
    BoosterUI = 5,
    TimeAttack = 6,
    EraseMode = 7
}

public static class GameplayInputLock
{
    private static int _activeLocks = 0;

    public static bool IsLocked => _activeLocks != 0;

    public static void SetLock(GameplayLockReason reason, bool locked)
    {
        int bit = 1 << (int)reason;
        if (locked)
        {
            _activeLocks |= bit;
        }
        else
        {
            _activeLocks &= ~bit;
        }
    }

    public static bool IsLockedBy(GameplayLockReason reason)
    {
        return (_activeLocks & (1 << (int)reason)) != 0;
    }

    public static void ClearAll()
    {
        _activeLocks = 0;
    }
}
