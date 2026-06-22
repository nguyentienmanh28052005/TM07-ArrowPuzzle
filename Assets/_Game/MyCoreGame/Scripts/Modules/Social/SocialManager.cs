//# define TEST_SOCIAL

using DG.Tweening;
using mygame.sdk;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
#if ENABLE_FBSDK
using Facebook.Unity;
#endif
using UnityEngine;
using UnityEngine.Networking;

namespace master
{
    public class SocialManager : Singleton<SocialManager>
    {
        private static bool HasChangeName
        {
            get => PlayerPrefs.GetInt("has_change_name") == 1;
            set => PlayerPrefs.SetInt("has_change_name", value ? 1 : 0);
        }

        public static Action<bool> OnLinkSocialAccountDone;
        public static Action<bool, UserData, UserData> OnGetCompareDataUserDone;
        public static Action<UserData> OnSynchUserDataDone;
        public static Action<bool> OnLogOutSocialAccountDone;

        protected override void Awake()
        {
            base.Awake();
            listSyncData = GetComponentsInChildren<ISyncData>().ToList();
        }

        private void Start()
        {
            DOVirtual.DelayedCall(1, () =>
            {
                if (!string.IsNullOrEmpty(UserDataManager.Instance.UserData.facebookId))
                {
                    #if ENABLE_FBSDK
                    if (FB.IsLoggedIn && AccessToken.CurrentAccessToken != null)
                    {
                        //FBHelper.Instance.GetFriends();
                        LoadAvatarFacebook(AccessToken.CurrentAccessToken.TokenString);
                    }
                    #endif
                }
                /*#if UNITY_ANDROID
                else if(!string.IsNullOrEmpty(UserDataManager.Instance.UserData.googleId))
                #else
                else if(!string.IsNullOrEmpty(UserDataManager.Instance.UserData.appleId))
                #endif
                {
                     LoginSocial((status, id, url) =>
                     {
                         StartCoroutine(requestAvatar(url));
                     });
                }*/
                else if (!string.IsNullOrEmpty(UserDataManager.Instance.UserData.avatarUrl))
                {
                    StartCoroutine(requestAvatar(UserDataManager.Instance.UserData.avatarUrl));
                }
            });
        }

        private static int cacheLevel
        {
            get => PlayerPrefs.GetInt("cache_level", 1);
            set => PlayerPrefs.SetInt("cache_level", value);
        }

        public void LoginFacebook(Action<int, string, string> onDone)
        {
            FBHelper.Instance.loginWithReadPermissions((int status, string fbid, string fbtoken, string disname) =>
            {
                if (status == 1)
                {
                    if (!string.IsNullOrEmpty(disname) || !HasChangeName)
                    {
                        HasChangeName = true;
                        UserDataManager.Instance.ChangeUserName(disname);
                    }
                }

                onDone?.Invoke(status, fbid, fbtoken);
            });
        }

        public void LoginSocial(Action<int, string, string> cb)
        {
            StartCoroutine(SocialHelper.Instance.authentSocialDelay((int status, string id, string arg3, string arg4) =>
            {
                if (status == 1)
                {
                    if (!string.IsNullOrEmpty(arg3) || !HasChangeName)
                    {
                        HasChangeName = true;
                        UserDataManager.Instance.ChangeUserName(arg3);
                    }
                }

                cb?.Invoke(status, id, arg4);
            }, 1));
        }

        public void LoadAvatarFacebook(string accessToken,Action<bool> onDone = null)
        {
#if ENABLE_FBSDK
            var userId = UserDataManager.Instance.UserData.facebookId;
            /*var url =
                $"https://graph.facebook.com/{userId}/picture?width=256&height=256";*/
            StartCoroutine(GetFacebookAvatarURL(userId, accessToken, avatarUrl =>
            {
                if (!string.IsNullOrEmpty(avatarUrl))
                {
                    StartCoroutine(requestAvatar(avatarUrl,onDone));
                }
                else
                {
                    onDone?.Invoke(false);
                }
            }));
#endif
        }

        IEnumerator GetFacebookAvatarURL(string facebookId, string accessToken, Action<string> onDone)
        {
            string url = $"https://graph.facebook.com/{facebookId}/picture?type=large&redirect=false";
            if (!string.IsNullOrEmpty(accessToken))
            {
                url += $"&access_token={accessToken}";
            }

            UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                var data = JsonUtility.FromJson<FBPictureData>(json).data;
                if (!data.is_silhouette)
                {
                    onDone?.Invoke(data.url);
                }
                else
                {
                    onDone?.Invoke(null);
                }
            }
            else
            {
                Debug.LogError("Failed to get Facebook avatar URL: " + request.error);
                onDone?.Invoke(null);
            }
        }

        private IEnumerator requestAvatar(string imageUrl,Action<bool> onDone = null)
        {
            using var imageRequest = UnityWebRequestTexture.GetTexture(imageUrl);
            yield return imageRequest.SendWebRequest();
            if (imageRequest.result == UnityWebRequest.Result.Success)
            {
                var texture = DownloadHandlerTexture.GetContent(imageRequest);
                var avatarSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));
                UserDataManager.Instance.UserData.avatarUrl = imageUrl;
                UserDataManager.Instance.SaveUserData();
                ServerHub.GetSendRequestServer<SendRequestUser>().UpdateUserInfo(avatarUrl: imageUrl);
                UserDataManager.Instance.SaveSpriteToLocal(avatarSprite);
                onDone?.Invoke(true);
            }
            else
            {
                Debug.LogError("Error loading image: " + imageRequest.error);
                onDone?.Invoke(false);
            }
        }

        private List<ISyncData> listSyncData = new();

        public UserData SendUserDataSync()
        {
            var dataUser = new UserData
            {
                game_info = new()
            };
            foreach (var syncData in listSyncData)
            {
                syncData.SendSyncData(dataUser);
            }
            dataUser.game_info.HasChangeName = HasChangeName;
            //send data to server
            return dataUser;
        }

        public void ReceiveUserDataSync(UserData newDataUser)
        {
            foreach (var syncData in listSyncData)
            {
                syncData.OnSyncData(newDataUser);
            }

            HasChangeName = newDataUser.game_info != null && newDataUser.game_info.HasChangeName;
            //receive data and save
            FIRhelper.Instance.callUpdateGcm();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                PushData();
            }
        }

        private void OnApplicationFocus(bool focusStatus)
        {
            if (!focusStatus)
            {
                PushData();
            }
        }

        private static int cachTimePushData = 0;

        public void PushData(bool force = false)
        {
            if (!force)
            {
                if (GameManager.GameState == GameState.Playing) return;
                if (cachTimePushData + 30 >= Time.time) return;
            }
            cachTimePushData = (int)Time.time;
            var data = SendUserDataSync();
            var json = JsonConvert.SerializeObject(data);
            Keychain.SaveString("data_game", json);
            ServerHub.GetSendRequestServer<SendRequestUser>().RequestSaveUserData(data);
        }
    }

    [Serializable]
    public class FBPictureData
    {
        public FBPicture data;
    }

    [Serializable]
    public class FBPicture
    {
        public string url;
        public bool is_silhouette;
    }
}