using System;

public class BuildEditor
{
    public event Action<Build> StartedEditing = delegate { };
    public event Action<Build> CommandCommited = delegate { };
    public event Action<Build> CommandUndone = delegate { };
    public event Action<Build> CommandRedone = delegate { }; 
    public event Action<Build> FinishedEditing = delegate { };
    public event Action UndoBecameAvailable = delegate { };
    public event Action UndoBecameUnavailable = delegate { };
    public event Action RedoBecameAvailable = delegate { };
    public event Action RedoBecameUnavailable = delegate { };
    
    public Build Build { get; private set; }

    private BuildEditorCommandStack _commandStack = new();

    public BuildEditor()
    {
        _commandStack.UndoBecameAvailable += () => UndoBecameAvailable();
        _commandStack.UndoBecameUnavailable += () => UndoBecameUnavailable();
        _commandStack.RedoBecameAvailable += () => RedoBecameAvailable();
        _commandStack.RedoBecameUnavailable += () => RedoBecameUnavailable();
    }

    public void StartEditing(Build build)
    {
        Build = build;
        _commandStack.Clear();
        StartedEditing(Build);
    }

    public void FinishEditing()
    {
        if (Build == null)
            return;
        
        FinishedEditing(Build);
        Build = null;
    }

    public void Commit(ICommand command)
    {
        command.Commit();
        _commandStack.Push(command);
        CommandCommited(Build);
    }

    public void Undo()
    {
        if (!_commandStack.TryUndo())
            return;

        CommandUndone(Build);
    }

    public void Redo()
    {
        if (!_commandStack.TryRedo())
            return;

        CommandRedone(Build);
    }
}
