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
    using System.Collections.Generic;
    using System.Linq;

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
        [SerializeField] private InputActionAsset _playerControls;
        [SerializeField] private VolumeProfile _globalProfile;

        [SerializeField] private Color _wrongColor;
        [SerializeField] private Color _rightColor;
        [SerializeField] private float _vignetteDecreaseSpeed = 0.25f;
        [SerializeField] private CameraLookBack _cameraLookBack;
        [SerializeField] private AudioSource _collectSource;
        [SerializeField] private AudioSource _collectBonusSource;
        [SerializeField] private AudioClip _collectCoin;
        [SerializeField] private AudioClip _winClip;
        [SerializeField] private AudioClip _loseClip;
        [SerializeField] private AudioClip _loseBolderClip;
        [SerializeField] private AudioClip _exampleClip;
        [SerializeField] private AudioClip _wrongAnswer;
        [SerializeField] private AudioClip _rightAnswer;
        [SerializeField] private AudioClip _anyKeyClip;

        [Space]
        [Multiline]
        [SerializeField] private string _endGameTextFormat = " <color=red>Ы</color>ЫЫыть!\n<color=red>У</color>ровень<color=red>:</color> {0}\n<color=red>Т</color>воя <color=red>Z</color>тата<color=red>:</color>\n<color=red>П</color>римеры правильно<color=red>:</color> {1}\n<color=red>П</color>римеры неправильно<color=red>:</color> {2}\n<color=red>П</color>робежал<color=red>:</color> {3:0.00} м\n<color=red>Z</color>тырил монет<color=red>:</color> {4}\n<cSpace=10>[<color=red>Max</color>]</cSpace> счет<color=red>:</color> {5}\n<cSpace=10>[<color=red>Max</color>]</cSpace> дистанция от валуна<color=red>:</color> {6:0.00} м";
        [Space]

        [Space]
        private MathRouteSubClass __prevExample;
        private Vignette _vignette;
        private bool _isWin = false;
        private Color _baseColor;

        [SerializeField] private List<RouteBonus> _bonus = new();

        public GoblinGameStats Stats => GoblinGameStats.Instance;
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

            if (!_playerControls.enabled)
            {
                _playerControls.Enable();
            }
        }

        private void Start()
        {
            _collectSource.PlayOneShot(_anyKeyClip);
        }

        private void OnEnable()
        {
            ShowStartUI();

            _bonus.Clear();

            if (!_playerControls.enabled)
            {
                _playerControls.Enable();
            }
            _playerControls["PressAnyKey"].started += OnAnyKey;
            _playerControls["Pause"].started += OnPauseGame;
            StartCoroutine(ClearNullBonuses());
        }

        private void OnDisable()
        {
            _vignette.intensity.value = 0.25f;
            _vignette.color.value = _baseColor;
            if (_scoreText != null)
            {
                _scoreText.gameObject.SetActive(false);
            }
            _playerControls["PressAnyKey"].started -= OnAnyKey;
            _playerControls["Pause"].started -= OnPauseGame;
        }

        private void Update()
        {
            HandleVignette();
            GoblinGameStats.Instance.GameTime += Time.deltaTime;
            _scoreText.text = $"{Stats.Score}"; //\n{(int)(RouteController.Instance.routeCounter % (12 * RouteController.Instance.routeSpeedModificator))}
            if (GameState != GameStateEnum.Playing) { return; }
            for (int i = _bonus.Count - 1; i >= 0; i--)
            {
                if (_bonus[i] != null && _bonus[i].time >= _bonus[i].duration)
                {
                    if(_bonus[i].type == RouteBonus.RouteBonusType.Rocket) {
                        GoblinCharacterController.Instance.HandleRocketEnd();
                    }
                    _bonus.RemoveAt(i);
                }
                else
                {
                    _bonus[i].time += Time.deltaTime;
                }
            }
            RouteController.Instance.routeSpeedModificator += routeSpeedIncrease * Time.deltaTime;
            Stats.runnedMeters = -Mathf.RoundToInt(_pathRoot.position.z);
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

        private void OnPauseGame(InputAction.CallbackContext context)
        {
            if (Stats.GameTime > 2 && GameState != GameStateEnum.Playing && GameState != GameStateEnum.Pause) { return; }
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
            _cameraLookBack.LookNormal();
            GoblinCharacterController.Instance.LookNormal();
        }

        public void PauseGame()
        {
            if (GameState == GameStateEnum.Playing)
            {
                GameState = GameStateEnum.Pause;
                _statsText.text = string.Format(_endGameTextFormat,
                                                Stats.currentLevel,
                                                Stats.examplesSolved,
                                                Stats.examplesFailed,
                                                Stats.runnedMeters,
                                                Stats.collectedCoins,
                                                Stats.maxScore,
                                                Stats.maxDistanceToBolder
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
            Stats.Reset();
            if (_isWin)
            {
                Stats.currentLevel++;
            }
            else
            {
                Stats.currentLevel = 1;
            }
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void TakeScreen()
        {
            string path, directoryPath;
#if UNITY_EDITOR
            path = $"Screenshots/Screenshot-{DateTime.Now:dd-MM-yy--HH-mm-ss}.png";
#else
            path = $"{Application.persistentDataPath}/Screenshots/Screenshot-{DateTime.Now:dd-MM-yy--HH-mm-ss}.png";
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

        private IEnumerator ClearNullBonuses()
        {
            while (GameState != GameStateEnum.Ended)
            {
                for (int i = _bonus.Count - 1; i >= 0; i--)
                {
                    if (_bonus[i] == null) { _bonus.RemoveAt(i); }
                }
                yield return new WaitForSeconds(10f);
            }
        }

        public void HandleMathStart()
        {
            _collectBonusSource.PlayOneShot(_exampleClip);
        }

        public void HandleMathAnswer(MathAnswerTrigger trigger, MathRouteSubClass example)
        {
            if (__prevExample != example)
            {
                Debug.Log($"Получен {(trigger.isValid ? "" : "не")}верный ответ: {trigger.value}; пример: {example.variableA} {example.sign} {example.variableB} = {example.variableR}");
                __prevExample = example;
                if (_bonus.Count > 0 && _bonus.Find((b) => b.type == RouteBonus.RouteBonusType.Sicentist) != null)
                {
                    Stats.examples.Add($"Example: {example.GetExampleString()} : Answered correct because bonus: {trigger.value}");
                    _vignette.intensity.value = 1f;
                    _vignette.color.value = _rightColor;

                    Stats.examplesSolved++;
                    Stats.Score += Stats.scoreAdd;

                    RouteController.Instance.routeSpeedModificator += 0.02f;
                    _collectBonusSource.PlayOneShot(_rightAnswer);
                }
                else
                {
                    Stats.examples.Add($"Example: {example.GetExampleString()} : Answered {(trigger.isValid ? "correct" : "incorrect")} : {trigger.value}");
                    if (trigger.isValid)
                    {
                        _vignette.intensity.value = 1f;
                        _vignette.color.value = _rightColor;

                        Stats.examplesSolved++;
                        Stats.Score += Stats.scoreAdd;

                        RouteController.Instance.routeSpeedModificator += 0.02f;
                        _collectBonusSource.PlayOneShot(_rightAnswer);
                    }
                    else
                    {
                        _vignette.intensity.value = 1f;
                        _vignette.color.value = _wrongColor;

                        Stats.examplesFailed++;
                        Stats.Score -= Stats.scoreAdd;

                        RouteController.Instance.routeSpeedModificator -= 0.2f;
                        _collectBonusSource.PlayOneShot(_wrongAnswer);
                    }
                }
            }
        }

        public void HandleCoin(int value)
        {
            Debug.Log($"Ха-ха! Очередная монетка падет в мои карманцы!");
            if (_bonus != null && _bonus.Find((b) => b.type == RouteBonus.RouteBonusType.Mult2) != null)
            {
                value *= 2;
            }
            Stats.collectedCoins += value;
            Stats.Score += value;
            _collectSource.PlayOneShot(_collectCoin);
        }

        public void HandleVignette()
        {
            _vignette.intensity.value = Mathf.Clamp(_vignette.intensity.value - _vignetteDecreaseSpeed * Time.deltaTime, 0.25f, 1);
            if (_vignette.intensity.value == 0.25)
            {
                _vignette.color.value = _baseColor;
            }
        }

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
                                              Stats.currentLevel,
                                              Stats.examplesSolved,
                                              Stats.examplesFailed,
                                              Stats.runnedMeters,
                                              Stats.collectedCoins,
                                              Stats.maxScore,
                                              Stats.maxDistanceToBolder
                                              );
            if (!_isWin)
            {
                if (Stats.DistanceToBolder < 20f)
                {

                    yield return new WaitUntil(() =>
                    {
                        return Stats.DistanceToBolder < -4.5f;
                    });
                    _collectBonusSource.PlayOneShot(_loseBolderClip);
                }
                else
                {
                    _collectBonusSource.PlayOneShot(_loseClip);
                }
            }
            else
            {
                _collectBonusSource.PlayOneShot(_winClip);
            }
            _EndGameUI.SetActive(true);
            Debug.Log($"Твои примеры за этот уровень ({Stats.currentLevel}) \n{string.Join(";\n", Stats.examples)}");

            TakeScreen();
        }

        public void HandleBonus(RouteBonus bonus)
        {
            _bonus.Add(bonus);
            _collectBonusSource.PlayOneShot(bonus.bonusClip);
            Debug.Log($"Bonus collected {bonus.type}");
        }
    }
}