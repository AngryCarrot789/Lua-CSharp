using System.Numerics;

namespace Lua.Standard;

public static class Vector4Library 
{
    public static readonly LuaFunction[] Functions;

    static Vector4Library() 
    {
        Functions = new[] 
        {
            new LuaFunction("add", Add),
            new LuaFunction("sub", Sub),
            new LuaFunction("dot", Dot),
            new LuaFunction("mag", Magnitude),
            new LuaFunction("mag2", MagnitudeSquared),
            new LuaFunction("normalize", Normalize),
            new LuaFunction("scale", Scale),
            new LuaFunction("lerp", Lerp),
        };
    }

    public static Vector4 ReadVector4Argument(in LuaFunctionExecutionContext context, int index) 
    {
        LuaTable arg = context.GetArgument<LuaTable>(index);
        int count = arg.ArrayLength;
        if (count != 4)
            LuaRuntimeException.BadArgument(context.State.GetTraceback(), index, context.Thread.GetCurrentFrame().Function.Name, "Invalid vec4. Four numeric elements required");

        Span<LuaValue> s = arg.GetArraySpan();
        return new Vector4((float) s[0].UnsafeReadDouble(), (float) s[1].UnsafeReadDouble(), (float) s[2].UnsafeReadDouble(), (float) s[3].UnsafeReadDouble());
    }

    public static LuaTable CreateTableFromVector4(Vector4 v) 
    {
        LuaTable t = new LuaTable(4, 0);
        Span<LuaValue> values = t.GetArraySpan();
        values[0] = v.X;
        values[1] = v.Y;
        values[2] = v.Z;
        values[3] = v.W;
        return t;
    }

    public static ValueTask<int> Add(LuaFunctionExecutionContext ctx, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        Vector4 a = ReadVector4Argument(in ctx, 0);
        Vector4 b = ReadVector4Argument(in ctx, 1);
        buffer.Span[0] = CreateTableFromVector4(a + b);
        return new ValueTask<int>(1);
    }

    public static ValueTask<int> Sub(LuaFunctionExecutionContext ctx, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        Vector4 a = ReadVector4Argument(in ctx, 0);
        Vector4 b = ReadVector4Argument(in ctx, 1);
        buffer.Span[0] = CreateTableFromVector4(a - b);
        return new ValueTask<int>(1);
    }

    public static ValueTask<int> Dot(LuaFunctionExecutionContext ctx, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        Vector4 a = ReadVector4Argument(in ctx, 0);
        Vector4 b = ReadVector4Argument(in ctx, 1);
        buffer.Span[0] = Vector4.Dot(a, b);
        return new ValueTask<int>(1);
    }

    public static ValueTask<int> Magnitude(LuaFunctionExecutionContext ctx, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        Vector4 v = ReadVector4Argument(in ctx, 0);
        buffer.Span[0] = v.Length();
        return new ValueTask<int>(1);
    }
    
    public static ValueTask<int> MagnitudeSquared(LuaFunctionExecutionContext ctx, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        Vector4 v = ReadVector4Argument(in ctx, 0);
        buffer.Span[0] = v.LengthSquared();
        return new ValueTask<int>(1);
    }

    public static ValueTask<int> Normalize(LuaFunctionExecutionContext ctx, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        Vector4 v = ReadVector4Argument(in ctx, 0);
        buffer.Span[0] = CreateTableFromVector4(Vector4.Normalize(v));
        return new ValueTask<int>(1);
    }

    public static ValueTask<int> Scale(LuaFunctionExecutionContext ctx, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        Vector4 v = ReadVector4Argument(in ctx, 0);
        float s = (float) ctx.GetArgument<double>(1);
        buffer.Span[0] = CreateTableFromVector4(v * s);
        return new ValueTask<int>(1);
    }

    public static ValueTask<int> Lerp(LuaFunctionExecutionContext ctx, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        Vector4 a = ReadVector4Argument(in ctx, 0);
        Vector4 b = ReadVector4Argument(in ctx, 1);
        float lerp = (float) ctx.GetArgument<double>(2);
        buffer.Span[0] = CreateTableFromVector4(Vector4.Lerp(a, b, lerp));
        return new ValueTask<int>(1);
    }
}