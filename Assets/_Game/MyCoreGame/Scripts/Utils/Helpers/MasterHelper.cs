using DG.Tweening;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace master
{
    public static class MasterHelper
    {
        #region Layer

        public static bool HasLayer(int idxLayerCheck, int valueAllLayer)
        {
            if ((valueAllLayer & 1 << idxLayerCheck) == 1 << idxLayerCheck)
            {
                return true;
            }

            return false;
        }

        public static bool HasLayer(LayerMask layerCheck, LayerMask allLayer)
        {
            return HasLayerValue(layerCheck.value, allLayer.value);
        }

        public static bool HasLayerValue(int valueLayerCheck, int valueAllLayer)
        {
            return valueLayerCheck == (valueAllLayer & valueLayerCheck);
        }

        #endregion Layer

        #region Json

        public static T JsonDeserialize<T>(string ob)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(ob);
            }
            catch
            {
                return default;
            }
        }

        public static string JsonSerializeObject<T>(T t)
        {
            return JsonConvert.SerializeObject(t);
        }

        #endregion Json

        #region Enum

        public static TEnum CovertToEnum<TEnum>(string nameOfEnum, TEnum defaulEnum)
            where TEnum : struct, System.IConvertible
        {
            bool isEnum = System.Enum.TryParse(nameOfEnum, out TEnum _enum);
            if (!isEnum) return defaulEnum;
            return _enum;
        }

        public static TEnum CovertToEnum<TEnum>(string nameOfEnum) where TEnum : struct, System.IConvertible
        {
            System.Enum.TryParse(nameOfEnum, out TEnum _enum);
            return _enum;
        }

        public static TEnum CovertToEnum<TEnum>(int idEnum) where TEnum : struct, System.IConvertible
        {
            TEnum _enum = (TEnum)System.Enum.ToObject(typeof(TEnum), idEnum);
            return _enum;
        }

        public static TEnum CovertToEnum<TEnum>(int idEnum, TEnum defaulEnum) where TEnum : struct, System.IConvertible
        {
            TEnum _enum = (TEnum)System.Enum.ToObject(typeof(TEnum), idEnum);
            if (_enum.ToString().Equals(idEnum.ToString())) return defaulEnum;
            return _enum;
        }

        #endregion Enum

        #region Debug

        public static void Debug<T>(T t)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log($"CheckErr: {t}");
#endif
        }

        #endregion Debug

        #region Mathf

        public static float Round(float _value, int indexRound = 0)
        {
            return Mathf.Round(_value * Mathf.Pow(10, indexRound)) / Mathf.Pow(10, indexRound);
        }

        public static float Floor(float _value, int indexRound = 0)
        {
            return Mathf.Floor(_value * Mathf.Pow(10, indexRound)) / Mathf.Pow(10, indexRound);
        }

        public static float Ceil(float _value, int indexRound = 0)
        {
            return Mathf.Ceil(_value * Mathf.Pow(10, indexRound)) / Mathf.Pow(10, indexRound);
        }

        #endregion Mathf

        /*#region Spine
        public static void RecalculateTankSize(SkeletonGraphic graphicSke)
        {
            if (graphicSke != null)
            {
                var bounds = graphicSke.GetRectBounds();
                var rect = graphicSke.rectTransform;
                var size = rect.rect.width / bounds.size.x;
                rect.localScale = new Vector3(size, size, 1);
            }
        }
        #endregion*/

        #region Spawn Obj

        public static void InitListObj<Tobj, Tdata>(IList<Tdata> data, Tobj objPf, IList<Tobj> objs, Transform holdObj,
            System.Action<Tobj, int> onSetup) where Tobj : MonoBehaviour
        {
            if (objs == null)
            {
                objs = new List<Tobj>();
            }

            objPf.gameObject.SetActive(false);
            if (data != null)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    Tobj n;
                    var idx = i;
                    if (i < objs.Count)
                    {
                        n = objs[idx];
                    }
                    else
                    {
                        n = Object.Instantiate(objPf, holdObj);
                        objs.Add(n);
                    }

                    onSetup?.Invoke(n, idx);
                }
            }

            var c = data == null ? 0 : data.Count;
            if (c < objs.Count)
            {
                for (int i = c; i < objs.Count; i++)
                {
                    objs[i].gameObject.SetActive(false);
                }
            }
        }

        public static void InitListObj<Tobj>(int num, Tobj objPf, IList<Tobj> objs, Transform holdObj,
            System.Action<Tobj, int> onSetup) where Tobj : MonoBehaviour
        {
            if (objs == null)
            {
                objs = new List<Tobj>();
            }

            objPf.gameObject.SetActive(false);
            for (int i = 0; i < num; i++)
            {
                Tobj n;
                var idx = i;
                if (i < objs.Count)
                {
                    n = objs[idx];
                }
                else
                {
                    n = Object.Instantiate(objPf, holdObj);
                    objs.Add(n);
                }

                onSetup?.Invoke(n, idx);
            }

            if (num < objs.Count)
            {
                for (int i = num; i < objs.Count; i++)
                {
                    objs[i].gameObject.SetActive(false);
                }
            }
        }

        public static Sequence InitListObjTween<Tobj, Tdata>(IList<Tdata> data, Tobj objPf, IList<Tobj> objs,
            Transform holdObj, System.Action<Tobj, int> onSetup) where Tobj : MonoBehaviour
        {
            Sequence sequence = DOTween.Sequence();
            if (objs == null)
            {
                objs = new List<Tobj>();
            }

            objPf.gameObject.SetActive(false);
            if (data != null)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    Tobj n;
                    var idx = i;
                    if (i < objs.Count)
                    {
                        n = objs[idx];
                    }
                    else
                    {
                        n = Object.Instantiate(objPf, holdObj);
                        objs.Add(n);
                    }

                    onSetup?.Invoke(n, idx);
                }
            }

            var c = data == null ? 0 : data.Count;
            if (c < objs.Count)
            {
                for (int i = c; i < objs.Count; i++)
                {
                    objs[i].gameObject.SetActive(false);
                }
            }

            return sequence;
        }

        #endregion

        #region Validate Name

        public static bool ValidateName(string input)
        {
            if (input[0] == ' ') return false;
            return System.Text.RegularExpressions.Regex.IsMatch(input, @"^[\p{L}\p{M}\p{N}' \.\-]+$");
        }

        #endregion

        public static IEnumerable<System.Type> GetAllTypesThatImplement<T>()
        {
            return System.Reflection.Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(type => typeof(T).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);
        }

        public static float DistanceRatioScreen()
        {
            return (float)Screen.height / Screen.width - 1920f / 1080;
        }

        public static void ChangeLayerRecursive(this Transform targetTransform, int newLayer)
        {
            targetTransform.gameObject.layer = newLayer;

            foreach (Transform child in targetTransform)
            {
                ChangeLayerRecursive(child, newLayer);
            }
        }

        public static void ChangeLayerRecursive(this Transform targetTransform, int newLayer, int fromLayer)
        {
            if (targetTransform.gameObject.layer == fromLayer)
            {
                targetTransform.gameObject.layer = newLayer;
            }

            foreach (Transform child in targetTransform)
            {
                child.ChangeLayerRecursive(newLayer, fromLayer);
            }
        }

        public static string ToSpacedString(this Enum value)
        {
            string input = value.ToString();
            string result = Regex.Replace(input, "(\\B[A-Z]|\\d+)", " $1").Trim();
            return result;
        }

        public static bool IsVisibleInScrollRect(RectTransform element, ScrollRect scrollRect)
        {
            RectTransform viewport = scrollRect.viewport;

            Vector3[] viewportCorners = new Vector3[4];
            viewport.GetWorldCorners(viewportCorners);
            Rect viewportRect = new Rect(viewportCorners[0].x, viewportCorners[0].y,
                viewportCorners[2].x - viewportCorners[0].x,
                viewportCorners[2].y - viewportCorners[0].y);

            Vector3[] elementCorners = new Vector3[4];
            element.GetWorldCorners(elementCorners);
            Rect elementRect = new Rect(elementCorners[0].x, elementCorners[0].y,
                elementCorners[2].x - elementCorners[0].x,
                elementCorners[2].y - elementCorners[0].y);

            return viewportRect.Overlaps(elementRect, true);
        }

        public static void RollToItem(RectTransform content, RectTransform viewPort, RectTransform targetItem,
            float offset = 0)
        {
            float destination = Mathf.Clamp(-targetItem.anchoredPosition.y + offset, 0,
                content.sizeDelta.y - viewPort.rect.height);
            content.DOAnchorPosY(destination, 0.3f);
        }

        public static float GetCustomPercentFill(float currentPoint, List<float> points, List<Vector2> fillRanges)
        {
            if (points.Count != fillRanges.Count) return 0f;

            float result = 0f;

            for (int i = 0; i < points.Count; i++)
            {
                float pointStart = (i == 0) ? 0f : points[i - 1];
                float pointEnd = points[i];

                if (currentPoint < pointEnd)
                {
                    float rangePercent = fillRanges[i].y - fillRanges[i].x;
                    float pointDelta = pointEnd - pointStart;
                    float valueDelta = currentPoint - pointStart;

                    float t = Mathf.Clamp01(valueDelta / pointDelta);
                    result = fillRanges[i].x + t * rangePercent;
                    return result;
                }
            }

            return fillRanges[fillRanges.Count - 1].y;
        }

        public static GameObject MergeChildrenToOneMesh(Transform root, bool disableChildren = true)
        {
            MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>();
            if (meshFilters.Length == 0) return null;

            List<CombineInstance> combineList = new List<CombineInstance>(meshFilters.Length);
            Material sharedMaterial = null;

            foreach (var mf in meshFilters)
            {
                Mesh mesh = mf.sharedMesh;
                if (mesh == null) continue;

                // Chỉ lấy material 1 lần
                if (sharedMaterial == null)
                {
                    var mr = mf.GetComponent<MeshRenderer>();
                    if (mr != null) sharedMaterial = mr.sharedMaterial;
                }

                CombineInstance ci = new CombineInstance
                {
                    mesh = mesh,
                    transform = root.worldToLocalMatrix * mf.transform.localToWorldMatrix
                };
                combineList.Add(ci);
            }

            if (combineList.Count == 0) return null;

            // Tạo object mới chứa mesh combine
            GameObject mergedObj = new GameObject(root.name + "_Merged");
            mergedObj.transform.SetPositionAndRotation(root.position, root.rotation);
            mergedObj.transform.localScale = root.lossyScale;

            MeshFilter mfNew = mergedObj.AddComponent<MeshFilter>();
            MeshRenderer mrNew = mergedObj.AddComponent<MeshRenderer>();
            mrNew.sharedMaterial = sharedMaterial;

            Mesh newMesh = new Mesh();
            newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            newMesh.CombineMeshes(combineList.ToArray(), true, true, false);

            // Tối ưu cực mạnh (giảm dữ liệu thừa, giảm memory)
            newMesh.Optimize();
            newMesh.RecalculateBounds();

            mfNew.sharedMesh = newMesh;

            // Tắt children để giảm drawcall
            if (disableChildren)
            {
                foreach (Transform child in root)
                    child.gameObject.SetActive(false);
            }

            return mergedObj;
        }
        public static GameObject MergeFromList(List<GameObject> objects, string mergedName = "Merged_From_List", bool disableObjects = true)
        {
            if (objects == null || objects.Count == 0)
                return null;

            List<CombineInstance> combineList = new List<CombineInstance>();
            Material sharedMaterial = null;

            foreach (var obj in objects)
            {
                if (obj == null) continue;

                MeshFilter mf = obj.GetComponent<MeshFilter>();
                if (mf == null || mf.sharedMesh == null) continue;

                // Lấy material 1 lần
                if (sharedMaterial == null)
                {
                    MeshRenderer mr = obj.GetComponent<MeshRenderer>();
                    if (mr != null) sharedMaterial = mr.sharedMaterial;
                }

                CombineInstance ci = new CombineInstance
                {
                    mesh = mf.sharedMesh,
                    transform = obj.transform.localToWorldMatrix
                };

                combineList.Add(ci);
            }

            if (combineList.Count == 0) return null;

            // Tạo object chứa mesh merge
            GameObject merged = new GameObject(mergedName);
            MeshFilter mfNew = merged.AddComponent<MeshFilter>();
            MeshRenderer mrNew = merged.AddComponent<MeshRenderer>();

            Mesh newMesh = new Mesh
            {
                indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
            };

            newMesh.CombineMeshes(combineList.ToArray(), true, true, false);
            newMesh.Optimize();
            newMesh.RecalculateBounds();

            mfNew.sharedMesh = newMesh;
            mrNew.sharedMaterial = sharedMaterial;

            // Optional: tắt object gốc
            if (disableObjects)
            {
                foreach (var obj in objects)
                {
                    if (obj != null)
                        obj.SetActive(false);
                }
            }

            return merged;
        }
        
        public static GameObject MergeFromCombineInstances(
            List<CombineInstance> combineList,
            Material sharedMaterial,
            Transform parent,
            string mergedName = "Merged_From_List")
        {
            if (combineList == null || combineList.Count == 0)
                return null;

            GameObject merged = new GameObject(mergedName);
            merged.transform.SetParent(parent, false);
            merged.transform.localPosition = Vector3.zero;
            merged.transform.localRotation = Quaternion.identity;
            merged.transform.localScale = Vector3.one;

            MeshFilter meshFilter = merged.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = merged.AddComponent<MeshRenderer>();

            Mesh newMesh = new Mesh
            {
                indexFormat = IndexFormat.UInt32
            };

            newMesh.CombineMeshes(combineList.ToArray(), true, true, false);
            newMesh.RecalculateBounds();
            newMesh.RecalculateNormals();
            newMesh.RecalculateTangents();

            meshFilter.sharedMesh = newMesh;
            meshRenderer.sharedMaterial = sharedMaterial;

            return merged;
        }
    }
}