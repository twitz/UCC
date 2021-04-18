using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviourSingleton<GameSceneManager>
{
    private const int NoLevel = -1;

    [SerializeField]
    private int MainMenuSceneIndex;

    public delegate void SimpleSceneLoadDelegate();

    // Useful Events to Listen to When Scene Loading Begins and Ends.
    // Can be used to show a loading screen overlay for example
    public event SimpleSceneLoadDelegate OnLoadingSceneBegin;
    public event SimpleSceneLoadDelegate OnLoadingSceneComplete;

    public bool isLoadingScene { get; private set; }

    public int CurrentLevel { get; private set; } = NoLevel;

    private void Start()
    {
        GoToMainMenu();
    }

    public void GoToMainMenu()
    {
        StartCoroutine(LoadScene(MainMenuSceneIndex));
    }

    public void GoToLevel(int sceneIndex)
    {
        StartCoroutine(LoadScene(sceneIndex));
    }

    public void LoadNextLevel()
    {
        StartCoroutine(LoadScene(NextSceneIndex()));
    }

    public void LoadPreviousLevel()
    {
        StartCoroutine(LoadScene(PreviousSceneIndex()));
    }

    private int NextSceneIndex()
    {
        var nextIndex = CurrentLevel + 1;
        return nextIndex >= SceneManager.sceneCountInBuildSettings ? MainMenuSceneIndex : nextIndex;
    }

    private int PreviousSceneIndex()
    {
        var previousIndex = CurrentLevel - 1;
        return previousIndex <= MainMenuSceneIndex ? MainMenuSceneIndex : previousIndex;
    }

    private IEnumerator LoadScene(int sceneIndex)
    {
        isLoadingScene = true;
        OnLoadingSceneBegin?.Invoke();
        if (CurrentLevel != NoLevel && CurrentLevel != sceneIndex)
        {
            yield return SceneManager.UnloadSceneAsync(CurrentLevel);
        }
        yield return SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
        isLoadingScene = false;
        CurrentLevel = sceneIndex;
        OnLoadingSceneComplete?.Invoke();
    }
}