using System.Diagnostics;
using System.Numerics;

namespace Lua.Standard;

public static class Matrix4x4Library
{
    public static readonly LuaFunction[] Functions;

    static Matrix4x4Library() 
    {
        Functions = [
            new LuaFunction("identity", CreateIdentity),
            new LuaFunction("add", Add),
            new LuaFunction("sub", Subtract),
            new LuaFunction("mul", Multiply),
            new LuaFunction("transpose", Transpose),
            new LuaFunction("invert", Invert),
            new LuaFunction("make_orthographic", CreateOrthographic),
            new LuaFunction("make_perspective", CreatePerspective),
            new LuaFunction("make_ortho_off_center", CreateOrthographicOffCenter),
            new LuaFunction("make_perspective_off_center", CreatePerspectiveOffCenter),
            new LuaFunction("make_perspective_fov", CreatePerspectiveFieldOfView),
            new LuaFunction("make_reflection", CreateReflection),
            new LuaFunction("make_shadow", CreateShadow),
            new LuaFunction("make_world_matrix", CreateWorld),
            new LuaFunction("make_billboard", CreateConstrainedBillboard),
            new LuaFunction("make_look_at", CreateLookAt),
            new LuaFunction("make_translation", CreateTranslation),
            new LuaFunction("make_scale", CreateScale),
            new LuaFunction("make_rot_x", CreateRotationX),
            new LuaFunction("make_rot_y", CreateRotationY),
            new LuaFunction("make_rot_z", CreateRotationZ),
            new LuaFunction("from_axis_angle", CreateFromAxisAngle),
            new LuaFunction("from_yaw_pitch_roll", CreateFromYawPitchRoll)
        ];
    }

    public static Matrix4x4 ReadMatrixArgument(in LuaFunctionExecutionContext ctx, int index) 
    {
        LuaTable arg = ctx.GetArgument<LuaTable>(index);
        int count = arg.ArrayLength;
        if (count != 16)
            LuaRuntimeException.BadArgument(ctx.State.GetTraceback(), index, ctx.Thread.GetCurrentFrame().Function.Name, "Invalid mat4x4. 16 numeric elements required");

        return CreateMatrixFromTable(arg.GetArraySpan());
    }

    public static Matrix4x4 CreateMatrixFromTable(Span<LuaValue> v) 
    {
        Debug.Assert(v.Length == 16);

        // Trust that all values are doubles
        return new Matrix4x4(
            (float) v[0].UnsafeReadDouble(), (float) v[1].UnsafeReadDouble(), (float) v[2].UnsafeReadDouble(), (float) v[3].UnsafeReadDouble(),
            (float) v[4].UnsafeReadDouble(), (float) v[5].UnsafeReadDouble(), (float) v[6].UnsafeReadDouble(), (float) v[7].UnsafeReadDouble(),
            (float) v[8].UnsafeReadDouble(), (float) v[9].UnsafeReadDouble(), (float) v[10].UnsafeReadDouble(), (float) v[11].UnsafeReadDouble(),
            (float) v[12].UnsafeReadDouble(), (float) v[13].UnsafeReadDouble(), (float) v[14].UnsafeReadDouble(), (float) v[15].UnsafeReadDouble()
        );
    }

    public static LuaTable CreateTableFromMatrix(Matrix4x4 m) 
    {
        LuaTable table = new LuaTable(16, 0);
        Span<LuaValue> v = table.GetArraySpan();
        v[0] = m.M11;
        v[1] = m.M12;
        v[2] = m.M13;
        v[3] = m.M14;
        v[4] = m.M21;
        v[5] = m.M22;
        v[6] = m.M23;
        v[7] = m.M24;
        v[8] = m.M31;
        v[9] = m.M32;
        v[10] = m.M33;
        v[11] = m.M34;
        v[12] = m.M41;
        v[13] = m.M42;
        v[14] = m.M43;
        v[15] = m.M44;
        return table;
    }

    public static ValueTask<int> CreateIdentity(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        buffer.Span[0] = CreateTableFromMatrix(Matrix4x4.Identity);
        return new ValueTask<int>(1);
    }

    public static ValueTask<int> Add(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        Matrix4x4 a = ReadMatrixArgument(in context, 0);
        Matrix4x4 b = ReadMatrixArgument(in context, 1);
        buffer.Span[0] = CreateTableFromMatrix(a + b);
        return new ValueTask<int>(1);
    }

    public static ValueTask<int> Subtract(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        Matrix4x4 a = ReadMatrixArgument(in context, 0);
        Matrix4x4 b = ReadMatrixArgument(in context, 1);
        buffer.Span[0] = CreateTableFromMatrix(a - b);
        return new ValueTask<int>(1);
    }

    public static ValueTask<int> Multiply(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        Matrix4x4 matrix = ReadMatrixArgument(in context, 0);
        if (context.GetArgument(1).Type == LuaValueType.Table) 
        {
            Matrix4x4 b = ReadMatrixArgument(in context, 1);
            buffer.Span[0] = CreateTableFromMatrix(matrix * b);
        }
        else 
        {
            float s = (float) context.GetArgument<double>(1);
            buffer.Span[0] = CreateTableFromMatrix(matrix * s);
        }

        return new ValueTask<int>(1);
    }

    public static ValueTask<int> Transpose(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        Matrix4x4 matrix = ReadMatrixArgument(in context, 0);
        buffer.Span[0] = CreateTableFromMatrix(Matrix4x4.Transpose(matrix));
        return new ValueTask<int>(1);
    }

    public static ValueTask<int> Invert(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        Matrix4x4 matrix = ReadMatrixArgument(in context, 0);
        if (Matrix4x4.Invert(matrix, out Matrix4x4 result)) {
            buffer.Span[0] = CreateTableFromMatrix(result);
        }
        else {
            buffer.Span[0] = LuaValue.Nil;
        }

        return new ValueTask<int>(1);
    }

    public static ValueTask<int> CreateOrthographic(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        float width = (float) context.GetArgument<double>(0);
        float height = (float) context.GetArgument<double>(1);
        float near = (float) context.GetArgument<double>(2);
        float far = (float) context.GetArgument<double>(3);

        buffer.Span[0] = CreateTableFromMatrix(Matrix4x4.CreateOrthographic(width, height, near, far));
        return new ValueTask<int>(1);
    }

    public static ValueTask<int> CreatePerspective(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        float width = (float) context.GetArgument<double>(0);
        float height = (float) context.GetArgument<double>(1);
        float near = (float) context.GetArgument<double>(2);
        float far = (float) context.GetArgument<double>(3);

        buffer.Span[0] = CreateTableFromMatrix(Matrix4x4.CreatePerspective(width, height, near, far));
        return new ValueTask<int>(1);
    }
    
    public static ValueTask<int> CreateOrthographicOffCenter(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        float left = (float) context.GetArgument<double>(0);
        float right = (float) context.GetArgument<double>(1);
        float bottom = (float) context.GetArgument<double>(2);
        float top = (float) context.GetArgument<double>(3);
        float near = (float) context.GetArgument<double>(4);
        float far = (float) context.GetArgument<double>(5);

        buffer.Span[0] = CreateTableFromMatrix(Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, near, far));
        return new ValueTask<int>(1);
    }

    public static ValueTask<int> CreatePerspectiveOffCenter(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        float left = (float) context.GetArgument<double>(0);
        float right = (float) context.GetArgument<double>(1);
        float bottom = (float) context.GetArgument<double>(2);
        float top = (float) context.GetArgument<double>(3);
        float near = (float) context.GetArgument<double>(4);
        float far = (float) context.GetArgument<double>(5);

        buffer.Span[0] = CreateTableFromMatrix(Matrix4x4.CreatePerspectiveOffCenter(left, right, bottom, top, near, far));
        return new ValueTask<int>(1);
    }
    
    public static ValueTask<int> CreatePerspectiveFieldOfView(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        float fov = (float) context.GetArgument<double>(0);
        float aspect = (float) context.GetArgument<double>(1);
        float near = (float) context.GetArgument<double>(2);
        float far = (float) context.GetArgument<double>(3);

        buffer.Span[0] = CreateTableFromMatrix(Matrix4x4.CreatePerspectiveFieldOfView(fov, aspect, near, far));
        return new ValueTask<int>(1);
    }

    public static ValueTask<int> CreateReflection(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        Vector4 plane = Vector4Library.ReadVector4Argument(in context, 0);
        Matrix4x4 result = Matrix4x4.CreateReflection(new Plane(plane.X, plane.Y, plane.Z, plane.W));
        buffer.Span[0] = CreateTableFromMatrix(result);
        return new ValueTask<int>(1);
    }

    public static ValueTask<int> CreateShadow(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        Vector3 dir = Vector3Library.ReadVector3Argument(in context, 0);
        Vector4 plane = Vector4Library.ReadVector4Argument(in context, 1);

        Matrix4x4 result = Matrix4x4.CreateShadow(dir, new Plane(plane.X, plane.Y, plane.Z, plane.W));
        buffer.Span[0] = CreateTableFromMatrix(result);
        return new ValueTask<int>(1);
    }

    public static ValueTask<int> CreateWorld(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        Vector3 pos = Vector3Library.ReadVector3Argument(in context, 0);
        Vector3 fwd = Vector3Library.ReadVector3Argument(in context, 1);
        Vector3 up = Vector3Library.ReadVector3Argument(in context, 2);

        buffer.Span[0] = CreateTableFromMatrix(Matrix4x4.CreateWorld(pos, fwd, up));
        return new ValueTask<int>(1);
    }

    public static ValueTask<int> CreateConstrainedBillboard(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        Vector3 objPos = Vector3Library.ReadVector3Argument(in context, 0);
        Vector3 camPos = Vector3Library.ReadVector3Argument(in context, 1);
        Vector3 axis = Vector3Library.ReadVector3Argument(in context, 2);
        Vector3 camFwd = Vector3Library.ReadVector3Argument(in context, 3);
        Vector3 objFwd = Vector3Library.ReadVector3Argument(in context, 4);

        buffer.Span[0] = CreateTableFromMatrix(Matrix4x4.CreateConstrainedBillboard(objPos, camPos, axis, camFwd, objFwd));
        return new ValueTask<int>(1);
    }

    public static ValueTask<int> CreateLookAt(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        Vector3 src = Vector3Library.ReadVector3Argument(in context, 0);
        Vector3 dst = Vector3Library.ReadVector3Argument(in context, 1);
        Vector3 up = Vector3Library.ReadVector3Argument(in context, 2);

        buffer.Span[0] = CreateTableFromMatrix(Matrix4x4.CreateLookAt(src, dst, up));
        return new ValueTask<int>(1);
    }
    
    public static ValueTask<int> CreateTranslation(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        float x = (float) context.GetArgument<double>(0);
        float y = (float) context.GetArgument<double>(1);
        float z = (float) context.GetArgument<double>(2);

        buffer.Span[0] = CreateTableFromMatrix(Matrix4x4.CreateTranslation(x, y, z));
        return new ValueTask<int>(1);
    }
    
    public static ValueTask<int> CreateScale(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        float sx = (float) context.GetArgument<double>(0);
        float sy = (float) context.GetArgument<double>(1);
        float sz = (float) context.GetArgument<double>(2);

        Matrix4x4 result = Matrix4x4.CreateScale(sx, sy, sz);
        buffer.Span[0] = CreateTableFromMatrix(result);
        return new ValueTask<int>(1);
    }

    public static ValueTask<int> CreateRotationX(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        float rads = (float) context.GetArgument<double>(0);
        buffer.Span[0] = CreateTableFromMatrix(Matrix4x4.CreateRotationX(rads));
        return new ValueTask<int>(1);
    }

    public static ValueTask<int> CreateRotationY(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        float rads = (float) context.GetArgument<double>(0);
        buffer.Span[0] = CreateTableFromMatrix(Matrix4x4.CreateRotationY(rads));
        return new ValueTask<int>(1);
    }

    public static ValueTask<int> CreateRotationZ(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        float rads = (float) context.GetArgument<double>(0);
        buffer.Span[0] = CreateTableFromMatrix(Matrix4x4.CreateRotationZ(rads));
        return new ValueTask<int>(1);
    }

    public static ValueTask<int> CreateFromAxisAngle(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        Vector3 axis = Vector3Library.ReadVector3Argument(in context, 0);
        float angle = (float) context.GetArgument<double>(1);

        buffer.Span[0] = CreateTableFromMatrix(Matrix4x4.CreateFromAxisAngle(axis, angle));
        return new ValueTask<int>(1);
    }

    public static ValueTask<int> CreateFromYawPitchRoll(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken cancellationToken) 
    {
        float yaw = (float) context.GetArgument<double>(0);
        float pitch = (float) context.GetArgument<double>(1);
        float roll = (float) context.GetArgument<double>(2);

        buffer.Span[0] = CreateTableFromMatrix(Matrix4x4.CreateFromYawPitchRoll(yaw, pitch, roll));
        return new ValueTask<int>(1);
    }
}