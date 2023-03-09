using System;
using System.Collections.Generic;

namespace SteamStorage.SteamStorageDB;

public partial class RemainGroup
{
    public long Id { get; set; }

    public string Title { get; set; } = null!;

    public virtual ICollection<Remain> Remains { get; } = new List<Remain>();
}
