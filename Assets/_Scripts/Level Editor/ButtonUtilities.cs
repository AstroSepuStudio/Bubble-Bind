using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonUtilities : MonoBehaviour
{
    [SerializeField] LevelSaver _levelSaver;
    [SerializeField] LevelEditorManager _levelEditorManager;

    public void SelectButton(Image button)
    {
        button.color = Color.green;
    }

    public void DeselectButton(Image button)
    {
        button.color = Color.white;
    }

    public void SwitchButtonState(Image button)
    {
        if (button.color == Color.green)
            button.color = Color.white;
        else
            button.color = Color.green;
    }

    public void ActivateGameobject(GameObject gameObjectToActivate)
    {
        gameObjectToActivate.SetActive(true);
    }

    public void DeactivateGameobject(GameObject gameObjectToActivate)
    {
        gameObjectToActivate.SetActive(false);
    }

    public void SwitchActiveStateOfGameobject(GameObject gameObjectToActivate)
    {
        gameObjectToActivate.SetActive(!gameObjectToActivate.activeInHierarchy);
    }

    public void ReturnToMainMenu()
    {
        _levelSaver.SaveLevel();
        _levelEditorManager.RemoveEventSubscriptions();
        LevelLoader.LevelName = null;
        SceneManager.LoadScene("MainMenu");
    }
}
