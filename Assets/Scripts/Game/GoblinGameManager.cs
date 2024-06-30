namespace GoblinzMechanics.Game
{
    using UnityEngine;
    using GoblinzMechanics.Utils;
    using TMPro;
    using UnityEngine.InputSystem;
    using UnityEngine.Events;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.HighDefinition;
    using System;
    using UnityEngine.SceneManagement;
    using System.Collections;
    using System.IO;

    public class GoblinGameManager : Singleton<GoblinGameManager>
    {
        public enum GameStateEnum
        {
            NotStarted,
            Comics,
            Playing,
            Pause,
            Ended
        }

        [SerializeField] private GameStateEnum _gameState = GameStateEnum.NotStarted;
        [SerializeField] private float routeSpeedIncrease = 0.01f;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _endGameText;
        [SerializeField] private TMP_Text _statsText;
        [SerializeField] private GameObject _prepareUI;
        [SerializeField] private GameObject _EndGameUI;
        [SerializeField] private Transform _pathRoot;
        [SerializeField] private InputActionAsset playerControls;
        [SerializeField] private VolumeProfile _globalProfile;

        private Color _baseColor;
        [SerializeField] private Color _wrongColor;
        [SerializeField] private Color _rightColor;
        [SerializeField] private float _vignetteDecreaseSpeed = 0.25f;
        [SerializeField] private CameraLookBack cameraLookBack;
        private Vignette _vignette;
        public GoblinGameStats stats => GoblinGameStats.Instance;
        public CameraSettings CameraSettingsVar;


        public GameStateEnum GameState
        {
            get
            {
                return _gameState;
            }
            private set
            {
                OnStateChanged?.Invoke(value);
                _gameState = value;
            }
        }

        public bool IsWin { get; internal set; }

        public Action<GameStateEnum> OnStateChanged;

        public UnityEvent OnAnyKeyPressed;
        public UnityEvent<bool> OnPausePressed;
        public UnityEvent OnWin;
        public UnityEvent OnDeath;

        private bool _isWin = false;

        private void OnDestroy()
        {
            CameraSettingsVar.Enabled = false;
        }

        private void Awake()
        {
            CameraSettingsVar.Enabled = true;
            Application.runInBackground = true;
            GameState = GameStateEnum.NotStarted;
            if (_globalProfile.TryGet(out _vignette))
            {
                _baseColor = _vignette.color.value;
            }

            if (!playerControls.enabled)
            {
                playerControls.Enable();
            }
        }

        private void OnEnable()
        {
            ShowStartUI();

            if (!playerControls.enabled)
            {
                playerControls.Enable();
            }
            playerControls["PressAnyKey"].started += OnAnyKey;
            playerControls["Pause"].started += OnPauseGame;
        }

        private void OnDisable()
        {
            _vignette.intensity.value = 0.25f;
            _vignette.color.value = _baseColor;
            if (_scoreText != null)
            {
                _scoreText.gameObject.SetActive(false);
            }
            playerControls["PressAnyKey"].started -= OnAnyKey;
            playerControls["Pause"].started -= OnPauseGame;
        }

        private void Update()
        {
            HandleVignette();
            _scoreText.text = $"{stats.Score}"; //\n{(int)(RouteController.Instance.routeCounter % (12 * RouteController.Instance.routeSpeedModificator))}
            if (GameState != GameStateEnum.Playing) { return; }
            RouteController.Instance.routeSpeedModificator += routeSpeedIncrease * Time.deltaTime;
            stats.runnedMeters = -Mathf.RoundToInt(_pathRoot.position.z);
        }

        private void ShowStartUI()
        {

            OnAnyKeyPressed.AddListener(StartGame);
            _scoreText.gameObject.SetActive(false);
            _prepareUI.SetActive(true);
        }

        private IEnumerator DoShowStartUI() {
            yield return GoblinComicsManager.Instance.StartGamePlay();
        }

        private void OnAnyKey(InputAction.CallbackContext context)
        {
            OnAnyKeyPressed?.Invoke();
        }

        private void OnPauseGame(InputAction.CallbackContext context)
        {
            if (GameState != GameStateEnum.Playing && GameState != GameStateEnum.Pause) { return; }
            PauseGame();
            OnPausePressed?.Invoke(GameState == GameStateEnum.Pause);

        }

        public void StartGame()
        {
            OnAnyKeyPressed.RemoveListener(StartGame);
            _prepareUI.SetActive(false);
            _EndGameUI.SetActive(false);
            GameState = GameStateEnum.Playing;
            _scoreText.gameObject.SetActive(true);
            _isWin = false;
            StartCoroutine(LookBack());
        }

        private IEnumerator LookBack()
        {
            yield return new WaitForSeconds(1f);
            cameraLookBack.LookNormal();
            GoblinCharacterController.Instance.LookNormal();
        }

        public void PauseGame()
        {
            if (GameState == GameStateEnum.Playing)
            {
                GameState = GameStateEnum.Pause;
                _statsText.text = string.Format(_endGameTextFormat,
                                                stats.currentLevel,
                                                stats.examplesSolved,
                                                stats.examplesFailed,
                                                stats.runnedMeters,
                                                stats.collectedCoins,
                                                stats.maxScore,
                                                stats.maxDistanceToBolder
                                                );
            }
            else if (GameState == GameStateEnum.Pause)
            {
                GameState = GameStateEnum.Playing;
            }
        }

        public void RestartGame()
        {
            _EndGameUI.SetActive(false);
            stats.Reset();
            if (_isWin)
            {
                stats.currentLevel++;
            }
            else
            {
                stats.currentLevel = 1;
            }
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void TakeScreen()
        {
            string path, directoryPath;
#if UNITY_EDITOR
            path = $"Screenshots/Screenshot-{DateTime.Now:dd-MM-yy--HH-mm-ss}.png";
#else
            path = $"{Application.dataPath}/Screenshots/Screenshot-{DateTime.Now:dd-MM-yy--HH-mm-ss}.png";
#endif
            directoryPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(Path.GetDirectoryName(directoryPath)))
            {
                Directory.CreateDirectory(directoryPath);
            }
            ScreenCapture.CaptureScreenshot(path);
        }

        public void EndGame(bool isWin)
        {
            if (!isWin)
            {
                Debug.Log($"Вы запутались в математическом подземелье и не нашли выход! Ваш счет: {_scoreText.text}");
                OnDeath?.Invoke();
            }
            else
            {
                _isWin = true;
                OnWin?.Invoke();
            }
            ShowEndUI();
            GameState = GameStateEnum.Ended;
        }

        private MathRouteSubClass __prevExample;
        public void HandleMathAnswer(MathAnswerTrigger trigger, MathRouteSubClass example)
        {
            if (__prevExample != example)
            {
                Debug.Log($"Получен {(trigger.isValid ? "" : "не")}верный ответ: {trigger.value}; пример: {example.variableA} {example.sign} {example.variableB} = {example.variableR}");
                __prevExample = example;
                stats.examples.Add($"Example: {example.GetExampleString()} : Answered: {trigger.value}");
                if (trigger.isValid)
                {
                    _vignette.intensity.value = 1f;
                    _vignette.color.value = _rightColor;

                    stats.examplesSolved++;
                    stats.Score += stats.scoreAdd;

                    RouteController.Instance.routeSpeedModificator += 0.02f;
                }
                else
                {
                    _vignette.intensity.value = 1f;
                    _vignette.color.value = _wrongColor;

                    stats.examplesFailed++;
                    stats.Score -= stats.scoreAdd;

                    RouteController.Instance.routeSpeedModificator -= 0.2f;
                }
            }
        }

        public void HandleCoin(int value)
        {
            Debug.Log($"Ха-ха! Очередная монетка падет в мои карманцы!");
            stats.collectedCoins += value;
            stats.Score += value;
        }

        public void HandleVignette()
        {
            _vignette.intensity.value = Mathf.Clamp(_vignette.intensity.value - _vignetteDecreaseSpeed * Time.deltaTime, 0.25f, 1);
            if (_vignette.intensity.value == 0.25)
            {
                _vignette.color.value = _baseColor;
            }
        }

        [Multiline]
        [SerializeField] private string _endGameTextFormat = " <color=red>Ы</color>ЫЫыть!\n<color=red>У</color>ровень<color=red>:</color> {0}\n<color=red>Т</color>воя <color=red>Z</color>тата<color=red>:</color>\n<color=red>П</color>римеры правильно<color=red>:</color> {1}\n<color=red>П</color>римеры неправильно<color=red>:</color> {2}\n<color=red>П</color>робежал<color=red>:</color> {3:0.00} м\n<color=red>Z</color>тырил монет<color=red>:</color> {4}\n<cSpace=10>[<color=red>Max</color>]</cSpace> счет<color=red>:</color> {5}\n<cSpace=10>[<color=red>Max</color>]</cSpace> дистанция от валуна<color=red>:</color> {6:0.00} м";
        private void ShowEndUI()
        {
            StartCoroutine(PlayShowEndUI());
        }
        private IEnumerator PlayShowEndUI()
        {
            if (_scoreText != null)
            {
                _scoreText.gameObject.SetActive(false);
            }
            _endGameText.text = string.Format(_endGameTextFormat,
                                              stats.currentLevel,
                                              stats.examplesSolved,
                                              stats.examplesFailed,
                                              stats.runnedMeters,
                                              stats.collectedCoins,
                                              stats.maxScore,
                                              stats.maxDistanceToBolder
                                              );
            if (stats.DistanceToBolder < 20f)
            {

                yield return new WaitUntil(() =>
                {
                    return stats.DistanceToBolder < -4.5f;
                });
            }
            _EndGameUI.SetActive(true);
            Debug.Log($"Твои примеры за этот уровень ({stats.currentLevel}) \n{string.Join(";\n", stats.examples)}");

            TakeScreen();
        }
    }
}