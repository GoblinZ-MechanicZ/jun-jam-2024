namespace GoblinzMechanics.Game
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "CameraSettings", menuName = "")]
    public class CameraSettings : ScriptableObject
    {
        public float Brightness = 1f;
        public float Saturation = 1f;
        public float Contrast = 1f;
        public float HUE = 0f;
        public bool Enabled = true;

    }

}