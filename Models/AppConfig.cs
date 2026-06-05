using System.Collections.Generic;
using System.Text.Json.Serialization;
using LuckyLilliaDesktop.Utils;

namespace LuckyLilliaDesktop.Models;

/// <summary>
/// 应用配置 - �?Python 版本�?config_manager.py 保持一�?
/// </summary>
public class AppConfig
{
    // 路径配置
    [JsonPropertyName("qq_path")]
    public string QQPath { get; set; } = string.Empty;

    [JsonPropertyName("pmhq_path")]
    public string PmhqPath { get; set; } = Constants.DefaultPaths.PmhqExe;

    [JsonPropertyName("llbot_path")]
    public string LLBotPath { get; set; } = "bin/RONoBot/llbot.js";

    [JsonPropertyName("node_path")]
    public string NodePath { get; set; } = Constants.DefaultPaths.NodeExe;

    // 启动选项
    [JsonPropertyName("auto_login_qq")]
    public string AutoLoginQQ { get; set; } = string.Empty;

    [JsonPropertyName("auto_start_bot")]
    public bool AutoStartBot { get; set; } = false;

    [JsonPropertyName("headless")]
    public bool Headless { get; set; } = false;

    [JsonPropertyName("debug")]
    public bool Debug { get; set; } = false;

    [JsonPropertyName("minimize_to_tray_on_start")]
    public bool MinimizeToTrayOnStart { get; set; } = false;

    [JsonPropertyName("startup_command_enabled")]
    public bool StartupCommandEnabled { get; set; } = false;

    [JsonPropertyName("startup_command")]
    public string StartupCommand { get; set; } = string.Empty;

    // 框架自动启动
    [JsonPropertyName("auto_start_frameworks")]
    public List<string> AutoStartFrameworks { get; set; } = new();

    // 日志设置
    [JsonPropertyName("log_save_enabled")]
    public bool LogSaveEnabled { get; set; } = true;

    [JsonPropertyName("log_retention_seconds")]
    public int LogRetentionSeconds { get; set; } = 604800; // 默认 7 �?

    // 关闭行为
    [JsonPropertyName("close_to_tray")]
    public bool? CloseToTray { get; set; } = null;

    // 窗口设置
    [JsonPropertyName("theme_mode")]
    public string ThemeMode { get; set; } = "dark";

    [JsonPropertyName("window_width")]
    public int WindowWidth { get; set; } = 1200;

    [JsonPropertyName("window_height")]
    public int WindowHeight { get; set; } = 800;

    [JsonPropertyName("window_left")]
    public int? WindowLeft { get; set; } = null;

    [JsonPropertyName("window_top")]
    public int? WindowTop { get; set; } = null;

    // 兼容性属�?- 只读取不写入
    [JsonPropertyName("auto_login")]
    public bool AutoLogin { get; set; } = true;

    /// <summary>
    /// 默认配置
    /// </summary>
    public static AppConfig Default => new();
}

