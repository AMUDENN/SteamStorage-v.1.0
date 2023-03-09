using System;
using System.Collections.Generic;

namespace SteamStorage.SteamStorageDB;

public partial class Remain
{
    public long Id { get; set; }

    public long IdSkin { get; set; }

    public long IdGroup { get; set; }

    public string DatePurchase { get; set; } = null!;

    public double CostPurchase { get; set; }

    public long Count { get; set; }

    public virtual RemainGroup IdGroupNavigation { get; set; } = null!;

    public virtual Skin IdSkinNavigation { get; set; } = null!;

    public virtual ICollection<PriceDynamic> PriceDynamics { get; } = new List<PriceDynamic>();
}
