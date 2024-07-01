namespace GoblinzMechanics.Game
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class CameraLookBack : MonoBehaviour
    {
        [SerializeField] private Animation _animation;
        [SerializeField] private bool lookBack;

        [SerializeField] private InputActionAsset inputActions;

        private void OnEnable()
        {
            inputActions["LookBack"].started += StartLoockBack;
            lookBack = true;
            LookBack();
        }

        private void OnDisable()
        {
            inputActions["LookBack"].started -= StartLoockBack;
        }

        public void LookNormal()
        {
            lookBack = false;
            LookNormalAnim();
        }

        public void LookBack()
        {
            lookBack = true;
            LookBackAnim();
        }

        private void StartLoockBack(InputAction.CallbackContext context)
        {
            if (GoblinGameManager.Instance.GameState != GoblinGameManager.GameStateEnum.Playing || GoblinGameStats.Instance.GameTime < 2f) { return; }
            if (lookBack)
            {
                LookNormalAnim();
            }
            else
            {
                LookBackAnim();
            }
            lookBack = !lookBack;
        }

        private void LookBackAnim()
        {
            _animation.Play("CameraLookBack");
        }

        private void LookNormalAnim()
        {
            _animation.Play("CameraLookNormal");
        }
    }

}