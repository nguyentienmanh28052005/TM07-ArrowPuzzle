using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
namespace master
{
    public static class Observer
    {
        private static Dictionary<ObserverName, ReactiveProperty<object>> dicObservable = new Dictionary<ObserverName, ReactiveProperty<object>>();
        public static IReadOnlyReactiveProperty<object> GetObservable(ObserverName notiName, object defaultValue)
        {
            return getReactiveProperty(notiName, defaultValue);
        }
        public static void Notify(ObserverName notiName, object value1, bool isForce = true)
        {
            if (dicObservable.TryGetValue(notiName, out ReactiveProperty<object> observable))
            {
                if (isForce)
                {
                    observable.SetValueAndForceNotify(value1);
                }
                else
                {
                    observable.Value = value1;
                }
            }
            else
            {
                observable = new ReactiveProperty<object> { Value = value1 };
                dicObservable.Add(notiName, observable);
            }
        }
        private static ReactiveProperty<object> getReactiveProperty(ObserverName notiName, object defaultValue)
        {
            if (!dicObservable.TryGetValue(notiName, out ReactiveProperty<object> observable))
            {
                observable = new ReactiveProperty<object> { Value = defaultValue };
                dicObservable.Add(notiName, observable);
            }
            return dicObservable[notiName];
        }
    }
    public enum ObserverName
    {
        purchase_success,
        purchase_success_shop_pack,
        purchase_success_gold_pack,
        purchase_success_ticket_pack,
        state_pack_remove_ads_24h,
        end_leaguesues,
        active_race,
        finish_event_race,
        select_rope,
        claim_gift_battlepass,
        unlock_premium_battlepass,
        visual_claim_battlepass_point,
        noti_claim_battlepass,
        active_battlepass,
        select_knot,
        state_pack_summer,
        new_pack_summer,
        state_pack_level,
        active_pack,
        active_pack_weekend,
        unlock_clamp,
        OnFinishBooster,
        hammer_change,
        end_leagues,
        state_pack_halloween,
        state_pack_noel,
        screen_resize,
        internet_reachability,
        flash_sale_active,
        active_event_golden,
        claim_gift_MissonController,
    }
}