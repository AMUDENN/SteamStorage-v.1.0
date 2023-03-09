using System;
using System.Collections.Generic;

namespace SteamStorage.SteamStorageDB;

public partial class PriceDynamic
{
    public long Id { get; set; }

    public long IdRemain { get; set; }

    public string DateUpdate { get; set; } = null!;

    public double CostUpdate { get; set; }

    public virtual Remain IdRemainNavigation { get; set; } = null!;
}
