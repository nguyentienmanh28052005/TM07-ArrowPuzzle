using UnityEngine;

public sealed class LevelRuntimeFactoryV2 : ILevelMechanicFactory
{
    private readonly GameObject keycardPrefab;
    private readonly GameObject gatePrefab;
    private readonly GameObject electricButtonPrefab;
    private readonly GameObject revealWaveButtonPrefab;
    private readonly GameObject electricWallPrefab;
    private readonly GameObject portalPrefab;
    private readonly GameObject deflectorPrefab;
    private readonly GameObject countdownBlockPrefab;
    private readonly GameObject stopBlockPrefab;
    private readonly GameObject turnStateBlockPrefab;
    private readonly GameObject blackHolePrefab;
    private readonly Material portalHoleEffectMaterial;

    public LevelRuntimeFactoryV2(
        GameObject keycardPrefab,
        GameObject gatePrefab,
        GameObject electricButtonPrefab,
        GameObject revealWaveButtonPrefab,
        GameObject electricWallPrefab,
        GameObject portalPrefab,
        GameObject deflectorPrefab,
        GameObject countdownBlockPrefab,
        GameObject stopBlockPrefab,
        GameObject turnStateBlockPrefab,
        GameObject blackHolePrefab,
        Material portalHoleEffectMaterial)
    {
        this.keycardPrefab = keycardPrefab;
        this.gatePrefab = gatePrefab;
        this.electricButtonPrefab = electricButtonPrefab;
        this.revealWaveButtonPrefab = revealWaveButtonPrefab;
        this.electricWallPrefab = electricWallPrefab;
        this.portalPrefab = portalPrefab;
        this.deflectorPrefab = deflectorPrefab;
        this.countdownBlockPrefab = countdownBlockPrefab;
        this.stopBlockPrefab = stopBlockPrefab;
        this.turnStateBlockPrefab = turnStateBlockPrefab;
        this.blackHolePrefab = blackHolePrefab;
        this.portalHoleEffectMaterial = portalHoleEffectMaterial;
    }

    public GameObject CreateArrow(ArrowEntityData data, Transform parent)
    {
        return null;
    }

    public GameObject CreateCell(CellEntityData data, Transform parent)
    {
        if (data == null) return null;

        switch (data.typeId)
        {
            case CellTypeIds.Keycard:
                return SpawnKeycard(data, parent);
            case CellTypeIds.Gate:
                return SpawnGate(data, parent);
            case CellTypeIds.ElectricButton:
                return SpawnElectricButton(data, parent);
            case CellTypeIds.RevealWaveButton:
                return SpawnRevealWaveButton(data, parent);
            case CellTypeIds.ElectricWall:
                return SpawnElectricWall(data, parent);
            case CellTypeIds.Deflector:
                return SpawnDeflector(data, parent);
            case CellTypeIds.CountdownBlock:
                return SpawnCountdownBlock(data, parent);
            case CellTypeIds.StopBlock:
                return SpawnStopBlock(data, parent);
            case CellTypeIds.TurnStateBlock:
                return SpawnTurnStateBlock(data, parent);
            case CellTypeIds.BlackHole:
                return SpawnBlackHole(data, parent);
            case CellTypeIds.Portal:
                return null;
            default:
                Debug.LogWarning($"[LevelRuntimeFactoryV2] Unknown cell type '{data.typeId}'.");
                return null;
        }
    }

    public void CreateLink(LinkEntityData data, LevelDataV2 level, Transform parent)
    {
        if (data == null || level == null) return;

        if (data.typeId == LinkTypeIds.PortalPair)
        {
            CellEntityData entrance = LevelDataV2Queries.FindCell(level, data.fromEntityId);
            CellEntityData exit = LevelDataV2Queries.FindCell(level, data.toEntityId);
            if (entrance == null || exit == null) return;

            PortalPairPayload payload = data.payload as PortalPairPayload;
            Color color = payload != null ? payload.color : entrance.color;
            ArrowDir entranceDir = entrance.payload is PortalEndpointPayload entrancePayload ? entrancePayload.exitDirection : entrance.direction;
            ArrowDir exitDir = exit.payload is PortalEndpointPayload exitPayload ? exitPayload.exitDirection : exit.direction;

            if (GridManager.Instance != null)
            {
                GridManager.Instance.RegisterPortalLink(entrance.position, exit.position, exitDir);
                GridManager.Instance.RegisterPortalLink(exit.position, entrance.position, entranceDir);
            }

            if (portalPrefab == null) return;

            GameObject entranceObject = Object.Instantiate(portalPrefab, ToWorld(entrance.position), GetRotationForDir(entranceDir), parent);
            ConfigurePortalVisual(entranceObject, color);

            GameObject exitObject = Object.Instantiate(portalPrefab, ToWorld(exit.position), GetRotationForDir(exitDir), parent);
            ConfigurePortalVisual(exitObject, color);
        }
    }

    private GameObject SpawnKeycard(CellEntityData data, Transform parent)
    {
        GameObject obj = Spawn(keycardPrefab, data.position, Quaternion.identity, parent);
        if (obj == null) return null;
        if (obj.TryGetComponent(out GridKeycard script)) script.keyColor = data.color;
        SetSpriteColor(obj, data.color);
        return obj;
    }

    private GameObject SpawnGate(CellEntityData data, Transform parent)
    {
        GameObject obj = Spawn(gatePrefab, data.position, Quaternion.identity, parent);
        if (obj == null) return null;
        if (obj.TryGetComponent(out GridLaserGate script)) script.gateColor = data.color;
        SetSpriteColor(obj, data.color);
        return obj;
    }

    private GameObject SpawnElectricButton(CellEntityData data, Transform parent)
    {
        GameObject obj = Spawn(electricButtonPrefab, data.position, Quaternion.identity, parent);
        if (obj != null && obj.TryGetComponent(out GridElectricButton script)) script.SetColor(data.color);
        return obj;
    }

    private GameObject SpawnRevealWaveButton(CellEntityData data, Transform parent)
    {
        GameObject obj = Spawn(revealWaveButtonPrefab, data.position, Quaternion.identity, parent);
        if (obj != null && obj.TryGetComponent(out GridRevealWaveButton script)) script.SetColor(data.color);
        return obj;
    }

    private GameObject SpawnElectricWall(CellEntityData data, Transform parent)
    {
        if (!(data.payload is ElectricWallPayload payload)) return null;
        GameObject obj = electricWallPrefab != null ? Object.Instantiate(electricWallPrefab, Vector3.zero, Quaternion.identity, parent) : null;
        if (obj != null && obj.TryGetComponent(out GridElectricWall wall)) wall.Initialize(payload.start, payload.end, data.color, true);
        return obj;
    }

    private GameObject SpawnDeflector(CellEntityData data, Transform parent)
    {
        GameObject obj = Spawn(deflectorPrefab, data.position, GetRotationForDir(data.direction), parent);
        if (obj == null) return null;
        GridDeflector deflector = obj.GetComponentInChildren<GridDeflector>();
        if (deflector != null) deflector.SetDirection(data.direction);
        if (obj.GetComponent<GridDeflectorVisual>() == null) obj.AddComponent<GridDeflectorVisual>();
        return obj;
    }

    private GameObject SpawnCountdownBlock(CellEntityData data, Transform parent)
    {
        GameObject obj = Spawn(countdownBlockPrefab, data.position, Quaternion.identity, parent);
        CountCellPayload payload = data.payload as CountCellPayload;
        if (obj != null && obj.TryGetComponent(out GridCountdownBlock script)) script.SetCount(payload != null ? payload.count : 0);
        return obj;
    }

    private GameObject SpawnStopBlock(CellEntityData data, Transform parent)
    {
        GameObject obj = Spawn(stopBlockPrefab, data.position, Quaternion.identity, parent);
        CountCellPayload payload = data.payload as CountCellPayload;
        if (obj != null && obj.TryGetComponent(out GridStopBlock script)) script.SetCount(payload != null ? payload.count : 0);
        return obj;
    }

    private GameObject SpawnTurnStateBlock(CellEntityData data, Transform parent)
    {
        GameObject obj = Spawn(turnStateBlockPrefab, data.position, Quaternion.identity, parent);
        TurnStatePayload payload = data.payload as TurnStatePayload;
        if (obj != null && obj.TryGetComponent(out GridTurnStateBlock script)) script.SetInitialState(payload != null && payload.startsRed);
        return obj;
    }

    private GameObject SpawnBlackHole(CellEntityData data, Transform parent)
    {
        GameObject obj = Spawn(blackHolePrefab, data.position, GetRotationForDir(data.direction), parent);
        if (obj != null && obj.TryGetComponent(out GridBlackHole script)) script.SetDirection(data.direction);
        return obj;
    }

    private static GameObject Spawn(GameObject prefab, Vector2Int position, Quaternion rotation, Transform parent)
    {
        return prefab != null ? Object.Instantiate(prefab, ToWorld(position), rotation, parent) : null;
    }

    private static Vector3 ToWorld(Vector2Int position)
    {
        return new Vector3(position.x, position.y, 0f);
    }

    private static void SetSpriteColor(GameObject obj, Color color)
    {
        SpriteRenderer spriteRenderer = obj != null ? obj.GetComponent<SpriteRenderer>() : null;
        if (spriteRenderer != null) spriteRenderer.color = color;
    }

    private void ConfigurePortalVisual(GameObject portalObject, Color portalColor)
    {
        if (portalObject == null) return;

        GridPortalVisual portalVisual = portalObject.GetComponent<GridPortalVisual>();
        if (portalVisual == null) portalVisual = portalObject.AddComponent<GridPortalVisual>();
        if (portalHoleEffectMaterial != null) portalVisual.SetHoleEffectMaterial(portalHoleEffectMaterial);
        SetSpriteColor(portalObject, portalColor);
    }

    private static Quaternion GetRotationForDir(ArrowDir dir)
    {
        float angle = 0f;
        switch (dir)
        {
            case ArrowDir.Up: angle = 0f; break;
            case ArrowDir.Down: angle = 180f; break;
            case ArrowDir.Left: angle = 90f; break;
            case ArrowDir.Right: angle = -90f; break;
        }
        return Quaternion.Euler(0f, 0f, angle);
    }
}
