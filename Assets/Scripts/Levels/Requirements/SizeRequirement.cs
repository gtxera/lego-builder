using UnityEngine;

public abstract class SizeRequirement : IBuildRequirement
{
    [SerializeField]
    private PieceVector _size;
    
    public Bounds SizeBounds => new (new Vector3(_size.X % 2 == 0 ? 0 : .4f, _size.Height / 2f, _size.Y % 2 == 0 ? 0 : .4f), _size.ToWorld());
    
    public bool IsSatisfied(Build build)
    {
        var buildBounds = build.GetBounds();
        
        Debug.Log(SizeBounds);
        Debug.Log(buildBounds);

        return SizeCondition(SizeBounds, buildBounds);
    }

    public string GetText() => string.Empty;

    protected abstract bool SizeCondition(Bounds sizeBounds, Bounds buildBounds);
}
