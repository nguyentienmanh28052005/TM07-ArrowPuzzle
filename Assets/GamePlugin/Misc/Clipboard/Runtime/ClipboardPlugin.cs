using System.Runtime.InteropServices;
using UnityEngine;

public class ClipboardPlugin
{
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void _CopyToClipboard(string text);
#endif

    public static void CopyClipboard(string text)
    {
#if UNITY_IOS && !UNITY_EDITOR
        _CopyToClipboard(text);


#elif UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject clipboardService = activity.Call<AndroidJavaObject>("getSystemService", "clipboard");

                AndroidJavaClass clipDataClass = new AndroidJavaClass("android.content.ClipData");
                AndroidJavaObject clip = clipDataClass.CallStatic<AndroidJavaObject>("newPlainText", "label", text);

                clipboardService.Call("setPrimaryClip", clip);

                // Optional: show toast
                // activity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                //     AndroidJavaObject toast = new AndroidJavaClass("android.widget.Toast").CallStatic<AndroidJavaObject>("makeText", activity, "Copied to clipboard!", 0);
                //     toast.Call("show");
                // }));
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("Clipboard copy failed: " + e.Message);
        }
#else
        GUIUtility.systemCopyBuffer = text;
#endif
    }
}