namespace GoblinzMechanics.Utils
{
    using UnityEngine;

    public class UIZRotate : MonoBehaviour
    {
        [SerializeField] private float _zSpeed = 20f;

        private void Update()
        {
            transform.Rotate(0, 0, _zSpeed * Time.deltaTime % 360);
        }

        public void SwapSpeed() {
            _zSpeed = -_zSpeed;
        }
    }

}