using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{

    public GameObject controlsButton;
    public GameObject controlsImage;
    public void StartGameVEasy()
    {
        SceneManager.LoadScene("VeryEasy");
    }

    public void StartGameEasy()
    {
        SceneManager.LoadScene("Easy");
    }

    public void StartGameNormal()
    {
        SceneManager.LoadScene("Normal");
    }

    public void StartGameHard()
    {
        SceneManager.LoadScene("Hard");
    }

    public void StartGameImpossible()
    {
        SceneManager.LoadScene("Impossible");
    }

    public void openControls()
    {
        controlsButton.SetActive(true);
        controlsImage.SetActive(true);

    }
    public void closeControls()
    {
        controlsButton.SetActive(false);
        controlsImage.SetActive(false);
    }
}
