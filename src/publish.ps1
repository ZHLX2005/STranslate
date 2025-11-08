# 清理之前的构建输出
Write-Host "正在清理之前的构建..." -ForegroundColor Yellow
#dotnet clean STranslate.sln --configuration Release
Remove-Item -Path .\.artifacts\Release\ -Recurse -Force

# 更新Fody配置
Write-Host "正在更新FodyWeavers..." -ForegroundColor Yellow
Copy-Item "STranslate/FodyWeavers.Release.xml" "STranslate/FodyWeavers.xml.bak"
Move-Item -Path "STranslate/FodyWeavers.xml.bak" -Destination "STranslate/FodyWeavers.xml" -Force

# 重新生成整个解决方案
Write-Host "正在重新生成解决方案..." -ForegroundColor Yellow
dotnet build .\STranslate.sln --configuration Release --no-incremental

# 重置FodyWeavers.xml
Write-Host "正在还原FodyWeavers.xml..." -ForegroundColor Yellow
git restore STranslate/FodyWeavers.xml

Write-Host "构建完成！" -ForegroundColor Green