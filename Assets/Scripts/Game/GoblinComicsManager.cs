namespace GoblinzMechanics.Game
{
    using System.Collections;
    using System.Collections.Generic;
    using GoblinzMechanics.Utils;
    using UnityEngine;

    public class GoblinComicsManager : Singleton<GoblinComicsManager>
    {
        [SerializeField] private AudioSource comicsSource;
        [SerializeField] private List<ComicsFrame> _startGameComics = new();

        public IEnumerator StartGamePlay()
        {
            foreach (var frame in _startGameComics)
            {
                frame.image.color = Color.black;
                yield return null;
            }
            float frameTimer;
            foreach (var frame in _startGameComics)
            {
                frameTimer = 0;
                if(comicsSource != null && frame.audioClip != null) {
                    comicsSource.PlayOneShot(frame.audioClip);
                }
                while(frameTimer <= frame.duration) {
                    frame.image.color = Color.Lerp(Color.black, Color.white, frameTimer / frame.duration);
                    frameTimer += Time.deltaTime;
                    yield return null;
                }
                if(comicsSource != null) {
                    comicsSource.Stop();
                }
                frame.image.color = Color.white;
            }
        }
    }
}