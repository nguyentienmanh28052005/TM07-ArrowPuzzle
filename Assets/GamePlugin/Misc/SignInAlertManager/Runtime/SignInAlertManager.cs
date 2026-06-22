using System.Runtime.InteropServices;
using UnityEngine;

public static class SignInAlertManager
{
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void _ShowSignInAlert();
#else
    private static void _ShowSignInAlert() { }
#endif

    public static void ShowSignInAlert()
    {
        _ShowSignInAlert();
    }
}