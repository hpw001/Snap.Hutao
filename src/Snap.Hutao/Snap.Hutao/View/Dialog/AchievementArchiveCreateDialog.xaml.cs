﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.UI.Xaml.Controls;

namespace Snap.Hutao.View.Dialog;

/// <summary>
/// 成就存档创建对话框
/// </summary>
[HighQuality]
internal sealed partial class AchievementArchiveCreateDialog : ContentDialog
{
    private readonly ITaskContext taskContext;

    /// <summary>
    /// 构造一个新的成就存档创建对话框
    /// </summary>
    /// <param name="serviceProvider">服务提供器</param>
    public AchievementArchiveCreateDialog(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        XamlRoot = serviceProvider.GetRequiredService<MainWindow>().Content.XamlRoot;

        taskContext = serviceProvider.GetRequiredService<ITaskContext>();
    }

    /// <summary>
    /// 获取输入的字符串
    /// </summary>
    /// <returns>输入的结果</returns>
    public async ValueTask<ValueResult<bool, string>> GetInputAsync()
    {
        await taskContext.SwitchToMainThreadAsync();
        ContentDialogResult result = await ShowAsync();
        string text = InputText.Text ?? string.Empty;

        return new(result == ContentDialogResult.Primary, text);
    }
}
