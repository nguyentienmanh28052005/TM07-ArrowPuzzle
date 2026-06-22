using System;
using System.Linq;
using mygame.sdk;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class BarController : MonoBehaviour
{
    
    [Space, Header("Bar")]
    [SerializeField] RectTransform rectArrow;
    [SerializeField] Text[] listTextPersen;
    [SerializeField] float[] valueCheck;
    [SerializeField] int[] persenReward;
    [SerializeField] float width = 340;
    [SerializeField] float speed = 1000;
    [SerializeField] float v = 0.1f;
    [SerializeField] float a = 1;

    private Text txtFollow, txtClaim;

    private bool isInit = false;
    public bool isStop = false;
    int left = 1, valueReward;
    private int[] currentPersenReward;
    
    private bool loadConfig(string cf)
    {
        try
        {
            currentPersenReward = !string.IsNullOrEmpty(cf) ? JsonConvert.DeserializeObject<int[]>(cf) : persenReward;
            return true;
        }
        catch (Exception e)
        {
            currentPersenReward = persenReward;
            return false;
        }
    }
    public void StartArrow(Text txtFollow, Text txtClaim, int valueReward)
    {
        loadConfig(ConfigManager.CF_X2ValueGoldWin);
        this.txtClaim = txtClaim;
        this.txtFollow = txtFollow;
        if (txtFollow != null)
        {
            int pesenCurrent = GetValueReward();
            int valueRewardCurrent = pesenCurrent * valueReward;
            txtClaim.SetText("_claim_x", StateCapText.None, FormatText.F_String, pesenCurrent, false);
            txtFollow.SetValue(valueRewardCurrent, false);
        }
        // txtClaim.resizeTextMaxSize = 60;
        // txtClaim.fontSize = 60;
        //txtClaim.SetText("_claim_x", StateCapText.None, FormatText.F_String, 10, false);
        var fontSize = txtClaim.cachedTextGeneratorForLayout.fontSizeUsedForBestFit;
        // txtClaim.resizeTextMaxSize = fontSize == 0 ? 60 : fontSize;
        // if (txtClaim.preferredWidth >= 310)
        // {
        //     txtClaim.GetComponent<RectTransform>().anchoredPosition = new Vector2(30, 50);
        // }
        // else
        // {
        //     txtClaim.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 50);
        // }
        
        this.valueReward = valueReward;
        isStop = false;
        isInit = true;
        left = 1;
        Vector2 cur = rectArrow.anchoredPosition;
        cur.x = 0;
        rectArrow.anchoredPosition = cur;
        for (int i = 0; i < listTextPersen.Length; i++)
        {
            listTextPersen[i].text = $"x{currentPersenReward[i]}";
        }
    }

    [SerializeField] float maxY = 100f;    
    [SerializeField] float baseY = 50f; // vị trí y gốc
    [SerializeField] float rotationMultiplier = 0.005f;
    private float t = 0f;  
    private int dir = 1;

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.T))
        {
            isStop = !isStop;
            Debug.LogError("Test Bar: x" + GetValueReward());
        }
#endif
        if (!isStop && isInit)
        {
            t += Time.deltaTime * speed * dir;
            if (t >= 1f)
            {
                t = 1f;
                dir = -1;
            }
            else if (t <= 0f)
            {
                t = 0f;
                dir = 1;
            }

            float x = Mathf.Lerp(-width, width, t);
            float y = baseY + maxY * (1f - 4f * Mathf.Pow(t - 0.5f, 2f));
            rectArrow.anchoredPosition = new Vector2(x, y);


            float dy = -8f * maxY * (t - 0.5f);
            float angle = Mathf.Atan2(dy * rotationMultiplier, 1f) * Mathf.Rad2Deg;
            rectArrow.rotation = Quaternion.Euler(0, 0, Mathf.Clamp(angle,-25,25));
            int pesenCurrent = GetValueReward();
            if (txtFollow != null)
            {
                int valueRewardCurrent = pesenCurrent * valueReward;
                txtClaim.SetText("_claim_x", StateCapText.None, FormatText.F_String, pesenCurrent, false);
                txtFollow.SetValue(valueRewardCurrent, false);
            }
        }
    }

    public int GetValueReward()
    {
        if(!isInit) return 2;
        float pos = rectArrow.anchoredPosition.x;
        //Debug.Log(rectArrow.anchoredPosition.x);

        for (int i = 0; i < valueCheck.Length - 1; i++)
        {
            if (valueCheck[i] < pos && pos <= valueCheck[i + 1])
            {
                return currentPersenReward[i];
            }
        }
        return currentPersenReward[0];
    }
    public bool isOnlyValue()
    {
        if(!isInit) return true;
        return currentPersenReward.All(x => x == currentPersenReward.First());
    }
}
