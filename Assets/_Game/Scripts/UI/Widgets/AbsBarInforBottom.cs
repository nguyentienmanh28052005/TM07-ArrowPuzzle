using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EventGame
{
    public abstract class AbsBarInforBottom : MonoBehaviour
    {
        public abstract bool IsEventActive();
        public abstract void Initialized(BarBottomManager manager);
        public abstract void SetUpVisual();
    }
}
