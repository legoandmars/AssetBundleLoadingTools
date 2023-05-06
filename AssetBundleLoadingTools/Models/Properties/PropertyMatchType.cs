using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBundleLoadingTools.Models.Properties
{
    public enum PropertyMatchType
    {
        NoMatch,
        PartialMatch, // name matches; properties don't
        FullMatch,
    }
}
