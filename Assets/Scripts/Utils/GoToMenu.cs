using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToMenu : MonoBehaviour
{
    public void Go()
    {
        SceneManager.LoadScene(0);
    }
}