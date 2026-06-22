using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
[CreateAssetMenu(fileName = "Avatar Guild SO", menuName = "SO/Avatar Guild")]
public class AvatarGuildSO : ScriptableObject
{
    public Sprite[] spritesReference;
    public Sprite LoadSprite(int idx)
    {
        return spritesReference[Mathf.Clamp(idx, 0, spritesReference.Length - 1)];
    }
}