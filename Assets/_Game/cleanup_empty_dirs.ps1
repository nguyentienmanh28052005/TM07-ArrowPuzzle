$basePath = "e:\Project\SandLoop\XGameFrame\Assets\_Game"

function Remove-EmptyFoldersRecurse {
    param ([string]$Dir)

    if (-not (Test-Path $Dir)) { return }

    # Traverse child directories first (Bottom-Up)
    $subDirs = Get-ChildItem -Path $Dir -Directory -Force
    foreach ($subDir in $subDirs) {
        Remove-EmptyFoldersRecurse -Dir $subDir.FullName
    }

    # Don't delete the root base path
    if ($Dir -eq $basePath) { return }

    # Check if directory only contains a .meta file or is entirely empty
    $items = Get-ChildItem -Path $Dir -Force
    $nonMetaItems = $items | Where-Object { $_.Name -notmatch "\.meta$" }
    
    if ($items.Count -eq 0 -or $nonMetaItems.Count -eq 0) {
        Write-Host "Cleaning up empty directory: $Dir" -ForegroundColor Yellow
        Remove-Item -Path $Dir -Recurse -Force
        
        # Cleanup its associated .meta file outside
        $metaPath = "$Dir.meta"
        if (Test-Path $metaPath) {
            Remove-Item -Path $metaPath -Force
        }
    }
}

Write-Host "Bắt đầu quét và dọn dẹp thư mục rỗng..." -ForegroundColor Cyan
Remove-EmptyFoldersRecurse -Dir $basePath
Write-Host "---------------------------------------------------" -ForegroundColor Cyan
Write-Host "Hoàn tất Dọn Dẹp. Dự án đã sạch sẽ 100%!" -ForegroundColor Green
