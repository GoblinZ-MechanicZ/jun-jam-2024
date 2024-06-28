namespace GoblinzMechanics.Game
{
    using UnityEngine;
    
    public class RouteObject : MonoBehaviour
    {
        public float routeChance = 100;
        public int id = 0;
        public float length = 6;

        public enum ObjectOnRoutePosition
        {
            Left,
            Middle,
            Right
        }
    }
}