namespace GoblinzMechanics
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class GoblinCharacter : MonoBehaviour
    {
        [SerializeField] private Rigidbody _body;
        [SerializeField] private float _floorCheckTime = 0.2f;

        private float _inTriggerTime = 0f;
        private bool _inTrigger = false;
        private bool _isJumping = false;

        public bool IsGrounded => _inTriggerTime >= _floorCheckTime;

        private void Update()
        {
            if (_inTrigger)
            {
                _inTriggerTime += Time.deltaTime;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag != "floor") return;
            _inTriggerTime = 0f;
            _inTrigger = true;
            _isJumping = false;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.tag != "floor") return;
            _inTriggerTime = 0f;
            _inTrigger = false;
        }

        public void Jump(Vector3 force)
        {
            if (_isJumping) return;

            _body.AddForce(force, ForceMode.Impulse);
            _isJumping = true;
        }

        public void Move(Vector3 newPos)
        {
            if(_isJumping) return;
            transform.position = newPos;
        }

        public void Crouch(InputActionPhase phase)
        {
            switch (phase)
            {
                case InputActionPhase.Started:
                case InputActionPhase.Performed:
                case InputActionPhase.Canceled:
                default:
                    break;
            }
        }

    }
}