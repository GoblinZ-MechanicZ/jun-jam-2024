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
            Playing,
            Ended
        }

        [SerializeField] private GameStateEnum _gameState = GameStateEnum.NotStarted;
        [SerializeField] private float routeSpeedIncrease = 0.01f;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _endGameText;
        [SerializeField] private GameObject _prepareUI;
        [SerializeField] private GameObject _EndGameUI;
        [SerializeField] private Transform _pathRoot;
        [SerializeField] private InputActionAsset playerControls;
        [SerializeField] private VolumeProfile _globalProfile;

        private Color _baseColor;
        [SerializeField] private Color _wrongColor;
        [SerializeField] private Color _rightColor;
        [SerializeField] private float _vignetteDecreaseSpeed = 0.25f;
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
        public UnityEvent OnWin;
        public UnityEvent OnDeath;

        private bool _isWin = false;

        private void OnDestroy() {
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
        }

        private void Update()
        {
            HandleVignette();
            if (GameState != GameStateEnum.Playing) { return; }
            _scoreText.text = $"{stats.Score}"; //\n{(int)(RouteController.Instance.routeCounter % (12 * RouteController.Instance.routeSpeedModificator))}
            RouteController.Instance.routeSpeedModificator += routeSpeedIncrease * Time.deltaTime;
            stats.runnedMeters = -Mathf.RoundToInt(_pathRoot.position.z);
        }

        private void ShowStartUI()
        {
            OnAnyKeyPressed.AddListener(StartGame);
            _scoreText.gameObject.SetActive(false);
            _prepareUI.SetActive(true);
        }

        private void OnAnyKey(InputAction.CallbackContext context)
        {
            OnAnyKeyPressed?.Invoke();
        }

        public void StartGame()
        {
            OnAnyKeyPressed.RemoveListener(StartGame);
            _prepareUI.SetActive(false);
            _EndGameUI.SetActive(false);
            GameState = GameStateEnum.Playing;
            _scoreText.gameObject.SetActive(true);
            _isWin = false;
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
            ShowEndUI(_isWin);
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

        [SerializeField] private string _endGameTextFormat = "ЫЫЫыть {0}!\nУровень: {1}\nТвоя стата:\nРешил примеры правильно: {2}\nРешил примеры неправильно: {3}\nПробежал: {4} м\nСтырил монет: {5}\nМаксимальный счет: {6}\nМаксимальное расстояние от булыги: {7} м";
        private void ShowEndUI(bool isWin)
        {
            StartCoroutine(PlayShowEndUI(isWin));
        }
        private IEnumerator PlayShowEndUI(bool isWin)
        {
            if (_scoreText != null)
            {
                _scoreText.gameObject.SetActive(false);
            }
            _endGameText.text = string.Format(_endGameTextFormat,
                                  GetWinMsg(isWin),
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

        private string GetWinMsg(bool isWin)
        {
            return isWin ? "Победа" : "Проигрышь";
        }
    }
}