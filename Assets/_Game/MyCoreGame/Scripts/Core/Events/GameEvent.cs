using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public delegate Task EventSynchData(UserData dataUser);

public static class GameEvent
{
    public static Action OnStartGame { get; set; }
    public static Action OnNewDay { get; set; }
    public static Action OnNewWeek { get; set; }

    /// <summary>
    /// Level | 
    /// Difficult |
    /// Game Mode |
    /// </summary>
    public static Action<int, int, EGameMode> OnStartLevel { get; set; }

    /// <summary>
    /// Level | 
    /// Result (win/lose) | 
    /// Difficult |
    /// Game Mode |
    /// </summary>
    public static Action<int, bool, int, EGameMode> OnFinishLevel { get; set; }
    
    public static Action OnIncreaseLevel { get; set; }

    public static Action OnPauseGame { get; set; }
    public static Action OnResumeGame { get; set; }

    public static Action OnBackToMenu { get; set; }
    public static Action LoadNewLevel { get; set; }

/*    public static EventSynchData OnReceivedDataToServer { get; set; }
    public static Action<UserData> OnSendDataToServer { get; set; }*/
    public static Action OnRequestGetAccessDone { get; set; }
    public static Action<string> OnReceiveSubcription { get; set; }

    /// <param name="arg1">receive where</param>
    /// <param name="arg2">call back</param>
    /// <param name="arg3">resource receive</param>
    public static Action<LogEvent.ReasonItem, string, DataResource[], int> OnReceiveResource { get; set; }

    public static Action OnGetPackConfig { get; set; }
    public static Action<EEventConfig> ClearSuggetedShowJoinEvent { get; set; }
    public static Action OnRefreshAvatar;
    public static Action OnReceiveFirebaseDataDone;
    public static Action OnClaimStar;

}


public enum ETypeReceiveGiftt
{
    none,
    popup,
    fixed_ui,
}

public enum EGameMode
{
    Level = 0,
}