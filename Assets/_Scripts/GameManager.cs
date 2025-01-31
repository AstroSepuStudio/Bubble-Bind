using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public PlayerInput Player_Input;
    public TransitionManager Transition_Manager;
    public PlayerMovement Player_Movement;
    public Rigidbody2D Player_Rigidbody;
    public GameObject Canvas_OBJ;
    public GameObject PlayerPopParticles;
    public BubbleShooting Bubble_Shooting;
    public AudioData PopSFX;

    string _sceneOnStandby;
    bool _loading = false;
    Vector3 _spawnPosition;

    private void Awake()
    {
        Instance = this;
        Canvas_OBJ.SetActive(true);
    }

    private void Start()
    {
        Transition_Manager.UnFadeBlack();
        Player_Input.actions["Reset"].started += KillPlayer;
        _spawnPosition = Player_Movement.transform.position;
    }

    public void KillPlayer()
    {
        if (_loading) return;
        _loading = true;

        DisablePlayer();
        Player_Movement._body.gameObject.SetActive(false);
        Player_Movement._faceTransform.gameObject.SetActive(false);
        Bubble_Shooting.gameObject.SetActive(false);
        PlayerPopParticles.SetActive(true);
        AudioManager.Instance.PlayClip(PopSFX);

        Transition_Manager.FadeBlack();
        Transition_Manager.OnFadeBlackFinish += ReloadScene;
    }

    void KillPlayer(InputAction.CallbackContext context)
    {
        KillPlayer();
    }

    public void ReloadScene()
    {
        Transition_Manager.OnFadeBlackFinish -= ReloadScene;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadScene(string sceneName)
    {
        if (_loading) return;
        _loading = true;

        Transition_Manager.FadeBlack();
        _sceneOnStandby = sceneName;
        Transition_Manager.OnFadeBlackFinish += LoadStandbyScene;
    }

    public void LoadStandbyScene()
    {
        SceneManager.LoadScene(_sceneOnStandby);
    }

    public void DisablePlayer()
    {
        Player_Movement.enabled = false;
        Player_Rigidbody.simulated = false;
    }

    public void EnablePlayer()
    {
        Player_Movement.enabled = true;
        Player_Rigidbody.simulated = true;
    }

    private void OnDestroy()
    {
        Player_Input.actions["Reset"].started -= KillPlayer;
    }

    public void TeleportPlayerToSpawn()
    {
        Player_Rigidbody.position = _spawnPosition;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
