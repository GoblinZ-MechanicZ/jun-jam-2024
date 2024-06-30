namespace GoblinzMechanics.Game
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class CameraLookBack : MonoBehaviour
    {
        [SerializeField] private Animation _animation;
        [SerializeField] private bool lookBack;

        [SerializeField] private InputActionAsset inputActions;

        private void OnEnable() {
            inputActions["LookBack"].started += StartLoockBack;
        }
        
        private void OnDisable() {
            inputActions["LookBack"].started -= StartLoockBack;
        }

        private void StartLoockBack(InputAction.CallbackContext context) {
            if(lookBack) {
                LookNormal();
            } else {
                LookBack();
            }
            lookBack = !lookBack;
        }

        private void LookBack()
        {
            _animation.Play("CameraLookBack");
        }

        private void LookNormal()
        {
            _animation.Play("CameraLookNormal");
        }
    }

}