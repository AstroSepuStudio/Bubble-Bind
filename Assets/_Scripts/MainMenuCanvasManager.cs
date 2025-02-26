using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuCanvasManager : MonoBehaviour
{
    public static List<int> OpenWindowsIndexes = new();
    [SerializeField] private GameObject[] _windows;
    public UnityEvent<GameObject> OnMainMenuRestored;

    [Header("Log In")]
    [SerializeField] AuthenticationManager _authenticationManager;
    [SerializeField] GameObject _loginWindow;
    [SerializeField] GameObject _logoutWindow;
    [SerializeField] TextMeshProUGUI _logoutUsernameTxt;

    [Header("Transitions")]
    [SerializeField] TransitionManager _transitionManager;
    bool _loading;
    string _sceneOnStandby;

    [Header("Message Window")]
    [SerializeField] GameObject _messageWindow;
    [SerializeField] TextMeshProUGUI _messageWinText;
    [SerializeField] Button _messageWinButton;

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

    public void OpenLoginWindow()
    {
        if (_authenticationManager.IsLoggedIn())
        {
            _logoutUsernameTxt.SetText(_authenticationManager.GetToken());
            OpenWindow(_logoutWindow);
        }
        else
            OpenWindow(_loginWindow);
    }

    public void SendAlertMessage(string message, Action onButtonClick)
    {
        // Set the message
        _messageWinText.text = message;

        // Wrap the action in a method that removes itself
        void ClickAction()
        {
            onButtonClick?.Invoke();
            _messageWinButton.onClick.RemoveListener(ClickAction); // Unsubscribe
        }

        _messageWinButton.onClick.AddListener(ClickAction);

        // Show the window
        _messageWindow.SetActive(true);
    }

    public void SendAlertMessage(string message)
    {
        // Set the message
        _messageWinText.text = message;

        // Show the window
        _messageWindow.SetActive(true);
    }
}
