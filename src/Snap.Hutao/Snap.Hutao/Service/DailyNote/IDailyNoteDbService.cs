﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Model.Entity;

namespace Snap.Hutao.Service.DailyNote;

internal interface IDailyNoteDbService
{
    ValueTask AddDailyNoteEntryAsync(DailyNoteEntry entry);

    bool ContainsUid(string uid);

    ValueTask DeleteDailyNoteEntryByIdAsync(Guid entryId);

    List<DailyNoteEntry> GetDailyNoteEntryIncludeUserList();

    List<DailyNoteEntry> GetDailyNoteEntryList();

    ValueTask UpdateDailyNoteEntryAsync(DailyNoteEntry entry);
}