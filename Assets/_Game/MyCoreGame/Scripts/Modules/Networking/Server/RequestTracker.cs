using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using time;

public class RequestTracker
{
    private static readonly object _lock = new object();
    private static readonly Dictionary<string, List<RequestInfo>> ActiveRequests = new();
    
    private static bool isRunning = false;
    private static CancellationTokenSource cts;
    public static void StartChecking()
    {
        lock (_lock)
        {
            if (isRunning) return;
            isRunning = true;
            cts = new CancellationTokenSource();
            CheckTimeOut().Forget();
        }
    }

    public static void StopChecking()
    {
        lock (_lock)
        {
            isRunning = false;
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }

            ClearAllRequests();
        }
    }

    private static async UniTaskVoid CheckTimeOut()
    {
        while (isRunning)
        {
            try
            {
                await UniTask.Delay(5000, cancellationToken: cts.Token);
                CheckForTimedOutRequests();
            }
            catch (System.OperationCanceledException)
            {
                break;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error in request timeout monitoring: {ex.Message}");
            }
        }
    }
    
    private static void CheckForTimedOutRequests()
    {
        var currentTime = GetCurrentTimeMillis();
        var actionsToUpdate = new List<string>();

        lock (_lock)
        {
            foreach (var (action, requests) in ActiveRequests)
            {
                var timedOutRequests = requests
                    .Where(req => currentTime - req.StartTime > 20000)
                    .ToList();

                if (timedOutRequests.Count > 0)
                {
                    foreach (var request in timedOutRequests)
                    {
                        LogEventCustom.Instance.LogRequestTimeOut(request.Action);
                        requests.Remove(request);
                    }

                    actionsToUpdate.Add(action);
                }
            }

            // Clean up empty request lists
            foreach (var action in actionsToUpdate)
            {
                if (ActiveRequests[action].Count == 0)
                {
                    ActiveRequests.Remove(action);
                }
            }
        }
    }
    
    public static void OnSocketServerError()
    {
        StopChecking();
        LogEventCustom.Instance.LogRequestError();
    }
    
    public static void TrackRequest(string action)
    {
        if (action != "authenication" && !SocketHub.IsLoggedGame.Value)
        {
            return;
        }
        if (string.IsNullOrEmpty(action))
        {
            Debug.Log("Attempted to track a request with null or empty action name");
            return;
        }

        lock (_lock)
        {
            StartChecking();
            var requestInfo = new RequestInfo
            {
                Action = action,
                StartTime = GetCurrentTimeMillis(),
            };
            if (ActiveRequests.TryGetValue(action, out var request))
            {
                request.Add(requestInfo);
            }
            else
            {
                ActiveRequests.Add(action, new List<RequestInfo>() { requestInfo });
            }
        }
    }

    public static void LogRequestSuccess(string action)
    {
        var foundRequest = false;
        var responseTimeSeconds = 0f;
        lock (_lock)
        {
            foundRequest = TryGetAndRemoveFirstRequest(action, out var request, out responseTimeSeconds);
        }

        if (foundRequest)
        {
            LogEventCustom.Instance.LogRequestSuccess(action, responseTimeSeconds);
        }
        else
        {
            Debug.LogWarning($"Cannot find matching request for response: {action}");
        }
    }

    public static void LogRequestFailed(string action, string errorMessage)
    {
        var foundRequest = false;
        var responseTimeSeconds = 0f;
        lock (_lock)
        {
            foundRequest = TryGetAndRemoveFirstRequest(action, out var request, out responseTimeSeconds);
        }
        if (foundRequest)
        {
            LogEventCustom.Instance.LogRequestFailed(action, errorMessage, responseTimeSeconds);
        }
        else
        {
            Debug.LogWarning($"Cannot find matching request for failed response: {action}");
        }
    }
    private static bool TryGetAndRemoveFirstRequest(string action, out RequestInfo request, out float responseTimeSeconds)
    {
        request = null;
        responseTimeSeconds = 0;
            
        if (!ActiveRequests.TryGetValue(action, out var requests) || requests.Count == 0)
        {
            return false;
        }
            
        request = requests.First();
        responseTimeSeconds = (float)(GetCurrentTimeMillis() - request.StartTime) / 1000f;
        requests.Remove(request);
            
        if (requests.Count == 0)
        {
            ActiveRequests.Remove(action);
        }
            
        return true;
    }
    private static void ClearAllRequests()
    {
        lock (_lock)
        {
            ActiveRequests.Clear();
        }
    }
    private static long GetCurrentTimeMillis()
    {
        return mygame.sdk.GameHelper.CurrentTimeMilisReal();
    }
}
public class RequestInfo
{
    public string Action { get; set; }
    public long StartTime { get; set; }
}