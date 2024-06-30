namespace GoblinzMechanics.Game
{
    using UnityEngine;

    public class BolderController : MonoBehaviour
    {
        [SerializeField] private float _bolderSpeed = 1f;
        [SerializeField] private float _bolderSpeedModificator = 0.01f;

        private void OnEnable()
        {
            GoblinGameManager.Instance.OnDeath.AddListener(() =>
            {
                _bolderSpeed *= 10;
            });
        }

        private void OnDisable()
        {

        }

        private void Update()
        {
            if (GoblinGameManager.Instance.GameState == GoblinGameManager.GameStateEnum.NotStarted) return;
            if (GoblinGameManager.Instance.GameState == GoblinGameManager.GameStateEnum.Pause) return;
            if (GoblinGameManager.Instance.GameState == GoblinGameManager.GameStateEnum.Ended && GoblinGameManager.Instance.IsWin) return;

            _bolderSpeed += _bolderSpeedModificator * Time.deltaTime;
            transform.position = new Vector3(0f, 0f, Mathf.Clamp(transform.position.z
                                                   + (_bolderSpeed - RouteController.Instance.routeSpeedModificator) * Time.deltaTime, -50f, 30f));

            if (transform.position.z < 5f)
            {
                GoblinGameStats.Instance.DistanceToBolder = -transform.position.z;
            }

            if (transform.position.z > -3f && GoblinGameManager.Instance.GameState == GoblinGameManager.GameStateEnum.Playing)
            {
                GoblinGameManager.Instance.EndGame(false);
            }
        }
    }

}