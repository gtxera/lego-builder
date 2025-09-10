using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;

public class LevelStarter : MonoBehaviour
{
    [Inject]
    private readonly LevelController _levelController;

    [SerializeField]
    private Level _level;
    
    [SerializeField, Child]
    private Build _build;

    private void Start()
    {
        _levelController.Start(_level, _build);
    }
}
