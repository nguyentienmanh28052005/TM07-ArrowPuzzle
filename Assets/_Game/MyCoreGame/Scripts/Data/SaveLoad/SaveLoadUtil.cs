using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using UnityEditor;

namespace master
{
    public enum TypeSave
    {
        None = 0,
        Encryption = 1,

    }
    public static class SaveLoadUtil
    {
        public static JsonSerializerSettings JsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore };
        private const string SALT = "pietien090200";
        private const string PASSWORD = "adddttienedkcn89903";

        /// <summary>
        /// Read a file at specified path
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="isAbsolutePath">Is this path an absolute one?</param>
        /// <returns>Data of the file, in byte[] format</returns>
        public static byte[] LoadFile(string filePath, bool isAbsolutePath = false)
        {
            if (filePath == null || filePath.Length == 0)
            {
                return null;
            }

            string absolutePath = filePath;
            if (!isAbsolutePath) { absolutePath = GetWritablePath(filePath); }

            if (System.IO.File.Exists(absolutePath))
            {
                return System.IO.File.ReadAllBytes(absolutePath);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Save a byte array to storage at specified path and return the absolute path of the saved file
        /// </summary>
        /// <param name="bytes">Data to write</param>
        /// <param name="filePath">Where to save file</param>
        /// <param name="isAbsolutePath">Is this path an absolute one or relative</param>
        /// <returns>Absolute path of the file</returns>
        public static string SaveFile(byte[] bytes, string filePath, bool isAbsolutePath = false)
        {
            if (filePath == null || filePath.Length == 0)
            {
                return null;
            }
            //path to the file, in absolute format
            string path = filePath;
            if (!isAbsolutePath)
            {
                path = GetWritablePath(filePath);
            }

            string folderName = Path.GetDirectoryName(path);
            //Debug.Log("Folder name: " + folderName);
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }
            File.WriteAllBytes(path, bytes);
            return path;
        }

        /// <summary>
        /// Return a path to a writable folder on a supported platform
        /// </summary>
        /// <param name="relativeFilePath">A relative path to the file, from the out most writable folder</param>
        /// <returns></returns>
        public static string GetWritablePath(string relativeFilePath)
        {
            string result = "";
            //folder += (folder.Trim().Equals("")) ? "" : "/";
            //extension = (fileName.Trim().Equals("")) ? "" : "." + extension;

#if UNITY_EDITOR
            result = Application.persistentDataPath + Path.DirectorySeparatorChar + relativeFilePath;
#elif UNITY_ANDROID
		    result = Application.persistentDataPath + Path.DirectorySeparatorChar + relativeFilePath;
#elif UNITY_IPHONE
            result = Application.persistentDataPath + Path.DirectorySeparatorChar + relativeFilePath;
#elif UNITY_WP8 || NETFX_CORE || UNITY_WSA
		    result = Application.persistentDataPath + "/" + relativeFilePath;
#else
            result = Application.persistentDataPath + Path.DirectorySeparatorChar + relativeFilePath;
#endif

            return result;
        }

        public static void DeleteFile(string fileName)
        {
            if (fileName == null || fileName.Length == 0)
                return;
            string file = GetWritablePath(fileName);
            if (dicStream != null && dicStream.ContainsKey(fileName))
            {
                dicStream[fileName].Close();
                dicStream.Remove(fileName);
            }
            if (System.IO.File.Exists(file))
            {
                System.IO.File.Delete(file);
            }
        }

        /// <summary>
        /// Serialize an object into JSON string and write it into file
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="data">Object to serialize</param>
        /// <param name="fileName">Filename to write to</param>
        public static void SerializeObjectToFile<T>(T data, string fileName, string password = null, bool isCacheStream = true)
        {
            if (data != null)
            {
                string json = JsonConvert.SerializeObject(data);
                byte[] bytes;
                if (!string.IsNullOrEmpty(password))
                {
                    bytes = EncryptionHelper.Encrypt(json, password, SALT);
                }
                else
                {
                    bytes = System.Text.Encoding.UTF8.GetBytes(json);
                }
                // test aaa
                //SaveFile(bytes, fileName, isAbsoluteFilePath);

                SaveDataFS(bytes, fileName, isCacheStream);
            }
        }

        /// <summary>
        /// Deserialize an object from JSON file
        /// </summary>
        /// <typeparam name="T">Type of result object</typeparam>
        /// <param name="fileName">Json file content the serialized object</param>
        /// <returns>Object serialized in json file, if the file is not existed or invalid, the result will be default(T)</returns>
        public static T DeserializeObjectFromFile<T>(string fileName, string password = null, bool isCacheStream = true)
        {
            T data = default(T);
            // byte[] localSaved = LoadFile(fileName, isAbsolutePath);
            byte[] localSaved = LoadDataFS(fileName, isCacheStream);
            if (localSaved == null)
            {
                Debug.Log(fileName + " not exist, returning null");
            }
            else
            {
                if (!string.IsNullOrEmpty(password))
                {
                    string decrypt = EncryptionHelper.Decrypt(localSaved, password, SALT);
                    if (string.IsNullOrEmpty(decrypt))
                    {
                        Debug.LogWarning("Can't decrypt file " + fileName);
                        return data;
                    }
                    else
                    {
                        data = JsonConvert.DeserializeObject<T>(decrypt);
                    }
                }
                else
                {
                    var str = System.Text.Encoding.UTF8.GetString(localSaved);
                    data = JsonConvert.DeserializeObject<T>(str);
                }
            }
            return data;
        }
        public static async Task<T> DeserializeObjectFromFileAsync<T>(string fileName, string password = null, bool isCacheStream = true)
        {
            T data = default(T);
            byte[] localSaved = await LoadDataFSAsync(fileName, isCacheStream);
            if (localSaved == null)
            {
                Debug.Log(fileName + " not exist, returning null");
            }
            else
            {
                if (!string.IsNullOrEmpty(password))
                {
                    string decrypt = await EncryptionHelper.DecryptAsync(localSaved, password, SALT);
                    if (string.IsNullOrEmpty(decrypt))
                    {
                        Debug.LogWarning("Can't decrypt file " + fileName);
                        return data;
                    }
                    else
                    {
                        data = JsonConvert.DeserializeObject<T>(decrypt);
                    }
                }
                else
                {
                    var str = System.Text.Encoding.UTF8.GetString(localSaved);
                    data = JsonConvert.DeserializeObject<T>(str);
                }
            }
            return data;
        }
        public static void ByteToFile(byte[] byteData, string fileName, string password = null, bool isCacheStream = true)
        {
            if (byteData != null)
            {
                byte[] bytes;
                if (!string.IsNullOrEmpty(password))
                {
                    bytes = EncryptionHelper.Encrypt(byteData, password, SALT);
                }
                else
                {
                    bytes = byteData;
                }
                SaveDataFS(bytes, fileName, isCacheStream);
            }
        }
        public static byte[] ByteFromFile(string fileName, string password = null, bool isCacheStream = true)
        {
            // byte[] localSaved = LoadFile(fileName, isAbsolutePath);
            byte[] localSaved = LoadDataFS(fileName, isCacheStream);
            if (localSaved == null)
            {
                Debug.Log(fileName + " not exist, returning null");
            }
            else
            {
                if (!string.IsNullOrEmpty(password))
                {
                    return EncryptionHelper.DecryptToByte(localSaved, password, SALT);

                }
                else
                {
                    return localSaved;
                }
            }
            return null;
        }

        #region FileStream
        public static Dictionary<string, FileStream> dicStream = new Dictionary<string, FileStream>();
        public const int BUFFER_SIZE = 4096;
        public static byte[] Buffer = new byte[BUFFER_SIZE];
        public static void SaveDataFS(byte[] data, string fileName, bool cacheStream = false, FileAccess fileAccess = FileAccess.ReadWrite)
        {
            var path = GetWritablePath(fileName);
            string folderName = Path.GetDirectoryName(path);
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }
            if (cacheStream)
            {
                if (!dicStream.ContainsKey(fileName) || !dicStream[fileName].CanSeek)
                {
                    if (dicStream.ContainsKey(fileName)) dicStream.Remove(fileName);
                    var str = new FileStream(path, FileMode.OpenOrCreate, fileAccess);
                    dicStream.Add(fileName, str);
                }

                dicStream[fileName].Seek(0, SeekOrigin.Begin);
                dicStream[fileName].Write(data, 0, data.Length);
                dicStream[fileName].SetLength(data.Length);
                dicStream[fileName].Flush();
            }
            else
            {
                using (var str = new FileStream(path, FileMode.OpenOrCreate, fileAccess))
                {
                    str.Seek(0, SeekOrigin.Begin);
                    str.Write(data, 0, data.Length);
                    str.SetLength(data.Length);
                    str.Flush();
                    str.Close();
                }
            }
        }
        public static byte[] LoadDataFS(string fileName, bool cacheStream = false, FileAccess fileAccess = FileAccess.ReadWrite)
        {
            var path = GetWritablePath(fileName);
            string folderName = Path.GetDirectoryName(path);
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }
            if (cacheStream)
            {
                if (!dicStream.ContainsKey(fileName) || !dicStream[fileName].CanSeek)
                {
                    if (dicStream.ContainsKey(fileName)) dicStream.Remove(fileName);
                    var str = new FileStream(path, FileMode.OpenOrCreate, fileAccess);
                    dicStream.Add(fileName, str);
                }

                dicStream[fileName].Seek(0, SeekOrigin.Begin);
                var ls = new List<byte>();
                int c;
                while ((c = dicStream[fileName].Read(Buffer, 0, Buffer.Length)) > 0)
                {
                    ls.AddRange(new ArraySegment<byte>(Buffer, 0, c));
                }
                return ls.Count == 0 ? null : ls.ToArray();
            }
            else
            {
                using (var str = new FileStream(path, FileMode.OpenOrCreate, fileAccess))
                {
                    str.Seek(0, SeekOrigin.Begin);
                    var ls = new List<byte>();
                    int c;
                    while ((c = str.Read(Buffer, 0, Buffer.Length)) > 0)
                    {
                        ls.AddRange(new ArraySegment<byte>(Buffer, 0, c));
                    }
                    str.Close();
                    return ls.Count == 0 ? null : ls.ToArray();
                }
            }
        }
        public static async Task<byte[]> LoadDataFSAsync(string fileName, bool cacheStream = false, FileAccess fileAccess = FileAccess.ReadWrite)
        {
            var path = GetWritablePath(fileName);
            string folderName = Path.GetDirectoryName(path);
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }
            if (cacheStream)
            {
                if (!dicStream.ContainsKey(fileName) || !dicStream[fileName].CanSeek)
                {
                    if (dicStream.ContainsKey(fileName)) dicStream.Remove(fileName);
                    var str = new FileStream(path, FileMode.OpenOrCreate, fileAccess);
                    dicStream.Add(fileName, str);
                }

                dicStream[fileName].Seek(0, SeekOrigin.Begin);
                var ls = new List<byte>();
                int c;
                var Buffer = new byte[BUFFER_SIZE];
                while ((c = await dicStream[fileName].ReadAsync(Buffer, 0, Buffer.Length)) > 0)
                {
                    ls.AddRange(new ArraySegment<byte>(Buffer, 0, c));
                }
                return ls.Count == 0 ? null : ls.ToArray();
            }
            else
            {
                using (var str = new FileStream(path, FileMode.OpenOrCreate, fileAccess))
                {
                    str.Seek(0, SeekOrigin.Begin);
                    var ls = new List<byte>();
                    int c;
                    var Buffer = new byte[BUFFER_SIZE];
                    while ((c = await str.ReadAsync(Buffer, 0, Buffer.Length)) > 0)
                    {
                        ls.AddRange(new ArraySegment<byte>(Buffer, 0, c));
                    }
                    str.Close();
                    return ls.Count == 0 ? null : ls.ToArray();
                }
            }
        }
        #endregion

        public static void SaveData<T>(T data, string fileName, bool isCacheStream = true, TypeSave typeSave = TypeSave.Encryption)
        {
            if (typeSave == TypeSave.None)
            {
                SerializeObjectToFile(data, fileName, null, isCacheStream);
            }
            else if (typeSave == TypeSave.Encryption)
            {
                SerializeObjectToFile(data, fileName, PASSWORD, isCacheStream);
            }
        }
        public static T LoadData<T>(string fileName, bool isCacheStream = true, TypeSave typeSave = TypeSave.Encryption)
        {
            T t = default;
            if (typeSave == TypeSave.None)
            {
                t = DeserializeObjectFromFile<T>(fileName, null, isCacheStream);
            }
            else if (typeSave == TypeSave.Encryption)
            {
                t = DeserializeObjectFromFile<T>(fileName, PASSWORD, isCacheStream);
            }
            return t;
        }
        public static void SaveDataBytes(byte[] data, string fileName, bool isCacheStream = true, TypeSave typeSave = TypeSave.Encryption)
        {
            ByteToFile(data, fileName, PASSWORD, isCacheStream);
        }
        public static byte[] LoadDataBytes(string fileName, bool isCacheStream = true, TypeSave typeSave = TypeSave.Encryption)
        {
            return ByteFromFile(fileName, PASSWORD, isCacheStream);
        }

        public static void SaveDataAsync<T>(T data, string fileName, bool isCacheStream = true, TypeSave typeSave = TypeSave.Encryption)
        {
            if (typeSave == TypeSave.None)
            {
                SerializeObjectToFile(data, fileName, null, isCacheStream);
            }
            else if (typeSave == TypeSave.Encryption)
            {
                SerializeObjectToFile(data, fileName, PASSWORD, isCacheStream);
            }
        }
        public static async Task<T> LoadDataAsync<T>(string fileName, bool isCacheStream = true, TypeSave typeSave = TypeSave.Encryption)
        {
            Debug.Log($"Start Load {fileName}");
            T t = default;
            if (typeSave == TypeSave.None)
            {
                t = await DeserializeObjectFromFileAsync<T>(fileName, null, isCacheStream);
            }
            else if (typeSave == TypeSave.Encryption)
            {
                t = await DeserializeObjectFromFileAsync<T>(fileName, PASSWORD, isCacheStream);
            }
            Debug.Log($"End Load {fileName}");
            return t;
        }

        public static void DisposeAll()
        {
            foreach (var stream in dicStream)
            {
                stream.Value.Dispose();
            }
            dicStream.Clear();
        }

        public static void Clear()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += PlayModeState;
            void PlayModeState(PlayModeStateChange state)
            {
                if (state == PlayModeStateChange.ExitingPlayMode)
                {
                    if (dicStream != null && dicStream.Count != 0)
                    {
                        foreach (var VARIABLE in dicStream)
                        {
                            VARIABLE.Value.Close();
                        }
                    }
                }
            }

#endif
        }
    }
}
