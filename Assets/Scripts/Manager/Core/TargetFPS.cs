using System.Collections; 
using System.Threading; 
using UnityEngine; 

public class TargetFPS: MonoBehaviour 
{ 
    [Header("Frame Settings")] 
    int MaxRate = 9999; 
    public float TargetFrameRate = 60.0f; 
    float currentFrameTime;

    /// <summary>
    /// Khởi tạo và tắt VSync để chuẩn bị ép xung khung hình bằng tay.
    /// </summary>
    void Awake()
    {
        QualitySettings.vSyncCount = 0; 
        Application.targetFrameRate = MaxRate; 
        currentFrameTime = Time.realtimeSinceStartup; 
        
        StartCoroutine(WaitForNextFrame()); 
    }

    /// <summary>
    /// Coroutine điều tiết luồng chạy của Main Thread để giữ FPS ở mức mục tiêu mong muốn.
    /// </summary>
    IEnumerator WaitForNextFrame()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            
            currentFrameTime += 1.0f / TargetFrameRate;
            var t = Time.realtimeSinceStartup;
            var sleepTime = currentFrameTime - t - 0.01f;
            
            if (sleepTime > 0)
                Thread.Sleep((int)(sleepTime * 1000));
                
            while (t < currentFrameTime)
                t = Time.realtimeSinceStartup;
        }
    }
}