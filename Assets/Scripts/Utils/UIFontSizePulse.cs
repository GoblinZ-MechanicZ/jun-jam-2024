namespace GoblinzMechanics.Utils
{
    using TMPro;
    using UnityEngine;

    [RequireComponent(typeof(TMP_Text))]
    public class UIFontSizePulse : MonoBehaviour
    {

        [SerializeField] private float _maxSize;
        [SerializeField] private float _morphSpeed;
        private TMP_Text _label;
        private float _fontSize => _label.fontSize;
        private float _startSize;
        private bool fwdAnimation = true;

        private void OnEnable()
        {
            _label = GetComponent<TMP_Text>();
            _startSize = _label.fontSize;
        }

        private void OnDisable()
        {
            _label.fontSize = _startSize;
        }

        private void Update()
        {
            if (fwdAnimation)
            {
                _label.fontSize += _morphSpeed * Time.deltaTime;
            }
            else
            {
                _label.fontSize -= _morphSpeed * Time.deltaTime;
            }

            if (_fontSize <= _startSize || _fontSize >= _maxSize)
            {
                fwdAnimation = !fwdAnimation;
            }
        }
    }

}