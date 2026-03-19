using System;
using System.Collections.Generic;
using System.Linq;

public class SetSelectionCommand : ICommand
{
    private readonly BuildSelection _buildSelection;
    private readonly Guid[] _previousSelection;
    private readonly Guid[] _nextSelection;

    public SetSelectionCommand(BuildSelection buildSelection, IEnumerable<Guid> previousSelection, IEnumerable<Guid> nextSelection)
    {
        _buildSelection = buildSelection;
        _previousSelection = previousSelection.Distinct().ToArray();
        _nextSelection = nextSelection.Distinct().ToArray();
    }

    public void Commit()
    {
        _buildSelection.ReplaceSelection(_nextSelection);
    }

    public void Redo()
    {
        _buildSelection.ReplaceSelection(_nextSelection);
    }

    public void Undo()
    {
        _buildSelection.ReplaceSelection(_previousSelection);
    }
}
