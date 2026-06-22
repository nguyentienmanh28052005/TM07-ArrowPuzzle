$basePath = "e:\Project\SandLoop\XGameFrame\Assets\_Game"
$vfxPath = "$basePath\VFX"

function Move-AssetContent {
    param (
        [string]$sourceDir,
        [string]$destDir
    )
    if (Test-Path $sourceDir) {
        if (-not (Test-Path $destDir)) {
            New-Item -ItemType Directory -Force -Path $destDir | Out-Null
            Write-Host "Created Directory: $destDir"
        }
        
        Write-Host "Moving contents from $sourceDir to $destDir"
        $items = Get-ChildItem -Path $sourceDir -Exclude "*.meta"
        foreach ($item in $items) {
            $destPath = Join-Path $destDir $item.Name
            
            # If destination item already exists, we might need to merge if it's a directory
            if ((Test-Path $destPath) -and $item.PSIsContainer) {
                # Recursively merge inner directory
                Move-AssetContent -sourceDir $item.FullName -destDir $destPath
            } else {
                # Add '-ErrorAction SilentlyContinue' to make sure if file is locked we skip it cleanly or throw error
                Move-Item -Path $item.FullName -Destination $destPath -Force
                
                $metaPath = "$($item.FullName).meta"
                if (Test-Path $metaPath) {
                    $metaDest = "$destPath.meta"
                    Move-Item -Path $metaPath -Destination $metaDest -Force
                }
            }
        }
        
        # Check if empty, then remove
        $remaining = Get-ChildItem -Path $sourceDir
        if ($remaining.Count -eq 0 -or ($remaining.Count -eq 1 -and $remaining[0].Name -match "\.meta$")) {
			# Delete folder
            Remove-Item -Path $sourceDir -Recurse -Force
            if (Test-Path "$sourceDir.meta") {
                Remove-Item -Path "$sourceDir.meta" -Force
            }
        }
    } else {
        Write-Host "Source directory not found: $sourceDir" -ForegroundColor Yellow
    }
}

# 1. Ensure basic structure
$targetDirs = @(
    "$vfxPath\Prefabs\App",
    "$vfxPath\Prefabs\Gameplay",
    "$vfxPath\Prefabs\MetaGame",
    "$vfxPath\Prefabs\UI",
    "$vfxPath\Textures",
    "$vfxPath\Materials",
    "$vfxPath\Shaders",
    "$vfxPath\Models"
)

foreach ($dir in $targetDirs) {
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Force -Path $dir | Out-Null
    }
}

# 2. Extract contents from old directories
Move-AssetContent -sourceDir "$basePath\XGameVfx\Textures" -destDir "$vfxPath\Textures"
Move-AssetContent -sourceDir "$basePath\XGameVfx\Materials" -destDir "$vfxPath\Materials"
Move-AssetContent -sourceDir "$basePath\XGameVfx\Shaders" -destDir "$vfxPath\Shaders"

# 3. Handle Prefabs in XGameVfx
Move-AssetContent -sourceDir "$basePath\XGameVfx\FX_Prefabs\DifficultyBadges" -destDir "$vfxPath\Prefabs\MetaGame\DifficultyBadges"
Move-AssetContent -sourceDir "$basePath\XGameVfx\FX_Prefabs\Particles" -destDir "$vfxPath\Prefabs\Gameplay\Particles_XGameVfx"

# 4. Handle Prefabs in Prefabs/VFX
Move-AssetContent -sourceDir "$basePath\Prefabs\VFX\Particles" -destDir "$vfxPath\Prefabs\Gameplay\Particles_Prefabs"
Move-AssetContent -sourceDir "$basePath\Prefabs\VFX\SpineAnim" -destDir "$vfxPath\Prefabs\Gameplay\SpineAnim"

# 5. Cleanup root remaining directories if empty
function Remove-EmptyDir {
    param([string]$dir)
    if (Test-Path $dir) {
        $items = Get-ChildItem -Path $dir
        if ($items.Count -eq 0) {
            Write-Host "Removing empty directory: $dir"
            Remove-Item -Path $dir -Force
            if (Test-Path "$dir.meta") {
                Remove-Item -Path "$dir.meta" -Force
            }
        }
    }
}

Remove-EmptyDir -dir "$basePath\XGameVfx\FX_Prefabs"
Remove-EmptyDir -dir "$basePath\XGameVfx"
Remove-EmptyDir -dir "$basePath\Prefabs\VFX"

Write-Host "---------------------------------------------------" -ForegroundColor Cyan
Write-Host "VFX Refactoring Completed Structure successfully!" -ForegroundColor Green
Write-Host "Tất cả file và .meta đã được di chuyển an toàn." -ForegroundColor Green
Write-Host "Bạn hãy bật Unity lên để nó tự re-import đường dẫn nhé." -ForegroundColor Yellow
Write-Host "---------------------------------------------------" -ForegroundColor Cyan
