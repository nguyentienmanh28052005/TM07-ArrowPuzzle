using UnityEngine;

[DisallowMultipleComponent]
public sealed class WinStreakManager : MonoBehaviour
{
    private const int ProgressKey = 300;
    private const int GoalKey = 301;

    public const int DefaultGoal = 20;
    public const int RewardCoins = 1000;
    public const int RewardDiamonds = 50;

    [Header("Streak Settings")]
    [SerializeField, Min(1)] private int streakGoal = DefaultGoal;

    [Header("Reward")]
    [SerializeField, Min(0)] private int rewardCoins = RewardCoins;
    [SerializeField, Min(0)] private int rewardDiamonds = RewardDiamonds;

    public static WinStreakManager Instance { get; private set; }

    public struct StreakState
    {
        public int progress;
        public int goal;
        public bool rewardClaimed;
        public int rewardCoins;
        public int rewardDiamonds;
    }

    private int ConfiguredGoal => Mathf.Max(1, streakGoal);
    private int ConfiguredRewardCoins => Mathf.Max(0, rewardCoins);
    private int ConfiguredRewardDiamonds => Mathf.Max(0, rewardDiamonds);

    public static int Progress => Mathf.Clamp(Mathf.RoundToInt(GetSaveValue(ProgressKey)), 0, Goal);

    public static int Goal
    {
        get
        {
            if (Instance != null)
            {
                return Instance.ConfiguredGoal;
            }

            int savedGoal = Mathf.RoundToInt(GetSaveValue(GoalKey));
            return savedGoal > 0 ? savedGoal : DefaultGoal;
        }
    }

    public static int CurrentRewardCoins => Instance != null ? Instance.ConfiguredRewardCoins : RewardCoins;
    public static int CurrentRewardDiamonds => Instance != null ? Instance.ConfiguredRewardDiamonds : RewardDiamonds;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        SaveValue(GoalKey, ConfiguredGoal);
    }

    private void OnEnable()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void OnDisable()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnValidate()
    {
        streakGoal = Mathf.Max(1, streakGoal);
        rewardCoins = Mathf.Max(0, rewardCoins);
        rewardDiamonds = Mathf.Max(0, rewardDiamonds);
    }

    public static StreakState GetState()
    {
        return new StreakState
        {
            progress = Progress,
            goal = Goal,
            rewardClaimed = false,
            rewardCoins = CurrentRewardCoins,
            rewardDiamonds = CurrentRewardDiamonds
        };
    }

    public static StreakState RegisterWin()
    {
        int goal = Goal;
        int coins = CurrentRewardCoins;
        int diamonds = CurrentRewardDiamonds;
        int nextProgress = Mathf.Clamp(Progress + 1, 0, goal);
        bool rewardClaimed = nextProgress >= goal;

        if (rewardClaimed)
        {
            GrantReward();
            nextProgress = 0;
        }

        SaveValue(ProgressKey, nextProgress);
        SaveValue(GoalKey, goal);

        StreakState state = new StreakState
        {
            progress = nextProgress,
            goal = goal,
            rewardClaimed = rewardClaimed,
            rewardCoins = rewardClaimed ? coins : 0,
            rewardDiamonds = rewardClaimed ? diamonds : 0
        };

        if (MessageManager.Instance != null)
        {
            MessageManager.Instance.SendMessage(ManhMessageType.OnWinStreakChanged, state);
        }

        return state;
    }

    public static void ResetProgress()
    {
        SaveValue(ProgressKey, 0);

        if (MessageManager.Instance != null)
        {
            MessageManager.Instance.SendMessage(ManhMessageType.OnWinStreakChanged, GetState());
        }
    }

    private static void GrantReward()
    {
        if (CurrencyManager.Instance == null) return;

        CurrencyManager.Instance.AddCoins(CurrentRewardCoins);
        CurrencyManager.Instance.AddDiamonds(CurrentRewardDiamonds);
    }

    private static float GetSaveValue(int key)
    {
        return SaveDataPlayer.Instance != null ? SaveDataPlayer.Instance.Value(key) : 0f;
    }

    private static void SaveValue(int key, float value)
    {
        if (SaveDataPlayer.Instance != null)
        {
            SaveDataPlayer.Instance.Save(key, value);
        }
    }
}
