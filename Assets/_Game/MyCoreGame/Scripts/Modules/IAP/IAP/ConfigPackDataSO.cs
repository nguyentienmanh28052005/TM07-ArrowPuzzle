using master;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "ConfigPackDataSO", menuName = "SO/ConfigPackData", order = 1)]
public class ConfigPackDataSO : ScriptableObject
{
    public ConfigPackData[] data;
}
