namespace GoblinzMechanics.Game
{
    using System.Collections;
    using System.Collections.Generic;
    using GoblinzMechanics.Utils;
    using UnityEngine;
    using UnityEngine.Events;

    public class GoblinComicsManager : Singleton<GoblinComicsManager>
    {
        [SerializeField] private AudioSource _comicsSource;
        [SerializeField] private float _fadeOutDuration = 0.5f;
        [SerializeField] private List<ComicsFrame> _startGameComics = new();
        [SerializeField] private List<ComicsFrame> _startSecondGameComics = new();

        public UnityEvent OnAllComicsDone;

        private void Start()
        {
            StartCoroutine(StartGamePlay());
        }

        public IEnumerator StartGamePlay()
        {
            if (GoblinGameStats.InstanceNonNull)
            {
                OnAllComicsDone?.Invoke();
                yield break;
            }

            yield return PlayComics(_startGameComics);
            yield return PlayComics(_startSecondGameComics);
            OnAllComicsDone?.Invoke();
        }

        private IEnumerator PlayComics(List<ComicsFrame> comics)
        {
            foreach (var frame in comics)
            {
                frame.image.color = new Color(0, 0, 0, 0);
                frame.gameObject.SetActive(true);
            }

            float frameTimer = 0f;
            while (frameTimer <= _fadeOutDuration)
            {
                foreach (var frame in comics)
                {
                    frame.image.color = Color.Lerp(new Color(0, 0, 0, 0), Color.black, frameTimer / _fadeOutDuration);

                }
                frameTimer += Time.deltaTime;
                yield return null;
            }
            foreach (var frame in comics)
            {
                frame.image.color = Color.black;
            }
            foreach (var frame in comics)
            {
                frameTimer = 0;
                while (frameTimer <= _fadeOutDuration)
                {
                    frame.image.color = Color.Lerp(Color.black, Color.white, frameTimer / _fadeOutDuration);
                    frameTimer += Time.deltaTime;
                    yield return null;
                }

                frame.image.color = Color.white;
                if (_comicsSource != null && frame.audioClip != null)
                {
                    _comicsSource.PlayOneShot(frame.audioClip);
                }

                yield return new WaitForSeconds(frame.duration);

                if (_comicsSource != null)
                {
                    _comicsSource.Stop();
                }
            }

            frameTimer = 0f;
            while (frameTimer <= _fadeOutDuration)
            {
                foreach (var frame in comics)
                {
                    frame.image.color = Color.Lerp(Color.white, new Color(0, 0, 0, 0), frameTimer / _fadeOutDuration);
                }
                frameTimer += Time.deltaTime;
                yield return null;
            }

            foreach (var frame in comics)
            {
                frame.image.color = new Color(0, 0, 0, 0);
                frame.gameObject.SetActive(false);
            }
        }
    }
}