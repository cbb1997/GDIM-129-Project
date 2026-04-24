using UnityEngine;
using System;

[System.Serializable]
public class VisualDictionary<T1, T2>
{
    [SerializeField] private VisualPair<T1, T2>[] m_Buckets;

    public VisualDictionary(int bucketCount)
    { 
        m_Buckets = new VisualPair<T1, T2>[bucketCount];
    }

    private int GetIndex(T1 key)
    {
        return new VisualPair<T1, T2>(key).Hash() % m_Buckets.Length;
    }

    public T2 Get(T1 key)
    {
        return m_Buckets[GetIndex(key)].Value;
    }

    public void Set(T1 key, T2 value)
    {
        m_Buckets[GetIndex(key)].Value = value;
    }
}

[System.Serializable]
public class VisualPair<T1, T2> 
{
    [SerializeField] private T1 m_Key;
    public T1 Key { get { return m_Key; } }

    [SerializeField] private T2 m_Value;
    public T2 Value { get { return m_Value; } set { m_Value = value; } }

    public VisualPair(T1 key, T2 value)
    {
        m_Key = key;
        m_Value = value;
    }

    public VisualPair(T1 key)
    {
        m_Key = key;
    }

    public int Hash()
    {
        return Math.Abs(m_Key.GetHashCode());
    }
}


