//#define ENABLE_FBSDK

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
#if ENABLE_FBSDK
using Facebook.Unity;

#endif

namespace mygame.sdk
{
    public delegate void onLoginFB(int status, string fbid, string fbtoken, string disname);

    public class FBHelper : MonoBehaviour
    {
        public static FBHelper Instance { get; private set; }
        const string key_public_permission = "publish_to_groups";

        private bool isCallInit = false;
        private onLoginFB _callbackLogin;
        private bool isGetProfile;
        private bool isLimitedLogin;
        private int flagLogin = 0;
        private bool isWaitingFacebookLogin = false;

        void Awake()
        {
            if (Instance == null)
            {
                Debug.Log("mysdk FB helper Awake 1");
                Instance = this;

#if ENABLE_FBSDK
                if (FB.IsInitialized == true)
                {
                    FB.ActivateApp();
                }
                else
                {
                    //Handle FB.Init
                    Debug.Log("mysdk FB helper Awake");
                    isCallInit = true;
                    FB.Init(() =>
                    {
                        Debug.Log("mysdk FB active from Awake");
                        FB.ActivateApp();
                    });
                }
#endif

                DontDestroyOnLoad(this);
            }
            else
            {
                Debug.Log("mysdk FB helper Awake 2");
                if (this != Instance)
                {
                    Debug.Log("mysdk FB helper Awake 3");
                    Destroy(gameObject);
                }
            }
        }

        void OnApplicationPause(bool pauseStatus)
        {
#if ENABLE_FBSDK
            // Check the pauseStatus to see if we are in the foreground
            // or background
            if (!pauseStatus)
            {
                //app resume
                if (FB.IsInitialized == true)
                {
                    Debug.Log("mysdk FB OnApplicationPause 1");
                    FB.ActivateApp();
                    Debug.Log("mysdk FB OnApplicationPause 11");
                }
                else
                {
                    //Handle FB.Init
                    Debug.Log("mysdk FB OnApplicationPause 2");
                    if (!isCallInit)
                    {
                        Debug.Log("mysdk FB OnApplicationPause 3");
                        isCallInit = true;
                        FB.Init(() =>
                        {
                            Debug.Log("mysdk FB active from OnApplicationPause");
                            FB.ActivateApp();
                        });
                    }
                }

                if (isWaitingFacebookLogin && !FB.IsLoggedIn)
                {
                    // Có thể đã bị kill lúc đang login
                    isWaitingFacebookLogin = false;
                    _callbackLogin(0, "", "", "");
                }
            }
#endif
        }

        #region Inviting

        public void FacebookGameRequest()
        {
#if ENABLE_FBSDK
            FB.AppRequest(
                message: "Here is a free gift!",
                to: null,
                filters: new List<object>() { "app_users" },
                excludeIds: null,
                maxRecipients: null,
                data: null,
                title: "Funny!!!",
                callback: delegate(IAppRequestResult result) { Debug.Log(result.RawResult); }
            );
#endif
        }

        #endregion

        public void shareLink(string title, string des, string url)
        {
#if ENABLE_FBSDK
            if (!FB.IsLoggedIn || AccessToken.CurrentAccessToken == null)
            {
                loginWithReadPermissions((int status, string fid, string ftk, string fname) =>
                {
                    if (status == 1)
                    {
                        _shareLink(title, des, url);
                    }
                }, false);
            }
            else
            {
                _shareLink(title, des, url);
            }
#endif
        }

        public void shareFeed(string nameGame, string title, string des, string url, string urlImage = null)
        {
#if ENABLE_FBSDK
            if (!FB.IsLoggedIn || AccessToken.CurrentAccessToken == null)
            {
                loginWithReadPermissions((int status, string fid, string ftk, string fname) =>
                {
                    if (status == 1)
                    {
                        _shareFeed(nameGame, title, des, url, urlImage);
                    }
                }, false);
            }
            else
            {
                _shareFeed(nameGame, title, des, url, urlImage);
            }
#endif
        }

        public void shareScreenShoot(string caption)
        {
#if ENABLE_FBSDK
            Debug.Log("mysdk fb shareScreenShoot 1");
            if (!FB.IsLoggedIn || AccessToken.CurrentAccessToken == null || !AccessToken.CurrentAccessToken.Permissions.Contains(key_public_permission))
            {
                Debug.Log("mysdk fb shareScreenShoot 21");
                loginWithPublishAction2Step((int status, string fid, string ftk, string fname) =>
                {
                    if (status == 1)
                    {
                        Debug.Log("mysdk fb shareScreenShoot 31");
                        mygame.sdk.SDKManager.Instance.Screenshot((Texture2D texture) =>
                        {
                            _sharescreenshoot(caption, texture);
                        });
                    }
                    else
                    {
                        Debug.Log("mysdk fb shareScreenShoot 32");
                    }
                }, false);
            }
            else
            {
                Debug.Log("mysdk fb shareScreenShoot 22");
                mygame.sdk.SDKManager.Instance.Screenshot((Texture2D texture) =>
                {
                    _sharescreenshoot(caption, texture);
                });
            }
#endif
        }

        private void _shareLink(string title, string des, string url)
        {
#if ENABLE_FBSDK
            Debug.Log("mysdk fb _shareLink link=" + url + ", title=" + title + ", des=" + des);
            FB.ShareLink(
                contentURL: new Uri(url),
                contentTitle: title,
                contentDescription: des,
                callback: ShareCallback
            );
#endif
        }

        private void _shareFeed(string nameGame, string title, string des, string url, string urlImage)
        {
#if ENABLE_FBSDK
            Debug.Log("mysdk fb _shareFeed nameGame=" + nameGame + ", title=" + title + ", des=" + des + ", url=" +
                      url);
            if (urlImage == null)
            {
                FB.FeedShare(
                    link: new Uri(url),
                    linkName: nameGame,
                    linkCaption: title,
                    linkDescription: des,
                    picture: null,
                    callback: ShareCallback
                );
            }
            else
            {
                FB.FeedShare(
                    link: new Uri(url),
                    linkName: nameGame,
                    linkCaption: title,
                    linkDescription: des,
                    picture: new Uri(urlImage),
                    callback: ShareCallback
                );
            }
#endif
        }

        private void _sharescreenshoot(string caption, Texture2D texture)
        {
#if ENABLE_FBSDK
            Debug.Log("mysdk fb _sharescreenshoot caption=" + caption);
            var wwwForm = new WWWForm();
            wwwForm.AddBinaryData("image", texture.EncodeToPNG(), "ScreenShare.png");

            wwwForm.AddField("name", caption);
            wwwForm.AddField("link", mygame.sdk.GameHelper.Instance.getlinkHttpStore());
            FB.API("me/photos", HttpMethod.POST, this.CallBackShareScreenShoot, wwwForm);
#endif
        }

        //---------------------------------------------------------------
        public void loginWithReadPermissions(onLoginFB callback, bool isGetProfile = true)
        {
#if ENABLE_FBSDK
            Debug.Log("mysdk fb loginWithReadPermissions 1");
            _callbackLogin = callback;
            this.isGetProfile = isGetProfile;
            flagLogin = 0;
            isLimitedLogin = PlayerPrefs.GetInt("user_allow_track_ads", 0) != 1;

            if (!FB.IsLoggedIn || (AccessToken.CurrentAccessToken == null && FB.Mobile.CurrentAuthenticationToken() == null))
            {
                Debug.Log("mysdk fb loginWithReadPermissions 2");
                var permissions = new List<string> { "public_profile", "user_friends", "email" };
   
#if UNITY_IOS
                var tracking = isLimitedLogin ? LoginTracking.LIMITED : LoginTracking.ENABLED;
                string nonce = Guid.NewGuid().ToString("N");
                FB.Mobile.LoginWithTrackingPreference(tracking, permissions, nonce, HandleResult);
#else
                // Nếu không phải iOS, chỉ cần login bình thường
                isLimitedLogin = false;
                FB.LogInWithReadPermissions(permissions, HandleResult);
                isWaitingFacebookLogin = true;
#endif
            }
            else
            {
                Debug.Log("mysdk fb loginWithReadPermissions 21");
                if (isLimitedLogin)
                {
                    FB.Mobile.CurrentProfile(OnProfileLoaded);
                }
                else
                {
                    if (isGetProfile)
                    {
                        Debug.Log("mysdk fb loginWithReadPermissions 211");
                        FB.API("/me?fields=name", HttpMethod.GET, LoginCallback2);
                    }
                    else
                    {
                        Debug.Log("mysdk fb loginWithReadPermissions 212");
                        _callbackLogin(1, AccessToken.CurrentAccessToken.UserId, AccessToken.CurrentAccessToken.TokenString, "");
                    }
                }
                
            }
#endif
        }

        public void loginWithPublishAction2Step(onLoginFB callback, bool isGetProfile = true)
        {
#if ENABLE_FBSDK
            Debug.Log("mysdk fb loginWithPublishAction2Step 1");
            _callbackLogin = callback;
            this.isGetProfile = isGetProfile;
            flagLogin = 2;
            if (!FB.IsLoggedIn || AccessToken.CurrentAccessToken == null || !AccessToken.CurrentAccessToken.Permissions.Contains(key_public_permission))
            {
                if (!FB.IsLoggedIn || AccessToken.CurrentAccessToken == null)
                {
                    Debug.Log("mysdk fb loginWithPublishAction2Step 21");
                    FB.LogInWithReadPermissions(new List<string>() { "public_profile", "user_friends", "email" }, this.HandleResult);
                }
                else
                {
                    Debug.Log("mysdk fb loginWithPublishAction2Step 22");
                    FB.LogInWithPublishPermissions(new List<string>() { key_public_permission }, this.HandleResult);
                }
            }
            else
            {
                Debug.Log("mysdk fb loginWithPublishAction2Step 3");
                if (isGetProfile)
                {
                    Debug.Log("mysdk fb loginWithPublishAction2Step 31");
                    FB.API("/me?fields=name", HttpMethod.GET, LoginCallback2);
                }
                else
                {
                    Debug.Log("mysdk fb loginWithPublishAction2Step 32");
                    _callbackLogin(1, AccessToken.CurrentAccessToken.UserId, AccessToken.CurrentAccessToken.TokenString, "");
                }
            }
#endif
        }

        public void GetFriends()
        {
#if ENABLE_FBSDK
            FB.API("/me/friends?fields=id,name,picture.width(100).height(100)", HttpMethod.GET, OnGetFriends);
#endif
        }
#if ENABLE_FBSDK
        void OnGetFriends(IGraphResult result)
        {
            if (result.Error != null)
            {
                Debug.LogError("User Friend Error: " + result.Error);
                return;
            }

            Debug.LogError("User Friend Ok: " + result.RawResult);
            var data = (List<object>)result.ResultDictionary["data"];
            foreach (var friend in data)
            {
                var friendDict = (Dictionary<string, object>)friend;
                string name = friendDict["name"].ToString();
                string id = friendDict["id"].ToString();

                var pictureDict = (Dictionary<string, object>)(((Dictionary<string, object>)friendDict["picture"])["data"]);
                string avatarUrl = pictureDict["url"].ToString();
            }
        }
#endif
        
#if ENABLE_FBSDK
        private void CallBackShareScreenShoot(IGraphResult result)
        {
            if (result == null)
            {
                Debug.Log("mysdk fb CallBackShareScreenShoot 1");
                //err
            }
            else if (!string.IsNullOrEmpty(result.Error))
            {
                //err
                Debug.Log("mysdk fb CallBackShareScreenShoot 2 " + result.Error);
            }
            else
            {
                //success
                Debug.Log("mysdk fb CallBackShareScreenShoot 3");
            }
        }

        private void ShareCallback(IShareResult result)
        {
            if (result.Cancelled || !String.IsNullOrEmpty(result.Error))
            {
                Debug.Log("ShareLink Error: " + result.Error);
            }
            else if (!String.IsNullOrEmpty(result.PostId))
            {
                // Print post identifier of the shared content
                Debug.Log(result.PostId);
            }
            else
            {
                // Share succeeded without postID
                Debug.Log("ShareLink success!");
            }
        }
        //-----------------------------------------------------------

        public void loginWithPublicAction(onLoginFB callback, bool isGetProfile = true)
        {
            Debug.Log("mysdk fb loginWithPublicAction 1");
            _callbackLogin = callback;
            this.isGetProfile = isGetProfile;
            flagLogin = 1;
            if (!FB.IsLoggedIn || AccessToken.CurrentAccessToken == null || !AccessToken.CurrentAccessToken.Permissions.Contains(key_public_permission))
            {
                Debug.Log("mysdk fb loginWithPublicAction 2");
                FB.LogInWithPublishPermissions(new List<string>() { key_public_permission }, this.HandleResult);
            }
            else
            {
                Debug.Log("mysdk fb loginWithPublicAction 3");
                if (isGetProfile)
                {
                    Debug.Log("mysdk fb loginWithPublicAction 31");
                    FB.API("/me?fields=name", HttpMethod.GET, LoginCallback2);
                }
                else
                {
                    Debug.Log("mysdk fb loginWithPublicAction 32");
                    _callbackLogin(1, AccessToken.CurrentAccessToken.UserId, AccessToken.CurrentAccessToken.TokenString, "");
                }
            }
        }
        
        private void OnProfileLoaded(IProfileResult result)
        {
            if (result.Error != null)
            {
                Debug.Log("mysdk: fb load profile fail: " + result.Error);
                _callbackLogin(0, "", "", "");
                return;
            }

            if (result.CurrentProfile != null)
            {
                var profile = result.CurrentProfile;
                _callbackLogin(1, profile.UserID, FB.Mobile.CurrentAuthenticationToken().TokenString, profile.Name);
            }
            else
            {
                Debug.Log("mysdk: fb load profile found");
                _callbackLogin(0, "", "", "");
            }
        }
        
        protected void HandleResult(IResult result)
        {
            if (result == null)
            {
                //          this.LastResponse = "Null Response\n";
                //          LogView.AddLog(this.LastResponse);
                Debug.Log("mysdk fb HandleResult 2");
                _callbackLogin(0, "", "", "");
                return;
            }
            Debug.Log("mysdk fb HandleResult 1" + result.RawResult);

            //      this.LastResponseTexture = null;

            // Some platforms return the empty string instead of null.
            if (!string.IsNullOrEmpty(result.Error))
            {
                //          this.Status = "Error - Check log for details";
                //          this.LastResponse = "Error Response:\n" + result.Error;
                Debug.Log("mysdk fb HandleResult 3");
                _callbackLogin(0, "", "", "");
            }
            else if (result.Cancelled)
            {
                //          this.Status = "Cancelled - Check log for details";
                //          this.LastResponse = "Cancelled Response:\n" + result.RawResult;
                Debug.Log("mysdk fb HandleResult 4");
                _callbackLogin(0, "", "", "");
            }
            else if (!string.IsNullOrEmpty(result.RawResult))
            {
                //          this.Status = "Success - Check log for details";
                //          this.LastResponse = "Success Response:\n" + result.RawResult;
                if (flagLogin == 2)
                {
                    Debug.Log("mysdk fb HandleResult 8");
                    flagLogin = 0;
                    FB.LogInWithPublishPermissions(new List<string>() { key_public_permission }, this.HandleResult);
                }
                else
                {
                    if (isGetProfile)
                    {
                        Debug.Log("mysdk fb HandleResult 5");
                        if (isLimitedLogin)
                        {
                            FB.Mobile.CurrentProfile(OnProfileLoaded);
                        }
                        else
                        {
                            FB.API("/me?fields=name", HttpMethod.GET, LoginCallback2);
                        }
                    }
                    else
                    {
                        Debug.Log("mysdk fb HandleResult 6");
                        if (isLimitedLogin)
                        {
                            FB.Mobile.CurrentProfile(OnProfileLoaded);
                        }
                        else
                        {
                            _callbackLogin(1, AccessToken.CurrentAccessToken.UserId, AccessToken.CurrentAccessToken.TokenString, "");
                        }
                    }
                }
            }
            else
            {
                //          this.LastResponse = "Empty Response\n";
                Debug.Log("mysdk fb HandleResult 7");
                _callbackLogin(0, "", "", "");
            }

            //      LogView.AddLog(result.ToString());
        }

        void LoginCallback2(IGraphResult result)
        {
            Debug.Log("mysdk fb LoginCallback2 1");
            if (result.Error != null)
            {
                //          lastResponse = "Error Response:\n" + result.Error;
                Debug.Log("mysdk fb LoginCallback2 2");
                _callbackLogin(0, "", "", "");
            }
            else if (!FB.IsLoggedIn)
            {
                //          lastResponse = "Login cancelled by Player";
                Debug.Log("mysdk fb LoginCallback2 3");
                _callbackLogin(0, "", "", "");
            }
            else
            {
                Debug.Log("mysdk fb LoginCallback2 4");
                string fbName = result.ResultDictionary["name"].ToString();
                _callbackLogin(1, AccessToken.CurrentAccessToken.UserId, AccessToken.CurrentAccessToken.TokenString, fbName);
            }
        }
#endif
        public void logEvent(string title)
        {
#if ENABLE_FBSDK
            FB.LogAppEvent(title, 1);
#endif
        }

        public void logEvent(string name, string parameter, int value)
        {
#if ENABLE_FBSDK
            FB.LogAppEvent(name, 1, new Dictionary<string, object>() { { parameter, value } });
#endif
        }

        public void logEvent(string name, string parameter, string value)
        {
#if ENABLE_FBSDK
            FB.LogAppEvent(name, 1, new Dictionary<string, object>() { { parameter, value } });
#endif
        }

        public void LogOut()
        {
#if ENABLE_FBSDK
            if (FB.IsLoggedIn)
            {
                FB.LogOut();
            }
#endif
        }
    }
}