using System.Numerics;

namespace Lua.Standard;

public static class Vector3Library 
{
    public static readonly LuaFunction[] Functions;

    static Vector3Library() 
    {
        Functions = new[] 
        {
            new LuaFunction("add", Add),
            new LuaFunction("sub", Sub),
            new LuaFunction("dot", Dot),
            new LuaFunction("cross", CrossProduce),
            new LuaFunction("mag", Magnitude),
            new LuaFunction("mag2", MagnitudeSquared),
            new LuaFunction("normalize", Normalize),
            new LuaFunction("scale", Scale),
            new LuaFunction("lerp", Lerp),
            new LuaFunction("distance", Distance),
        };
    }

    public static Vector3 ReadVector3Argument(in LuaFunctionExecutionContext context, int index) 
    {
        LuaTable arg = context.GetArgument<LuaTable>(index);
        int count = arg.ArrayLength;
        if (count != 3)
            LuaRuntimeException.BadArgument(context.State.GetTraceback(), index, context.Thread.GetCurrentFrame().Function.Name, "Invalid vec3. Three numeric elements required");

        Span<LuaValue> s = arg.GetArraySpan();
        return new Vector3((float) s[0].UnsafeReadDouble(), (float) s[1].UnsafeReadDouble(), (float) s[2].UnsafeReadDouble());
    }

    public static LuaTable CreateTableFromVector3(Vector3 v) 
    {
        LuaTable t = new LuaTable(3, 0);
        Span<LuaValue> values = t.GetArraySpan();
        values[0] = v.X;
        values[1] = v.Y;
        values[2] = v.Z;
        return t;
    }

    public static ValueTask<int> Add(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        Vector3 a = ReadVector3Argument(in context, 0);
        Vector3 b = ReadVector3Argument(in context, 1);
        buffer.Span[0] = CreateTableFromVector3(a + b);
        return new ValueTask<int>(1);
    }

    public static ValueTask<int> Sub(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        Vector3 a = ReadVector3Argument(in context, 0);
        Vector3 b = ReadVector3Argument(in context, 1);
        buffer.Span[0] = CreateTableFromVector3(a - b);
        return new ValueTask<int>(1);
    }

    public static ValueTask<int> Dot(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        Vector3 a = ReadVector3Argument(in context, 0);
        Vector3 b = ReadVector3Argument(in context, 1);
        buffer.Span[0] = Vector3.Dot(a, b);
        return new ValueTask<int>(1);
    }

    public static ValueTask<int> CrossProduce(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        Vector3 a = ReadVector3Argument(in context, 0);
        Vector3 b = ReadVector3Argument(in context, 1);
        buffer.Span[0] = CreateTableFromVector3(Vector3.Cross(a, b));
        return new ValueTask<int>(1);
    }

    public static ValueTask<int> Magnitude(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        Vector3 v = ReadVector3Argument(in context, 0);
        buffer.Span[0] = v.Length();
        return new ValueTask<int>(1);
    }
    
    public static ValueTask<int> MagnitudeSquared(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        Vector3 v = ReadVector3Argument(in context, 0);
        buffer.Span[0] = v.LengthSquared();
        return new ValueTask<int>(1);
    }

    public static ValueTask<int> Normalize(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        Vector3 v = ReadVector3Argument(in context, 0);
        buffer.Span[0] = CreateTableFromVector3(Vector3.Normalize(v));
        return new ValueTask<int>(1);
    }

    public static ValueTask<int> Scale(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        Vector3 v = ReadVector3Argument(in context, 0);
        float s = (float) context.GetArgument<double>(1);
        buffer.Span[0] = CreateTableFromVector3(v * s);
        return new ValueTask<int>(1);
    }

    public static ValueTask<int> Lerp(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        Vector3 a = ReadVector3Argument(in context, 0);
        Vector3 b = ReadVector3Argument(in context, 1);
        float lerp = (float) context.GetArgument<double>(2);
        buffer.Span[0] = CreateTableFromVector3(Vector3.Lerp(a, b, lerp));
        return new ValueTask<int>(1);
    }

    public static ValueTask<int> Distance(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        Vector3 a = ReadVector3Argument(in context, 0);
        Vector3 b = ReadVector3Argument(in context, 1);
        buffer.Span[0] = Vector3.Distance(a, b);
        return new ValueTask<int>(1);
    }
}