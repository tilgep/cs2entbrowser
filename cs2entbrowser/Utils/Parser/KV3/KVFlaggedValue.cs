using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs2entbrowser.Utils.Parser.KV3;

public enum KVFlag
{
    None = 0,
    Resource = 1,
    ResourceName = 2,
    Panorama = 3,
    SoundEvent = 4,
    SubClass = 5,
    // March 2023: There are more types available in the S2 binaries, but they should not be persisted.
}

public class KVFlaggedValue : KVValue
{
    public KVFlag Flag { get; private set; }

    public KVFlaggedValue(KVType type, object value)
        : base(type, value)
    {
        Flag = KVFlag.None;
    }

    public KVFlaggedValue(KVType type, KVFlag flag, object value)
        : base(type, value)
    {
        Flag = flag;
    }
}
