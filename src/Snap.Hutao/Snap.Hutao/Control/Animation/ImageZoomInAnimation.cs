﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Animations;
using Microsoft.UI.Composition;
using System.Numerics;

namespace Snap.Hutao.Control.Animation;

/// <summary>
/// 图片放大动画
/// </summary>
[HighQuality]
internal sealed class ImageZoomInAnimation : ImplicitAnimation<string, Vector3>
{
    /// <summary>
    /// 构造一个新的图片放大动画
    /// </summary>
    public ImageZoomInAnimation()
    {
        Duration = AnimationDurations.ImageZoom;
        EasingMode = Microsoft.UI.Xaml.Media.Animation.EasingMode.EaseOut;
        EasingType = CommunityToolkit.WinUI.Animations.EasingType.Circle;
        To = Core.StringLiterals.OnePointOne;
    }

    /// <inheritdoc/>
    protected override string ExplicitTarget
    {
        get => nameof(Visual.Scale);
    }

    /// <inheritdoc/>
    protected override (Vector3?, Vector3?) GetParsedValues()
    {
        return (To?.ToVector3(), From?.ToVector3());
    }
}