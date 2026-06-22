using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.Util;

public enum ABOperationStatus
{
    None,
    InProgress,
    Succeeded,
    Failed
}

public class ABOperationHandle<T>
{
    public static int HandleID;
    public int Id;
    public object key { get; private set; }
    public bool IsDone { get; private set; }
    public float Progress { get; private set; }
    public long Size { get; private set; }
    public T Result { get; private set; }

    public ABOperationStatus Status { get; private set; }
    
    public Exception Exception { get; private set; }

    public Action<ABOperationHandle<T>> Completed;

    public static ABOperationHandle<T> Create(object obKey)
    {
        ABOperationHandle<T> h = new ABOperationHandle<T>();
        h.Id = HandleID++;
        h.Status = ABOperationStatus.None;
        h.IsDone = false;
        h.Progress = 0;
        h.Size = 0;
        h.Exception = null;
        h.key = obKey;
        return h;
    }

    
    public void SetProgress(float p) => Progress = p;

    public void SetResult(T res)
    {
        IsDone = true;
        Result = res;
        Status = ABOperationStatus.Succeeded;
        Completed?.Invoke(this);
    }
    
    
    //===========================
    // FAIL
    //===========================
    public void SetFail(Exception ex)
    {
        Exception = ex;
        IsDone = true;
        Status = ABOperationStatus.Failed;
        Progress = 1f;
        Completed?.Invoke(this);
    }

    //===========================
    // FAIL string message
    //===========================
    public void SetFail(string message)
    {
        SetFail(new Exception(message));
    }
    
    public bool Equals(ABOperationHandle<T> other)
    {
        return Id == other.Id;
    }

    public override bool Equals(object obj)
    {
        return obj is ABOperationHandle<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(ABOperationHandle<T> left, ABOperationHandle<T> right)
    {
        if (ReferenceEquals(left, right)) 
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(ABOperationHandle<T> left, ABOperationHandle<T> right)
    {
        return !(left == right);
    }

    public void Release()
    {
        SafeDestroyOrUnload(Result as Object);
        Result = default;
        // Resources.UnloadAsset(Result as Object);
    }
    
    public static void SafeDestroyOrUnload(Object obj)
    {
        if (obj == null)
            return;
    
        // Nếu là GameObject instance
        if (obj is GameObject go)
        {
            if (go.scene.IsValid())   // => đang ở trong scene, là instance
                Object.Destroy(go);
            else
                Debug.LogWarning($"Bạn đang destroy prefab asset: {go.name}");
            return;
        }

        // Nếu là Component instance
        if (obj is Component comp)
        {
            if (comp.gameObject.scene.IsValid())  // instance
                Object.Destroy(comp);
            else
                Debug.LogWarning($"Bạn đang destroy prefab asset: {comp.name}");
            return;
        }

        // Các asset khác không destroy
        Debug.LogWarning($"Không thể Destroy asset loại: {obj.GetType()}");

        // 3. Nếu là asset trong Project → không destroy → unload asset
        // UnloadAsset chỉ dùng cho: Texture, Material, AudioClip, Mesh, TextAsset...
        if (obj is Texture or Material or AudioClip or Mesh or TextAsset)
        {
            Resources.UnloadAsset(obj);
            return;
        }

        Debug.LogWarning($"SafeDestroyOrUnload: Không thể Destroy hoặc Unload asset loại {obj.GetType()}");
    }
}

public class ABDownloadHandleException : Exception
{
    public enum ExceptionStatus
    {
        None = 0,
        SizeMismatch,
        NetworkError,
        KeyNotFound,
    }
    public ExceptionStatus Status { get; private set; }

    public ABDownloadHandleException(ExceptionStatus status, string message) : base(message)
    {
        Status = status;
    }
}

public class ABOperationDownloadHandle
{
    public static int HandleID;
    public int Id;
    public bool SizeMismatch;
    
    public bool IsDone { get; private set; }
    public float Progress { get; private set; }
    public long Size { get; set; }
    public long DownloadedBytes { get; set; }
    
    public ABOperationStatus Status { get; set; }
    
    public ABDownloadHandleException Exception { get; private set; }

    public event Action<ABOperationDownloadHandle> Completed;
    
    public static ABOperationDownloadHandle Create()
    {
        ABOperationDownloadHandle h = new ABOperationDownloadHandle();
        h.Id = HandleID++;
        h.Status = ABOperationStatus.None;
        h.IsDone = false;
        h.Progress = 0;
        h.Size = 0;
        h.Exception = null;
        return h;
    }
    
    public void SetProgress(float p)
    {
        Progress = p;
    }

    public void SetResult()
    {
        IsDone = true;
        Status = ABOperationStatus.Succeeded;
        Completed?.Invoke(this);
    }
    
    
    //===========================
    // FAIL
    //===========================
    public void SetFail(ABDownloadHandleException ex)
    {
        Exception = ex;
        IsDone = true;
        Status = ABOperationStatus.Failed;
        Progress = 1f;
        Completed?.Invoke(this);
    }

    //===========================
    // FAIL string message
    //===========================
    public void SetFail(string message, ABDownloadHandleException.ExceptionStatus status)
    {
        SetFail(new ABDownloadHandleException(status, message));
    }
    
    public bool Equals(ABOperationDownloadHandle other)
    {
        return Id == other.Id;
    }

    public override bool Equals(object obj)
    {
        return obj is ABOperationDownloadHandle other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(ABOperationDownloadHandle left, ABOperationDownloadHandle right)
        => left.Equals(right);

    public static bool operator !=(ABOperationDownloadHandle left, ABOperationDownloadHandle right)
        => !left.Equals(right);
}