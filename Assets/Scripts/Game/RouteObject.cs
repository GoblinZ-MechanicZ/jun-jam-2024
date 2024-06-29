namespace GoblinzMechanics.Game
{
    using UnityEngine;

    public class RouteObject : MonoBehaviour
    {
        public float routeChance = 100;
        public int id = 0;
        public float length = 6;

        public bool isRotate = false;

        [SerializeField] private Transform _caveModule;

        public void Init(bool rotate)
        {
            isRotate = rotate;

            if (isRotate && _caveModule != null)
            {
                _caveModule.Rotate(Vector3.up, 180);
            }
        }
    }
}