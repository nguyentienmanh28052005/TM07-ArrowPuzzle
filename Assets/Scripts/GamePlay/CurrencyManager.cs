using UnityEngine;
using System;
using Pixelplacement;

public class CurrencyManager : Singleton<CurrencyManager>
{
    private const int COIN_KEY = 100;
    private const int DIAMOND_KEY = 101;

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


    private void Start()
    {
        _coins = SaveDataPlayer.Instance.Value(COIN_KEY);
        _diamonds = SaveDataPlayer.Instance.Value(DIAMOND_KEY);
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
}