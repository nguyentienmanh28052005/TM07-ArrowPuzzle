using UnityEngine;

// 1. Kế thừa MonoBehaviour để kéo thả được
// 2. Implements ITutorialCondition để TutorialManager có thể hiểu được
public class LevelSpecificCondition : MonoBehaviour, ITutorialCondition
{
    [Header("Cài đặt Level muốn hiện Tutorial")]
    public int targetLevelIndex = 15;

    // Bắt buộc phải có hàm này vì Interface quy định
    public bool IsApplicable(LevelDataSO levelData)
    {
        // Kiểm tra nếu Data truyền vào bị rỗng thì báo False (không hiện)
        if (levelData == null) return false;

        // Nếu Level hiện tại đúng bằng Target Level -> Báo True (Cho phép hiện Tutorial)
        return levelData.levelIndex == targetLevelIndex; 
    }
}