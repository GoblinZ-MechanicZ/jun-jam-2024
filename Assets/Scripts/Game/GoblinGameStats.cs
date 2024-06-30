namespace GoblinzMechanics
{
    using System.Collections.Generic;
    using GoblinzMechanics.Game;
    using GoblinzMechanics.Utils;
    using UnityEngine;
    
    public class GoblinGameStats : Singleton<GoblinGameStats>
    {
        public List<string> examples = new List<string>();
        public int examplesSolved = 0;
        public int examplesFailed = 0;
        public int runnedMeters = 0;
        public int collectedCoins = 0;
        public float maxDistanceToBolder = 0f;
        private int _score = 0;
        private float _speed = 0;
        private float _distanceToBolder = 0;
        public int Score {
            get {
                return _score;
            } 
            set {
                if(value > maxScore) {
                    maxScore = value;
                }
                _score = value;

                if(value < 0) {
                    GoblinGameManager.Instance.EndGame(false);
                } else if(value > WinScore) {
                    GoblinGameManager.Instance.EndGame(true);
                }

            }
        }
        public float Speed {
            get {
                return _speed;
            }
            set {
                if(value > maxSpeed) {
                    maxSpeed = value;
                }
                _speed = value;
            }
        }

        public float DistanceToBolder {
            get {
                return _distanceToBolder;
            }
            set {
                if(value > maxDistanceToBolder) {
                    maxDistanceToBolder = value;
                }

                _distanceToBolder = value;
            }
        }
        public int maxScore;
        public float maxSpeed;
        public int scoreAdd = 10;

        public bool fistPlay = true;

        [Space()]
        [Header("WinCondition")]
        public int currentLevel = 1;
        public int levelScoreNeed = 100;
        public int WinScore => currentLevel * levelScoreNeed;

        private void Awake() {
            if(Instance != null && Instance != this) {
                Destroy(gameObject);
            }
        }

        public void Reset() {
            examples.Clear();
            examplesFailed = examplesSolved = runnedMeters = maxScore =collectedCoins = _score = 0;
            _speed = maxDistanceToBolder =  maxSpeed = 0;
        }
    }
}