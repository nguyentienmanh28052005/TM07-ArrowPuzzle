using UnityEngine;
using System.Runtime.InteropServices;

public class SharePlugin
{
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void _ShareText(string message, string subject);
#endif
    
    
    public static void ShareText(string message, string subject = "")
    {
#if UNITY_IOS && !UNITY_EDITOR
        _ShareText(message, subject);
#elif UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent"))
        {
            using (AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent"))
            {
                intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
                intentObject.Call<AndroidJavaObject>("setType", "text/plain");
                intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), subject);
                intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), message);

                using (AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");

                    AndroidJavaObject chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, "Share via");
                    currentActivity.Call("startActivity", chooser);
                }
            }
        }
#else
        Debug.Log("ShareText only works on Mobile device.");
#endif
    }
}