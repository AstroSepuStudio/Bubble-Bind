using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class TransitionManager : MonoBehaviour
{
    [SerializeField] GameObject _menuObj;
    [SerializeField] bool _isOnMainMenu;
    [SerializeField] CanvasGroup _fadeTransitionGroup;
    [SerializeField] float _fadeTransitionDuration;

    public Action OnFadeBlackFinish;
    public Action OnUnFadeBlackFinish;

    public static bool _gamePaused;

    private void Start()
    {
        if (_isOnMainMenu) return;
        GameManager.Instance.Player_Input.actions["Menu"].started += OpenMenu;
        CloseMenu();
    }

    private void OpenMenu(InputAction.CallbackContext context)
    {
        if (!_menuObj.activeInHierarchy)
        {
            _menuObj.SetActive(true);
            _gamePaused = true;
            Time.timeScale = 0f;
        }
        else
        {
            _menuObj.SetActive(false);
            _gamePaused = false;
            Time.timeScale = 1f;
        }
    }

    public void CloseMenu()
    {
        _menuObj.SetActive(false);
        _gamePaused = false;
        Time.timeScale = 1f;
    }

    public void FadeBlack()
    {
        StartCoroutine(FadeToBlack());
    }

    public void UnFadeBlack()
    {
        StartCoroutine(UnFadeFromBlack());
    }

    IEnumerator FadeToBlack()
    {
        float timer = 0;

        while (timer < _fadeTransitionDuration)
        {
            _fadeTransitionGroup.alpha = Mathf.Lerp(0, 1, timer / _fadeTransitionDuration);
            timer += Time.unscaledDeltaTime;

            yield return null;
        }

        _fadeTransitionGroup.alpha = 1f;
        OnFadeBlackFinish?.Invoke();
    }

    IEnumerator UnFadeFromBlack()
    {
        float timer = 0;

        while (timer < _fadeTransitionDuration)
        {
            _fadeTransitionGroup.alpha = Mathf.Lerp(1, 0, timer / _fadeTransitionDuration);
            timer += Time.unscaledDeltaTime;

            yield return null;
        }

        _fadeTransitionGroup.alpha = 0f;
        OnUnFadeBlackFinish?.Invoke();
    }

    private void OnDestroy()
    {
        if (_isOnMainMenu) return;
        GameManager.Instance.Player_Input.actions["Menu"].started -= OpenMenu;
    }
}
