using System;
using UnityEngine;

public class MobileInputHandler : MonoBehaviour
{
    private Vector3 firstTouchPos;
    private Vector3 lastTouchPos;
    private float dragDistance;

    [SerializeField] private float longTapTime = .5f; //500 ms

    public Action OnSwipeLeft;
    public Action OnSwipeRight;
    public Action OnSwipeUp;
    public Action OnSwipeDown;
    public Action OnTap;
    public Action OnLongTap;

    void Start()
    {
        dragDistance = Screen.height * 15 / 100; //dragDistance is 15% height of the screen
    }

    void Update()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                firstTouchPos = touch.position;
                lastTouchPos = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                lastTouchPos = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                lastTouchPos = touch.position;

                if (Mathf.Abs(lastTouchPos.x - firstTouchPos.x) > dragDistance || Mathf.Abs(lastTouchPos.y - firstTouchPos.y) > dragDistance)
                {
                    if (Mathf.Abs(lastTouchPos.x - firstTouchPos.x) > Mathf.Abs(lastTouchPos.y - firstTouchPos.y))
                    {
                        if ((lastTouchPos.x > firstTouchPos.x))
                        {   //Right swipe
                            OnSwipeRight?.Invoke();
                        }
                        else
                        {   //Left swipe
                            OnSwipeLeft?.Invoke();
                        }
                    }
                    else if (lastTouchPos.y > firstTouchPos.y)
                    {   //Up swipe
                        OnSwipeUp?.Invoke();
                    }
                    else
                    {   //Down swipe
                        OnSwipeDown?.Invoke();
                    }
                }
            }
            else
            {
                if (touch.deltaTime >= longTapTime)
                {
                    OnLongTap?.Invoke();
                }
                else
                {
                    OnTap?.Invoke();
                }
            }
        }
    }
}