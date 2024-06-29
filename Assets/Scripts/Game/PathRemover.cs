namespace GoblinzMechanics.Game
{
    using UnityEngine;

    public class PathRemover : MonoBehaviour
    {
        private RouteObject removing;

        private void OnTriggerEnter(Collider other)
        {
            RouteObject routeObject = other.GetComponentInParent<RouteObject>();
            if (routeObject == null) return;
            if (removing == routeObject) return;
            removing = routeObject;
            RouteController.Instance.DestroyBehind();
        }
    }
}