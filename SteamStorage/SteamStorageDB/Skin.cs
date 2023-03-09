using System;
using System.Collections.Generic;

namespace SteamStorage.SteamStorageDB;

public partial class Skin
{
    public long Id { get; set; }

    public string Title { get; set; } = null!;

    public string Url { get; set; } = null!;

    public virtual ICollection<Archive> Archives { get; } = new List<Archive>();

    public virtual ICollection<Remain> Remains { get; } = new List<Remain>();
}
