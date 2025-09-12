using System;

public class BuildEditor
{
    public event Action<Build> StartedEditing = delegate { };
    public event Action<Build> CommandCommited = delegate { };
    public event Action<Build> CommandUndone = delegate { };
    public event Action<Build> CommandRedone = delegate { }; 
    public event Action<Build> FinishedEditing = delegate { };
    
    public Build Build { get; private set; }

    private BuildEditorCommandStack _commandStack = new();

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
