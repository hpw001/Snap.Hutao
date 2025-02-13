﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Service.Metadata;
using Snap.Hutao.Service.User;
using Snap.Hutao.ViewModel.User;
using Snap.Hutao.Web.Hoyolab.Hk4e.Event.GachaInfo;
using Snap.Hutao.Web.Hoyolab.Takumi.Binding;
using Snap.Hutao.Web.Response;

namespace Snap.Hutao.Service.GachaLog.QueryProvider;

/// <summary>
/// 使用 SToken 提供祈愿 Url
/// </summary>
[HighQuality]
[ConstructorGenerated]
[Injection(InjectAs.Transient)]
internal sealed partial class GachaLogQuerySTokenProvider : IGachaLogQueryProvider
{
    private readonly BindingClient2 bindingClient2;
    private readonly MetadataOptions metadataOptions;
    private readonly IUserService userService;

    /// <inheritdoc/>
    public async ValueTask<ValueResult<bool, GachaLogQuery>> GetQueryAsync()
    {
        if (UserAndUid.TryFromUser(userService.Current, out UserAndUid? userAndUid))
        {
            if (userAndUid.User.IsOversea)
            {
                return new(false, SH.ServiceGachaLogUrlProviderStokenUnsupported);
            }

            GenAuthKeyData data = GenAuthKeyData.CreateForWebViewGacha(userAndUid.Uid);
            Response<GameAuthKey> authkeyResponse = await bindingClient2.GenerateAuthenticationKeyAsync(userAndUid.User, data).ConfigureAwait(false);

            if (authkeyResponse.IsOk())
            {
                return new(true, new(GachaLogQueryOptions.ToQueryString(data, authkeyResponse.Data, metadataOptions.LanguageCode)));
            }
            else
            {
                return new(false, SH.ServiceGachaLogUrlProviderAuthkeyRequestFailed);
            }
        }
        else
        {
            return new(false, SH.MustSelectUserAndUid);
        }
    }
}