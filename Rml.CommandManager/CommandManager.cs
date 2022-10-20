using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rml.CommandManager;

public delegate ValueTask<ReadOnlyMemory<byte>> UndoRedoDelegate(in ReadOnlySpan<byte> paramBin);

public readonly record struct CommandExecutor(UndoRedoDelegate Undo, UndoRedoDelegate Redo)
{
    public readonly UndoRedoDelegate Undo = Undo;
    public readonly UndoRedoDelegate Redo = Redo;
}

public interface IExecuteCommand<in TTarget, TParam>
{
    public ValueTask<TParam> Execute(TTarget target);
}

public interface IUndoRedoCommand<in TTarget, TParam>
{
    public ValueTask<TParam> Undo(TTarget target);
    public ValueTask<TParam> Redo(TTarget target);
}

public interface ICommand
{
    public ushort Uid { get; init; }
    public ReadOnlyMemory<byte> UndoParamBin { get; init; }
    public ReadOnlyMemory<byte> RedoParamBin { get; init; }
}

public interface ICommandManagerSerializer
{
    public ReadOnlyMemory<byte> Serialize<T>(in T value);
    public T? Deserialize<T>(in ReadOnlySpan<byte> bin);
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public class CommandManagerCommandAttribute : Attribute
{
    public ushort Uid { get; }

    public CommandManagerCommandAttribute(ushort uid)
    {
        Uid = uid;
    }
}

public static class CommandManagerCommandAttributeGetter<T>
{
    // ReSharper disable once StaticMemberInGenericType
    public static readonly CommandManagerCommandAttribute? CommandManagerCommandAttribute;
    
    static CommandManagerCommandAttributeGetter()
    {
        CommandManagerCommandAttribute = Attribute.GetCustomAttribute(typeof(T), typeof(CommandManagerCommandAttribute)) as CommandManagerCommandAttribute;
    }
}

public class CommandManager<TCommand>
where TCommand : ICommand, new()
{
    private readonly ICommandManagerSerializer _commandManagerSerializer;
    
    private readonly Dictionary<ushort, CommandExecutor> _commandExecutors = new Dictionary<ushort, CommandExecutor>();
    private readonly Stack<TCommand> _undo = new Stack<TCommand>();
    private readonly Stack<TCommand> _redo = new Stack<TCommand>();
    
    public IEnumerable<TCommand> UndoCommands => _undo;
    public IEnumerable<TCommand> RedoCommands => _redo;

    public CommandManager(ICommandManagerSerializer commandManagerSerializer)
    {
        _commandManagerSerializer = commandManagerSerializer;
    }

    public void RegisterCommand<T>(Func<T, ValueTask<T>> undo, Func<T, ValueTask<T>> redo)
    {
        var commandAttribute = CommandManagerCommandAttributeGetter<T>.CommandManagerCommandAttribute;
        if (commandAttribute is null)
            throw new ArgumentException($"{nameof(T)}に{nameof(CommandManagerCommandAttribute)}を設定してください");

        ValueTask<ReadOnlyMemory<byte>> UndoDelegate(in ReadOnlySpan<byte> paramBin)
        {
            return Impl(_commandManagerSerializer.Deserialize<T>(paramBin) ?? throw new InvalidOperationException());

            async ValueTask<ReadOnlyMemory<byte>> Impl(T param)
            {
                var redoParam = await undo(param);
                return _commandManagerSerializer.Serialize(redoParam);
            }
        }

        ValueTask<ReadOnlyMemory<byte>> RedoDelegate(in ReadOnlySpan<byte> paramBin)
        {
            return Impl(_commandManagerSerializer.Deserialize<T>(paramBin) ?? throw new InvalidOperationException());

            async ValueTask<ReadOnlyMemory<byte>> Impl(T param)
            {
                var undoParam = await redo(param);
                return _commandManagerSerializer.Serialize(undoParam);
            }
        }

        _commandExecutors.Add(commandAttribute.Uid, new CommandExecutor(UndoDelegate, RedoDelegate));
    }

    public void RegisterCommand<T>(Func<T, T> undo, Func<T, T> redo)
    {
        var commandAttribute = CommandManagerCommandAttributeGetter<T>.CommandManagerCommandAttribute;
        if (commandAttribute is null)
            throw new ArgumentException($"{nameof(T)}に{nameof(CommandManagerCommandAttribute)}を設定してください");

        ValueTask<ReadOnlyMemory<byte>> UndoDelegate(in ReadOnlySpan<byte> param)
        {
            var redoParam = undo(_commandManagerSerializer.Deserialize<T>(param) ?? throw new InvalidOperationException());
            return ValueTask.FromResult(_commandManagerSerializer.Serialize(redoParam));
        }
        
        ValueTask<ReadOnlyMemory<byte>> RedoDelegate(in ReadOnlySpan<byte> param)
        {
            var undoParam = redo(_commandManagerSerializer.Deserialize<T>(param) ?? throw new InvalidOperationException());
            return ValueTask.FromResult(_commandManagerSerializer.Serialize(undoParam));
        }

        _commandExecutors.Add(commandAttribute.Uid, new CommandExecutor(UndoDelegate, RedoDelegate));
    }

    public void RegisterCommand<T>(Func<T, ValueTask<T>> undoRedo)
    {
        RegisterCommand(undoRedo, undoRedo);
    }

    public void RegisterCommand<T>(Func<T, T> undoRedo)
    {
        RegisterCommand(undoRedo, undoRedo);
    }

    public void RegisterExecuteCommand<T, TTarget>(Func<TTarget> getTargetFunc)
        where T : IExecuteCommand<TTarget, T>
    {
        RegisterCommand<T>(o => o.Execute(getTargetFunc()));
    }
    
    public void RegisterUndoRedoCommand<T, TTarget>(Func<TTarget> getTargetFunc)
        where T : IUndoRedoCommand<TTarget, T>
    {
        RegisterCommand<T>(o => o.Undo(getTargetFunc()), o => o.Redo(getTargetFunc()));
    }

    public ValueTask Execute<T>(T param)
    {
        var commandAttribute = CommandManagerCommandAttributeGetter<T>.CommandManagerCommandAttribute;
        if (commandAttribute is null)
            throw new ArgumentException($"{nameof(T)}に{nameof(CommandManagerCommandAttribute)}を設定してください");
        
        var command = new TCommand
        {
            Uid = commandAttribute.Uid,
            UndoParamBin = null,
            RedoParamBin = _commandManagerSerializer.Serialize(param),
        };
        
        _redo.Clear();
        
        return Redo(command);
    }


    public async ValueTask ExecuteCommands(IEnumerable<TCommand> commands)
    {
        _redo.Clear();
        
        foreach (var command in commands)
        {
            await Redo(command);
        }
    }

    public async ValueTask Undo(TCommand command)
    {
        var redoParamBin = await _commandExecutors[command.Uid].Undo(command.UndoParamBin.Span);

        command = new TCommand
        {
            Uid = command.Uid,
            UndoParamBin = command.UndoParamBin,
            RedoParamBin = redoParamBin,
        };
        
        _redo.Push(command);
    }

    public ValueTask Undo()
    {
        return Undo(_undo.Pop());
    }
    
    public async ValueTask Redo(TCommand command)
    {
        var undoParamBin = await _commandExecutors[command.Uid].Redo(command.RedoParamBin.Span);

        command = new TCommand
        {
            Uid = command.Uid,
            UndoParamBin = undoParamBin,
            RedoParamBin = command.RedoParamBin,
        };

        _undo.Push(command);
    }

    public ValueTask Redo()
    {
        return Redo(_redo.Pop());
    }

    public void ClearCommands()
    {
        _undo.Clear();
        _redo.Clear();
    }

    public ReadOnlyMemory<byte> SerializeCommands(TCommand[] commands)
    {
        return _commandManagerSerializer.Serialize(commands);
    }

    public TCommand[]? DeserializeCommands(in ReadOnlySpan<byte> bin)
    {
        return _commandManagerSerializer.Deserialize<TCommand[]>(bin);
    }
}