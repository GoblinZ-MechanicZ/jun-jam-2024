namespace GoblinzMechanics.Game
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject _loading;

        public void PlayGame()
        {
            _loading.SetActive(true);
            var load = SceneManager.LoadSceneAsync(1);

        }

        private IEnumerator StartGameEnum()
        {
            var asyncOp = SceneManager.LoadSceneAsync("Game");

            yield return new WaitUntil(() => asyncOp.isDone);
        }

        public void OpenURL(string url) {
            Application.OpenURL(url);
        }

        public void ExitApp()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit(0);
#endif
        }
    }

}