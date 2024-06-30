namespace GoblinzMechanics.Game
{
    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections;
    
    public class ComicsFrame : MonoBehaviour
    {
        public Image image;
        public AudioClip audioClip;
        public float duration;

        public IEnumerator ShowFrame(float showDuration)
        {
            yield return new WaitForSeconds(showDuration);
        }

        public IEnumerator HideFrame(float showDuration)
        {
            yield return new WaitForSeconds(showDuration);
        }
    }
}