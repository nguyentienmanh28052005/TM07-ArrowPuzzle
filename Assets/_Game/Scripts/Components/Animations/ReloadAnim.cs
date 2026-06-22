using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class ReloadAnim : MonoBehaviour
{
    public SkeletonGraphic skeletonGraphic;

    void Start()
    {
        skeletonGraphic = GetComponent<SkeletonGraphic>();
    }

    void OnEnable()
    {
        // 1. Khởi tạo
        if (!skeletonGraphic.IsValid)
        {
            skeletonGraphic.Initialize(false);
        }

        // 2. Dọn dẹp trạng thái cũ
        skeletonGraphic.AnimationState.ClearTracks();
        skeletonGraphic.Skeleton.SetToSetupPose();

        // 3. TỰ ĐỘNG PLAY ANIMATION MẶC ĐỊNH TỪ INSPECTOR
        // Lấy tên animation và trạng thái loop đã cài đặt trong ô "Starting Animation"
        if (!string.IsNullOrEmpty(skeletonGraphic.startingAnimation))
        {
            skeletonGraphic.AnimationState.SetAnimation(0, skeletonGraphic.startingAnimation, skeletonGraphic.startingLoop);
        }

        // 4. Cập nhật khung xương ngay lập tức cho frame đầu tiên
        skeletonGraphic.Skeleton.UpdateWorldTransform();
        skeletonGraphic.Update(0f);
    }
}
