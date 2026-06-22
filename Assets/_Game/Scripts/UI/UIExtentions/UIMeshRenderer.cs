using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class UIMeshRenderer : MonoBehaviour
{
    public float scale = 1f;
    public bool isMask = true;
    public Shader shader;
    
    private MeshFilter[] meshFilter;

    private MeshFilter[] MeshFilter
    {
        get
        {
            if (meshFilter == null) meshFilter = GetComponentsInChildren<MeshFilter>();
            return meshFilter;
        }
    }
    
    private RectTransform rectTransform;

    private RectTransform RectTransform
    {
        get
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            return rectTransform;
        }
    }
    
    private MeshRenderer[] meshRenderer;

    private MeshRenderer[] MeshRenderer
    {
        get
        {
            if (meshRenderer == null) meshRenderer = GetComponentsInChildren<MeshRenderer>();
            return meshRenderer;
        }
    }
    
    public void SetMesh(GameObject prefab, Vector3 rotation = default)
    {
        for (int k = 0; k < MeshRenderer.Length; k++)
        {
            for (int m = 0; m < MeshRenderer[k].sharedMaterials.Length; m++)
            {
                var material = MeshRenderer[k].sharedMaterials[m];
                if (material != null && string.IsNullOrEmpty(material.name)) Destroy(material);
            }
        }
        
        var allRender = prefab.GetComponentsInChildren<MeshRenderer>();

        var length = Mathf.Max(allRender.Length, MeshRenderer.Length);
        
        for (int i = 0; i < MeshRenderer.Length; i++)
        {
            if (i >= allRender.Length)
            {
                MeshRenderer[i].enabled = false;
                continue;
            }
            var render = allRender[i];
            MeshRenderer[i].enabled = true;
            var materials = new Material[render.sharedMaterials.Length];
            for (int k = 0; k < render.sharedMaterials.Length; k++)
            {
                materials[k] = new Material(render.sharedMaterials[k]);
                materials[k].shader = shader;
                materials[k].SetInt("_EnableMask", isMask ? 1 : 0);
            }

            MeshRenderer[i].sharedMaterials = materials;
            MeshFilter[i].sharedMesh = allRender[i].GetComponent<MeshFilter>().sharedMesh;
            
            if (i == 0)
            {
                // transform.rotation = Quaternion.Euler(270, 165, 0);
                if (MeshFilter[i].sharedMesh == null) return;
                Vector3 centerDelta = MeshFilter[i].sharedMesh.bounds.center;
 
                float maxCoord = 0.0001f;
                var verts = MeshFilter[i].sharedMesh.vertices;
                for (int k = 0; k < verts.Length; k++)
                {
                    verts[k] -= centerDelta;
 
                    maxCoord = Mathf.Max(Mathf.Abs(verts[k].x), Mathf.Abs(verts[k].y), maxCoord);
                }

                var bounds = MeshFilter[i].sharedMesh.bounds;

                var size = allRender[i].transform.rotation * bounds.size;
                var center = allRender[i].transform.rotation * bounds.center;
                var scaleRate = Mathf.Max(Mathf.Sqrt(Mathf.Pow(size.x, 2) + Mathf.Pow(size.z, 2)), size.y + .65f);
                var sizeRate = 90;

                var sc = Mathf.Min(40, sizeRate / scaleRate) * scale;
                transform.localScale = Vector3.one * sc;
        
                transform.localPosition = new Vector3(0, (size.y / 2 - center.y) * transform.localScale.y - 30, -100);
                var roundY = Mathf.Abs(Mathf.RoundToInt(allRender[i].transform.eulerAngles.y));
                // transform.gameObject.name = prefab.name;
                if (allRender[i].transform.eulerAngles.x == 0)
                {
                    transform.localRotation = Quaternion.Euler(0, 0, 0);
                }
                else if (roundY == 180)
                {
                    transform.localRotation = Quaternion.Euler(245, 0, 328);
                    transform.localPosition += transform.up * allRender[i].transform.position.z * transform.localScale.z / 2.75f;
                }
                else if (roundY == 90)
                {
                    transform.localRotation = Quaternion.Euler(245, 0, -122);
                    transform.localPosition -= transform.right * allRender[i].transform.position.z * transform.localScale.z / 2.5f;
                }
                else
                {
                    transform.localRotation = Quaternion.Euler(295, 180, 328);
                    transform.localPosition -= transform.up * allRender[i].transform.position.z * transform.localScale.z / 2;
                }
            }
            else
            {
                MeshFilter[i].transform.localScale = allRender[i].transform.lossyScale / allRender[0].transform.lossyScale.x;
                MeshFilter[i].transform.localPosition = allRender[0].transform.InverseTransformPoint(allRender[i].transform.position);
                MeshFilter[i].transform.localRotation = Quaternion.Inverse(allRender[0].transform.rotation) * allRender[i].transform.rotation;

                // if (Mathf.Abs(Mathf.RoundToInt(allRender[0].transform.eulerAngles.y)) == 180)
                // {
                //     MeshFilter[i].transform.Rotate(new Vector3(25, 0, 180));
                // }
                // else
                // {
                //     MeshFilter[i].transform.Rotate(new Vector3(-25, 0, 0));
                // }
            }
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < MeshRenderer.Length; i++)
        {
            MeshRenderer[i].enabled = false;
        }
    }

    private void OnEnable()
    {
        for (int i = 0; i < MeshRenderer.Length; i++)
        {
            MeshRenderer[i].enabled = true;
        }
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        // if (mesh != cacheMesh || material != cacheMaterial)
        // {
        //     SetMesh(mesh, material);
        //     cacheMesh = mesh;
        //     cacheMaterial = material;
        // }
    }
#endif
}