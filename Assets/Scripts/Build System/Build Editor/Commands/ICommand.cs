using UnityEngine;

public interface ICommand
{
    void Commit();
    
    void Redo();

    void Undo();
}
