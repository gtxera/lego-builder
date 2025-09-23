using System;
using UnityEngine;
using UnityEngine.Scripting;

public class Stud : PieceConnector<Stud, Socket>
{
    protected override string Layer => "Connectors";
    
    [Preserve] private void UsedOnlyForAOTCodeGeneration()
    {
        var a = TryGetComponent<Stud>(out var b); 
        throw new Exception("This method is used for AOT code generation only. Do not call it at runtime.");
    }
}
