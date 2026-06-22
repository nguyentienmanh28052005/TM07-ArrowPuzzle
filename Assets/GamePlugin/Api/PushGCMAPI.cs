using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using Newtonsoft.Json;
using mygame.sdk;

namespace Myapi
{
    public class PushGCMAPI : SingleTonApi<PushGCMAPI, PushGCMOb>
    {
        public bool isLastPushSuccess
        {
            get => PlayerPrefs.GetInt("last_push_gcm_suc", 0) == 1;
            set => PlayerPrefs.SetInt("last_push_gcm_suc", value ? 1 : 0);
        }

        public void Push(string token, Action<PushGCMOb> cb)
        {
            long idrq = GameHelper.CurrentTimeMilisReal();
            idrq = addQueueCallback(idrq, cb);
            PostRequest(idrq, "");
        }

        protected override void OnError(long idRequest, string error)
        {
            base.OnError(idRequest, error);
            if (callBacks.ContainsKey(idRequest) && callBacks[idRequest] != null)
            {
                callBacks[idRequest](null);
                callBacks.Remove(idRequest);
            }
        }

        protected override void Process(long idRequest, PushGCMOb data)
        {
            if (callBacks.ContainsKey(idRequest) && callBacks[idRequest] != null)
            {
                callBacks[idRequest](data);
                callBacks.Remove(idRequest);
            }
        }
    }
}