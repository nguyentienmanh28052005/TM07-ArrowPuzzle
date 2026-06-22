# =============================================================================
# Refactor Animations Directory
# Tổ chức lại thư mục Animations theo feature-based architecture
# =============================================================================
# LƯU Ý: Script này di chuyển cả file .meta đi kèm.
#         Unity sẽ tự nhận diện lại nếu .meta được giữ nguyên GUID.
#         Sau khi chạy, mở Unity và đợi reimport.
# =============================================================================

$root = "E:\Project\SandLoop\XGameFrame\Assets\_Game\Animations"

# Thư mục nguồn cũ
$animation    = "$root\Animation"
$animations   = "$root\Animations"
$animClip     = "$root\AnimationClip"
$animatiors   = "$root\Animatiors"   # typo gốc

# --- Helper ---
function Move-WithMeta($Source, $DestDir) {
    if (Test-Path $Source) {
        if (-not (Test-Path $DestDir)) {
            New-Item -ItemType Directory -Path $DestDir -Force | Out-Null
        }
        Move-Item -Path $Source -Destination $DestDir -Force
        $metaSource = "$Source.meta"
        if (Test-Path $metaSource) {
            Move-Item -Path $metaSource -Destination $DestDir -Force
        }
    } else {
        Write-Warning "NOT FOUND: $Source"
    }
}

function Ensure-Dir($Path) {
    if (-not (Test-Path $Path)) {
        New-Item -ItemType Directory -Path $Path -Force | Out-Null
    }
}

# =============================================================================
# 1. Tạo cấu trúc thư mục mới
# =============================================================================
$newFolders = @(
    "Gameplay\LevelTransition",
    "Gameplay\LevelPlay",
    "Gameplay\Effects",
    "UI\Popup",
    "UI\Settings\SettingButton",
    "UI\Settings\SettingPanel",
    "UI\Shop",
    "UI\Reward",
    "UI\Rating",
    "UI\Common",
    "Tutorial",
    "Loading",
    "Clock"
)

foreach ($f in $newFolders) {
    Ensure-Dir (Join-Path $root $f)
}

Write-Host "=== Created folder structure ===" -ForegroundColor Green

# =============================================================================
# 2. Gameplay/LevelTransition  (từ Animatiors/)
# =============================================================================
$dest = "$root\Gameplay\LevelTransition"
Move-WithMeta "$animatiors\IntroLevel.anim"  $dest
Move-WithMeta "$animatiors\OutroLevel.anim"  $dest
Move-WithMeta "$animatiors\PreIntro.anim"    $dest

# =============================================================================
# 3. Gameplay/Effects  (từ Animatiors/ + Animation/)
# =============================================================================
$dest = "$root\Gameplay\Effects"
Move-WithMeta "$animatiors\MaskLight.anim"   $dest
Move-WithMeta "$animatiors\MaskLight_2.anim" $dest
Move-WithMeta "$animation\Rotate.anim"       $dest

# =============================================================================
# 4. Gameplay/LevelPlay  (từ Animation/LevelPlay/)
# =============================================================================
$dest = "$root\Gameplay\LevelPlay"
Move-WithMeta "$animation\LevelPlay\HardLevel.anim"   $dest
Move-WithMeta "$animation\LevelPlay\NormalLevel.anim"  $dest

# =============================================================================
# 5. UI/Popup  (từ Animation/ + Animatiors/ + Animations/)
# =============================================================================
$dest = "$root\UI\Popup"
Move-WithMeta "$animation\PopupRevive.anim"        $dest
Move-WithMeta "$animatiors\PopupNewFeature.anim"    $dest
Move-WithMeta "$animations\CompleteUI_New.anim"     $dest
Move-WithMeta "$animations\FinishFx.anim"           $dest
Move-WithMeta "$animations\WarningDefeat.anim"      $dest

# =============================================================================
# 6. UI/Settings  (từ Animation/InGame/)
# =============================================================================
# SettingButton
$dest = "$root\UI\Settings\SettingButton"
Move-WithMeta "$animation\InGame\SettingButton\Init.anim"                $dest
Move-WithMeta "$animation\InGame\SettingButton\Open.anim"                $dest
Move-WithMeta "$animation\InGame\SettingButton\SettingButton.controller"  $dest

# SettingPanel
$dest = "$root\UI\Settings\SettingPanel"
Move-WithMeta "$animation\InGame\SettingPanel\Init.anim"                $dest
Move-WithMeta "$animation\InGame\SettingPanel\Open.anim"                $dest
Move-WithMeta "$animation\InGame\SettingPanel\SettingPanel.controller"   $dest

# Toggle On/Off
$dest = "$root\UI\Settings"
Move-WithMeta "$animations\SettingOff_New.anim"  $dest
Move-WithMeta "$animations\SettingOn_New.anim"   $dest

# =============================================================================
# 7. UI/Shop  (từ Animations/UI/)
# =============================================================================
$dest = "$root\UI\Shop"
Move-WithMeta "$animations\UI\Tag.controller"   $dest
Move-WithMeta "$animations\UI\Tag_anim.anim"    $dest
Move-WithMeta "$animations\UI\Tag_anim_2.anim"  $dest
Move-WithMeta "$animations\UI\Tag_shake.anim"   $dest
Move-WithMeta "$animations\UI\TagSale.anim"     $dest
Move-WithMeta "$animations\UI\bottom_event.anim" $dest

# =============================================================================
# 8. UI/Reward  (từ Animation/ + Animations/)
# =============================================================================
$dest = "$root\UI\Reward"
Move-WithMeta "$animation\claim_reward.anim"    $dest
Move-WithMeta "$animation\stop_reward.anim"     $dest
Move-WithMeta "$animation\holder.controller"    $dest
Move-WithMeta "$animations\Coin.anim"           $dest
Move-WithMeta "$animations\Coin_0.controller"   $dest

# =============================================================================
# 9. UI/Rating  (từ Animations/UI/)
# =============================================================================
$dest = "$root\UI\Rating"
Move-WithMeta "$animations\UI\AnimShowRate.anim"  $dest

# =============================================================================
# 10. UI/Common  (từ nhiều nơi)
# =============================================================================
$dest = "$root\UI\Common"
Move-WithMeta "$animations\UI\AnimPunchScale.anim"  $dest
Move-WithMeta "$animations\UI\Root.controller"      $dest
Move-WithMeta "$animations\IconScaleAnim.anim"      $dest
Move-WithMeta "$animations\UIHandClick.anim"        $dest
Move-WithMeta "$animation\LightTextGold.anim"       $dest
Move-WithMeta "$animClip\AnimScale_Legacy.anim"     $dest

# =============================================================================
# 11. Tutorial  (từ Animation/Arrow/ + Animation/Tutorial/ + Animations/UI/)
# =============================================================================
$dest = "$root\Tutorial"
Move-WithMeta "$animation\Arrow\Arrow Tutorial.controller"  $dest
Move-WithMeta "$animation\Arrow\ArrowTutoial.anim"          $dest
Move-WithMeta "$animation\TipTutorial.anim"                 $dest
Move-WithMeta "$animation\Tutorial\IntroTutMission.anim"    $dest
Move-WithMeta "$animations\UI\AnimTutorialBooster.anim"     $dest

# =============================================================================
# 12. Loading  (từ Animation/Loading/)
# =============================================================================
$dest = "$root\Loading"
Move-WithMeta "$animation\Loading\Loading.controller"  $dest
Move-WithMeta "$animation\Loading\Loading_Idle.anim"   $dest

# =============================================================================
# 13. Clock  (từ Animation/Clock/)
# =============================================================================
$dest = "$root\Clock"
Move-WithMeta "$animation\Clock\Clock.controller"  $dest
Move-WithMeta "$animation\Clock\Clock.anim"         $dest

# =============================================================================
# 14. Dọn dẹp thư mục cũ (chỉ xóa nếu rỗng)
# =============================================================================
Write-Host ""
Write-Host "=== Cleaning up empty old folders ===" -ForegroundColor Yellow

$oldFolders = @($animation, $animations, $animClip, $animatiors)

foreach ($old in $oldFolders) {
    if (Test-Path $old) {
        # Xóa các thư mục con rỗng trước (đệ quy từ sâu nhất)
        Get-ChildItem -Path $old -Directory -Recurse |
            Sort-Object { $_.FullName.Length } -Descending |
            ForEach-Object {
                $children = Get-ChildItem -Path $_.FullName -Recurse -File | Where-Object { $_.Name -ne "*.meta" -or $_.Name -eq $_.Name }
                if ($children.Count -eq 0) {
                    Remove-Item $_.FullName -Recurse -Force
                    $metaDir = "$($_.FullName).meta"
                    if (Test-Path $metaDir) { Remove-Item $metaDir -Force }
                    Write-Host "  Removed empty: $($_.FullName)" -ForegroundColor DarkGray
                }
            }

        # Kiểm tra thư mục gốc cũ
        $remaining = Get-ChildItem -Path $old -Recurse -File
        if ($remaining.Count -eq 0) {
            Remove-Item $old -Recurse -Force
            $metaOld = "$old.meta"
            if (Test-Path $metaOld) { Remove-Item $metaOld -Force }
            Write-Host "  Removed: $old" -ForegroundColor DarkGray
        } else {
            Write-Host "  KEPT (still has files): $old" -ForegroundColor Red
            $remaining | ForEach-Object { Write-Host "    - $($_.FullName)" -ForegroundColor Red }
        }
    }
}

# =============================================================================
# 15. Hiển thị kết quả
# =============================================================================
Write-Host ""
Write-Host "=== Final Structure ===" -ForegroundColor Cyan
Get-ChildItem -Path $root -Recurse -Directory | ForEach-Object {
    $indent = "  " * ($_.FullName.Replace($root, "").Split("\").Length - 1)
    $fileCount = (Get-ChildItem -Path $_.FullName -File | Where-Object { $_.Extension -ne ".meta" }).Count
    Write-Host "$indent$($_.Name)/ ($fileCount files)" -ForegroundColor White
}

Write-Host ""
Write-Host "=== DONE! ===" -ForegroundColor Green
Write-Host "Mo Unity va doi reimport. Kiem tra Animator Controllers xem references co dung khong." -ForegroundColor Yellow
