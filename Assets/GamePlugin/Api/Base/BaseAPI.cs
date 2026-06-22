using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Myapi
{
    public abstract class BaseApi
    {
        private const bool isLogContent = false;

        public void GetRequest(long idRequest, string url, int timeout = -1, bool isgetCache = false, bool isCache = false)
        {
            ApiManager.Instance.getRequest(url, (b, s) =>
            {
                if (b)
                {
                    onLoadOk(idRequest, s);
                }
                else
                {
                    onLoadErr(idRequest, s);
                }

                if (isLogContent)
                {
                    Debug.Log("mysdk: BaseApi apicontent=" + s);
                }
            }, timeout, isgetCache, isCache);
        }

        public void PostRequest(long idRequest, string url, int timeout = -1, bool isgetCache = false, bool isCache = false)
        {
            ApiManager.Instance.postRequest(url, (b, s) =>
            {
                if (b)
                {
                    onLoadOk(idRequest, s);
                }
                else
                {
                    onLoadErr(idRequest, s);
                }

                if (isLogContent)
                {
                    Debug.Log("mysdk: BaseApi apicontent=" + s);
                }
            }, timeout, isgetCache, isCache);
        }

        public void PostRequest(long idRequest, string url, string data, int timeout = -1, bool isgetCache = false, bool isCache = false)
        {
            ApiManager.Instance.postRequest(url, data, (b, s) =>
            {
                if (b)
                {
                    onLoadOk(idRequest, s);
                }
                else
                {
                    onLoadErr(idRequest, s);
                }

                if (isLogContent)
                {
                    Debug.Log("mysdk: BaseApi apicontent=" + s);
                }
            }, timeout, isgetCache, isCache);
        }

        public void PostRequest(long idRequest, string url, byte[] bytes, int timeout = -1, bool isgetCache = false, bool isCache = false)
        {
            ApiManager.Instance.postRequest(url, bytes, (b, s) =>
            {
                if (b)
                {
                    onLoadOk(idRequest, s);
                }
                else
                {

                    onLoadErr(idRequest, s);
                }

                if (isLogContent)
                {
                    Debug.Log("mysdk: BaseApi apicontent=" + s);
                }
            }, timeout, isgetCache, isCache);
        }

        public void PostRequest(long idRequest, string url, Dictionary<string, string> field, Dictionary<string, string> dicfiles = null, int timeout = -1, bool isgetCache = false, bool isCache = false)
        {
            ApiManager.Instance.postRequest(url, field, dicfiles, (b, s) =>
            {
                if (b)
                {
                    onLoadOk(idRequest, s);
                }
                else
                {
                    onLoadErr(idRequest, s);
                }

                if (isLogContent)
                {
                    Debug.Log("mysdk: BaseApi apicontent=" + s);
                }
            }, timeout, isgetCache, isCache);
        }

        protected abstract void onLoadOk(long idRequest, string data);

        protected abstract void onLoadErr(long idRequest, string err);
    }
}
