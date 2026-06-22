using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;

public class DebugMenuManager : MonoBehaviour
{
    [Header("Gesture Settings")] public int leftTapRequired = 5;
    public int rightTapRequired = 3;
    public float maxDuration = 3f;

    public static string[] allowedDeviceIds = { };

    private int leftTapCount = 0;
    private int rightTapCount = 0;
    private float timer = 0f;
    private bool gestureTriggered = false;

    private void Start()
    {
        Debug.Log("Device Name: " + SystemInfo.deviceName);
        Debug.Log("Device ID: " + SystemInfo.deviceUniqueIdentifier);
        LoadCachedDeviceIds();
    }

    void LoadCachedDeviceIds()
    {
        string cached = PlayerPrefs.GetString("allowed_device_ids", "");
        if (!string.IsNullOrEmpty(cached))
        {
            try
            {
                allowedDeviceIds = JsonConvert.DeserializeObject<string[]>(cached);
                Debug.Log($"DebugMenuManager: {cached}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning("DebugMenuManager: ⚠️ Failed to parse cached allowed_device_ids: " + ex.Message);
            }
        }
    }


    public static void FetchRemoteConfig(string json)
    {
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                allowedDeviceIds = JsonConvert.DeserializeObject<string[]>(json);
                Debug.Log($"DebugMenuManager: {json}");
                PlayerPrefs.SetString("allowed_device_ids", json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("DebugMenuManager: ⚠️ Failed to parse cached allowed_device_ids: " + ex.Message);
            }
        }
    }

    void Update()
    {
        if (gestureTriggered) return;
        SimulateGestureInEditor();

        timer += Time.deltaTime;
        if (timer > maxDuration)
        {
            ResetGesture();
        }

        if (rightTapCount >= rightTapRequired)
        {
            gestureTriggered = true;
            TryActivateDebugMenu();
        }
    }

    void SimulateGestureInEditor()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 pos = Input.mousePosition;
            if (pos.x < Screen.width * 0.2f && pos.y > Screen.height * 0.8f)
            {
                leftTapCount++;
            }
            else if (pos.x > Screen.width * 0.8f && pos.y > Screen.height * 0.8f)
                rightTapCount++;
        }
    }

    void ResetGesture()
    {
        leftTapCount = 0;
        rightTapCount = 0;
        timer = 0f;
    }

    void TryActivateDebugMenu()
    {
        string deviceName = SystemInfo.deviceName;
        string deviceId = SystemInfo.deviceUniqueIdentifier;

#if UNITY_EDITOR
        Debug.Log("✅ Debug menu activated in Editor.");
        InstantiateDebugMenu();
#else
        bool isAllowed = allowedDeviceIds.Contains(deviceId) || allowedDeviceIds.Contains(UserDataManager.Instance.UserData.playerUUID) || allowedDeviceIds.Contains(UserDataManager.Instance.UserData.name);
        if (isAllowed)
        {
            Debug.Log($"✅ Debug menu activated on device: {deviceName} ({deviceId})");
            InstantiateDebugMenu();
        }
        else
        {
            Debug.LogWarning($"❌ Device not whitelisted: {deviceName} ({deviceId})");
        }
#endif
    }

    void InstantiateDebugMenu()
    {
        var obj = Resources.Load<GameObject>("IngameDebugConsole");
        if (obj != null)
        {
            Instantiate(obj);
        }
        else
        {
            Debug.LogError("❌ IngameDebugConsole prefab not found in Resources folder!");
        }
    }
}