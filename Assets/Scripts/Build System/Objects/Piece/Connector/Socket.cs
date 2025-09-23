using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;

public class Socket : PieceConnector<Socket, Stud>
{
    protected override string Layer => "Connectors";
    
    [Preserve] private void UsedOnlyForAOTCodeGeneration()
    {
        var a = TryGetComponent<Socket>(out var b); 
        throw new Exception("This method is used for AOT code generation only. Do not call it at runtime.");
    }
}
