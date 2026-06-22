using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPackFailOffer : UIPackage
{
   protected override void Start()
   {
      base.Start();
      onBuySuccess += () =>
      {
         var UIRevive = UIManager.Instance.GetPopupActive<UIRevive>();
         if (UIRevive != null)
         {
            UIRevive.Hide();
         }
         LevelManager.Instance.OnReviveSuccess(ReviveType.None);
      };
   }
}
