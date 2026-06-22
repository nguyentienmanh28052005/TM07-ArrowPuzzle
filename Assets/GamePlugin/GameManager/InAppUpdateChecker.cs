using System.Collections;
#if ENABLE_UPDATE_CHECKER
using Google.Play.AppUpdate;
using Google.Play.Common;
#endif

public class InAppUpdateChecker : UnityEngine.MonoBehaviour
{
#if ENABLE_UPDATE_CHECKER
    
    private AppUpdateManager _appUpdateManager;
    private AppUpdateInfo _cachedAppUpdateInfo;

    private void Start()
    {
        _appUpdateManager = new AppUpdateManager();
        StartCoroutine(CheckForUpdate());
    }

    IEnumerator CheckForUpdate()
    {
        var appUpdateInfoOperation = _appUpdateManager.GetAppUpdateInfo();
        yield return appUpdateInfoOperation;

        if (!appUpdateInfoOperation.IsSuccessful)
        {
            UnityEngine.Debug.LogError($"Check update failed: {appUpdateInfoOperation.Error}");
            yield break;
        }

        _cachedAppUpdateInfo = appUpdateInfoOperation.GetResult();

        UnityEngine.Debug.Log(
            $"Availability: {_cachedAppUpdateInfo.UpdateAvailability}, " +
            $"Priority: {_cachedAppUpdateInfo.UpdatePriority}, " +
            $"ClientVersionStalenessDays: {_cachedAppUpdateInfo.ClientVersionStalenessDays}"
        );

        // Có update và cho phép Flexible
        if (_cachedAppUpdateInfo.UpdateAvailability == UpdateAvailability.UpdateAvailable && _cachedAppUpdateInfo.IsUpdateTypeAllowed(AppUpdateOptions.ImmediateAppUpdateOptions()))
        {
            yield return StartImmediateUpdate(_cachedAppUpdateInfo);
        }

        // Hoặc ép Immediate nếu muốn
        /*
        if (_cachedAppUpdateInfo.UpdateAvailability == UpdateAvailability.UpdateAvailable &&
            _cachedAppUpdateInfo.IsUpdateTypeAllowed(AppUpdateType.Immediate))
        {
            yield return StartImmediateUpdate(_cachedAppUpdateInfo);
        }
        */
    }

    IEnumerator StartFlexibleUpdate(AppUpdateInfo appUpdateInfo)
    {
        var appUpdateOptions = AppUpdateOptions.FlexibleAppUpdateOptions();
        var startUpdateRequest = _appUpdateManager.StartUpdate(appUpdateInfo, appUpdateOptions);

        while (!startUpdateRequest.IsDone)
        {
            if (startUpdateRequest.Status == AppUpdateStatus.Downloading)
            {
                if (appUpdateInfo.TotalBytesToDownload > 0)
                {
                    float progress = (float)appUpdateInfo.BytesDownloaded / appUpdateInfo.TotalBytesToDownload;
                    UnityEngine.Debug.Log($"Downloading update: {(progress * 100f):0.0}%");
                }
            }

            yield return null;
        }

        if (startUpdateRequest.Error != AppUpdateErrorCode.NoError)
        {
            UnityEngine.Debug.LogError($"Start flexible update failed: {startUpdateRequest.Error}");
            yield break;
        }

        UnityEngine.Debug.Log("Flexible update downloaded. Calling CompleteUpdate...");

        var completeUpdateOperation = _appUpdateManager.CompleteUpdate();
        yield return completeUpdateOperation;

        if (!completeUpdateOperation.IsSuccessful)
        {
            UnityEngine.Debug.LogError($"Complete update failed: {completeUpdateOperation.Error}");
        }
    }

    IEnumerator StartImmediateUpdate(AppUpdateInfo appUpdateInfo)
    {
        var appUpdateOptions = AppUpdateOptions.ImmediateAppUpdateOptions();
        var startUpdateRequest = _appUpdateManager.StartUpdate(appUpdateInfo, appUpdateOptions);

        while (!startUpdateRequest.IsDone)
        {
            yield return null;
        }

        if (startUpdateRequest.Error != AppUpdateErrorCode.NoError)
        {
            UnityEngine.Debug.LogError($"Start immediate update failed: {startUpdateRequest.Error}");
            yield break;
        }

        UnityEngine.Debug.Log("Immediate update flow finished.");
    }
#endif
}