namespace GoblinzMechanics.Game
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class GoblinCharacter : MonoBehaviour
    {
        [SerializeField] private Rigidbody _body;
        [SerializeField] private float _floorCheckTime = 0.2f;
        [SerializeField] private float _dieNormalZ = -0.8f;
        [SerializeField] private float _dieNormalY = 0.5f;
        [SerializeField] private Animator _characterAnimator;
        [SerializeField] private GameObject _rocket;
        private Vector3 _oldLinVelocity, _oldAngVelocity;

        private float _inTriggerTime = 0f;
        private bool _inTrigger = false;
        private bool _isJumping = false;
        private bool _onRocket = false;

        public bool IsGrounded => _inTriggerTime >= _floorCheckTime;
        public RouteBonus bonus;

        private void OnEnable()
        {
            GoblinGameManager.Instance.OnStateChanged += (newState) =>
            {
                if (newState == GoblinGameManager.GameStateEnum.Pause)
                {
                    _characterAnimator.speed = 0f;
                    _oldLinVelocity = _body.linearVelocity;
                    _oldAngVelocity = _body.angularVelocity;
                    _body.isKinematic = true;
                }
                else
                {
                    _characterAnimator.speed = 1f;
                    _body.isKinematic = false;
                    _body.linearVelocity = _oldLinVelocity;
                    _body.angularVelocity = _oldAngVelocity;
                    _characterAnimator.SetBool("Win", GoblinGameManager.Instance.IsWin);
                    _rocket.SetActive(_onRocket = false);
                }
            };
        }

        private void Update()
        {

            if (_inTrigger)
            {
                _inTriggerTime += Time.deltaTime;
            }

            if (GoblinGameManager.Instance.GameState == GoblinGameManager.GameStateEnum.Playing)
            {
                _characterAnimator.SetFloat("BDistance", GoblinGameStats.Instance.DistanceToBolder);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            HandleJumpTriggerEnter(other);
            HandleMathAnswer(other);
            HandleCoin(other);
            HandleBonus(other);
            HandleMathStart(other);
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
                GoblinGameManager.Instance.EndGame(false);
            }
        }

        public void Jump(Vector3 force)
        {
            if (_isJumping) return;
            if (GoblinGameManager.Instance.GameState != GoblinGameManager.GameStateEnum.Playing) return;
            _characterAnimator.SetBool("Jumping", true);

            _body.AddForce(force, ForceMode.Impulse);
            _isJumping = true;
        }

        public void Move(Vector3 newPos, float xMovement)
        {
            if (_isJumping) return;
            if (GoblinGameManager.Instance.GameState != GoblinGameManager.GameStateEnum.Playing) return;
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

        private void HandleMathStart(Collider other)
        {
            if (!other.CompareTag("mathStart")) return;
            GoblinGameManager.Instance.HandleMathStart();
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

        private void HandleBonus(Collider other)
        {
            if (!other.CompareTag("bonus")) return;

            if (other.TryGetComponent<RouteBonusObject>(out var b))
            {
                bonus = GoblinCharacterController.Instance.GetRandomRouteBonus();
                if (bonus.type == RouteBonus.RouteBonusType.Rocket)
                {
                    HandleRocketStart();
                }
                GoblinGameManager.Instance.HandleBonus(bonus);
                b.DestroyBonus();
            }
        }


        private void HandleRocketStart()
        {
            _rocket.SetActive(_body.isKinematic = _onRocket = true);
            _characterAnimator.SetBool("Rocket", _onRocket);
            transform.position.Set(transform.position.x, 1.6f, transform.position.z);
        }

        private void HandleRocketEnd()
        {
            _rocket.SetActive(_body.isKinematic = _onRocket = false);
            _characterAnimator.SetBool("Rocket", _onRocket);

        }

        public void LookSwitch()
        {
            if (_isJumping) return; if (GoblinGameManager.Instance.GameState != GoblinGameManager.GameStateEnum.Playing || GoblinGameStats.Instance.GameTime < 2f) { return; }
            _characterAnimator.SetBool("LookBack", !_characterAnimator.GetBool("LookBack"));
        }

        public void LookBack()
        {
            _characterAnimator.SetBool("LookBack", true);
        }

        public void LookNormal()
        {
            _characterAnimator.SetBool("LookBack", false);
        }
    }
}