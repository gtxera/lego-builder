using System;

public class BuildEditor
{
    public event Action<Build> StartedEditing = delegate { };
    public event Action<Build> CommandCommited = delegate { };
    public event Action<Build> FinishedEditing = delegate { };
    
    public Build Build { get; private set; }

    private readonly BuildEditorCommandStack _commandStack;

    public BuildEditor(BuildEditorCommandStack commandStack)
    {
        _commandStack = commandStack;
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
}
