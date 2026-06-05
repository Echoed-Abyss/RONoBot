using System;
using System.Threading;
using System.Threading.Tasks;
using LuckyLilliaDesktop.Utils;
using Microsoft.Extensions.Logging;

namespace LuckyLilliaDesktop.Services;

/// <summary>
/// 更新信息
/// </summary>
public class UpdateInfo
{
    public bool HasUpdate { get; set; }
    public string CurrentVersion { get; set; } = "";
    public string LatestVersion { get; set; } = "";
    public string ReleaseUrl { get; set; } = "";
    public string? Error { get; set; }
}

/// <summary>
/// 更新检查服务接口
/// </summary>
public interface IUpdateChecker
{
    Task<UpdateInfo> CheckAppUpdateAsync(string currentVersion, CancellationToken ct = default);
    Task<UpdateInfo> CheckPmhqUpdateAsync(string currentVersion, CancellationToken ct = default);
    Task<UpdateInfo> CheckLLBotUpdateAsync(string currentVersion, CancellationToken ct = default);
    /// <summary>
    /// 检查 RONoBot 是否有新版本（通过 GitHub Releases）
    /// </summary>
    Task<UpdateInfo> CheckRoNoBotUpdateAsync(string currentVersion, CancellationToken ct = default);
}

/// <summary>
/// 更新检查服务实现
/// </summary>
public class UpdateChecker : IUpdateChecker
{
    private readonly ILogger<UpdateChecker> _logger;
    private readonly NpmApiClient _npmClient;
    private readonly IGitHubHelper _gitHubHelper;

    public UpdateChecker(ILogger<UpdateChecker> logger, IGitHubHelper gitHubHelper)
    {
        _logger = logger;
        _npmClient = new NpmApiClient();
        _gitHubHelper = gitHubHelper;
    }

    public async Task<UpdateInfo> CheckAppUpdateAsync(string currentVersion, CancellationToken ct = default)
    {
        return await CheckRoNoBotUpdateAsync(currentVersion, ct);
    }

    public async Task<UpdateInfo> CheckPmhqUpdateAsync(string currentVersion, CancellationToken ct = default)
    {
        return await CheckUpdateAsync(
            Constants.NpmPackages.Pmhq,
            currentVersion,
            $"https://github.com/{Constants.GitHubRepos.Pmhq}/releases",
            ct);
    }

    public async Task<UpdateInfo> CheckLLBotUpdateAsync(string currentVersion, CancellationToken ct = default)
    {
        return await CheckUpdateAsync(
            Constants.NpmPackages.LLBot,
            currentVersion,
            $"https://github.com/{Constants.GitHubRepos.LLBot}/releases",
            ct);
    }

    /// <summary>
    /// 检查 RONoBot 是否有新版本（通过 GitHub Releases，而非 npm）
    /// </summary>
    public async Task<UpdateInfo> CheckRoNoBotUpdateAsync(string currentVersion, CancellationToken ct = default)
    {
        var repo = Constants.GitHubRepos.App; // "Echoed-Abyss/RONoBot"
        var parts = repo.Split('/');
        if (parts.Length != 2)
        {
            return new UpdateInfo
            {
                HasUpdate = false,
                CurrentVersion = currentVersion,
                LatestVersion = "未知",
                ReleaseUrl = $"https://github.com/{repo}/releases",
                Error = "仓库地址格式错误"
            };
        }

        try
        {
            var latestTag = await _gitHubHelper.GetLatestTagAsync(parts[0], parts[1], ct);

            if (string.IsNullOrEmpty(latestTag))
            {
                return new UpdateInfo
                {
                    HasUpdate = false,
                    CurrentVersion = currentVersion,
                    LatestVersion = "未知",
                    ReleaseUrl = $"https://github.com/{repo}/releases",
                    Error = "无法获取 GitHub Release 信息"
                };
            }

            var latestVersion = CleanVersion(latestTag);
            var hasUpdate = CompareVersions(currentVersion, latestVersion) < 0;

            _logger.LogInformation("RoNoBot: 当前版本 {Current}, 最新版本 {Latest} (tag: {Tag}), 有更新: {HasUpdate}",
                currentVersion, latestVersion, latestTag, hasUpdate);

            return new UpdateInfo
            {
                HasUpdate = hasUpdate,
                CurrentVersion = currentVersion,
                LatestVersion = latestVersion,
                ReleaseUrl = $"https://github.com/{repo}/releases/tag/{latestTag}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查 RoNoBot 更新失败");
            return new UpdateInfo
            {
                HasUpdate = false,
                CurrentVersion = currentVersion,
                LatestVersion = "未知",
                ReleaseUrl = $"https://github.com/{repo}/releases",
                Error = ex.Message
            };
        }
    }

    private async Task<UpdateInfo> CheckUpdateAsync(
        string packageName,
        string currentVersion,
        string releaseUrl,
        CancellationToken ct)
    {
        try
        {
            var packageInfo = await _npmClient.GetPackageInfoAsync(packageName, specificVersion: null, ct);

            if (packageInfo == null)
            {
                return new UpdateInfo
                {
                    HasUpdate = false,
                    CurrentVersion = currentVersion,
                    LatestVersion = "未知",
                    ReleaseUrl = releaseUrl,
                    Error = "无法获取版本信息"
                };
            }

            var latestVersion = packageInfo.Version;
            var hasUpdate = CompareVersions(currentVersion, latestVersion) < 0;

            _logger.LogInformation("{Package}: 当前版本 {Current}, 最新版本 {Latest}, 有更新: {HasUpdate}",
                packageName, currentVersion, latestVersion, hasUpdate);

            return new UpdateInfo
            {
                HasUpdate = hasUpdate,
                CurrentVersion = currentVersion,
                LatestVersion = latestVersion,
                ReleaseUrl = releaseUrl
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查更新失败: {Package}", packageName);
            return new UpdateInfo
            {
                HasUpdate = false,
                CurrentVersion = currentVersion,
                LatestVersion = "未知",
                ReleaseUrl = releaseUrl,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// 比较版本号
    /// </summary>
    /// <returns>-1 if v1 < v2, 0 if v1 == v2, 1 if v1 > v2</returns>
    private static int CompareVersions(string v1, string v2)
    {
        // 清理版本号 (去除 v 前缀等)
        v1 = CleanVersion(v1);
        v2 = CleanVersion(v2);

        if (Version.TryParse(v1, out var version1) && Version.TryParse(v2, out var version2))
        {
            return version1.CompareTo(version2);
        }

        // 如果无法解析，使用字符串比较
        return string.Compare(v1, v2, StringComparison.OrdinalIgnoreCase);
    }

    private static string CleanVersion(string version)
    {
        if (string.IsNullOrEmpty(version)) return "0.0.0";

        // 去除 v 前缀
        if (version.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            version = version[1..];

        // 只保留数字和点
        var parts = version.Split('-')[0]; // 去除 -beta, -alpha 等后缀
        return parts;
    }
}
