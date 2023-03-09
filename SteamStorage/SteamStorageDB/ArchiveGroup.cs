using System;
using System.Collections.Generic;

namespace SteamStorage.SteamStorageDB;

public partial class ArchiveGroup
{
    public long Id { get; set; }

    public string Title { get; set; } = null!;

    public virtual ICollection<Archive> Archives { get; } = new List<Archive>();
}
