using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenuController : MonoBehaviour
{
    private GameSceneManager _gameSceneManager;
    
    private void Start()
    {
        _gameSceneManager = GameSceneManager.Instance;
    }

    public void OnAnyInput(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton() && !_gameSceneManager.isLoadingScene)
        {
            _gameSceneManager.LoadNextLevel();
        }
    }
}
