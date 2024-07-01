namespace GoblinzMechanics.Game
{
    using GoblinzMechanics.Utils;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class OSTContinuePlay : Singleton<OSTContinuePlay>
    {
        [SerializeField] private AudioSource _source;
        [SerializeField] private float _prevValue;
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            _source.time = _prevValue;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _prevValue = _source.time;

            _source.time = _prevValue;
        }
    }
}