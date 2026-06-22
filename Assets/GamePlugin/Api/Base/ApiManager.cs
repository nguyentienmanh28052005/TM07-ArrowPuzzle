using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using mygame.sdk;

namespace Myapi
{
    public class ApiManager : MonoBehaviour
    {
        private const bool isLog = false;
        public static ApiManager Instance;
        private bool isLoading = false;
        //for game
        public const string baseAppUrl = "https://gangmaster.club/";
        public const string API_GIFT_CODE = "checkgiftcode?";

        public List<BaseApi> listApi { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                isLoading = false;
                listApi = new List<BaseApi>();
            }
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        public T checkApi<T>() where T : BaseApi
        {
            for (int i = 0; i < listApi.Count; i++)
            {
                if (isOfType<T>(listApi[i]))
                {
                    return (T)listApi[i];
                }
            }

            return null;
        }

        private bool isOfType<T>(object va)
        {
            return va is T;
        }

        public void getTimeOnline(Action<bool, long> result)
        {
            StartCoroutine(_getTimeOnline(result));
        }

        public void getRequest(string urlget, Action<bool, string> result = null, int timeout = -1, bool isgetCache = false, bool isCache = false)
        {
            StartCoroutine(_getRequest(urlget, result, timeout, isgetCache, isCache));
        }

        public void postRequest(string urlpost, Action<bool, string> result = null, int timeout = -1, bool isgetCache = false, bool isCache = false)
        {
            StartCoroutine(_postRequest(urlpost, "", result, timeout, isgetCache, isCache));
        }

        public void postRequest(string urlpost, string data, Action<bool, string> result = null, int timeout = -1, bool isgetCache = false, bool isCache = false)
        {
            StartCoroutine(_postRequest(urlpost, data, result, timeout, isgetCache, isCache));
        }

        public void postRequest(string urlpost, byte[] bytes, Action<bool, string> result = null, int timeout = -1, bool isgetCache = false, bool isCache = false)
        {
            StartCoroutine(_postRequest(urlpost, bytes, result, timeout, isgetCache, isCache));
        }
        public void postRequest(string urlpost, Dictionary<string, string> field, Dictionary<string, string> dicfiles, Action<bool, string> result = null, int timeout = -1, bool isgetCache = false, bool isCache = false)
        {
            StartCoroutine(_postRequest(urlpost, field, dicfiles, result, timeout, isgetCache, isCache));
        }

        private IEnumerator _getRequest(string url, Action<bool, string> result, int timeout, bool isgetCache, bool isCache)
        {
            if (isLog)
            {
                Debug.Log("mysdk: ApiManager get request=" + url);
            }
            if (isgetCache && Application.internetReachability != NetworkReachability.NotReachable)
            {
                string datacache = getCache(url);
                if (datacache != null && datacache.Length > 5)
                {
                    yield return new WaitForSeconds(0.2f);
                    result?.Invoke(true, datacache);
                    yield break;
                }
            }
            UnityWebRequest uwr = UnityWebRequest.Get(url);
            if (timeout > 0)
            {
                uwr.timeout = timeout;
            }
            yield return uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                if (isLog)
                {
                    Debug.Log("mysdk: ApiManager " + uwr.error + "=" + uwr.error);
                }
                result?.Invoke(false, uwr.error);
            }
            else
            {
                if (isLog)
                {
                    Debug.Log("mysdk: ApiManager Form upload complete!");
                }
                result?.Invoke(true, uwr.downloadHandler.text);
                if (isCache)
                {
                    saveCache(url, uwr.downloadHandler.text);
                }
            }
        }

        private IEnumerator _postRequest(string url, string data, Action<bool, string> result, int timeout, bool isgetCache, bool isCache)
        {
            if (isLog)
            {
                Debug.Log("mysdk: ApiManager postRequest=" + url);
            }
            if (isgetCache && Application.internetReachability != NetworkReachability.NotReachable)
            {
                string datacache = getCache(url);
                if (datacache != null && datacache.Length > 5)
                {
                    yield return new WaitForSeconds(0.2f);
                    result?.Invoke(true, datacache);
                    yield break;
                }
            }
            UnityWebRequest uwr = UnityWebRequest.PostWwwForm(url, data);
            if (timeout > 0)
            {
                uwr.timeout = timeout;
            }
            yield return uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                if (isLog)
                {
                    Debug.Log("mysdk: ApiManager " + uwr.error + "=" + uwr.error);
                }
                result?.Invoke(false, uwr.error);
            }
            else
            {
                if (isLog)
                {
                    Debug.Log("mysdk: ApiManager Form upload complete!");
                }
                result?.Invoke(true, uwr.downloadHandler.text);
                if (isCache)
                {
                    saveCache(url, uwr.downloadHandler.text);
                }
            }
        }

        private IEnumerator _postRequest(string url, byte[] data, Action<bool, string> result, int timeout, bool isgetCache, bool isCache)
        {
            if (isLog)
            {
                Debug.Log("mysdk: ApiManager postRequest=" + url);
            }
            if (isgetCache && Application.internetReachability != NetworkReachability.NotReachable)
            {
                string datacache = getCache(url);
                if (datacache != null && datacache.Length > 5)
                {
                    yield return new WaitForSeconds(0.2f);
                    result?.Invoke(true, datacache);
                    yield break;
                }
            }
            UnityWebRequest uwr = new UnityWebRequest(url, "POST")
            {
                uploadHandler = new UploadHandlerRaw(data),
                downloadHandler = new DownloadHandlerBuffer()
            };
            uwr.SetRequestHeader("Content-Type", "application/json");
            if (timeout > 0)
            {
                uwr.timeout = timeout;
            }
            yield return uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                if (isLog)
                {
                    Debug.Log("mysdk: ApiManager " + uwr.error + "=" + uwr.error);
                }
                result?.Invoke(false, uwr.error);
            }
            else
            {
                if (isLog)
                {
                    Debug.Log("mysdk: ApiManager Form upload complete!");
                }
                result?.Invoke(true, uwr.downloadHandler.text);
                if (isCache)
                {
                    saveCache(url, uwr.downloadHandler.text);
                }
            }
        }

        private IEnumerator _postRequest(string url, Dictionary<string, string> fields, Dictionary<string, string> dicfiles, Action<bool, string> result, int timeout, bool isgetCache, bool isCache)
        {
            if (isLog)
            {
                Debug.Log("mysdk: ApiManager postRequest=" + url);
            }
            if (isgetCache && Application.internetReachability != NetworkReachability.NotReachable)
            {
                string datacache = getCache(url);
                if (datacache != null && datacache.Length > 5)
                {
                    yield return new WaitForSeconds(0.2f);
                    result?.Invoke(true, datacache);
                    yield break;
                }
            }
            WWWForm form = new WWWForm();
            if (fields != null)
            {
                foreach (var item in fields)
                {
                    form.AddField(item.Key, item.Value);
                }
            }

            if (dicfiles != null)
            {
                int idx = 0;
                foreach (var item in dicfiles)
                {
                    byte[] bpush = System.IO.File.ReadAllBytes(item.Value);
                    form.AddBinaryData(item.Key, bpush, Path.GetFileName(item.Value));
                    idx++;
                }
            }
            UnityWebRequest uwr = UnityWebRequest.Post(url, form);
            if (timeout > 0)
            {
                uwr.timeout = timeout;
            }
            yield return uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                if (isLog)
                {
                    Debug.Log("mysdk: ApiManager uwr.error=" + uwr.error);
                }
                result?.Invoke(false, uwr.error);
            }
            else
            {
                if (isLog)
                {
                    Debug.Log("mysdk: ApiManager Form upload complete!");
                }
                result?.Invoke(true, uwr.downloadHandler.text);
                if (isCache)
                {
                    saveCache(url, uwr.downloadHandler.text);
                }
            }
        }

        private IEnumerator _getTimeOnline(Action<bool, long> result)
        {
            UnityWebRequest www = new UnityWebRequest("http://www.google.com");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                result?.Invoke(false, -1);
                yield break;
            }
            else
            {
                string date = www.GetResponseHeader("date");
                var dd = DateTime.ParseExact(date, "ddd, dd MMM yyyy HH:mm:ss 'GMT'"
                    , System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat
                    , System.Globalization.DateTimeStyles.AssumeUniversal);
                result?.Invoke(true, (dd.Ticks - 621355968000000000) / 10000);
            }
        }

        private string getCache(string url)
        {
            try
            {
                string pathdata = mygame.sdk.ImageLoader.url2nameData(url, 0);
                if (File.Exists(DownLoadUtil.pathCache() + "/" + pathdata))
                {
                    string txtcahe = File.ReadAllText(DownLoadUtil.pathCache() + "/" + pathdata);
                    return txtcahe;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("mysdk: ApiManager-getCache err=" + ex.ToString());
            }
            return "";
        }

        private void saveCache(string url, string dataurl)
        {
            try
            {
                if (dataurl != null && url != null && dataurl.Length > 5 && url.Length > 5)
                {
                    DownLoadUtil.checkDirecCache();
                    string pathdata = mygame.sdk.ImageLoader.url2nameData(url, 0);
                    File.WriteAllText(DownLoadUtil.pathCache() + "/" + pathdata, dataurl);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("mysdk: ApiManager-saveCache err=" + ex.ToString());
            }
        }
    }
}
