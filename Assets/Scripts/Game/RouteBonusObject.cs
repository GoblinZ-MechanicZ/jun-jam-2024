namespace GoblinzMechanics.Game
{
    using UnityEngine;

    public class RouteBonusObject : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private Collider _bonusCollider;

        public void DestroyBonus()
        {
            // var emmisiion = _particle.emission;
            // emmisiion.enabled = false;
            _meshRenderer.enabled = false;
            _bonusCollider.enabled = false;
        }
    }

    [System.Serializable]
    public class RouteBonus
    {
        public enum RouteBonusType
        {
            Mult2,
            Rocket,
            Sicentist
        }
        public AudioClip bonusClip;
        public float duration;
        public RouteBonusType type;
        public float time = 0;
    }
}