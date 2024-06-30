namespace GoblinzMechanics.Game
{
    using UnityEngine;

    public class StepPlayer : MonoBehaviour
    {
        [SerializeField] private bool hitFloor = true;

        [SerializeField] private AudioSource stepSound;
        [SerializeField] private LayerMask floorLayerMask;

        private void OnTriggerExit(Collider other)
        {
            if (hitFloor && floorLayerMask == (floorLayerMask | (1 << (LayerMask)other.gameObject.layer)))
            {
                hitFloor = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!hitFloor && floorLayerMask == (floorLayerMask | (1 << (LayerMask)other.gameObject.layer)))
            {
                stepSound.Play();
                hitFloor = true;
            }
        }
    }
}