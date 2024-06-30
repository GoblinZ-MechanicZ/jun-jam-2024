namespace GoblinzMechanics.Utils
{
    using UnityEngine;

    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();

                    if (_instance == null)
                    {
                        _instance = new GameObject($"[Singleton] {typeof(T).Name}", typeof(T)).GetComponent<T>();
                    }
                }
                return _instance;
            }
        }

        public static bool InstanceNonNull { get { return _instance != null; } }
    }
}