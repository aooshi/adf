using System;
using System.Collections.Generic;
using System.Text;

namespace Adf
{
    /// <summary>
    /// 带返回值的调用
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public delegate TResult CFunc<out TResult>();
    /// <summary>
    /// 带返回值的调用
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="arg"></param>
    /// <returns></returns>
    public delegate TResult CFunc<in T, out TResult>(T arg);
    /// <summary>
    /// 带返回值的调用
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <returns></returns>
    public delegate TResult CFunc<in T1, in T2, out TResult>(T1 t1, T2 t2);
    /// <summary>
    /// 带返回值的调用
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <param name="t3"></param>
    /// <returns></returns>
    public delegate TResult CFunc<in T1, in T2, in T3, out TResult>(T1 t1, T2 t2,T3 t3);
}
