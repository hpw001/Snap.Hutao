﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using CommunityToolkit.Mvvm.ComponentModel;
using Snap.Hutao.Core.Abstraction;
using Snap.Hutao.Model.Intrinsic;
using Snap.Hutao.Model.Metadata.Avatar;
using Snap.Hutao.Model.Metadata.Converter;
using Snap.Hutao.Model.Primitive;
using Snap.Hutao.ViewModel.AvatarProperty;

namespace Snap.Hutao.Model.Calculable;

/// <summary>
/// 可计算的技能
/// </summary>
[HighQuality]
internal sealed class CalculableSkill
    : ObservableObject,
    ICalculableSkill,
    IMappingFrom<CalculableSkill, ProudableSkill>,
    IMappingFrom<CalculableSkill, SkillView>
{
    private uint levelCurrent;
    private uint levelTarget;

    /// <summary>
    /// 构造一个新的可计算的技能
    /// </summary>
    /// <param name="skill">技能</param>
    private CalculableSkill(ProudableSkill skill)
    {
        GroupId = skill.GroupId;
        LevelMin = 1;
        LevelMax = 10; // hard coded 10 here
        Name = skill.Name;
        Icon = SkillIconConverter.IconNameToUri(skill.Icon);
        Quality = QualityType.QUALITY_NONE;

        LevelCurrent = LevelMin;
        LevelTarget = LevelMax;
    }

    /// <summary>
    /// 构造一个新的可计算的技能
    /// </summary>
    /// <param name="skill">技能</param>
    private CalculableSkill(SkillView skill)
    {
        GroupId = skill.GroupId;
        LevelMin = skill.LevelNumber;
        LevelMax = 10; // hard coded 10 here
        Name = skill.Name;
        Icon = skill.Icon;
        Quality = QualityType.QUALITY_NONE;

        LevelCurrent = LevelMin;
        LevelTarget = LevelMax;
    }

    /// <inheritdoc/>
    public SkillGroupId GroupId { get; }

    /// <inheritdoc/>
    public uint LevelMin { get; }

    /// <inheritdoc/>
    public uint LevelMax { get; }

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public Uri Icon { get; }

    /// <inheritdoc/>
    public QualityType Quality { get; }

    /// <inheritdoc/>
    public uint LevelCurrent { get => levelCurrent; set => SetProperty(ref levelCurrent, value); }

    /// <inheritdoc/>
    public uint LevelTarget { get => levelTarget; set => SetProperty(ref levelTarget, value); }

    public static CalculableSkill From(ProudableSkill source)
    {
        return new(source);
    }

    public static CalculableSkill From(SkillView source)
    {
        return new(source);
    }
}