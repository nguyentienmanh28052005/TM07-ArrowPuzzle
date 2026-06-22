using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "UserNameSO", menuName = "SO/Name", order = 1)]
public class UserNameSO : ScriptableObject
{
  
    [Serializable]
    public class NameData
    {
        public int id;
        public string description;
        public NameType type;
        public Text text;
    }
    public NameData[] Data;
}
public enum NameType
{
    Infinity = 0,
    TimeLimit = 1
}
