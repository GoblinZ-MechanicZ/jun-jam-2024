namespace GoblinzMechanics.Game
{
    using UnityEngine;
    using GoblinzMechanics.Utils;
    using TMPro;
    using UnityEngine.InputSystem;
    using UnityEngine.Events;
    using System;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.HighDefinition;

    public class GoblinGameManager : Singleton<GoblinGameManager>
    {
        [System.Serializable]
        public enum GameStateEnum
        {
            NotStarted,
            Playing,
            Ended
        }

        [SerializeField] private GameStateEnum _gameState = GameStateEnum.NotStarted;
        [SerializeField] private float routeSpeedIncrease = 0.01f;
        [SerializeField] private TMP_Text _runnedDistance;
        [SerializeField] private GameObject _prepareUI;
        [SerializeField] private Transform _pathRoot;
        [SerializeField] private InputActionAsset playerControls;
        [SerializeField] private VolumeProfile _globalProfile;

        private Color _baseColor;
        [SerializeField] private Color _wrongColor;
        [SerializeField] private Color _rightColor;
        [SerializeField] private float _vignetteDecreaseSpeed = 0.25f;
        private Vignette _vignette;

        public GoblinGameStats stats;

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

        public Action<GameStateEnum> OnStateChanged;

        public UnityEvent OnAnyKeyPressed;

        private void Awake()
        {
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
            if (_runnedDistance != null)
            {
                _runnedDistance.gameObject.SetActive(false);
            }
            playerControls["PressAnyKey"].started -= OnAnyKey;
        }

        private void Update()
        {
            if (GameState != GameStateEnum.Playing) { return; }
            _runnedDistance.text = $"{stats.Score}"; //\n{(int)(RouteController.Instance.routeCounter % (12 * RouteController.Instance.routeSpeedModificator))}
            RouteController.Instance.routeSpeedModificator += routeSpeedIncrease * Time.deltaTime;
            stats.runnedMeters = -Mathf.RoundToInt(_pathRoot.position.z);
            HandleVignette();
        }

        private void ShowStartUI()
        {
            OnAnyKeyPressed.AddListener(StartGame);
            _runnedDistance.gameObject.SetActive(false);
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
            GameState = GameStateEnum.Playing;
            _runnedDistance.gameObject.SetActive(true);

        }

        public void EndGame(bool isDie)
        {
            if (isDie)
            {
                Debug.Log($"Вы запутались в математическом подземелье и не нашли выход! Ваш счет: {_runnedDistance.text}");
            }
            GameState = GameStateEnum.Ended;
            _vignette.intensity.value = 0.25f;
            _vignette.color.value = _baseColor;
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
                    stats.examplesSolved++;
                    stats.Score += stats.scoreAdd;

                    _vignette.intensity.value = 1f;
                    _vignette.color.value = _rightColor;
                }
                else
                {
                    stats.examplesFailed++;
                    stats.Score -= stats.scoreAdd;

                    _vignette.intensity.value = 1f;
                    _vignette.color.value = _wrongColor;
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
    }
}