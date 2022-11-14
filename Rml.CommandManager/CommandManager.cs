using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
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
    public short Uid { get; init; }
    public ReadOnlyMemory<byte>? ParamBin { get; init; }
    public ReadOnlyMemory<byte>? UndoParamBin { get; init; }
    public ReadOnlyMemory<byte>? RedoParamBin { get; init; }
}

public interface ICommandManagerSerializer
{
    public ReadOnlyMemory<byte> Serialize<T>(in T value);
    public T? Deserialize<T>(in ReadOnlySpan<byte> bin);
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public class CommandManagerCommandAttribute : Attribute
{
    public short Uid { get; }

    public CommandManagerCommandAttribute(short uid)
    {
        if (uid < 0)
            throw new ArgumentException($"マイナスの{nameof(uid)}はシステムで予約済みです");
        
        Uid = uid;
    }

    public CommandManagerCommandAttribute(string unique)
    {
        Uid = CreateUid(unique);
    }

    public CommandManagerCommandAttribute()
    {
    }

    public static short CreateUid(string? unique)
    {
        if (unique is null)
            throw new ArgumentException($"{nameof(unique)} is null or empty");
        
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(unique));
        return BitConverter.ToInt16(hash, 0);
    }
}

public static class CommandManagerCommandCache<T>
{
    // ReSharper disable once StaticMemberInGenericType
    public static readonly short Uid;

    // ReSharper disable once StaticMemberInGenericType
    public static readonly string? Name;
    
    static CommandManagerCommandCache()
    {
        var type = typeof(T);
        var commandManagerCommandAttribute =
            Attribute.GetCustomAttribute(type,
                typeof(CommandManagerCommandAttribute)) as CommandManagerCommandAttribute;
        if (commandManagerCommandAttribute is null)
            throw new InvalidOperationException($"{typeof(CommandManagerCommandAttribute)} is null");

        Uid = commandManagerCommandAttribute.Uid is 0
            ? CommandManagerCommandAttribute.CreateUid(type.FullName)
            : commandManagerCommandAttribute.Uid;
        Name = type.FullName;
    }
}

public class CommandManager<TCommand>
where TCommand : ICommand, new()
{
    private const short UndoCommandUid = -1;
    private const short RedoCommandUid = -2;
    
    private readonly ICommandManagerSerializer _commandManagerSerializer;

    private readonly Dictionary<short, CommandExecutor> _commandExecutors = new Dictionary<short, CommandExecutor>
    {
        { UndoCommandUid, new CommandExecutor() },
        { RedoCommandUid, new CommandExecutor() },
    };

    private readonly Dictionary<short, string?> _commandNames = new Dictionary<short, string?>
    {
        { UndoCommandUid, "Undo" },
        { RedoCommandUid, "Redo" },
    };
    
    private readonly Queue<TCommand> _executed = new Queue<TCommand>();
    private readonly Stack<TCommand> _undo = new Stack<TCommand>();
    private readonly Stack<TCommand> _redo = new Stack<TCommand>();
    private readonly bool _isRecordExecuteCommand;
    
    public IEnumerable<TCommand> ExecutedCommands => _executed;
    public IEnumerable<TCommand> UndoCommands => _undo;
    public IEnumerable<TCommand> RedoCommands => _redo;

    public CommandManager(ICommandManagerSerializer commandManagerSerializer, bool isRecordExecuteCommand)
    {
        _commandManagerSerializer = commandManagerSerializer;
        _isRecordExecuteCommand = isRecordExecuteCommand;
    }
    
    public void RegisterCommand<TExecuteParam, TUndoParam, TRedoParam>(
        Func<TExecuteParam, ValueTask<(TUndoParam undoParam, TRedoParam redoParam)>> createUndoRedoParam,
        Func<TUndoParam, ValueTask> undo,
        Func<TRedoParam, ValueTask> redo)
    {
        var uid = CommandManagerCommandCache<TExecuteParam>.Uid;

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

        _commandExecutors.Add(uid, new CommandExecutor(CreateUndoRedoParamDelegate, UndoDelegate, RedoDelegate));
        _commandNames.Add(uid, CommandManagerCommandCache<TExecuteParam>.Name);
    }
    
    public void RegisterCommand<TExecuteParam, TUndoParam, TRedoParam>(
        Func<TExecuteParam, (TUndoParam undoParam, TRedoParam redoParam)> createUndoRedoParam,
        Action<TUndoParam> undo,
        Action<TRedoParam> redo)
    {
        var uid = CommandManagerCommandCache<TExecuteParam>.Uid;

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

        _commandExecutors.Add(uid, new CommandExecutor(CreateUndoRedoParamDelegate, UndoDelegate, RedoDelegate));
        _commandNames.Add(uid, CommandManagerCommandCache<TExecuteParam>.Name);
    }

    public void RegisterCommand<T>(Func<T, ValueTask<T>> undo, Func<T, ValueTask<T>> redo)
    {
        var uid = CommandManagerCommandCache<T>.Uid;

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

        _commandExecutors.Add(uid, new CommandExecutor(null, UndoDelegate, RedoDelegate));
        _commandNames.Add(uid, CommandManagerCommandCache<T>.Name);
    }

    public void RegisterCommand<T>(Func<T, T> undo, Func<T, T> redo)
    {
        var uid = CommandManagerCommandCache<T>.Uid;

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

        _commandExecutors.Add(uid, new CommandExecutor(null, UndoDelegate, RedoDelegate));
        _commandNames.Add(uid, CommandManagerCommandCache<T>.Name);
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

    public async ValueTask ExecuteCommand(TCommand command)
    {
        if (_isRecordExecuteCommand)
            _executed.Enqueue(command);
        
        // UndoRedoコマンドの場合
        switch (command.Uid)
        {
            case UndoCommandUid:
                await Undo(_undo.Pop());
                return;
            case RedoCommandUid:
                await Redo(_redo.Pop());
                return;
        }
        
        var commandExecutor = _commandExecutors[command.Uid];
        
        if (command.ParamBin is not null)
        {
            if (commandExecutor.CreateUndoRedoParam is not null)
            {
                var undoRedoParam =
                    await commandExecutor.CreateUndoRedoParam(command.ParamBin.Value.Span);

                command = new TCommand
                {
                    Uid = command.Uid,
                    UndoParamBin = undoRedoParam.undoParamBin,
                    RedoParamBin = undoRedoParam.redoParamBin,
                };
            }
            // CreateUndoRedoParamしない場合はRedoParamとして使用
            else
            {
                command = new TCommand
                {
                    Uid = command.Uid,
                    UndoParamBin = null,
                    RedoParamBin = command.ParamBin,
                };
            }
        }
        
        _redo.Clear();
        
        await Redo(command);
    }

    public ValueTask Execute<T>(T param)
    {
        var uid = CommandManagerCommandCache<T>.Uid;

        var command = new TCommand
        {
            Uid = uid,
            ParamBin = _commandManagerSerializer.Serialize(param),
        };

        return ExecuteCommand(command);
    }


    public async ValueTask ExecuteCommands(IEnumerable<TCommand> commands)
    {
        foreach (var command in commands)
        {
            await ExecuteCommand(command);
        }
    }

    public async ValueTask Undo(TCommand command)
    {
        var redoParamBin = await _commandExecutors[command.Uid]
            .Undo((command.UndoParamBin ?? throw new InvalidOperationException()).Span);

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
        return ExecuteCommand(new TCommand
        {
            Uid = UndoCommandUid,
        });
    }
    
    public async ValueTask Redo(TCommand command)
    {
        var undoParamBin = await _commandExecutors[command.Uid]
            .Redo((command.RedoParamBin ?? throw new InvalidOperationException()).Span);
            
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
        return ExecuteCommand(new TCommand
        {
            Uid = RedoCommandUid,
        });
    }

    public void ClearCommands()
    {
        _executed.Clear();
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