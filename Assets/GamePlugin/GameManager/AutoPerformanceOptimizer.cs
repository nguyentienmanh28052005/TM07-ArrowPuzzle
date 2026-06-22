using UnityEngine;
using System.Collections;
using mygame.sdk;

public class AutoPerformanceOptimizer : MonoBehaviour
{
    [Header("Settings for low-end devices")]
    [Range(0.5f, 1f)] public float lowEndRenderScale = 0.75f;
    [Tooltip("Target FPS for low-end devices")]
    public int lowEndTargetFPS = 30;
    public int lowRamThresholdMB = 3000; // 2GB

    [Header("Settings for mid/high-end devices")]
    [Range(0.5f, 1f)] public float highEndRenderScale = 1f;
    public int highEndTargetFPS = 60;

    [Header("Settings detect low-end devices")]
    public int FPSDetectLowEnd = 30;

    public bool devicesIsLowEnd { get; private set; }

    public static AutoPerformanceOptimizer Instance = null;

    public static int flagCheckDevice4Ads { get; set; } = AppConfig.FlagCheckLowEndDevice;
    public static int isChangeLogicAdsLowRam { get; private set; } = 0;

    private void Awake()
    {
        Instance = this;
        devicesIsLowEnd = false;
    }

    void Start()
    {
        flagCheckDevice4Ads = PlayerPrefs.GetInt("flag_cde_4_ad", AppConfig.FlagCheckLowEndDevice);
        checkChangeLogicAdsLowRam();
        bool islowdevice = false;
        int flag = flagCheckDevice4Ads % 100;
        if (flag > 0)
        {
            islowdevice = IsLowEndDevice();
            if (islowdevice)
            {
                ApplyLowEndSettings(1);
            }
        }
        if (flag > 1 && !islowdevice)
        {
            islowdevice = IslowDeviceByName();
            if (islowdevice)
            {
                ApplyLowEndSettings(2);
            }
        }
#if UNITY_ANDROID
        if (flag > 2 && !islowdevice)
        {
            islowdevice = IslowDeviceByScore();
            if (islowdevice)
            {
                ApplyLowEndSettings(3);
            }
        }
#endif
    }

    public static void checkChangeLogicAdsLowRam()
    {
        if (flagCheckDevice4Ads >= 100)
        {
            int ram = SystemInfo.systemMemorySize; // MB
            if (ram < 2100)
            {
                SdkUtil.logd($"change logic ads to low ram");
                FIRhelper.logEvent($"to_low_ram_logicad");
                isChangeLogicAdsLowRam = 101;
            }
            else
            {
                isChangeLogicAdsLowRam = 0;
            }
        }
    }

    bool IsLowEndDevice()
    {
        int ram = SystemInfo.systemMemorySize; // MB
        Debug.Log($"[AutoPerformanceOptimizer] IsLowEndDevice RAM:{ram}MB)");
        // Các điều kiện nhận dạng máy yếu
        if (ram < lowRamThresholdMB) return true; // Dưới 3 GB RAM
        return false; // Mặc định không yếu
    }

    bool IslowDeviceByScore()
    {
        int score = 0;

        // RAM
        int ram = SystemInfo.systemMemorySize;
        if (ram >= 8000) score += 3;
        else if (ram >= 4000) score += 2;
        else score += 1;

        // CPU
        int cpuCores = SystemInfo.processorCount;
        if (cpuCores >= 8) score += 3;
        else if (cpuCores >= 4) score += 2;
        else score += 1;

        // GPU (thô)
        string gpu = SystemInfo.graphicsDeviceName.ToLower();
        if (gpu.Contains("g710") || gpu.Contains("adreno 7")) score += 3;
        else if (gpu.Contains("g76") || gpu.Contains("adreno 6")) score += 2;
        else score += 1;

        Debug.Log($"[AutoPerformanceOptimizer] IslowDeviceByScore RAM:{ram}MB | GPU:{gpu} (cpuCores:{cpuCores})");
        Debug.Log($"[AutoPerformanceOptimizer] IslowDeviceByScore score={score}");

        return (score < 4); // Tổng tối đa = 9
    }

    bool IslowDeviceByName()
    {
        string[] lowDevices = new string[]
        {
            "SM-A107F", "SAMSUNG J6PRIMELTE", // Galaxy A10
            "SM-J2", "SM-J200", "SM-J250",
            "VIVO Y11", "VIVO Y12", "VIVO Y15", "VIVO 2034", "VIVO 1906", "VIVO 1904", "VIVO 1906", "VIVO 2015", "VIVO 1915", "VIVO 1907", "VIVO 2120",
            "REDMI 6A", "REDMI 7A", "REDMI 8A", "REALME RMX2185", "XIAOMI OLIVELITE", "REDMI OLIVEWOOD",
            "CPH1803", "CPH1909", // OPPO A3s, A5s
            "TECNO", "INFINIX", "ITEL", // các hãng giá rẻ
            "MALI-400", "MALI-450", "MALI-T720", // GPU yếu
            "POSITIVO T770G", "LENOVO 8505F",
            "LGE DH0LM", "MOTOROLA CHANNEL"
        };

        string model = SystemInfo.deviceModel.ToUpper();

        Debug.Log($"[AutoPerformanceOptimizer] IslowDeviceByName deviceModel={model}");

        foreach (var s in lowDevices)
            if (model.Contains(s)) return true;

        return false;
    }

    private void getNormalSetting()
    {
        if (!devicesIsLowEnd)
        {
            int qlv = QualitySettings.GetQualityLevel();
            int tf = Application.targetFrameRate;
        }
    }

    public void ApplyLowEndSettings(int type)
    {
        if (devicesIsLowEnd)
        {
            Debug.Log($"[AutoPerformanceOptimizer] ApplyLowEndSettings but is low devices");
            return;
        }
        Debug.Log($"[AutoPerformanceOptimizer] ApplyLowEndSettings {type}");
        FIRhelper.logEvent($"to_low_device_v2_{type}");
        AdsHelper.Instance.hideBanner(0);
        getNormalSetting();
        devicesIsLowEnd = true;
        // Chất lượng
        QualitySettings.SetQualityLevel(0, true); // Low preset
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = lowEndTargetFPS;

        // Giảm render resolution
        ScalableBufferManager.ResizeBuffers(lowEndRenderScale, lowEndRenderScale);

        // Bật texture streaming để tiết kiệm VRAM
        QualitySettings.streamingMipmapsActive = true;

        // Giảm GC pressure
        StartCoroutine(PeriodicMemoryCleanup());

        // Gamma color space nhẹ hơn Linear
        if (QualitySettings.activeColorSpace == ColorSpace.Linear)
            Debug.Log("[Optimizer] Using Gamma color space is recommended for low-end devices.");

        // Giảm audio sample rate nếu dùng Unity Audio
        AudioSettings.outputSampleRate = 22050;
    }

    private void ApplyHighEndSettings()
    {
        Debug.Log($"[AutoPerformanceOptimizer] ApplyHighEndSettings");
        QualitySettings.SetQualityLevel(QualitySettings.names.Length - 1, true);
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = highEndTargetFPS;
        ScalableBufferManager.ResizeBuffers(highEndRenderScale, highEndRenderScale);
        QualitySettings.streamingMipmapsActive = false;
    }

    private IEnumerator PeriodicMemoryCleanup()
    {
        while (true)
        {
            yield return new WaitForSeconds(15f);
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }
    }

    private int GetAndroidAPILevel()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                return version.GetStatic<int>("SDK_INT");
            }
        }
        catch { return -1; }
#else
        return -1;
#endif
    }
}
