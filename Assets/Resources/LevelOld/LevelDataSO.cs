using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "ArrowPuzzle/LevelData")]
public class LevelDataSO : ScriptableObject
{
    public int levelIndex;
    public GameMode gameMode;
    public LevelDifficulty levelDifficulty;

    [Header("Camera Intro")]
    public bool returnToDefaultZoomAfterIntro = true;

    public float timeLimit = 60f;
    
    public float rewardCoins;
    public float rewardDiamonds;
    
    public List<SnakeSaveData> snakes = new List<SnakeSaveData>();
    public List<KeycardSaveData> keycards = new List<KeycardSaveData>();
    public List<GateSaveData> gates = new List<GateSaveData>();
    public List<ElectricButtonSaveData> electricButtons = new List<ElectricButtonSaveData>();
    public List<RevealWaveButtonSaveData> revealWaveButtons = new List<RevealWaveButtonSaveData>();
    public List<ElectricWallSaveData> electricWalls = new List<ElectricWallSaveData>();
    public List<PortalData> portals = new List<PortalData>(); 
    public List<DeflectorSaveData> deflectors = new List<DeflectorSaveData>();
    public List<CountdownBlockSaveData> countdownBlocks = new List<CountdownBlockSaveData>();
    public List<StopBlockSaveData> stopBlocks = new List<StopBlockSaveData>();
    public List<TurnStateBlockSaveData> turnStateBlocks = new List<TurnStateBlockSaveData>();
    public List<BlackHoleSaveData> blackHoles = new List<BlackHoleSaveData>();
}
