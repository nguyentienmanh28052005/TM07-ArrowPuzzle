using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BoosterConfig
{
    //public static BoosterConfigData configManet = new BoosterConfigData(6,6);
    //public static BoosterConfigData configMutilColorBox = new BoosterConfigData(4,4);
    //public static BoosterConfigData configAddHole = new BoosterConfigData(1,2);
    //public static BoosterConfigData configBreakObject = new BoosterConfigData(1,4);
    //public static BoosterConfigData configClear = new BoosterConfigData(1,9);
    public static BoosterConfigDataWrapper boosterConfigDataWrapper = new BoosterConfigDataWrapper();
    public static BoosterConfigDataWrapper df_BoosterConfigDataWrapper = new BoosterConfigDataWrapper();
    private static string CF_Booster_Data => PlayerPrefs.GetString("config_booster_all");

    public static void SetConfigAll()
    {
        //LoadConfigBooster(CF_Magnet, configManet);
        //LoadConfigBooster(CF_MutilColorBox ,configMutilColorBox);
        //LoadConfigBooster(CF_AddHole, configAddHole);
        //LoadConfigBooster(CF_BreakObject, configBreakObject);
        //LoadConfigBooster(CF_Clear, configClear);
        if (!string.IsNullOrEmpty(CF_Booster_Data) || CF_Booster_Data.Length > 0)
        {
            try
            {
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<BoosterConfigDataWrapper>(CF_Booster_Data);
                boosterConfigDataWrapper = data;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                Debug.LogError(CF_Booster_Data);
            }
        }
    }

    public static void LoadConfigBooster(string CF_Str, BoosterConfigData configBooster)
    {
        if (CF_Str.Length > 0)
        {
            try
            {
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<BoosterConfigData>(CF_Str);
                configBooster = data;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                Debug.LogError(CF_Str);
            }
        }
    }

    public static BoosterConfigData GetConfigData(BoosterType boosterType)
    {
        var res = new BoosterConfigData(0, 0);
        switch (boosterType)
        {
            case BoosterType.Hand:
                if (boosterConfigDataWrapper.configHand == null)
                {
                    res = df_BoosterConfigDataWrapper.configHand;
                }
                else
                {
                    res = boosterConfigDataWrapper.configHand;
                }

                break;
            case BoosterType.Clear:
                if (boosterConfigDataWrapper.configMoveObject == null)
                {
                    res = df_BoosterConfigDataWrapper.configMoveObject;
                }
                else
                {
                    res = boosterConfigDataWrapper.configMoveObject;
                }

                break;
            case BoosterType.Shuffle:
                if (boosterConfigDataWrapper.configShuffle == null)
                {
                    res = df_BoosterConfigDataWrapper.configShuffle;
                }
                else
                {
                    res = boosterConfigDataWrapper.configShuffle;
                }

                break;
            case BoosterType.ExtraSlot:
                if (boosterConfigDataWrapper.configExtraSlot == null)
                {
                    res = df_BoosterConfigDataWrapper.configExtraSlot;
                }
                else
                {
                    res = boosterConfigDataWrapper.configExtraSlot;
                }
                
                break;
        }

        res.levelTutorial = Mathf.Max(res.levelTutorial, res.levelUnlock);
        return res;
    }
}

public class BoosterConfigData
{
    public int levelUnlock;
    public int levelTutorial;
    public int numUseToEasy = 1;
    public int showPopup = 1;
    public int numGift = 1;
    public int forceClick = 0;
    public int surfaceCheck = 1;
    public int showHand = 1;

    public BoosterConfigData(int levelUnlock, int levelTutorial, int numUseToEasy = 1, int showPopup = 1,
        int numGift = 1, int forceClick = 0, int surfaceCheck = 1,int showHand =1)
    {
        this.levelUnlock = levelUnlock;
        this.levelTutorial = levelTutorial;
        this.numUseToEasy = numUseToEasy;
        this.showPopup = showPopup;
        this.numGift = numGift;
        this.forceClick = forceClick;
        this.surfaceCheck = surfaceCheck;
        this.showHand = showHand;
    }
}

public class BoosterConfigDataWrapper
{
    public BoosterConfigData configExtraSlot = new BoosterConfigData(0, 0);
    public BoosterConfigData configHand = new BoosterConfigData(4, 4);
    public BoosterConfigData configMoveObject = new BoosterConfigData(7, 7);
    public BoosterConfigData configShuffle = new BoosterConfigData(12, 12);
}