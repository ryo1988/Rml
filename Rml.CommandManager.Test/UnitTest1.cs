using MemoryPack;
using Xunit.Abstractions;

namespace Rml.CommandManager.Test;

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
    public ushort Uid { get; init; }
    public ReadOnlyMemory<byte> UndoParamBin { get; init; }
    public ReadOnlyMemory<byte> RedoParamBin { get; init; }
}

public class Personal
{
    public int Age;
    public string? Name;
    public string? Class;
    public bool IsRegistered;
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
        await Task.Delay(TimeSpan.FromSeconds(1));
        
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
        await Task.Delay(TimeSpan.FromSeconds(1));
        
        target.output.WriteLine("Undo");

        return undoParam;
    }

    public async ValueTask<UpdateClass> Redo((Personal personal, ITestOutputHelper output) target)
    {
        var redoParam = new UpdateClass(target.personal.Class);
    
        target.personal.Class = Class;

        // テスト
        await Task.Delay(TimeSpan.FromSeconds(1));
        
        target.output.WriteLine("Redo");

        return redoParam;
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
        
        var commandManager = new CommandManager<Command>(new CommandManagerSerializer());
        
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
            await Task.Delay(TimeSpan.FromSeconds(1));
            
            _output.WriteLine("Lambda Async");
            
            return undoParam;
        });
        
        // 定義に処理が含まれているが、対象の取得処理の登録が必要
        commandManager.RegisterExecuteCommand<UpdateName, (Personal personal, ITestOutputHelper output)>(() => (personal, _output));
        commandManager.RegisterUndoRedoCommand<UpdateClass, (Personal personal, ITestOutputHelper output)>(() => (personal, _output));

        await commandManager.Execute(new UpdateAge(10));
        
        Assert.Equal(10, personal.Age);
        
        await commandManager.Execute(new UpdateName("test"));
        
        Assert.Equal("test", personal.Name);
        
        await commandManager.Execute(new UpdatePersonal<bool>(true));

        Assert.True(personal.IsRegistered);
        
        await commandManager.Execute(new UpdateClass("testClass"));
        
        Assert.Equal("testClass", personal.Class);


        // Undoコマンドをバックアップ
        var backupCommands = commandManager.UndoCommands.Reverse().ToArray();
        
        // Undoコマンドをシリアライズ
        var serializedBackupCommands = commandManager.SerializeCommands(backupCommands);


        await commandManager.Undo();

        Assert.Equal(default, personal.Class);
        
        await commandManager.Undo();
        
        Assert.False(personal.IsRegistered);
        
        await commandManager.Undo();

        Assert.Equal(default, personal.Name);
        
        await commandManager.Undo();
        
        Assert.Equal(0, personal.Age);
        
        
        
        await commandManager.Redo();
        
        Assert.Equal(10, personal.Age);
        
        await commandManager.Redo();
        
        Assert.Equal("test", personal.Name);
        
        await commandManager.Redo();
        
        Assert.True(personal.IsRegistered);
        
        await commandManager.Redo();

        Assert.Equal("testClass", personal.Class);


        
        await commandManager.Undo();

        Assert.Equal(default, personal.Class);
        
        await commandManager.Undo();
        
        Assert.False(personal.IsRegistered);
        
        await commandManager.Undo();

        Assert.Equal(default, personal.Name);
        
        await commandManager.Undo();
        
        Assert.Equal(0, personal.Age);
        
        
        // バックアップしたUndoコマンドを実行
        commandManager.ClearCommands();
        await commandManager.ExecuteCommands(backupCommands);
        
        
        Assert.Equal(10, personal.Age);
        
        Assert.Equal("test", personal.Name);
        
        Assert.True(personal.IsRegistered);
        
        Assert.Equal("testClass", personal.Class);
        
        
        await commandManager.Undo();

        Assert.Equal(default, personal.Class);
        
        await commandManager.Undo();
        
        Assert.False(personal.IsRegistered);
        
        await commandManager.Undo();

        Assert.Equal(default, personal.Name);
        
        await commandManager.Undo();
        
        Assert.Equal(0, personal.Age);
        
        
        // シリアライズしたUndoコマンドをデシリアライズして実行
        commandManager.ClearCommands();
        await commandManager.ExecuteCommands(commandManager.DeserializeCommands(serializedBackupCommands.Span) ?? throw new InvalidOperationException());
        
        
        Assert.Equal(10, personal.Age);
        
        Assert.Equal("test", personal.Name);
        
        Assert.True(personal.IsRegistered);
        
        Assert.Equal("testClass", personal.Class);
    }
}