using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if ENABLE_AdInMo
using Adinmo;
#endif

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace mygame.sdk
{

    [CustomEditor(typeof(AdInMoObject)), CanEditMultipleObjects]
    public class AdInMoSetting : Editor
    {
        public AdInMoObject groupControl = null;

        public override void OnInspectorGUI()
        {
            GUILayout.BeginVertical();
            if (GUILayout.Button("InitAds"))
            {
                getGroupControl(target);
                doInit();
                AssetDatabase.Refresh();
            }
            GUILayout.EndVertical();

            try
            {
                base.OnInspectorGUI();
            }
            catch (Exception ex)
            {
                Debug.Log("mysdk: AdInMoSetting OnInspectorGUI ex=" + ex.ToString());
            }
        }

        private void getGroupControl(UnityEngine.Object ob)
        {
            if (groupControl == null)
            {
                groupControl = (AdInMoObject)ob;
            }
        }

        private void doInit()
        {
#if UNITY_EDITOR
            groupControl.listAdInfo.Clear();
            for (int i = 0; i < groupControl.transform.childCount; i++)
            {
                if (groupControl.transform.GetChild(i).name.StartsWith("Face"))
                {
                    Transform facepl = groupControl.transform.GetChild(i);
                    if (facepl.childCount >= 2 && facepl.Find("RectPos"))
                    {
                        Transform rpos = facepl.Find("RectPos");
                        Transform mdf = facepl.Find("Default");
                        Transform tpl;
                        if (mdf != null)
                        {
                            tpl = facepl.GetChild(2);
                        }
                        else
                        {
                            tpl = facepl.GetChild(1);
                        }

#if ENABLE_AdInMo
                        AdinmoTexture pl = tpl.GetComponent<AdinmoTexture>();
                        if (pl != null)
                        {
                            pl.transform.localPosition = rpos.localPosition;
                            pl.transform.localRotation = rpos.localRotation;
                            pl.transform.localScale = rpos.localScale;

                            AdInMoInfo info = new AdInMoInfo();
                            info.mesh = tpl.GetComponent<Renderer>();
                            if (mdf != null)
                            {
                                info.defaultMesh = mdf.GetComponent<Renderer>();
                            }
                            info.placement = pl;
                            groupControl.listAdInfo.Add(info);
                        }
#endif
                    }
                }
            }
            EditorUtility.SetDirty(groupControl);
#endif
        }

    }
}
