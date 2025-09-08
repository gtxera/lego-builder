using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildEditorCommandStack
{
    private readonly Stack<ICommand> _undoStack = new();
    private readonly Stack<ICommand> _redoStack = new();

    public event Action UndoBecameAvailable = delegate { };
    public event Action UndoBecameUnavailable = delegate { };
    public event Action RedoBecameAvailable = delegate { };
    public event Action RedoBecameUnavailable = delegate { };
        
    public void Push(ICommand command)
    {
        _undoStack.Push(command);
        
        PublishUndoAvailable();

        _redoStack.Clear();
        
        PublishRedoUnavailable();
    }

    public void Undo()
    {
        if (!_undoStack.TryPop(out var command))
            return;
        
        command.Undo();
        PublishUndoUnavailable();
        
        _redoStack.Push(command);
        PublishRedoAvailable();
    }

    public void Redo()
    {
        if (!_redoStack.TryPop(out var command))
            return;
        
        command.Redo();
        PublishRedoUnavailable();
        
        _undoStack.Push(command);
        PublishUndoAvailable();
    }
    
    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
        
        PublishUndoUnavailable();
        PublishRedoUnavailable();
    }

    private void PublishUndoAvailable()
    {
        if (_undoStack.Count == 1)
            UndoBecameAvailable();
    }

    private void PublishUndoUnavailable()
    {
        if (_undoStack.Count == 0)
            UndoBecameUnavailable();
    }
    
    private void PublishRedoAvailable()
    {
        if (_redoStack.Count == 1)
            RedoBecameAvailable();
    }

    private void PublishRedoUnavailable()
    {
        if (_redoStack.Count == 0)
            RedoBecameUnavailable();
    }
}
