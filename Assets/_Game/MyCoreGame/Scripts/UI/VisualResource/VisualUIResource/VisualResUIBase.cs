using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class VisualResUIBase : MonoBehaviour
{
    public abstract void Init(DataResource type, Sprite bgDf = null, Sprite iconDf = null, byte star = 0);
    public abstract Image BG { get; set; }
    public abstract Image Icon { get; set; }
}
