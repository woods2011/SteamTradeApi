using OneOf;

namespace SteamClientTestPolygonWebApi.Helpers.Extensions;

public static class OneOfExtensions
{
    public static bool TryPickResult<T0, T1>(this OneOfBase<T0, T1> oneOf, out T0 resultT0, out T1 remainder) 
        => oneOf.TryPickT0(out resultT0, out remainder);
    
    public static bool TryPickResult<T0, T1, T2>(
        this OneOfBase<T0, T1, T2> oneOf,
        out T0 resultT0,
        out OneOf<T1, T2> remainder) 
        => oneOf.TryPickT0(out resultT0, out remainder);
    
    public static bool TryPickResult<T0, T1, T2, T3>(
        this OneOfBase<T0, T1, T2, T3> oneOf,
        out T0 resultT0,
        out OneOf<T1, T2, T3> remainder) 
        => oneOf.TryPickT0(out resultT0, out remainder);

    public static bool TryPickResult<T0, T1, T2, T3, T4>(
        this OneOfBase<T0, T1, T2, T3, T4> oneOf,
        out T0 resultT0,
        out OneOf<T1, T2, T3, T4> remainder) 
        => oneOf.TryPickT0(out resultT0, out remainder);
    
    public static bool TryPickResult<T0, T1, T2, T3, T4, T5>(
        this OneOfBase<T0, T1, T2, T3, T4, T5> oneOf,
        out T0 resultT0,
        out OneOf<T1, T2, T3, T4, T5> remainder) 
        => oneOf.TryPickT0(out resultT0, out remainder);
}