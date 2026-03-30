using UnityEngine;

public interface ICatalogItemPlacement
{
    void UpdatePosition(Ray ray);
    void RotateClockwise();
    ICommand Confirm();
    void Cancel();
}
