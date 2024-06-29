namespace GoblinzMechanics
{
    using GoblinzMechanics.Game;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class GoblinCharacter : MonoBehaviour
    {
        [SerializeField] private Rigidbody _body;
        [SerializeField] private float _floorCheckTime = 0.2f;
        [SerializeField] private float _dieNormalZ = -0.8f;
        [SerializeField] private float _dieNormalY = 0.5f;
        [SerializeField] private Animator _characterAnimator;

        private float _inTriggerTime = 0f;
        private bool _inTrigger = false;
        private bool _isJumping = false;

        public bool IsGrounded => _inTriggerTime >= _floorCheckTime;

        private void OnEnable()
        {
            GoblinGameManager.Instance.OnStateChanged += (newState) =>
            {
                _body.useGravity = newState == GoblinGameManager.GameStateEnum.Playing;
                _characterAnimator.SetBool("Win", newState != GoblinGameManager.GameStateEnum.Playing);
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
            HandleMathAnswer(other);
            HandleCoin(other);
        }

        private void OnTriggerExit(Collider other)
        {
            HandleJumpTriggerExit(other);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!other.collider.CompareTag("obstacle")) return;

            if (other.contacts[0].normal.z <= _dieNormalZ && other.contacts[0].normal.y <= _dieNormalY)
            {
                _characterAnimator.SetBool("Death", true);
                GoblinGameManager.Instance.EndGame(true);
            }
        }

        public void Jump(Vector3 force)
        {
            if (GoblinGameManager.Instance.GameState != GoblinGameManager.GameStateEnum.Playing) return;
            if (_isJumping) return;
            _characterAnimator.SetBool("Jumping", true);

            _body.AddForce(force, ForceMode.Impulse);
            _isJumping = true;
        }

        public void Move(Vector3 newPos, float xMovement)
        {
            if (GoblinGameManager.Instance.GameState != GoblinGameManager.GameStateEnum.Playing) return;
            if (_isJumping) return;
            _characterAnimator.SetInteger("XMovement", (int)xMovement);
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
            if (!other.CompareTag("floor")) return;
            _inTriggerTime = 0f;
            _inTrigger = true;
            _isJumping = false;
            _characterAnimator.SetBool("Jumping", false);
        }

        private void HandleJumpTriggerExit(Collider other)
        {
            if (!other.CompareTag("floor")) return;
            _inTriggerTime = 0f;
            _inTrigger = false;
        }

        private void HandleMathAnswer(Collider other)
        {
            if (!other.CompareTag("mathAnswer")) return;

            if (other.TryGetComponent<MathAnswerTrigger>(out var trigger))
            {
                MathRouteObject mathRouteObject = other.GetComponentInParent<MathRouteObject>();

                if (mathRouteObject != null)
                {
                    GoblinGameManager.Instance.HandleMathAnswer(trigger, mathRouteObject.example);
                }
            }
        }

        private void HandleCoin(Collider other)
        {
            if (!other.CompareTag("coin")) return;

            if (other.TryGetComponent<GoblinCoin>(out var coin))
            {
                GoblinGameManager.Instance.HandleCoin(coin.value);
                coin.DestroyCoin();
            }
        }
    }
}