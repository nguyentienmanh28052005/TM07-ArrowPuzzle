using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnEnableDrop : MonoBehaviour
{
    public Action EnableAction;
    private void OnEnable()
    {
        EnableAction?.Invoke();
    }
}
