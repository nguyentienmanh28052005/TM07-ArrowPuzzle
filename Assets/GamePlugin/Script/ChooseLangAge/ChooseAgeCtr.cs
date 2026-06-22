using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using mygame.sdk;
using UnityEngine.UI;
using System.Diagnostics;
using Myapi;
namespace mygame.sdk
{
    public class ChooseAgeCtr : MonoBehaviour
    {
        [SerializeField] private Text Agetxt;
        [SerializeField] private Button ConfirmBtn;
        [SerializeField] private Button LeftBtn;
        [SerializeField] private Button RightBtn;

        public int minValue;
        public int maxValue;
        private int currValue = 18;
        private float currentTime;
        private bool countdownStarted = false;

        void Start()
        {
            LeftBtn.onClick.AddListener(PreClick);
            RightBtn.onClick.AddListener(NextClick);
            Agetxt.text = "---";
            countdownStarted = false;
        }

        private void PreClick()
        {
            currValue = Mathf.Clamp(currValue - 1, minValue, maxValue);
            Agetxt.text = $"{currValue}";
        }

        private void NextClick()
        {
            currValue = Mathf.Clamp(currValue + 1, minValue, maxValue);
            Agetxt.text = $"{currValue}";
        }
    }
}