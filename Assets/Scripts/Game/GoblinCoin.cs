namespace GoblinzMechanics.Game
{
    using UnityEngine;
    
    public class GoblinCoin : MonoBehaviour
    {
        public int value;

        [SerializeField] private ParticleSystem _particle;
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private Collider coinCollider;

        public void DestroyCoin() {
            var emmisiion = _particle.emission;
            emmisiion.enabled = false;
            _meshRenderer.enabled = false;
            coinCollider.enabled = false;
        }
    }
}