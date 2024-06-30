namespace GoblinzMechanics.Game
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class GoblinCharacterController : MonoBehaviour
    {
        [SerializeField] private GoblinCharacter _character;
        [SerializeField] private float _sideMaxDistance = 2.4f;
        [SerializeField] private float _characterMoveSpeed = 5f;
        [SerializeField] private float _characterJumpForce = 5f;

        [SerializeField] private InputActionAsset _playerControls;
        private string _moveActionName = "Move";
        private string _jumpActionName = "Jump";
        private string _crouchActionName = "Crouch";

        private float _movementInput;

        private Vector3 _velocity;

        private void OnEnable()
        {
            _playerControls.Enable();
            _playerControls[_moveActionName].started += OnMovement;
            _playerControls[_moveActionName].performed += OnMovement;
            _playerControls[_moveActionName].canceled += OnMovement;

            _playerControls[_jumpActionName].started += OnJump;
            _playerControls[_jumpActionName].performed += OnJump;
            _playerControls[_jumpActionName].canceled += OnJump;

            _playerControls[_crouchActionName].started += OnCrouch;
            _playerControls[_crouchActionName].performed += OnCrouch;
            _playerControls[_crouchActionName].canceled += OnCrouch;
            
            _playerControls["LookBack"].started += StartLoockBack;
        }

        private void OnDisable()
        {
            _playerControls.Disable();
            _playerControls[_moveActionName].started -= OnMovement;
            _playerControls[_moveActionName].performed -= OnMovement;
            _playerControls[_moveActionName].canceled -= OnMovement;

            _playerControls[_jumpActionName].started -= OnJump;
            _playerControls[_jumpActionName].performed -= OnJump;
            _playerControls[_jumpActionName].canceled -= OnJump;

            _playerControls[_crouchActionName].started -= OnCrouch;
            _playerControls[_crouchActionName].performed -= OnCrouch;
            _playerControls[_crouchActionName].canceled -= OnCrouch;

            _playerControls["LookBack"].started -= StartLoockBack;
        }

        private void FixedUpdate()
        {
            HandleMovement();
        }

        private void StartLoockBack(InputAction.CallbackContext context) {
            _character.LookBack();
        }

        private void HandleMovement() 
        {
            _velocity.Set(_movementInput * _characterMoveSpeed * Time.deltaTime, 0, 0);

            _character.Move(new Vector3(Mathf.Clamp(_character.transform.position.x + _velocity.x, -_sideMaxDistance, _sideMaxDistance),
                                    _character.transform.position.y,
                                    _character.transform.position.z), _movementInput);
        }

        private void OnMovement(InputAction.CallbackContext context)
        {
            if (context.action.name == _moveActionName)
            {
                _movementInput = context.ReadValue<float>();
            }
        }
        private void OnJump(InputAction.CallbackContext context)
        {
            if (context.action.name == _jumpActionName)
            {
                if (_character.IsGrounded)
                {
                    _character.Jump(Vector3.up * _characterJumpForce);
                }
            }
        }
        private void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.action.name == _crouchActionName)
            {
                _character.Crouch(context.action.phase);
            }
        }
    }
}