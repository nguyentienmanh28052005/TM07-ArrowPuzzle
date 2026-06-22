using UnityEngine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic; // Cần thêm thư viện này để dùng List

public class home_anim_ctrl_2 : MonoBehaviour
{
    public SkeletonGraphic skeletonAnimation;

    // Tên các animation khai báo chuẩn theo Spine
    [SpineAnimation] public string idleAnim;
    [SpineAnimation] public string idleBlueAnim;

    void Start()
    {
        skeletonAnimation = GetComponent<SkeletonGraphic>();

        // Bắt đầu tiến trình loop
        StartCoroutine(SpineAnimationRoutine());
    }

    private IEnumerator SpineAnimationRoutine()
    {
        // 1. Khởi tạo ban đầu: Chạy idle lặp lại liên tục ở Track 0
        skeletonAnimation.AnimationState.SetAnimation(0, idleAnim, true);

        while (true)
        {
            // 2. Chờ một khoảng thời gian random từ 1 đến 3 giây
            float waitTime = Random.Range(1f, 4f);
            yield return new WaitForSeconds(waitTime);

            // 3. Chạy animation idle_blue (chỉ 1 lần, false loop)
            // Spine trả về một TrackEntry chứa toàn bộ thông tin của lần chạy animation này
            Spine.TrackEntry blueTrack = skeletonAnimation.AnimationState.SetAnimation(0, idleBlueAnim, false);

            // 4. Xếp hàng (Queue) animation idle chạy lại ngay sau khi idle_blue kết thúc
            // Tham số delay = 0f nghĩa là tự động nối tiếp mượt mà ngay khi clip trước dừng
            skeletonAnimation.AnimationState.AddAnimation(0, idleAnim, true, 0f);

            // 5. Tạm dừng Coroutine bằng đúng thời gian của animation idle_blue
            // blueTrack.Animation.Duration lấy ra chính xác độ dài tính bằng giây của clip idle_blue
            yield return new WaitForSeconds(blueTrack.Animation.Duration);

            // Sau khi đợi xong, vòng lặp while quay lại bước 2 để tiếp tục random thời gian chờ
        }
    }
}