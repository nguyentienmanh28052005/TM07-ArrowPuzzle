using UnityEngine;
namespace master
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        public static T Instance
        {
            get
            {
#if UNITY_EDITOR
                if (instance == null)
                    Debug.LogWarning($"[Singleton] {typeof(T).Name}.Instance is null");
#endif
                return instance;
            }
        }
        protected virtual void Awake()
        {
            if (instance != null && instance != this)
            {
                Debug.LogWarning($"[Singleton] Duplicate {typeof(T).Name} detected. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }
            instance = this as T;
        }
        protected virtual void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }
    }

    // Persistent Singleton - DontDestroyOnLoad, chỉ giữ instance đầu tiên
    public class SingletonPersistent<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        public static T Instance
        {
            get
            {
#if UNITY_EDITOR
                if (instance == null)
                    Debug.LogWarning($"[SingletonPersistent] {typeof(T).Name}.Instance is null");
#endif
                return instance;
            }
        }
        protected virtual void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this as T;
            if (transform.root == transform)
                DontDestroyOnLoad(gameObject);
        }
        protected virtual void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }
    }

    public class SingletonSO<T> : ScriptableObject where T : ScriptableObject
    {
        private static T instance;
        public static T Instance => instance ?? (instance = Resources.Load<T>(typeof(T).Name));
    }
}
