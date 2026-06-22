using System;
using System.Collections.Generic;
using UnityEngine;

public class AdAudioHelper : MonoBehaviour
{
    public static AdAudioHelper Instance = null;

    public List<BaseAdAudio> listAdRes;
    List<int> ListTypeAdUse = new List<int>();
    public Dictionary<int, BaseAdAudio> listAdUse { get; set; }

    long tShowAd = 0;
    int idxShow = 0;
    public bool isPlayAudioMob { get; set; }
    public int CfCountShowAudioMob = 3;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            listAdUse = new Dictionary<int, BaseAdAudio>();

            foreach (var ad in listAdRes)
            {
                listAdUse.Add((int)ad.typeAdAudio, ad);
            }

            foreach (var ad in listAdUse)
            {
                ad.Value.onAwake();
            }
            GameAdsHelperBridge.CBRequestGDPR += onShowCmp;
            isPlayAudioMob = false;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (this != Instance) Destroy(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        string cfadau = PlayerPrefs.GetString("cf_list_adaudio", "0,1,0");
        CfCountShowAudioMob = PlayerPrefs.GetInt("cf_count_show_aodiomob", 3);
        initListAdAu(cfadau);
        foreach (var ad in listAdUse)
        {
            ad.Value.onStart();
        }
    }

    // Update is called once per frame
    void Update()
    {
#if ENABLE_ADAUDIO
        foreach (var ad in listAdUse)
        {
            ad.Value.onUpdate();
        }
#endif
    }

    private void onShowCmp(int state, string des)
    {
        if (state == 0)
        {
            onShowCmpNative();
        }
        else if (state == 1)
        {
            if (des != null && des.Length > 5)
            {
                onCMPOK(des);
            }
        }
    }

    private void onShowCmpNative()
    {
        foreach (var ad in listAdUse)
        {
            ad.Value.onShowCmpNative();
        }
    }

    private void onCMPOK(string iABTCv2String)
    {
        PlayerPrefs.SetString("mem_iab_tcv2", iABTCv2String);
        foreach (var ad in listAdUse)
        {
            ad.Value.onCMPOK(iABTCv2String);
        }
    }
    void addRes2Use()
    {
        ListTypeAdUse.Clear();
        foreach (var ad in listAdRes)
        {
            ListTypeAdUse.Add((int)ad.typeAdAudio);
        }
    }
    public void initListAdAu(string datalist)
    {
        Debug.Log($"mysdk: adau AdAudioHelper initListAdAu={datalist}");
        ListTypeAdUse.Clear();
        if (PlayerPrefs.GetInt("cf_adaudio_enable", 1) != 1)
        {
            return;
        }
        if (datalist == null || datalist.Length == 0)
        {
            addRes2Use();
        }
        else
        {
            string[] arrtype = datalist.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (arrtype != null && arrtype.Length > 0)
            {
                for (int i = 0; i < arrtype.Length; i++)
                {
                    int type = -1;
                    if (int.TryParse(arrtype[i], out type))
                    {
                        if (listAdUse.ContainsKey(type))
                        {
                            ListTypeAdUse.Add(type);
                        }
                    }
                }
                if (datalist == null || datalist.Length == 0)
                {
                    addRes2Use();
                }
            }
            else
            {
                addRes2Use();
            }
        }
        if (idxShow >= ListTypeAdUse.Count)
        {
            idxShow = 0;
        }
    }
    //-----------------
    public static void show(Canvas cv, RectTransform rectTf)
    {
        int countIntershow = PlayerPrefs.GetInt("count_full_4_point", 0);
        int cfco = PlayerPrefs.GetInt("cf_adaudio_cofull", 0);
        Debug.Log($"mysdk: adau AdAudioHelper show cofuu={countIntershow}, cf={cfco}");
        if (Instance != null && Instance.isOverTimeShow() && PlayerPrefs.GetInt("cf_adaudio_enable", 1) == 1 && countIntershow > cfco)
        {
            BaseAdAudio adau = Instance.getAd();
            if (adau != null)
            {
                adau.show(cv, rectTf);
            }
        }
        else
        {
            if (Instance != null)
            {
                Debug.Log($"mysdk: adau AdAudioHelper show cf time={Instance.isOverTimeShow()}");
            }
        }
    }
    public static void show(int xOffset, int yOffset, int size)
    {
        int countIntershow = PlayerPrefs.GetInt("count_full_4_point", 0);
        int cfco = PlayerPrefs.GetInt("cf_adaudio_cofull", 3);
        Debug.Log($"mysdk: adau AdAudioHelper show cofuu={countIntershow}, cf={cfco}");
        if (Instance != null && Instance.isOverTimeShow() && PlayerPrefs.GetInt("cf_adaudio_enable", 1) == 1 && countIntershow > cfco)
        {
            BaseAdAudio adau = Instance.getAd();
            if (adau != null)
            {
                adau.show(xOffset, yOffset, size);
            }
        }
        else
        {
            if (Instance != null)
            {
                Debug.Log($"mysdk: adau AdAudioHelper show cf time={Instance.isOverTimeShow()}");
            }
        }
    }

    private BaseAdAudio getAd()
    {
        int n = ListTypeAdUse.Count;
        BaseAdAudio adau = null;
        while (adau == null && n > 0)
        {
            adau = listAdUse[ListTypeAdUse[idxShow]];
            idxShow++;
            if (idxShow >= ListTypeAdUse.Count)
            {
                idxShow = 0;
            }
            n--;
            if (!adau.isShow())
            {
                adau = null;
            }
        }
        return adau;
    }

    public static void hide()
    {
        if (Instance != null)
        {
            foreach (var ad in Instance.listAdUse)
            {
                ad.Value.hideAds();
            }
        }
    }

    public static void close()
    {
        if (Instance != null)
        {
            foreach (var ad in Instance.listAdUse)
            {
                ad.Value.close();
            }
        }
    }

    //

    bool isOverTimeShow()
    {
        long tcurr = mygame.sdk.SdkUtil.CurrentTimeMilis();
        long dtcf = PlayerPrefs.GetInt("cf_adaudio_deltatime", 90);
        if ((tcurr - tShowAd) >= dtcf * 1000)
        {
            return true;
        }
        return false;
    }

    public void onShowAd(BaseAdAudio ad)
    {
        tShowAd = mygame.sdk.SdkUtil.CurrentTimeMilis();
    }

    public void onCloseAd(BaseAdAudio ad)
    {
        tShowAd = mygame.sdk.SdkUtil.CurrentTimeMilis();
    }
}
