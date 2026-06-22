using System.Runtime.InteropServices;
using UnityEngine;

public static class URLSchemeChecker
{
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern bool _CanOpenURL(string urlScheme);
#endif

    public static bool CanOpen(string urlScheme)
    {
#if UNITY_IOS && !UNITY_EDITOR
        return _CanOpenURL(urlScheme);
#else
        return false;
#endif
    }
}