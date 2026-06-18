using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class ArrowEntityData
{
    public string entityId;
    public string typeId = ArrowTypeIds.Standard;
    public ArrowDir direction;
    public Color color = Color.white;
    public List<Vector2Int> segmentPositions = new List<Vector2Int>();

    [SerializeReference]
    public ArrowPayload payload = new StandardArrowPayload();
}

[Serializable]
public sealed class CellEntityData
{
    public string entityId;
    public string typeId;
    public Vector2Int position;
    public ArrowDir direction;
    public Color color = Color.white;

    [SerializeReference]
    public CellPayload payload;
}

[Serializable]
public sealed class LinkEntityData
{
    public string linkId;
    public string typeId;
    public string fromEntityId;
    public string toEntityId;
    public Vector2Int fromPosition;
    public Vector2Int toPosition;
    public bool usesPositions;

    [SerializeReference]
    public LinkPayload payload;
}

[CreateAssetMenu(fileName = "NewLevelV2", menuName = "ArrowPuzzle/LevelData V2")]
public sealed class LevelDataV2 : ScriptableObject
{
    public int levelIndex;
    public GameMode gameMode;
    public LevelDifficulty levelDifficulty;

    [Header("Camera Intro")]
    public bool returnToDefaultZoomAfterIntro = true;

    public float timeLimit = 60f;
    public float rewardCoins;
    public float rewardDiamonds;

    public List<ArrowEntityData> arrows = new List<ArrowEntityData>();
    public List<CellEntityData> cells = new List<CellEntityData>();
    public List<LinkEntityData> links = new List<LinkEntityData>();
}
