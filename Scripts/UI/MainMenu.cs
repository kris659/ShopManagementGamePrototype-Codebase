using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public static MainMenu instance;
    [SerializeField] private Image startingImage;
    [SerializeField] private Image transitionImage;
    [SerializeField] private GameObject mainMenuCamera;

    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button discordButton;

    [SerializeField] List<GameObject> enableOnGameStart;

    private SplineAnimate splineAnimate;
    private LoadGameUI loadGameUI;
    private EnterSaveNameUI enterSaveNameUI;

    public static bool isMainMenuOpen = true;

    private void Awake()
    {
        if(instance != null) {
            Debug.LogError("Multiple MainMenu");
            Destroy(this);
            return;
        }
        instance = this;
        splineAnimate = GetComponentInChildren<SplineAnimate>(includeInactive: true);
        splineAnimate.StartOffset = UnityEngine.Random.Range(0, 1.0f);

        loadGameUI = GetComponentInChildren<LoadGameUI>(includeInactive: true);
        enterSaveNameUI = GetComponentInChildren<EnterSaveNameUI>(includeInactive: true);
        continueButton.onClick.AddListener(OnContinueButton);
        newGameButton.onClick.AddListener(OnNewGameButton);
        loadGameButton.onClick.AddListener(OnLoadButton);
        settingsButton.onClick.AddListener(OnSettingsButton);
        quitButton.onClick.AddListener(OnQuitButton);
        discordButton.onClick.AddListener(OnDiscordButton);

        loadGameUI.CloseUI();
        enterSaveNameUI.CloseUI();
    }

    private void Start()
    {
        OpenMainMenu();
    }

    public void OpenMainMenu()
    {        
        foreach (GameObject gameObject in enableOnGameStart) {
            gameObject.SetActive(false);
        }
        isMainMenuOpen = true;
        Cursor.lockState = CursorLockMode.None;
        mainMenuCamera.SetActive(true);
        continueButton.interactable = (PlayerPrefs.GetString("LastSaveName", "") != "");
        loadGameUI.UpdateSavesList();
        loadGameButton.interactable = loadGameUI.CanLoadGame;
        transform.GetChild(0).gameObject.SetActive(true); 
    }

    private IEnumerator CloseMainMenuCoroutine(Action actionToPreformWhileLoading)
    {        
        FadeInImage(transitionImage, 1f);
        yield return new WaitForSeconds(1f);

        mainMenuCamera.SetActive(false);
        foreach (GameObject gameObject in enableOnGameStart) {
            gameObject.SetActive(true);
        }
        transform.GetChild(0).gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        UIManager.settingsUI.CloseUI();
        loadGameUI.CloseUI();
        enterSaveNameUI.CloseUI();
        actionToPreformWhileLoading?.Invoke();


        FadeOutImage(transitionImage, 1f);
        yield return new WaitForSeconds(1f);
        isMainMenuOpen = false;
    }

    private void FadeInImage(Image image, float duration)
    {
        Color color = image.color;
        color.a = 1;
        image.DOColor(color, duration);
    }

    private void FadeOutImage(Image image, float duration)
    {
        Color color = image.color;
        color.a = 0;
        image.DOColor(color, duration);
    }

    private void OnContinueButton()
    {
        StartCoroutine(CloseMainMenuCoroutine(() => SavingManager.instance.Load(PlayerPrefs.GetString("LastSaveName"))));
    }
    private void OnNewGameButton()
    {
        UIManager.settingsUI.CloseUI();
        loadGameUI.CloseUI();
        enterSaveNameUI.OpenUI(loadGameUI.saveNames);
    }
    private void OnLoadButton()
    {
        UIManager.settingsUI.CloseUI();
        enterSaveNameUI.CloseUI();
        loadGameUI.OpenUI();
    }
    private void OnSettingsButton()
    {
        loadGameUI.CloseUI();
        enterSaveNameUI.CloseUI();
        UIManager.settingsUI.OpenUI();
    }

    public void OnSaveSelected(string saveName)
    {
        StartCoroutine(CloseMainMenuCoroutine(() => SavingManager.instance.Load(saveName)));
    }
    public void OnNewGameNameSelected(string newGameName)
    {
        StartCoroutine(CloseMainMenuCoroutine(() => SavingManager.instance.StartNewGame(newGameName)));
    }
    private void OnQuitButton()
    {
        Application.Quit();
    }
    private void OnDiscordButton()
    {
        Application.OpenURL("https://discord.gg/n89Kt7CtsJ");
        //Application.OpenURL(Application.persistentDataPath);
    }
}
