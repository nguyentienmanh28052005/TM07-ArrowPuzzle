using System.Collections;
using System.Collections.Generic;
using time;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "UserFrameSO", menuName = "SO/Frame", order = 1)]
public class UserFrameSO : ScriptableObject
{
    [System.Serializable]
    public class FrameObject
    {
        public int id;
        public string description;  
        public FrameType type;
        public GameObject frame;
    }

    public FrameObject[] Data;

}
public enum FrameType
{
    Infinity = 0,
    TimeLimit = 1
}

public enum EFrameSpecial
{
    Normal = 0,
    Battle_Pass = 5,
}

public enum ENameSpecial
{
    Normal = 0,
    Battle_Pass = 1,
}