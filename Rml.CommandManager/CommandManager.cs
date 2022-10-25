using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rml.CommandManager;

public delegate ValueTask<(ReadOnlyMemory<byte> undoParamBin, ReadOnlyMemory<byte> redoParamBin)> CreateUndoRedoParamDelegate(in ReadOnlySpan<byte> paramBin);
public delegate ValueTask<ReadOnlyMemory<byte>> UndoRedoDelegate(in ReadOnlySpan<byte> paramBin);

public readonly record struct CommandExecutor(CreateUndoRedoParamDelegate? CreateUndoRedoParam, UndoRedoDelegate Undo, UndoRedoDelegate Redo)
{
    public readonly CreateUndoRedoParamDelegate? CreateUndoRedoParam = CreateUndoRedoParam;
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

public interface ICreateUndoRedoParamCommand<in TTarget, TParam>
{
    public ValueTask<(TParam undoParam, TParam redoParam)> CreateUndoRedoParam(TTarget target);
    public ValueTask Undo(TTarget target);
    public ValueTask Redo(TTarget target);
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

public static class CommandManagerCommandCache<T>
{
    // ReSharper disable once StaticMemberInGenericType
    public static readonly CommandManagerCommandAttribute? CommandManagerCommandAttribute;

    // ReSharper disable once StaticMemberInGenericType
    public static readonly string? Name;
    
    static CommandManagerCommandCache()
    {
        var type = typeof(T);
        CommandManagerCommandAttribute = Attribute.GetCustomAttribute(type, typeof(CommandManagerCommandAttribute)) as CommandManagerCommandAttribute;
        Name = type.FullName;
    }
}

public class CommandManager<TCommand>
where TCommand : ICommand, new()
{
    private readonly ICommandManagerSerializer _commandManagerSerializer;
    
    private readonly Dictionary<ushort, CommandExecutor> _commandExecutors = new Dictionary<ushort, CommandExecutor>();
    private readonly Dictionary<ushort, string?> _commandNames = new Dictionary<ushort, string?>();
    private readonly Stack<TCommand> _undo = new Stack<TCommand>();
    private readonly Stack<TCommand> _redo = new Stack<TCommand>();
    
    public IEnumerable<TCommand> UndoCommands => _undo;
    public IEnumerable<TCommand> RedoCommands => _redo;

    public CommandManager(ICommandManagerSerializer commandManagerSerializer)
    {
        _commandManagerSerializer = commandManagerSerializer;
    }
    
    public void RegisterCommand<TExecuteParam, TUndoParam, TRedoParam>(
        Func<TExecuteParam, ValueTask<(TUndoParam undoParam, TRedoParam redoParam)>> createUndoRedoParam,
        Func<TUndoParam, ValueTask> undo,
        Func<TRedoParam, ValueTask> redo)
    {
        var commandAttribute = CommandManagerCommandCache<TExecuteParam>.CommandManagerCommandAttribute;
        if (commandAttribute is null)
            throw new ArgumentException($"{nameof(TExecuteParam)}に{nameof(CommandManagerCommandAttribute)}を設定してください");

        ValueTask<(ReadOnlyMemory<byte> undoParamBin, ReadOnlyMemory<byte> redoParamBin)> CreateUndoRedoParamDelegate(in ReadOnlySpan<byte> paramBin)
        {
            return Impl(_commandManagerSerializer.Deserialize<TExecuteParam>(paramBin) ?? throw new InvalidOperationException());

            async ValueTask<(ReadOnlyMemory<byte> undoParamBin, ReadOnlyMemory<byte> redoParamBin)> Impl(TExecuteParam param)
            {
                var undoRedoParam = await createUndoRedoParam(param);
                return (_commandManagerSerializer.Serialize(undoRedoParam.undoParam), _commandManagerSerializer.Serialize(undoRedoParam.redoParam));
            }
        }

        ValueTask<ReadOnlyMemory<byte>> UndoDelegate(in ReadOnlySpan<byte> paramBin)
        {
            return Impl(_commandManagerSerializer.Deserialize<TUndoParam>(paramBin) ?? throw new InvalidOperationException());

            async ValueTask<ReadOnlyMemory<byte>> Impl(TUndoParam param)
            {
                await undo(param);
                return ReadOnlyMemory<byte>.Empty;
            }
        }

        ValueTask<ReadOnlyMemory<byte>> RedoDelegate(in ReadOnlySpan<byte> paramBin)
        {
            return Impl(_commandManagerSerializer.Deserialize<TRedoParam>(paramBin) ?? throw new InvalidOperationException());

            async ValueTask<ReadOnlyMemory<byte>> Impl(TRedoParam param)
            {
                await redo(param);
                return ReadOnlyMemory<byte>.Empty;
            }
        }

        _commandExecutors.Add(commandAttribute.Uid, new CommandExecutor(CreateUndoRedoParamDelegate, UndoDelegate, RedoDelegate));
        _commandNames.Add(commandAttribute.Uid, CommandManagerCommandCache<TExecuteParam>.Name);
    }
    
    public void RegisterCommand<TExecuteParam, TUndoParam, TRedoParam>(
        Func<TExecuteParam, (TUndoParam undoParam, TRedoParam redoParam)> createUndoRedoParam,
        Action<TUndoParam> undo,
        Action<TRedoParam> redo)
    {
        var commandAttribute = CommandManagerCommandCache<TExecuteParam>.CommandManagerCommandAttribute;
        if (commandAttribute is null)
            throw new ArgumentException($"{nameof(TExecuteParam)}に{nameof(CommandManagerCommandAttribute)}を設定してください");

        ValueTask<(ReadOnlyMemory<byte> undoParamBin, ReadOnlyMemory<byte> redoParamBin)> CreateUndoRedoParamDelegate(in ReadOnlySpan<byte> paramBin)
        {
            return Impl(_commandManagerSerializer.Deserialize<TExecuteParam>(paramBin) ?? throw new InvalidOperationException());

            ValueTask<(ReadOnlyMemory<byte> undoParamBin, ReadOnlyMemory<byte> redoParamBin)> Impl(TExecuteParam param)
            {
                var undoRedoParam = createUndoRedoParam(param);
                return ValueTask.FromResult((_commandManagerSerializer.Serialize(undoRedoParam.undoParam), _commandManagerSerializer.Serialize(undoRedoParam.redoParam)));
            }
        }

        ValueTask<ReadOnlyMemory<byte>> UndoDelegate(in ReadOnlySpan<byte> paramBin)
        {
            return Impl(_commandManagerSerializer.Deserialize<TUndoParam>(paramBin) ?? throw new InvalidOperationException());

            ValueTask<ReadOnlyMemory<byte>> Impl(TUndoParam param)
            {
                undo(param);
                return ValueTask.FromResult(ReadOnlyMemory<byte>.Empty);
            }
        }

        ValueTask<ReadOnlyMemory<byte>> RedoDelegate(in ReadOnlySpan<byte> paramBin)
        {
            return Impl(_commandManagerSerializer.Deserialize<TRedoParam>(paramBin) ?? throw new InvalidOperationException());

            ValueTask<ReadOnlyMemory<byte>> Impl(TRedoParam param)
            {
                redo(param);
                return ValueTask.FromResult(ReadOnlyMemory<byte>.Empty);
            }
        }

        _commandExecutors.Add(commandAttribute.Uid, new CommandExecutor(CreateUndoRedoParamDelegate, UndoDelegate, RedoDelegate));
        _commandNames.Add(commandAttribute.Uid, CommandManagerCommandCache<TExecuteParam>.Name);
    }

    public void RegisterCommand<T>(Func<T, ValueTask<T>> undo, Func<T, ValueTask<T>> redo)
    {
        var commandAttribute = CommandManagerCommandCache<T>.CommandManagerCommandAttribute;
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

        _commandExecutors.Add(commandAttribute.Uid, new CommandExecutor(null, UndoDelegate, RedoDelegate));
        _commandNames.Add(commandAttribute.Uid, CommandManagerCommandCache<T>.Name);
    }

    public void RegisterCommand<T>(Func<T, T> undo, Func<T, T> redo)
    {
        var commandAttribute = CommandManagerCommandCache<T>.CommandManagerCommandAttribute;
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

        _commandExecutors.Add(commandAttribute.Uid, new CommandExecutor(null, UndoDelegate, RedoDelegate));
        _commandNames.Add(commandAttribute.Uid, CommandManagerCommandCache<T>.Name);
    }

    public void RegisterCommand<TExecuteParam, TUndoRedoParam>(
        Func<TExecuteParam, ValueTask<(TUndoRedoParam undoParam, TUndoRedoParam redoParam)>> createUndoRedoParam,
        Func<TUndoRedoParam, ValueTask> undoRedo)
    {
        RegisterCommand(createUndoRedoParam, undoRedo, undoRedo);
    }
    
    public void RegisterCommand<TExecuteParam, TUndoRedoParam>(
        Func<TExecuteParam, (TUndoRedoParam undoParam, TUndoRedoParam redoParam)> createUndoRedoParam,
        Action<TUndoRedoParam> undoRedo)
    {
        RegisterCommand(createUndoRedoParam, undoRedo, undoRedo);
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
    
    public void RegisterCreateUndoRedoParamCommand<T, TTarget>(Func<TTarget> getTargetFunc)
        where T : ICreateUndoRedoParamCommand<TTarget, T>
    {
        RegisterCommand<T, T, T>(o => o.CreateUndoRedoParam(getTargetFunc()), o => o.Undo(getTargetFunc()), o => o.Redo(getTargetFunc()));
    }

    public async ValueTask Execute<T>(T param)
    {
        var commandAttribute = CommandManagerCommandCache<T>.CommandManagerCommandAttribute;
        if (commandAttribute is null)
            throw new ArgumentException($"{nameof(T)}に{nameof(CommandManagerCommandAttribute)}を設定してください");
        
        var commandExecutor = _commandExecutors[commandAttribute.Uid];

        TCommand command;
        if (commandExecutor.CreateUndoRedoParam is not null)
        {
            var undoRedoParam =
                await commandExecutor.CreateUndoRedoParam(_commandManagerSerializer.Serialize(param).Span);

            command = new TCommand
            {
                Uid = commandAttribute.Uid,
                UndoParamBin = undoRedoParam.undoParamBin,
                RedoParamBin = undoRedoParam.redoParamBin,
            };
        }
        // CreateUndoRedoParamしない場合はRedoParamとして使用
        else
        {
            command = new TCommand
            {
                Uid = commandAttribute.Uid,
                UndoParamBin = null,
                RedoParamBin = _commandManagerSerializer.Serialize(param),
            };
        }

        _redo.Clear();
        
        await Redo(command);
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
            RedoParamBin = redoParamBin.IsEmpty ? command.RedoParamBin : redoParamBin,
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
            UndoParamBin = undoParamBin.IsEmpty ? command.UndoParamBin : undoParamBin,
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

    public string? GetCommandName(TCommand command)
    {
        return _commandNames[command.Uid];
    }
}