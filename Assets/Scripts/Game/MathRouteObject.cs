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

        [SerializeField] private int _minLength = 5;
        [SerializeField] private TMP_Text _exampleText;
        [SerializeField] private TMP_Text _leftText;
        [SerializeField] private TMP_Text _middleText;
        [SerializeField] private TMP_Text _rightText;
        [SerializeField] private MathAnswerTrigger _leftAnswer;
        [SerializeField] private MathAnswerTrigger _middleAnswer;
        [SerializeField] private MathAnswerTrigger _rightAnswer;
        [SerializeField] private RouteObject _emptySpace;
        [SerializeField] private RouteObject firstObject;
        [SerializeField] private RouteObject lastObject;

        public List<RouteObject> routeObjects = new();

        private void Awake()
        {
            RouteObject routeObject, prevRouteObject;
            routeObjects.Clear();
            routeObjects.Add(firstObject);
            for (int i = 0; i < _minLength * RouteController.Instance.routeSpeedModificator; i++)
            {
                routeObject = Instantiate(_emptySpace, transform);
                routeObject.id = 898;
                prevRouteObject = routeObjects[^1];
                routeObject.transform.SetPositionAndRotation(prevRouteObject.transform.position + Vector3.forward * prevRouteObject.length, Quaternion.identity);
                routeObjects.Add(routeObject);
            }

            prevRouteObject = routeObjects[^1];
            lastObject.transform.SetPositionAndRotation(prevRouteObject.transform.position + Vector3.forward * prevRouteObject.length, Quaternion.identity);
            routeObjects.Add(lastObject);

            example = Random.Range(0, 101) switch
            {
                >= 0 and < 25 => new MathDivClass(),
                >= 25 and < 50 => new MathMultClass(),
                >= 50 and < 75 => new MathSubClass(),
                >= 75 and <= 100 => new MathAddClass(),
                _ => new MathAddClass(),
            };
            _exampleText.text = $"{example.variableA} {example.sign} {example.variableB} = ...";
            SetAnswers();
        }

        private void SetAnswers()
        {
            int varLeft, varRight, varMiddle;

            varLeft = example.GetRandom();
            varMiddle = example.GetRandom();
            varRight = example.GetRandom();

            switch (Random.Range(0, 3))
            {
                case 0:
                    varLeft = example.variableR;
                    _leftAnswer.isValid = true;
                    _middleAnswer.isValid = false;
                    _rightAnswer.isValid = false;
                    break;
                case 1:
                    varMiddle = example.variableR;
                    _leftAnswer.isValid = false;
                    _middleAnswer.isValid = true;
                    _rightAnswer.isValid = false;
                    break;
                case 2:
                    varRight = example.variableR;
                    _leftAnswer.isValid = false;
                    _middleAnswer.isValid = false;
                    _rightAnswer.isValid = true;
                    break;
            }

            _leftAnswer.value = varLeft;
            _middleAnswer.value = varMiddle;
            _rightAnswer.value = varRight;

            _leftText.text = $"{varLeft}";
            _middleText.text = $"{varMiddle}";
            _rightText.text = $"{varRight}";

        }
    }

    public abstract class MathRouteSubClass
    {
        public int variableA;
        public int variableB;
        public int variableR;
        public char sign;
        public MathRouteSubClass()
        {
            variableA = Mathf.RoundToInt(Random.Range(1, 10));
            variableB = Mathf.RoundToInt(Random.Range(0, 10));
        }

        public virtual int GetRandom()
        {
            int result = variableR + Random.Range(1, ((variableA >= variableB) ? variableA : variableB) / 2);
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
            if (variableA < variableB)
            {
                (variableB, variableA) = (variableA, variableB);
            }
            variableR = variableA - variableB;
            sign = '-';
        }
    }
    public class MathMultClass : MathRouteSubClass
    {
        public MathMultClass() : base()
        {
            variableA = Mathf.RoundToInt(Random.Range(1, 10));
            variableB = Mathf.RoundToInt(Random.Range(0, 10));
            variableR = variableA * variableB;
            sign = '*';
        }
    }
    public class MathDivClass : MathRouteSubClass
    {
        public MathDivClass() : base()
        {
            variableB = Random.Range(1, 10);
            variableR = Random.Range(1, 10);
            variableA = variableR * variableB;
            sign = '/';
        }
        public override int GetRandom()
        {
            int result = variableR + Random.Range(1, variableB / 2);
            return result;
        }
    }
}