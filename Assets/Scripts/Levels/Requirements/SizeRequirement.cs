using UnityEngine;

public abstract class SizeRequirement : IBuildRequirement
{
    [SerializeField]
    private PieceVector _size;
    
    public bool IsSatisfied(Build build)
    {
        var bounds = new Bounds(new Vector3(0, _size.Height / 2f, 0), _size.ToWorld());
        var buildBounds = build.GetBounds();
        
        Debug.Log(bounds);
        Debug.Log(buildBounds);

        return SizeCondition(bounds, buildBounds);
    }

    public string GetText() => string.Empty;

    protected abstract bool SizeCondition(Bounds sizeBounds, Bounds buildBounds);
}
