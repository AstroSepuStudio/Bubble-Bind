using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MainMenuCanvasManager : MonoBehaviour
{
    public static List<int> OpenWindowsIndexes = new();
    [SerializeField] private GameObject[] _windows;
    public UnityEvent<GameObject> OnMainMenuRestored;

    [Header("Transitions")]
    [SerializeField] TransitionManager _transitionManager;
    bool _loading;
    string _sceneOnStandby;

    private void Start()
    {
        _transitionManager.UnFadeBlack();

        foreach (int index in OpenWindowsIndexes)
        {
            _windows[index].SetActive(true);
            OnMainMenuRestored?.Invoke(_windows[index]);
        }
    }

    public void OpenWindow(int index)
    {
        _windows[index].SetActive(true);
        OpenWindowsIndexes.Add(index);
    }

    public void OpenWindow(GameObject windowToOpen)
    {
        for (int i = 0; i < _windows.Length; i++)
        {
            if (_windows[i] == windowToOpen)
            {
                windowToOpen.SetActive(true);
                OpenWindowsIndexes.Add(i);
                break;
            }
        }        
    }

    public void CloseWindow(int index)
    {
        _windows[index].SetActive(false);
        OpenWindowsIndexes.Remove(index);
    }

    public void CloseWindow(GameObject windowToClose)
    {
        for (int i = 0; i < _windows.Length; i++)
        {
            if (_windows[i] == windowToClose)
            {
                windowToClose.SetActive(false);
                OpenWindowsIndexes.Remove(i);
                break;
            }
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void LoadScene(string sceneName)
    {
        if (_loading) return;
        _loading = true;

        _transitionManager.FadeBlack();
        _sceneOnStandby = sceneName;
        _transitionManager.OnFadeBlackFinish += LoadStandbyScene;
    }

    void LoadStandbyScene()
    {
        SceneManager.LoadScene(_sceneOnStandby);
    }
}
