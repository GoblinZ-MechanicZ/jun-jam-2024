namespace GoblinzMechanics.Utils
{
    using UnityEngine;
    
    public class DontDestroyOnLoad : MonoBehaviour
    {
        private void Awake() {
            DontDestroyOnLoad(gameObject);
        }
    }
}