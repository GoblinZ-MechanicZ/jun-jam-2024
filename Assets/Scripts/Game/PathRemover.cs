namespace GoblinzMechanics.Game
{
    using UnityEngine;

    public class PathRemover : MonoBehaviour
    {
        [SerializeField] private RouteController routeController;
        private RouteObject removing;

        private void OnTriggerEnter(Collider other)
        {
            RouteObject routeObject = other.GetComponentInParent<RouteObject>();
            if(removing == routeObject) return;
            removing = routeObject;
            if (routeObject == null) return;
            routeController.DestroyBehind();
        }
    }
}