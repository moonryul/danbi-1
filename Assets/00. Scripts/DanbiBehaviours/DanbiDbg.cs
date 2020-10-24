using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    public static class DanbiDbg
    {
        public static ComputeBuffer DbgBuf_direct;

        public static void PrepareDbgBuffers()
        {
            DbgBuf_direct = new ComputeBuffer(14, 4);
        }
    };
};
