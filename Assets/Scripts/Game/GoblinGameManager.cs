namespace GoblinzMechanics.Game
{
    using UnityEngine;
    using GoblinzMechanics.Utils;
    using TMPro;
    using UnityEngine.InputSystem;
    using UnityEngine.Events;
    using System;

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

        public GameStateEnum GameState
        {
            get
            {
                return _gameState;
            }
            private set
            {
                OnStateChanged?.Invoke();
                _gameState = value;
            }
        }

        public Action OnStateChanged;

        public UnityEvent OnAnyKeyPressed;

        private void Awake()
        {
            Application.runInBackground = true;
            GameState = GameStateEnum.NotStarted;

            ShowStartUI();

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
            _runnedDistance.text = $"{-Mathf.RoundToInt(_pathRoot.position.z)} М\n{(int)(RouteController.Instance.routeCounter % (12 * RouteController.Instance.routeSpeedModificator))}";
            if (GameState != GameStateEnum.Playing) { return; }
            RouteController.Instance.routeSpeedModificator += routeSpeedIncrease * Time.deltaTime;
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
            // GameState = GameStateEnum.Ended;
        }

        private MathRouteSubClass __prevExample;
        public void HandleMathAnswer(MathAnswerTrigger trigger, MathRouteSubClass example)
        {
            if (__prevExample != example)
            {
                Debug.Log($"Получен {(trigger.isValid ? "" : "не")}верный ответ: {trigger.value}; пример: {example.variableA} {example.sign} {example.variableB} = {example.variableR}");
                __prevExample = example;
            }
        }
    }
}