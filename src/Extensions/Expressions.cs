using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;

namespace Extensions;

public static class Expressions
{
    public static Func<T, T, T> Add<T>() where T : IAdditionOperators<T, T, T> => (a, b) => a + b;

    public static Func<T, T, T> Subtract<T>() where T : ISubtractionOperators<T, T, T> => (a, b) => a - b;

    public static Func<T, T, T> Multiply<T>() where T : IMultiplyOperators<T, T, T> => (a, b) => a * b;

    public static Func<T, T, T> Divide<T>() where T : IDivisionOperators<T, T, T> => (a, b) => a / b;

    public static Func<T, T, T> Modulo<T>() where T : IModulusOperators<T, T, T> => (a, b) => a % b;

    public static Func<T, T, T> LeftShift<T>() where T : IShiftOperators<T, T, T> => (a, b) => a << b;

    public static Func<T, T, T> RightShift<T>() where T : IShiftOperators<T, T, T> => (a, b) => a >> b;

    public static Func<T, R> GetProperty<T, R>(string name) => GetMethod<T, R>(typeof(T).GetProperty(name)!.GetMethod!);

    public static Func<T, R> GetMethod<T, R>(string name) => GetMethod<T, R>(typeof(T).GetMethod(name)!);

    public static Func<T, R> GetMethod<T, R>(MethodInfo method)
    {
        var param = Expression.Parameter(typeof(T));
        var call = Expression.Call(param, method);
        return Expression.Lambda<Func<T, R>>(
                method.ReturnParameter.ParameterType == typeof(R) ? call : Expression.Convert(call, typeof(R)),
                param
            ).Compile();
    }
}
