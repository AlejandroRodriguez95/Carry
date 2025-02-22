using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour, ISaveSystem
{
    [Header("Menuing")]
    [SerializeField] Button firstButton;
    [SerializeField] GameObject pauseElements;
    [SerializeField] GameObject optionsElements;
    [SerializeField] GameObject pauseButton;
    [SerializeField] GameObject playButton;

    [Header("UI")]
    [SerializeField] GameObject stressLvlBar;
    [SerializeField] AudioSource LevelMusic;

    [Header("Statistic")]
    [SerializeField] TMP_Text patientsHealed;
    [SerializeField] TMP_Text patientsLost;
    [SerializeField] TMP_Text treatments;

    [Tooltip("timer for activating UI after losing or winning a level")]
    float sceneTransitionTimer = 0f;
    public float SceneTransitionTimer { get { return sceneTransitionTimer; } }

    public GameObject StressLevelBar => stressLvlBar;


    GameObject mainMenuElements;


    public void Awake()
    {
        //Activating/Deactivating menu elements
        if (pauseElements != null)
        {
            pauseElements.SetActive(false);
        }

        if (optionsElements != null)
        {
            optionsElements.SetActive(false);
        }

        mainMenuElements = GameObject.Find("MainMenu");
        if (mainMenuElements != null)
        {
            mainMenuElements.SetActive(true);
        }

        if (firstButton != null)
        {
            firstButton.Select();
        }
    }

    private void Start()
    {
        if (GlobalData.instance.IsSaveFileLoaded)
        {
            LoadData();
        }
    }
    private void Update()
    {
        if(SceneManager.GetActiveScene().name == "GameOver" || SceneManager.GetActiveScene().name == "LevelComplete")
            sceneTransitionTimer += Time.deltaTime;
        GamePaused();
    }

    #region Pausing the Game
    /// <summary>
    /// pauses the game and sets the Pause UI active
    /// </summary>
    public void GamePaused()
    {

        if (Input.GetKeyUp(KeyCode.Escape) && !optionsElements.activeSelf && pauseElements != null)
        {
            SoundManager.instance.PlayAudioClip(ESoundeffects.Button, stressLvlBar.gameObject.GetComponent<AudioSource>());

            if (Time.timeScale > 0)
            {
                pauseButton.gameObject.SetActive(false);
                playButton.gameObject.SetActive(true);
                Time.timeScale = 0f;
                pauseElements.SetActive(true);
                LevelMusic.Pause();
            }
            else
            {
                pauseButton.gameObject.SetActive(true);
                playButton.gameObject.SetActive(false);
                pauseElements.SetActive(false);
                Time.timeScale = 1f;
                LevelMusic.Play();

            }
        }
        else if (Input.GetKeyUp(KeyCode.Escape) && optionsElements.activeSelf)
        {
            if (pauseElements != null)
                pauseElements.SetActive(true);

            if (mainMenuElements != null)
                mainMenuElements.SetActive(true);

            optionsElements.SetActive(false);
            SoundManager.instance.PlayAudioClip(ESoundeffects.Button, stressLvlBar.gameObject.GetComponent<AudioSource>());
        }
    }

    public void GamePauseByClick()
    {
        SoundManager.instance.PlayAudioClip(ESoundeffects.Button, stressLvlBar.gameObject.GetComponent<AudioSource>());

        if (Time.timeScale > 0)
        {
            pauseButton.gameObject.SetActive(false);
            playButton.gameObject.SetActive(true);
            Time.timeScale = 0f;
            pauseElements.SetActive(true);
            LevelMusic.Pause();
        }
        else
        {
            playButton.gameObject.SetActive(false);
            pauseButton.gameObject.SetActive(true);
            pauseElements.SetActive(false);
            Time.timeScale = 1f;
            LevelMusic.Play();
        }
    }

    public void Continue()
    {
        SoundManager.instance.PlayAudioClip(ESoundeffects.Button, stressLvlBar.gameObject.GetComponent<AudioSource>());
        pauseElements.SetActive(false);
        playButton.gameObject.SetActive(false);
        pauseButton.gameObject.SetActive(true);
        Time.timeScale = 1f;
        LevelMusic.Play();

    }
    #endregion

    #region Activate/Deactive Options
    //i think following two methods can be optimized. iam tired, i will look over it another time
    /// <summary>
    /// Sets the right UI elements active/inactive
    /// </summary>
    public void EnterOptions()
    {

        if (SceneManager.GetActiveScene().name == "Level 1" || SceneManager.GetActiveScene().name == "Level 2" ||
            SceneManager.GetActiveScene().name == "Level 3" || SceneManager.GetActiveScene().name == "Level 4" ||
            SceneManager.GetActiveScene().name == "Tutorial")
        {
            pauseElements.SetActive(false);
            optionsElements.SetActive(true);
            SoundManager.instance.PlayAudioClip(ESoundeffects.Button, optionsElements.GetComponent<AudioSource>());
        }

        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            mainMenuElements.SetActive(false);
            if (optionsElements != null)
            {
                optionsElements.SetActive(true);
                SoundManager.instance.PlayAudioClip(ESoundeffects.Button, optionsElements.GetComponent<AudioSource>());
            }
        }
    }

    /// <summary>
    /// Sets the right UI elements active/inactive
    /// </summary>
    public void LeaveOptions()
    {

        if (SceneManager.GetActiveScene().name == "Level 1" || SceneManager.GetActiveScene().name == "Level 2" ||
            SceneManager.GetActiveScene().name == "Level 3" || SceneManager.GetActiveScene().name == "Level 4" ||
            SceneManager.GetActiveScene().name == "Tutorial")
        {
            pauseElements.SetActive(true);
            SoundManager.instance.PlayAudioClip(ESoundeffects.Button, pauseElements.GetComponent<AudioSource>());
            optionsElements.SetActive(false);
        }

        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            mainMenuElements.SetActive(true);
            SoundManager.instance.PlayAudioClip(ESoundeffects.Button, mainMenuElements.GetComponent<AudioSource>());
            optionsElements.SetActive(false);
        }
    }


    #endregion

    #region Show statistic after a level
    public void ShowStatsCompleteShift()
    {
        SoundManager.instance.PlayAudioClip(ESoundeffects.Winning, GetComponent<AudioSource>());
        patientsHealed.text = "Patients Healed: " + GlobalData.instance.ShiftPatientsHealed;
        patientsLost.text = "Patients Lost: " + GlobalData.instance.ShiftPatientsLost;
        treatments.text = "Treatments: " + GlobalData.instance.ShiftTreatments;
    }

    public void ShowStatsGameOver()
    {
        SoundManager.instance.PlayAudioClip(ESoundeffects.Losing, GetComponent<AudioSource>());
        patientsHealed.text = "Patients Healed: " + GlobalData.instance.TotalPatientsHealed;
        patientsLost.text = "Patients Lost: " + GlobalData.instance.TotalPatientsLost;
        treatments.text = "Treatments: " + GlobalData.instance.TotalTreatments;
    }
    #endregion

    #region Save/Load methods
    public void SaveData()
    {
        BinaryFormatter formatter = new BinaryFormatter();

        string path = Application.persistentDataPath + "/SaveDataAudio.carry";
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, LevelMusic.time);

        stream.Close();
    }

    public void LoadData()
    {
        string path = Application.persistentDataPath + "/SaveDataAudio.carry";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            if (LevelMusic != null)
            {
                LevelMusic.time = (float)formatter.Deserialize(stream);
                LevelMusic.Play();
            }

            stream.Close();

        }
    }
    #endregion
}