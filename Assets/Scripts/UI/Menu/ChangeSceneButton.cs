using System;
using KBCore.Refs;
using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangeSceneButton : MonoBehaviour
{
    [SerializeField]
    private int _sceneIndex;
    
    [SerializeField, Self]
    private Button _button;

    private void Awake()
    {
        _button.onClick.AddListener(() => SceneManager.LoadScene(_sceneIndex));
        Tween.StopAll();
    }
}
