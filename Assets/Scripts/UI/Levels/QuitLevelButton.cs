using System;
using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class QuitLevelButton : ValidatedMonoBehaviour
{
   [Inject]
   private readonly LevelController _levelController;

   [SerializeField, Self]
   private Button _button;

   private void Awake()
   {
      _button.onClick.AddListener(_levelController.Quit);
   }
}
