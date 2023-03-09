using System;
using System.Collections.Generic;

namespace SteamStorage.SteamStorageDB;

public partial class Archive
{
    public long Id { get; set; }

    public long IdSkin { get; set; }

    public long IdGroup { get; set; }

    public string DatePurchase { get; set; } = null!;

    public string DateSold { get; set; } = null!;

    public long Count { get; set; }

    public double CostPurchase { get; set; }

    public double CostSold { get; set; }

    public virtual ArchiveGroup IdGroupNavigation { get; set; } = null!;

    public virtual Skin IdSkinNavigation { get; set; } = null!;
}
