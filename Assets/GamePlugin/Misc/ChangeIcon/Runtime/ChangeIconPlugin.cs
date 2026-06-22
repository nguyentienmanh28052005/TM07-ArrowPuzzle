using System.Runtime.InteropServices;
using UnityEngine;

namespace mygame.sdk
{
    public class ChangeIconPlugin
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern bool _SetAlternateIconName(string iconName);

        [DllImport("__Internal")]
        private static extern string _GetAlternateIconName();
#endif

        /// <summary>
        /// Set alternate app icon by name.
        /// </summary>
        /// <param name="iconName">The name of the alternate icon (matching the png file name without extension). Use null, empty, or "default" to revert to the primary app icon.</param>
        /// <returns>True if the request was successfully sent to the OS (iOS 10.3+).</returns>
        public static bool SetAlternateIcon(string iconName)
        {
#if UNITY_IOS && !UNITY_EDITOR
            return _SetAlternateIconName(iconName);
#elif UNITY_ANDROID && !UNITY_EDITOR
            // Android doesn't natively support dynamic runtime app icon changing in the same clean way as iOS
            return false;
#else
            Debug.Log($"[ChangeIconPlugin] Simulating setting app icon to: {iconName}");
            return true;
#endif
        }

        /// <summary>
        /// Gets the current active alternate icon name. Returns "default" if the primary icon is active.
        /// </summary>
        public static string GetCurrentIconName()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return _GetAlternateIconName();
#else
            return "default";
#endif
        }
    }
}
