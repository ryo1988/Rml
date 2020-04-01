﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Rml
{
    /// <summary>
    /// 
    /// </summary>
    public class CommandProcessor : Disposable
    {
        /// <summary>
        /// 
        /// </summary>
        public interface ICommand
        {
            /// <summary>
            /// 
            /// </summary>
            Type Type { get; }
        }

        private readonly Dictionary<Type, Func<ICommand, (Action undo, Action redo)>> _executes = new Dictionary<Type, Func<ICommand, (Action undo, Action redo)>>();
        private readonly Stack<(Action undo, Action redo)> _undos = new Stack<(Action undo, Action redo)>();
        private readonly Stack<(Action undo, Action redo)> _redos = new Stack<(Action undo, Action redo)>();
        private readonly Subject<ICommand> _commandSubject;

        /// <summary>
        /// 
        /// </summary>
        public readonly ReactivePropertySlim<int> UndoCount;
        /// <summary>
        /// 
        /// </summary>
        public readonly ReactivePropertySlim<int> RedoCount;
        /// <summary>
        /// 
        /// </summary>
        public readonly IObservable<ICommand> CommandObservable;

        /// <inheritdoc />
        public CommandProcessor()
        {
            _commandSubject = new Subject<ICommand>().AddTo(Cd);
            UndoCount = new ReactivePropertySlim<int>().AddTo(Cd);
            RedoCount = new ReactivePropertySlim<int>().AddTo(Cd);
            CommandObservable = _commandSubject.AsObservable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execute"></param>
        /// <typeparam name="TCommand"></typeparam>
        /// <exception cref="ArgumentException"></exception>
        public void Register<TCommand>(Func<TCommand, (Action undo, Action redo)> execute)
            where TCommand : struct, ICommand
        {
            var command = new TCommand();
            if (command.Type != typeof(TCommand))
                throw new ArgumentException($"{nameof(TCommand)}.{nameof(ICommand.Type)} != typeof({nameof(TCommand)})");

            _executes.Add(typeof(TCommand), o => execute((TCommand)o));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        public void Execute(ICommand command)
        {
            _commandSubject.OnNext(command);

            var undoRedo = _executes[command.Type](command);
            if (undoRedo == default)
                return;

            _undos.Push(undoRedo);
            _redos.Clear();
            undoRedo.redo();
            UpdateUndoRedoCount();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Undo()
        {
            if (_undos.IsEmpty())
                return;

            var undoRedo = _undos.Pop();
            _redos.Push(undoRedo);
            undoRedo.undo();
            UpdateUndoRedoCount();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Redo()
        {
            if (_redos.IsEmpty())
                return;

            var undoRedo = _redos.Pop();
            _undos.Push(undoRedo);
            undoRedo.redo();
            UpdateUndoRedoCount();
        }

        private void UpdateUndoRedoCount()
        {
            UndoCount.Value = _undos.Count;
            RedoCount.Value = _redos.Count;
        }
    }
}