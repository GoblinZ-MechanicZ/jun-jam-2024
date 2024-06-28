namespace GoblinzMechanics
{
    using GoblinzMechanics.Game;
    using UnityEngine;
    using UnityEditor;
    using UnityEngine.InputSystem;

    public class GoblinCharacter : MonoBehaviour
    {
        [SerializeField] private Rigidbody _body;
        [SerializeField] private float _floorCheckTime = 0.2f;
        [SerializeField] private float _dieNormalZ = -0.8f;
        [SerializeField] private float _dieNormalY = 0.5f;

        private float _inTriggerTime = 0f;
        private bool _inTrigger = false;
        private bool _isJumping = false;

        public bool IsGrounded => _inTriggerTime >= _floorCheckTime;

        private void OnEnable()
        {
            GoblinGameManager.Instance.OnStateChanged += () =>
            {
                if (GoblinGameManager.Instance.GameState != GoblinGameManager.GameStateEnum.Playing)
                {
                    _body.useGravity = false;
                    return;
                }
                else
                {
                    _body.useGravity = true;
                }
            };
        }

        private void Update()
        {

            if (_inTrigger)
            {
                _inTriggerTime += Time.deltaTime;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            HandleJumpTriggerEnter(other);
        }

        private void OnTriggerExit(Collider other)
        {
            HandleJumpTriggerExit(other);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.collider.tag == "Untagged") return;

            if (other.contacts[0].normal.z <= _dieNormalZ && other.contacts[0].normal.y <= _dieNormalY)
            {
                GoblinGameManager.Instance.EndGame(true);
            }
        }

        public void Jump(Vector3 force)
        {
            if (GoblinGameManager.Instance.GameState != GoblinGameManager.GameStateEnum.Playing) return;
            if (_isJumping) return;

            _body.AddForce(force, ForceMode.Impulse);
            _isJumping = true;
        }

        public void Move(Vector3 newPos)
        {
            if (GoblinGameManager.Instance.GameState != GoblinGameManager.GameStateEnum.Playing) return;
            if (_isJumping) return;
            transform.position = newPos;
        }

        public void Crouch(InputActionPhase phase)
        {
            if (GoblinGameManager.Instance.GameState != GoblinGameManager.GameStateEnum.Playing) return;
            switch (phase)
            {
                case InputActionPhase.Started:
                case InputActionPhase.Performed:
                case InputActionPhase.Canceled:
                default:
                    break;
            }
        }


        private void HandleJumpTriggerEnter(Collider other)
        {
            if (other.tag != "floor") return;
            _inTriggerTime = 0f;
            _inTrigger = true;
            _isJumping = false;
        }

        private void HandleJumpTriggerExit(Collider other)
        {
            if (other.tag != "floor") return;
            _inTriggerTime = 0f;
            _inTrigger = false;
        }

    }
}