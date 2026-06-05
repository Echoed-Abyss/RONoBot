# LuckyLilliaDesktop.Avalonia — GitHub Release 发布指南

## 版本检测原理

更新检测不再通过 npm，而是直接读取 **GitHub Releases API**：

1. 请求 `https://api.github.com/repos/Echoed-Abyss/RONoBot/releases/latest`
2. 拿到最新 release 的 `tag_name`（如 `v1.1.0`）
3. 与本地版本（`csproj` 的 `Version` 字段）比较
4. 有更新就显示提示

代码实现：
- `Services/UpdateChecker.cs` → `CheckRoNoBotUpdateAsync()`
- 注入 `IGitHubHelper` 通过 GitHub API 获取版本
- `Constants.GitHubRepos.App` = `"Echoed-Abyss/RONoBot"`（可修改）

---

## 发布步骤

### 1. 修改版本号

打开 `LuckyLilliaDesktop.csproj`，改：
```xml
<Version>1.0.0</Version>
```

改成新版本号，比如：
```xml
<Version>1.1.0</Version>
```

### 2. 编译 Release 版本

```bash
# Windows
dotnet publish -c Release -r win-x64 --self-contained

# macOS (Apple Silicon)
dotnet publish -c Release -r osx-arm64 --self-contained

# macOS (Intel)
dotnet publish -c Release -r osx-x64 --self-contained

# Linux
dotnet publish -c Release -r linux-x64 --self-contained
```

编译产物在：`bin/Release/net8.0/{runtime}/publish/`

### 3. 创建 GitHub Release

**方式 A — 网页操作：**

1. 打开 https://github.com/Echoed-Abyss/RONoBot/releases
2. 点 **"Create a new release"**
3. **Tag:** `v1.1.0`（必须带 `v` 前缀，且与 csproj 的 `Version` 一致）
4. **Title:** `RoNoBot Desktop v1.1.0`
5. **Description:** 写更新日志（变更内容）
6. 上传编译产物压缩包

**方式 B — gh CLI：**
```bash
gh release create v1.1.0 \
    --title "RoNoBot Desktop v1.1.0" \
    --notes "更新内容..." \
    ./publish/*.exe ./publish/*.dll
```

---

## 版本号对照规则

| csproj 中的 Version | GitHub Tag | 显示效果 |
|---------------------|------------|----------|
| `1.0.0`            | `v1.0.0`   | v1.0.0   |
| `1.1.0`            | `v1.1.0`   | v1.1.0   |
| `1.1.1`            | `v1.1.1`   | v1.1.1   |

Tag 必须带 `v` 前缀，代码里的 `CleanVersion()` 会自动去掉它来比较。

---

## 仓库地址配置

如果你改了 GitHub 仓库地址，修改 `Utils/Constants.cs`：
```csharp
public static class GitHubRepos
{
    public const string LLBot = "你的用户名/RONoBot";    // ← 非 LLBot，是 RONoBot
    public const string App = "你的用户名/RONoBot";      // ← 应用本身
    public const string Pmhq = "linyuchen/pmhq";         // PMHQ 不变
}
```

---

## 发布检查清单

- [ ] `LuckyLilliaDesktop.csproj` 的 `Version` 已更新
- [ ] `Utils/Constants.cs` 的 `GitHubRepos` 指向正确的仓库
- [ ] GitHub Release 的 Tag 带 `v` 前缀
- [ ] Release Notes 写了更新内容
- [ ] 编译产物（.exe / .app / 压缩包）已上传
