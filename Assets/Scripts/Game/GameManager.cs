namespace GoblinzMechanics.Game
{
    using UnityEngine;
    using GoblinzMechanics.Utils;

    public class GoblinGameManager : Singleton<GoblinGameManager>
    {
        [System.Serializable]
        public enum GameStateEnum {
            NotStarted,
            Playing,
            Ended
        }

        [SerializeField] private GameStateEnum _gameState = GameStateEnum.NotStarted;

        public GameStateEnum GameState => _gameState;

        private void Awake() {
            Application.runInBackground = true;
        }
    }
}