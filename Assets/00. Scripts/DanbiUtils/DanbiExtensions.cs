using UnityEngine;
public static class DanbiExtensions
{
    /// <summary>
    /// Check is Null.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool Null(this Object obj)
    {
        return obj is null || obj == null;
    }

    /// <summary>
    /// Check is Null and if it's so, execute the expression.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="expr"></param>
    /// <returns></returns>
    public static bool NullFinally(this Object obj, System.Action expr)
    {
        bool res = Null(obj);
        if (res)
        {
            expr.Invoke();
        }
        return res;
    }

    /// <summary>
    /// Different name of Null(..)
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool Assigned(this Object obj)
    {
        return obj is null || obj == null;
    }
    /// <summary>
    /// Different name of NullFinally(..)
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="expr"></param>
    /// <returns></returns>
    public static bool AssignedFinally(this Object obj, System.Action expr)
    {
        bool res = Assigned(obj);
        if (res)
        {
            expr.Invoke();
        }
        return res;
    }

    /// <summary>
    /// Check is null only for ComputeShader
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool Null(this ComputeBuffer obj)
    {
        return ReferenceEquals(obj, null);
    }

    /// <summary>
    /// Check is null and if it's so, execute the expr and it's only for ComputeShader
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="expr"></param>
    /// <returns></returns>
    public static bool NullFinally(this ComputeBuffer obj, System.Action expr)
    {
        bool res = Null(obj);
        if (res)
        {
            expr.Invoke();
        }
        return res;
    }
};