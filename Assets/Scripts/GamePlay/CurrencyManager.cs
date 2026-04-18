using UnityEngine;
using System;
using Pixelplacement;

public class CurrencyManager : Singleton<CurrencyManager>
{
    private const int COIN_KEY = 100;
    private const int DIAMOND_KEY = 101;

    private const int ERASE_TOOL = 200;
    private const int HINT_TOOL = 201;
    private const int DASH_TOOL = 202;

    [SerializeField] private float _coins;
    public float Coins 
    {
        get => _coins;
        private set 
        {
            _coins = value;
            MessageManager.Instance.SendMessage(ManhMessageType.OnCoinChanged, _coins);
            SaveDataPlayer.Instance.Save(COIN_KEY, _coins);
        }
    }

     [SerializeField] private float _diamonds;
    public float Diamonds 
    {
        get => _diamonds;
        private set 
        {
            _diamonds = value;
            MessageManager.Instance.SendMessage(ManhMessageType.OnDiamondChanged, _diamonds);
            SaveDataPlayer.Instance.Save(DIAMOND_KEY, _diamonds);
        }
    }

    [SerializeField] private int _eraseToolCount;
    public int EraseToolCount
    {
        get => _eraseToolCount;
        private set
        {
            _eraseToolCount = value;
            MessageManager.Instance.SendMessage(ManhMessageType.OnEraseToolChanged, _eraseToolCount);
            SaveDataPlayer.Instance.Save(ERASE_TOOL, _eraseToolCount);
        }
    }

    [SerializeField] private int _hintToolCount;
    public int HintToolCount
    {
        get => _hintToolCount;
        private set
        {
            _hintToolCount = value;
            MessageManager.Instance.SendMessage(ManhMessageType.OnHintToolChanged, _hintToolCount);
            SaveDataPlayer.Instance.Save(HINT_TOOL, _hintToolCount);
        }
    }

    [SerializeField] private int _dashToolCount;
    public int DashToolCount
    {
        get => _dashToolCount;
        private set
        {
            _dashToolCount = value;
            MessageManager.Instance.SendMessage(ManhMessageType.OnDashToolChanged, _dashToolCount);
            SaveDataPlayer.Instance.Save(DASH_TOOL, _dashToolCount);
        }
    }


    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetCurrency();
        }
    }


    public void SetCurrency()
    {
        Coins = _coins;
        Diamonds = _diamonds;
        EraseToolCount = _eraseToolCount;
        HintToolCount = _hintToolCount;
        DashToolCount = _dashToolCount;
    }


    private void Start()
    {
        _coins = SaveDataPlayer.Instance.Value(COIN_KEY);
        _diamonds = SaveDataPlayer.Instance.Value(DIAMOND_KEY);
        _eraseToolCount = (int)SaveDataPlayer.Instance.Value(ERASE_TOOL);
        _hintToolCount = (int)SaveDataPlayer.Instance.Value(HINT_TOOL);
        _dashToolCount = (int)SaveDataPlayer.Instance.Value(DASH_TOOL);

        AddHintTool(30);
        AddEraseTool(30);
        AddDashTool(30);
        AddCoins(100000);
        AddDiamonds(20000);
    }

    // ==========================================
    // CÁC HÀM GIAO DỊCH (TRANSACTIONS)
    // ==========================================
    
    public void AddCoins(float amount)
    {
        if (amount < 0) return;
        Coins += amount;
    }

    public bool SpendCoins(float amount)
    {
        if (Coins >= amount)
        {
            Coins -= amount;
            return true; // Trừ tiền thành công
        }
        return false; // Không đủ tiền
    }

    public void AddDiamonds(float amount)
    {
        if (amount < 0) return;
        Diamonds += amount;
    }

    public bool SpendDiamonds(float amount)
    {
        if (Diamonds >= amount)
        {
            Diamonds -= amount;
            return true;
        }
        return false;
    }

    public void AddEraseTool(int amount)
    {
        if (amount < 0) return;
        EraseToolCount += amount;
    }

    public bool SpendEraseTool(int amount)
    {
        if (EraseToolCount >= amount)
        {
            EraseToolCount -= amount;
            return true;
        }
        return false;
    }

    public void AddHintTool(int amount)
    {
        if (amount < 0) return;
        HintToolCount += amount;
    }

    public bool SpendHintTool(int amount)
    {
        if (HintToolCount >= amount)
        {
            HintToolCount -= amount;
            return true;
        }
        return false;
    }

    public void AddDashTool(int amount)
    {
        if (amount < 0) return;
        DashToolCount += amount;
    }

    public bool SpendDashTool(int amount)
    {
        if (DashToolCount >= amount)
        {
            DashToolCount -= amount;
            return true;
        }
        return false;
    }
}