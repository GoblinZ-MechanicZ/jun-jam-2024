namespace GoblinzMechanics
{
    using System.Collections.Generic;
    using GoblinzMechanics.Game;
    using UnityEngine;
    
    public class GoblinGameStats : MonoBehaviour
    {
        public List<string> examples = new List<string>();
        public int examplesSolved = 0;
        public int examplesFailed = 0;
        public int runnedMeters = 0;
        public int collectedCoins = 0;
        public float maxSpeed = 0f;
        public float maxDistanceToBolder = 0f;
        private int _score = 0;
        public int Score {
            get {
                return _score;
            } 
            set {
                if(value > maxScore) {
                    maxScore = value;
                }

                if(value < 0) {
                    GoblinGameManager.Instance.EndGame(true);
                }

                _score = value;
            }
        }
        public int maxScore;
        public int scoreAdd = 10;
    }
}