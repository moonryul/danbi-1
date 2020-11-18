using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ComputeBuffersDict = System.Collections.Generic.Dictionary<string, UnityEngine.ComputeBuffer>;

namespace Danbi
{
    public class DanbiComputeShaderBufferDictionary
    {
        ComputeBuffersDict m_buffersDict = new ComputeBuffersDict();

        public void AddBuffer_NoOverlap(string key, ComputeBuffer buf)
        {
            if (m_buffersDict.ContainsKey(key))
            {
                m_buffersDict.Remove(key);
            }
            m_buffersDict.Add(key, buf);
        }

        public ComputeBuffer GetBuffer(string key)
        {
            return m_buffersDict[key];
        }

        public void ClearBuffer()
        {
            m_buffersDict.Clear();
        }
    };
};
