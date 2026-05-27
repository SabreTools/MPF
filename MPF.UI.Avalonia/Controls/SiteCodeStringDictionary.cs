// MPF cross-platform (Avalonia) UI — contributed by Knutwurst (https://github.com/knutwurst)
using System;
using System.Collections.Generic;
using SabreTools.RedumpLib.Data;

namespace MPF.UI.Avalonia.Controls
{
    /// <summary>
    /// String-keyed view over a Dictionary&lt;SiteCode, string&gt; so Avalonia bindings can use
    /// [EnumMemberName] indexer keys (Avalonia does not support WPF's (ns:Type)Member cast syntax).
    /// Re-fetches the backing dictionary on each access via the provided accessor, so it stays
    /// correct if the ViewModel replaces the dictionary instance during Load().
    /// </summary>
    public sealed class SiteCodeStringDictionary
    {
        private readonly Func<IDictionary<SiteCode, string>?> _accessor;

        public SiteCodeStringDictionary(Func<IDictionary<SiteCode, string>?> accessor) => _accessor = accessor;

        public string? this[string key]
        {
            get
            {
                var dict = _accessor();
                if (dict is null || !Enum.TryParse<SiteCode>(key, out var sc)) return null;
                return dict.TryGetValue(sc, out var v) ? v : null;
            }
            set
            {
                var dict = _accessor();
                if (dict is null || !Enum.TryParse<SiteCode>(key, out var sc)) return;
                dict[sc] = value ?? string.Empty;
            }
        }
    }
}
