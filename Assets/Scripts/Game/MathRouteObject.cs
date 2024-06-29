namespace GoblinzMechanics.Game
{
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;

    public class MathRouteObject : MonoBehaviour
    {
        public float routeChance = 100;
        public int id = 99;
        public float length = 6;
        public MathRouteSubClass example;
        [SerializeField] private TMP_Text _exampleText;
        [SerializeField] private TMP_Text _leftText;
        [SerializeField] private TMP_Text _middleText;
        [SerializeField] private TMP_Text _rightText;
        [SerializeField] private MathAnswerTrigger _leftAnswer;
        [SerializeField] private MathAnswerTrigger _middleAnswer;
        [SerializeField] private MathAnswerTrigger _rightAnswer;
        public List<RouteObject> routeObjects = new List<RouteObject>();

        private void Awake()
        {
            switch (Random.Range(0, 101))
            {
                case 0:
                case 25:
                    example = new MathDivClass();
                    break;
                case 26:
                case 50:
                    example = new MathMultClass();
                    break;
                case 51:
                case 75:
                    example = new MathSubClass();
                    break;
                case 76:
                case 100:
                    example = new MathAddClass();
                    break;
                default:
                    example = new MathAddClass();
                    break;
            }
            _exampleText.text = $"{example.variableA} {example.sign} {example.variableB} = ...";
            SetAnswers();
        }

        private void SetAnswers()
        {
            switch (Random.Range(0, 3))
            {
                case 0:
                    _leftText.text = $"{example.variableR}";
                    _middleText.text = $"{example.GetRandom()}";
                    _rightText.text = $"{example.GetRandom()}";
                    _leftAnswer.isValid = true;
                    _middleAnswer.isValid = false;
                    _rightAnswer.isValid = false;
                    break;
                case 1:
                    _leftText.text = $"{example.GetRandom()}";
                    _middleText.text = $"{example.variableR}";
                    _rightText.text = $"{example.GetRandom()}";
                    _leftAnswer.isValid = false;
                    _middleAnswer.isValid = true;
                    _rightAnswer.isValid = false;
                    break;
                case 2:
                    _leftText.text = $"{example.GetRandom()}";
                    _middleText.text = $"{example.GetRandom()}";
                    _rightText.text = $"{example.variableR}";
                    _leftAnswer.isValid = false;
                    _middleAnswer.isValid = false;
                    _rightAnswer.isValid = true;
                    break;
            }

        }
    }

    public abstract class MathRouteSubClass
    {
        public float variableA;
        public float variableB;
        public float variableR;
        public char sign;
        public MathRouteSubClass()
        {
            variableA = Mathf.RoundToInt(Random.Range(0, 101));
            variableB = Mathf.RoundToInt(Random.Range(0, 101));
        }

        public virtual int GetRandom()
        {
            int result = 0;
            result = Random.Range(0, (int)((variableA > variableB) ? variableA : variableB) * 2);
            return result;
        }
    }

    public class MathAddClass : MathRouteSubClass
    {
        public MathAddClass() : base()
        {
            variableR = variableA + variableB; ;
            sign = '+';
        }
    }
    public class MathSubClass : MathRouteSubClass
    {
        public MathSubClass() : base()
        {
            variableR = variableA - variableB;
            sign = '-';
        }
    }
    public class MathMultClass : MathRouteSubClass
    {
        public MathMultClass() : base()
        {
            variableA = Mathf.RoundToInt(Random.Range(0, 26));
            variableB = Mathf.RoundToInt(Random.Range(0, 26));
            variableR = variableA * variableB;
            sign = '*';
        }
    }
    public class MathDivClass : MathRouteSubClass
    {
        public MathDivClass() : base()
        {
            variableA = Mathf.RoundToInt(Random.Range(0, 101));
            variableB = Mathf.RoundToInt(Random.Range(1, 101));
            variableR = variableA / variableB;
            sign = '/';
        }
    }
}