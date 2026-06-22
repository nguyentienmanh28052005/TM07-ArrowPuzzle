using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using master;
public class ScreenResizeHandler : MonoBehaviour
{
    private Vector2 lastScreenSize;
    private void Awake()
    {
        lastScreenSize = new Vector2(Screen.width, Screen.height);
    }
    public static int tst;
    void OnRectTransformDimensionsChange()
    {
        if (lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height)
        {
            tst++;
            lastScreenSize = new Vector2(Screen.width, Screen.height);
            Observer.Notify(ObserverName.screen_resize, tst);
        }
    }
}