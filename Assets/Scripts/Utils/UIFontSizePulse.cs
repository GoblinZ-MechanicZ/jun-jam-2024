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
        private float FontSize => _label.fontSize;
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
                if (FontSize >= _maxSize)
                {
                    _label.fontSize = _maxSize;
                    fwdAnimation = false;
                }
            }
            else
            {
                _label.fontSize -= _morphSpeed * Time.deltaTime;
                if (FontSize <= _startSize)
                {
                    _label.fontSize = _startSize;
                    fwdAnimation = true;
                }
            }
        }
    }

}