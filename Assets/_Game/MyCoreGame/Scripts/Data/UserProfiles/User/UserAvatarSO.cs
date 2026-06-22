using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "UserAvatarSO", menuName = "SO/Avatar", order = 1)]
public class UserAvatarSO : ScriptableObject
{
    public Sprite defaultAvatar;
    public Sprite[] Data;
}
