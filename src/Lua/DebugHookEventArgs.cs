using Lua.CodeAnalysis;
using Lua.Runtime;

namespace Lua;

public readonly struct DebugHookEventArgs 
{
    public readonly CallStackFrame CallStackFrame;
    public readonly LuaState State;
    public readonly LuaStack Stack;
    public readonly LuaThread Thread;
    public readonly int Pc;
    public readonly Instruction Instruction;
    // public PostOperationType PostOperation;

    public Chunk? ClosureChunk => (CallStackFrame.Function as LuaClosure)?.Proto;
    public int FrameBase => CallStackFrame.Base;
    public int VariableArgumentCount => CallStackFrame.VariableArgumentCount;

    /// <summary>
    /// Attempt to get the info for a method call instruction
    /// </summary>
    public (LuaFunction Function, bool IsMetaMethod)? CallInfo 
    {
        get 
        {
            int RA = Instruction.A + FrameBase;
            LuaValue va = Stack.Get(RA);
            if (va.TryReadFunction(out LuaFunction func))
                return (func, false);

            if (va.TryGetMetamethod(State, Metamethods.Call, out LuaValue metamethod) && metamethod.TryReadFunction(out func))
                return (func, true);

            return null;
        }
    }

    /// <summary>
    /// Attempt to get the source position of the current program counter (<see cref="Pc"/>). Returns <c>default</c> if failed 
    /// </summary>
    public SourcePosition SourcePosition 
    {
        get 
        {
            if (Pc >= 0 && !CallStackFrame.IsTailCall && CallStackFrame.Function is LuaClosure closure) 
            {
                Chunk p = closure.Proto;
                return Pc < p.SourcePositions.Length 
                    ? p.SourcePositions[Pc] 
                    : default;
            }

            return default;
        }
    }

    internal DebugHookEventArgs(ref LuaVirtualMachine.VirtualMachineExecutionContext context, Instruction instruction) 
    {
        Thread = context.Thread;
        CallStackFrame = context.Thread.GetCurrentFrame();
        State = context.State;
        Stack = context.Stack;
        Pc = context.Pc;
        Instruction = instruction;
    }
}