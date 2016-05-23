using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuButtons : MonoBehaviour
{
    public Text Playtext;

    public GameObject FPSCounter;

    public GameObject MainPage;
    public GameObject OptionsPage;

    GameObject currentPage;

    bool fpsactive = true;
    bool Musicmuted = false;

    void Start()
    {
        currentPage = MainPage;
    }

    public void Play()
    {
        if (GameMaster.gm.HasGameStarted)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        else
        {
            GameMaster.gm.StartGame();

            Playtext.text = "REPLAY";
        }
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void GoToOptionsPage()
    {
        if (OptionsPage == currentPage)
            return;

        OptionsPage.SetActive(true);
        currentPage.SetActive(false);
        currentPage = OptionsPage;
    }

    public void GoToMainPage()
    {
        if (MainPage == currentPage)
            return;
      
        MainPage.SetActive(true);
        currentPage.SetActive(false);
        currentPage = MainPage;
    }

    public void ShowFPS()
    {
        FPSCounter.SetActive(!fpsactive);
        fpsactive = !fpsactive;
    }

    public void MuteMusic()
    {
        Camera.main.GetComponent<AudioSource>().mute = !Musicmuted;
        Musicmuted = !Musicmuted;
    }
}
