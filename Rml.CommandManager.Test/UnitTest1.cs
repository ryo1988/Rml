using MemoryPack;
using Xunit.Abstractions;

namespace Rml.CommandManager.Test;

public static class Define
{
    public static readonly TimeSpan TestDelay = TimeSpan.FromSeconds(0);
}

// 好きなシリアライザを使用
public class CommandManagerSerializer : ICommandManagerSerializer
{
    public ReadOnlyMemory<byte> Serialize<T>(in T value)
    {
        return MemoryPackSerializer.Serialize(value);
    }

    public T? Deserialize<T>(in ReadOnlySpan<byte> bin)
    {
        return MemoryPackSerializer.Deserialize<T>(bin);
    }
}

[MemoryPackable]
public readonly partial record struct Command : ICommand
{
    public short Uid { get; init; }
    public ReadOnlyMemory<byte>? ParamBin { get; init; }
    public ReadOnlyMemory<byte>? UndoParamBin { get; init; }
    public ReadOnlyMemory<byte>? RedoParamBin { get; init; }
}

public class Tag
{
    public Guid Uid;
    public string? Name;
}

public class Personal
{
    public int Age;
    public string? Name;
    public string? Class;
    public bool IsRegistered;
    public List<Tag> Tags = new List<Tag>();
    public string? Format;
}

// シリアライザにあわせてシリアライズ可能な定義
[MemoryPackable]
[CommandManagerCommand(0)]
public readonly partial record struct UpdateAge(int Age);

// 型がキーなので、あまり出番はないかも
[MemoryPackable]
[CommandManagerCommand(1)]
public readonly partial record struct UpdatePersonal<T>(T Value);

// パラメータとUndoRedoどちらでも使用する処理を同梱
[MemoryPackable]
[CommandManagerCommand(2)]
public readonly partial record struct UpdateName(string? Name) : IExecuteCommand<(Personal personal, ITestOutputHelper output), UpdateName>
{
    public async ValueTask<UpdateName> Execute((Personal personal, ITestOutputHelper output) target)
    {
        var undoParam = new UpdateName(target.personal.Name);
    
        target.personal.Name = Name;
    
        // テスト
        await Task.Delay(Define.TestDelay);
        
        target.output.WriteLine("Execute");

        return undoParam;
    }
}

// パラメータとUndoRedoそれぞれの処理を同梱
[MemoryPackable]
[CommandManagerCommand(3)]
public readonly partial record struct UpdateClass(string? Class) : IUndoRedoCommand<(Personal personal, ITestOutputHelper output), UpdateClass>
{
    public async ValueTask<UpdateClass> Undo((Personal personal, ITestOutputHelper output) target)
    {
        var undoParam = new UpdateClass(target.personal.Class);
    
        target.personal.Class = Class;
    
        // テスト
        await Task.Delay(Define.TestDelay);
        
        target.output.WriteLine("Undo");

        return undoParam;
    }

    public async ValueTask<UpdateClass> Redo((Personal personal, ITestOutputHelper output) target)
    {
        var redoParam = new UpdateClass(target.personal.Class);
    
        target.personal.Class = Class;

        // テスト
        await Task.Delay(Define.TestDelay);
        
        target.output.WriteLine("Redo");

        return redoParam;
    }
}

[MemoryPackable]
[CommandManagerCommand(4)]
public readonly partial record struct AddTags((Guid Uid, string Name)[] Tags);

[MemoryPackable]
[CommandManagerCommand(5)]
public readonly partial record struct RemoveTags(Guid[] Tags);

[MemoryPackable]
[CommandManagerCommand(6)]
public readonly partial record struct UpdateTags((Guid Uid, string Name)[] Tags);

[MemoryPackable]
[CommandManagerCommand(7)]
public readonly partial record struct UpdateFormat(string? Format)
    : ICreateUndoRedoParamCommand<(Personal personal, ITestOutputHelper output), UpdateFormat>
{
    public async ValueTask<(UpdateFormat undoParam, UpdateFormat redoParam)> CreateUndoRedoParam((Personal personal, ITestOutputHelper output) target)
    {
        var undoParam = new UpdateFormat(target.personal.Format);
        
        // テスト
        await Task.Delay(Define.TestDelay);
        
        target.output.WriteLine("CreateUndoRedoParam");
        
        return (undoParam, this);
    }

    public async ValueTask Undo((Personal personal, ITestOutputHelper output) target)
    {
        target.personal.Format = Format;
        
        // テスト
        await Task.Delay(Define.TestDelay);
        
        target.output.WriteLine("Undo");
    }

    public async ValueTask Redo((Personal personal, ITestOutputHelper output) target)
    {
        target.personal.Format = Format;
        
        // テスト
        await Task.Delay(Define.TestDelay);
        
        target.output.WriteLine("Redo");
    }
}

public class UnitTest1
{
    private readonly ITestOutputHelper _output;

    public UnitTest1(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async void Test1()
    {
        var personal = new Personal();
        
        var commandManager = new CommandManager<Command>(new CommandManagerSerializer(), true);
        
        // 定義はパラメータのみなので、ここで処理の登録が必要
        commandManager.RegisterCommand<UpdateAge>(o =>
        {
            var undoParam = new UpdateAge(personal.Age);
            personal.Age = o.Age;
            
            _output.WriteLine("Lambda");
            
            return undoParam;
        });
        
        // 非同期処理も登録可能
        commandManager.RegisterCommand<UpdatePersonal<bool>>(async o =>
        {
            var undoParam = new UpdatePersonal<bool>(personal.IsRegistered);
            personal.IsRegistered = o.Value;

            // テスト
            await Task.Delay(Define.TestDelay);
            
            _output.WriteLine("Lambda Async");
            
            return undoParam;
        });
        
        // 定義に処理が含まれているが、対象の取得処理の登録が必要
        commandManager.RegisterExecuteCommand<UpdateName, (Personal personal, ITestOutputHelper output)>(() => (personal, _output));
        commandManager.RegisterUndoRedoCommand<UpdateClass, (Personal personal, ITestOutputHelper output)>(() => (personal, _output));
        
        // まずUndoRedoParamを作成して、その値を使ってUndoRedoする
        commandManager.RegisterCommand<AddTags, Guid[], (Guid Uid, string Name)[]>(
            async o =>
            {
                var undoParam = o.Tags
                    .Select(oo => oo.Uid)
                    .ToArray();
                
                // テスト
                await Task.Delay(Define.TestDelay);
                
                _output.WriteLine("Lambda:createUndoRedoParam");

                return (undoParam, o.Tags);
            },
            async o =>
            {
                var targets = personal.Tags
                    .Join(o, oo => oo.Uid, i => i, (oo, _) => oo)
                    .ToArray();
                
                foreach (var target in targets)
                {
                    personal.Tags.Remove(target);
                }
                
                // テスト
                await Task.Delay(Define.TestDelay);
                
                _output.WriteLine("Lambda:undo");
            },
            async o =>
            {
                var targets = o
                    .Select(oo => new Tag
                    {
                        Uid = oo.Uid,
                        Name = oo.Name,
                    })
                    .ToArray();
                
                personal.Tags.AddRange(targets);
                
                // テスト
                await Task.Delay(Define.TestDelay);
                
                _output.WriteLine("Lambda:redo");
            });
        
        // まずUndoRedoParamを作成して、その値を使ってUndoRedoする（同期）
        commandManager.RegisterCommand<RemoveTags, (Guid Uid, string Name)[], Guid[]>(
            o =>
            {
                var undoParam = personal.Tags
                    .Join(o.Tags, oo => oo.Uid, ii => ii, (oo, _) => (oo.Uid, oo.Name ?? string.Empty))
                    .ToArray();
                
                _output.WriteLine("Lambda:createUndoRedoParam");

                return (undoParam, o.Tags);
            },
            o =>
            {
                var targets = o
                    .Select(oo => new Tag
                    {
                        Uid = oo.Uid,
                        Name = oo.Name,
                    })
                    .ToArray();
                
                personal.Tags.AddRange(targets);
                
                _output.WriteLine("Lambda:undo");
            },
            o =>
            {
                var targets = personal.Tags
                    .Join(o, oo => oo.Uid, i => i, (oo, _) => oo)
                    .ToArray();
                
                foreach (var target in targets)
                {
                    personal.Tags.Remove(target);
                }
                
                _output.WriteLine("Lambda:redo");
            });

        // まずUndoRedoParamを作成して、その値を使ってUndoRedoする（UndoRedo処理が同じ）
        commandManager.RegisterCommand<UpdateTags, (Guid Uid, string Name)[]>(
            o =>
            {
                var undoParam = o.Tags
                    .Join(personal.Tags, oo => oo.Uid, ii => ii.Uid, (_, ii) => (ii.Uid, ii.Name ?? string.Empty))
                    .ToArray();
                
                _output.WriteLine("Lambda:createUndoRedoParam");

                return ValueTask.FromResult((undoParam, o.Tags));
            },
            o =>
            {
                foreach (var (tag, value) in personal.Tags
                             .Join(o, oo => oo.Uid, ii => ii.Uid, (oo, ii) => (tag:oo, value:ii)))
                {
                    tag.Name = value.Name;
                }
                
                _output.WriteLine("Lambda:undoRedo");
                
                return ValueTask.CompletedTask;
            });
        
        commandManager.RegisterCreateUndoRedoParamCommand<UpdateFormat, (Personal personal, ITestOutputHelper output)>(() => (personal, _output));
        
        _output.WriteLine("---start execute---");
        
        await commandManager.Execute(new UpdateAge(10));
        
        Assert.Equal(10, personal.Age);
        
        await commandManager.Execute(new UpdateName("test"));
        
        Assert.Equal("test", personal.Name);
        
        await commandManager.Execute(new UpdatePersonal<bool>(true));

        Assert.True(personal.IsRegistered);
        
        await commandManager.Execute(new UpdateClass("testClass"));
        
        Assert.Equal("testClass", personal.Class);
        
        await commandManager.Execute(new AddTags(new []
        {
            (Guid.NewGuid(), "tag1"),
            (Guid.NewGuid(), "tag2"),
        }));
        
        Assert.Equal(new []{"tag1", "tag2"}, personal.Tags.Select(o => o.Name));

        var updateTags = personal.Tags
            .Select(o => (o.Uid, o.Name + "add"))
            .ToArray();
        await commandManager.Execute(new UpdateTags(updateTags));
        
        Assert.Equal(new []{"tag1add", "tag2add"}, personal.Tags.Select(o => o.Name));

        var removeTags = personal.Tags
            .Select(o => o.Uid)
            .ToArray();
        await commandManager.Execute(new RemoveTags(removeTags));
        
        Assert.Equal(Array.Empty<string>(), personal.Tags.Select(o => o.Name));
        
        await commandManager.Execute(new UpdateFormat("testFormat"));
        
        Assert.Equal("testFormat", personal.Format);
        
        _output.WriteLine("---end execute---");


        // Undoコマンド（UndoRedoにのみ必要なデータをパラメータに持つコマンド、実行負荷：低、容量：大）をバックアップ
        var backupUndoCommands = commandManager.UndoCommands.Reverse().ToArray();

        // Undoコマンドをシリアライズ
        var serializedBackupUndoCommands = commandManager.SerializeCommands(backupUndoCommands);
        
        
        // Executedコマンド（UndoRedoコマンドを生成するのに必要なデータをパラメータに持つコマンド、実行負荷：高（UndoRedoコマンドを生成するため）、容量：小）をバックアップ
        var backupExecuted1Commands = commandManager.ExecutedCommands.ToArray();
        
        // Executedコマンドをシリアライズ
        var serializedBackupExecuted1Commands = commandManager.SerializeCommands(backupExecuted1Commands);
        

        _output.WriteLine("---start undo---");
        
        await commandManager.Undo();
        
        Assert.Equal(default, personal.Format);

        await commandManager.Undo();
        
        Assert.Equal(new []{"tag1add", "tag2add"}, personal.Tags.Select(o => o.Name));
        
        await commandManager.Undo();
        
        Assert.Equal(new []{"tag1", "tag2"}, personal.Tags.Select(o => o.Name));
        
        await commandManager.Undo();

        Assert.Equal(Array.Empty<string>(), personal.Tags.Select(o => o.Name));

        await commandManager.Undo();

        Assert.Equal(default, personal.Class);
        
        await commandManager.Undo();
        
        Assert.False(personal.IsRegistered);
        
        await commandManager.Undo();

        Assert.Equal(default, personal.Name);
        
        await commandManager.Undo();
        
        Assert.Equal(0, personal.Age);
        
        _output.WriteLine("---end undo---");
        
        
        
        _output.WriteLine("---start redo---");

        await commandManager.Redo();
        
        Assert.Equal(10, personal.Age);
        
        await commandManager.Redo();
        
        Assert.Equal("test", personal.Name);
        
        await commandManager.Redo();
        
        Assert.True(personal.IsRegistered);
        
        await commandManager.Redo();

        Assert.Equal("testClass", personal.Class);
        
        await commandManager.Redo();
        
        Assert.Equal(new []{"tag1", "tag2"}, personal.Tags.Select(o => o.Name));

        await commandManager.Redo();
        
        Assert.Equal(new []{"tag1add", "tag2add"}, personal.Tags.Select(o => o.Name));
        
        await commandManager.Redo();
        
        Assert.Equal(Array.Empty<string>(), personal.Tags.Select(o => o.Name));

        await commandManager.Redo();
        
        Assert.Equal("testFormat", personal.Format);
        
        _output.WriteLine("---end redo---");
        

        _output.WriteLine("---start undo---");

        await commandManager.Undo();
        
        Assert.Equal(default, personal.Format);
        
        await commandManager.Undo();

        Assert.Equal(new []{"tag1add", "tag2add"}, personal.Tags.Select(o => o.Name));
        
        await commandManager.Undo();

        Assert.Equal(new []{"tag1", "tag2"}, personal.Tags.Select(o => o.Name));
        
        await commandManager.Undo();
        
        Assert.Equal(Array.Empty<string>(), personal.Tags.Select(o => o.Name));
        
        await commandManager.Undo();

        Assert.Equal(default, personal.Class);
        
        await commandManager.Undo();
        
        Assert.False(personal.IsRegistered);
        
        await commandManager.Undo();

        Assert.Equal(default, personal.Name);
        
        await commandManager.Undo();
        
        Assert.Equal(0, personal.Age);
        
        _output.WriteLine("---end undo---");
        
        
        // Executedコマンド（UndoRedoコマンドを生成するのに必要なデータをパラメータに持つコマンド、実行負荷：高（UndoRedoコマンドを生成するため）、容量：小）をバックアップ
        var backupExecuted2Commands = commandManager.ExecutedCommands.ToArray();
        
        // Executedコマンドをシリアライズ
        var serializedBackupExecuted2Commands = commandManager.SerializeCommands(backupExecuted2Commands);
        

        _output.WriteLine("---start backupUndoCommands---");
        
        // バックアップしたUndoコマンドを実行
        commandManager.ClearCommands();
        await commandManager.ExecuteCommands(backupUndoCommands);
        

        Assert.Equal(10, personal.Age);
        
        Assert.Equal("test", personal.Name);
        
        Assert.True(personal.IsRegistered);
        
        Assert.Equal("testClass", personal.Class);
        
        Assert.Equal(Array.Empty<string>(), personal.Tags.Select(o => o.Name));
        
        Assert.Equal("testFormat", personal.Format);
        
        
        await commandManager.Undo();
        
        Assert.Equal(default, personal.Format);
        
        await commandManager.Undo();

        Assert.Equal(new []{"tag1add", "tag2add"}, personal.Tags.Select(o => o.Name));
        
        await commandManager.Undo();

        Assert.Equal(new []{"tag1", "tag2"}, personal.Tags.Select(o => o.Name));
        
        await commandManager.Undo();
        
        Assert.Equal(Array.Empty<string>(), personal.Tags.Select(o => o.Name));
        
        await commandManager.Undo();

        Assert.Equal(default, personal.Class);
        
        await commandManager.Undo();
        
        Assert.False(personal.IsRegistered);
        
        await commandManager.Undo();

        Assert.Equal(default, personal.Name);
        
        await commandManager.Undo();
        
        Assert.Equal(0, personal.Age);
        
        _output.WriteLine("---end backupUndoCommands---");
        
        
        _output.WriteLine("---start serializedBackupUndoCommands---");

        // シリアライズしたUndoコマンドをデシリアライズして実行
        commandManager.ClearCommands();
        await commandManager.ExecuteCommands(commandManager.DeserializeCommands(serializedBackupUndoCommands.Span) ?? throw new InvalidOperationException());
        
        
        Assert.Equal(10, personal.Age);
        
        Assert.Equal("test", personal.Name);
        
        Assert.True(personal.IsRegistered);
        
        Assert.Equal("testClass", personal.Class);
        
        Assert.Equal(Array.Empty<string>(), personal.Tags.Select(o => o.Name));
        
        Assert.Equal("testFormat", personal.Format);
        
        _output.WriteLine("---end serializedBackupUndoCommands---");


        // 値を初期化
        personal.Age = default;
        personal.Name = default;
        personal.Class = default;
        personal.IsRegistered = default;
        personal.Tags = new List<Tag>();
        personal.Format = default;
        
        _output.WriteLine("---start serializedBackupExecuted1Commands---");
        
        // シリアライズしたExecutedコマンドをデシリアライズして実行
        commandManager.ClearCommands();
        await commandManager.ExecuteCommands(commandManager.DeserializeCommands(serializedBackupExecuted1Commands.Span) ?? throw new InvalidOperationException());
        
        Assert.Equal(10, personal.Age);
        
        Assert.Equal("test", personal.Name);
        
        Assert.True(personal.IsRegistered);
        
        Assert.Equal("testClass", personal.Class);
        
        Assert.Equal(Array.Empty<string>(), personal.Tags.Select(o => o.Name));
        
        Assert.Equal("testFormat", personal.Format);
        
        _output.WriteLine("---end serializedBackupExecuted1Commands---");
        
        
        // 値を初期化
        personal.Age = default;
        personal.Name = default;
        personal.Class = default;
        personal.IsRegistered = default;
        personal.Tags = new List<Tag>();
        personal.Format = default;
        
        
        _output.WriteLine("---start serializedBackupExecuted2Commands---");
        
        // シリアライズしたExecutedコマンドをデシリアライズして実行
        commandManager.ClearCommands();
        await commandManager.ExecuteCommands(commandManager.DeserializeCommands(serializedBackupExecuted2Commands.Span) ?? throw new InvalidOperationException());
        
        Assert.Equal(default, personal.Format);
        
        Assert.Equal(Array.Empty<string>(), personal.Tags.Select(o => o.Name));
        
        Assert.Equal(default, personal.Class);
        
        Assert.False(personal.IsRegistered);
        
        Assert.Equal(default, personal.Name);
        
        Assert.Equal(0, personal.Age);
        
        _output.WriteLine("---end serializedBackupExecuted2Commands---");


        
        _output.WriteLine("---start commandName1---");

        foreach (var command in commandManager.DeserializeCommands(serializedBackupExecuted1Commands.Span) ?? throw new InvalidOperationException())
        {
            var commandName = commandManager.GetCommandName(command);
            _output.WriteLine(commandName);
        }
        
        _output.WriteLine("---end commandName1---");
        
        
        
        _output.WriteLine("---start commandName2---");

        foreach (var command in commandManager.DeserializeCommands(serializedBackupExecuted2Commands.Span) ?? throw new InvalidOperationException())
        {
            var commandName = commandManager.GetCommandName(command);
            _output.WriteLine(commandName);
        }
        
        _output.WriteLine("---end commandName2---");
    }
}