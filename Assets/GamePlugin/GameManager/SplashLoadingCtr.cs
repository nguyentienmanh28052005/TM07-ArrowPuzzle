using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using mygame.sdk;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class SplashLoadingCtr : MonoBehaviour
{
    public static int CFShowSplashVN
    {
        get
        {
            return PlayerPrefs.GetInt("cf_show_splash_vn", 0);
        }
        set
        {
            PlayerPrefs.SetInt("cf_show_splash_vn", value);
        }
    }
    private int lastDurationVNSplash
    {
        get
        {
            return PlayerPrefs.GetInt("cf_duration_splash_vn", 0);
        }
        set
        {
            PlayerPrefs.SetInt("cf_duration_splash_vn", value);
        }
    }
    private const float perStep = 0.75f;
    public Slider imgFill;
    public Text txtFill;
    public bool isSlide = true;
    private int isWaitConfig = 0;
    public float toTimeWait = 2;
    private float minWait = 2;
    private int stateGetCf = 0;
    float timestep1 = 0;
    float timestep2 = 0;
    public float timeRun = 0;
    float timer;
    float kTime = 1;
    float tcheckads = 0;
    bool isFinishSplash = false;
    bool isLoadNext = false;
    AsyncOperation asyncLoadNext;
    long tShow = 0;
    private int stateShowSplash;
    private float wFill = 0;
    private float xFill = 0;
    private float ySizeDelta = 0;
    public bool isPause;

    public static bool isLoading = false;

    public void showLoading(float time = 2, int isWaitCondition = 0, float minwait = 2)
    {
        LogEvent.ScreenGo(LogEvent.ScreenName.Loading);
        RectTransform rc = GetComponent<RectTransform>();
        rc.anchorMin = new Vector2(0, 0);
        rc.anchorMax = new Vector2(1, 1);
        rc.sizeDelta = new Vector2(0, 0);
        rc.anchoredPosition = Vector2.zero;
        rc.anchoredPosition3D = Vector3.zero;
        SDKManager.Instance.isAllowShowFirstOpen = false;
        toTimeWait = time;
        this.minWait = minwait;
        if (this.minWait < 1)
        {
            this.minWait = 1;
        }
        isWaitConfig = isWaitCondition;
        stateGetCf = 0;
        if (isWaitConfig == 1)
        {
            //if (GameHelper.checkLvXaDu())
            //{
            //    toTimeWait = this.minWait;
            //}
        }
        if (toTimeWait < 1)
        {
            toTimeWait = 1;
        }

        //FIRhelper.CBGetconfig += onGetConfig;
        timestep1 = perStep * toTimeWait;
        timestep2 = toTimeWait - timestep1;
        timeRun = 0;
        timer = 0;
        imgFill.value = 0.06f;
        txtFill.text = "0%";
        kTime = 1;
        isFinishSplash = false;
        isLoadNext = false;
        asyncLoadNext = null;
        tShow = SdkUtil.CurrentTimeMilis();
        SDKManager.Instance.onChangePlacement("splash");
        stateShowSplash = 0;
        //StartCoroutine(sssttt());
        AdsProcessCB.Instance().Enqueue(() =>
        {
            if (AppConfig.FlagShowRectSplash == 1)
            {
                AdsHelper.Instance.showBannerRect("splash", AD_BANNER_POS.BOTTOM, 0, 300, 0, 0, 1);
            }
            else if (AppConfig.FlagShowRectSplash == 2)
            {
                AdsHelper.Instance.showRectNt("splash", 0, 0, 0.99f, 0.4f, 0, 0);
            }
        }, 0.3f);

        if (isSlide)
        {
            RectTransform rt = (RectTransform)imgFill.transform.parent;
            wFill = rt.sizeDelta.x;
            rt = (RectTransform)imgFill.transform;
            xFill = rt.anchoredPosition.x;
            ySizeDelta = rt.sizeDelta.y;
            wFill -= 2 * xFill;
            rt.sizeDelta = new Vector2(-wFill - 2*xFill, ySizeDelta);
        }
    }
    public void updateTimeSplash(int newTime)
    {
        toTimeWait = newTime;
        if (toTimeWait < 1)
        {
            toTimeWait = 1;
        }
        timestep1 = perStep * toTimeWait;
        timestep2 = toTimeWait - timestep1;
    }
    public void onGetConfig()
    {
        //if (isWaitConfig != 0)
        //{
        //    stateGetCf = 1;
        //}
        //int tmptoTimeWait = PlayerPrefs.GetInt("time_splash_scr", 4);
        /*Debug.Log("aaaaa 2=" + tmptoTimeWait);
        if (tmptoTimeWait < 1)
        {
            tmptoTimeWait = 1;
        }
        if (Mathf.Abs(tmptoTimeWait - toTimeWait) >= 0.5f)
        {
            timeRun = tmptoTimeWait * timeRun / toTimeWait;
            toTimeWait = tmptoTimeWait;
            timestep1 = perStep * toTimeWait;
            timestep2 = toTimeWait - timestep1;
        }*/
    }
    private void HandleSplash()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            CFShowSplashVN = 3;
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            CFShowSplashVN = -1;
        }
        timer += Time.deltaTime;
    }
    private void Update()
    {
        if (isPause) return;
        if (stateGetCf == 1 && timeRun >= minWait)
        {
            stateGetCf = 2;
            //kTime = 3;
        }
        HandleSplash();
        if (timer >= CFShowSplashVN && stateShowSplash == 0 && CFShowSplashVN > 0)
        {
            stateShowSplash = 1;
        }
        /*if (stateGetCf == 0 && timeRun >= minWait && toTimeWait > 3)
        {
            tcheckads += Time.deltaTime;
            if (tcheckads >= 0.5f)
            {
                tcheckads = 0;
                if (AdsHelper.Instance.hasFullLoaded(AdsBase.PLFullSplash, false))
                {
                    stateGetCf = 2;
                    kTime = 10;
                }
            }
        }*/
        if (asyncLoadNext != null && !asyncLoadNext.isDone)
        {
            if (asyncLoadNext.allowSceneActivation && kTime >= 0.9f)
            {
                Debug.Log("mysdk: splash loading 10");
                kTime = 0.75f;
            }
        }
        else
        {
            kTime = 1.0f;
        }
        timeRun += Time.deltaTime * kTime;
        float fillAmount = 1;
        if (timeRun <= timestep1)
        {
            fillAmount = (timeRun * 0.9f / perStep) / toTimeWait;
        }
        else
        {
            fillAmount = 0.9f + 0.1f * (timeRun - timestep1) / (toTimeWait * (1.0f - perStep));
        }
        if (isSlide)
        {
            RectTransform rt = (RectTransform)imgFill.transform;
            xFill = rt.anchoredPosition.x;
            rt.sizeDelta = new Vector2((fillAmount - 1) * wFill - 2*xFill, ySizeDelta);
        }
        else
        {
            imgFill.value = fillAmount;
        }
        int nf = (int)(fillAmount * 100);
        if (asyncLoadNext != null)
        {
            if (nf > 99)
            {
                nf = 99;
            }
        }
        else
        {
            if (nf > 100)
            {
                nf = 100;
            }
        }
        txtFill.text = "" + nf + "%";

        if (timeRun >= toTimeWait && !isFinishSplash)
        {
            isFinishSplash = true;
            if (asyncLoadNext != null)
            {
                txtFill.text = "99%";
            }
            else
            {
                txtFill.text = "100%";
            }
            SDKManager.Instance.isAllowShowFirstOpen = true;
            this.enabled = false;
            if (asyncLoadNext != null)
            {
                Debug.Log("mysdk: splash loading 2");
                asyncLoadNext.allowSceneActivation = true;
            }
            else
            {
                SDKManager.Instance.onSplashFinishLoding(gameObject);
                //FIRhelper.CBGetconfig -= onGetConfig;
                gameObject.SetActive(false);
                Destroy(gameObject);
            }
        }
        else if (!isLoadNext)
        {
            if (nf >= 20)
            {
                isLoadNext = true;
#if UNITY_ANDROID
                if (SDKManager.Instance.IndexSceneLoading > 0)
                {
                    Debug.Log("mysdk: splash loading 0");
                    SDKManager.Instance.StartCoroutine(LoadYourAsyncScene());
                }
#endif
            }
        }
        else if (nf >= 85)
        {
            if (asyncLoadNext != null && !asyncLoadNext.allowSceneActivation)
            {
                Debug.Log("mysdk: splash loading 1");
                asyncLoadNext.allowSceneActivation = true;
            }
        }
    }

    IEnumerator LoadYourAsyncScene()
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        asyncLoadNext = SceneManager.LoadSceneAsync(1);
        asyncLoadNext.allowSceneActivation = false;

        // Wait until the asynchronous scene fully loads
        while (!asyncLoadNext.isDone)
        {
            yield return null;
        }
        long tcu = SdkUtil.CurrentTimeMilis();
        Debug.Log($"mysdk: splash loading 3={tcu - tShow}");
        txtFill.text = "100%";
        asyncLoadNext.allowSceneActivation = true;
        yield return new WaitForSeconds(0.01f);
        SDKManager.Instance.onSplashFinishLoding(gameObject);
        //FIRhelper.CBGetconfig -= onGetConfig;
        yield return new WaitForSeconds(0.9f);
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
