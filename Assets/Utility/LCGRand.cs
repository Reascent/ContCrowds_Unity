using UnityEngine;
using System.Collections;

public static class LCGRand {

    public static uint Next (uint a)
    {
        return (uint)(((ulong)a * 279470273uL)% 4294967291uL);
    }

    public static float NextF(uint a)
    {
        return (float)((((ulong)a * 279470273uL) % 4294967291uL) / 4294967291f);
    }

}
