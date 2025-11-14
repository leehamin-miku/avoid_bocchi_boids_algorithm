using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class TouchInput : MonoBehaviour
{
    // Start is called before the first frame update
    static TouchInput instance;
    

    private bool isDragging = false;  // 드래그 중인지 여부를 체크하는 플래그
    private Vector2 touchOffset;      // 터치와 오브젝트의 상대 위치 저장
    private GraphicRaycaster raycaster;
    private EventSystem eventSystem;




    public ITouchableObject to; //터치한 오브젝트를 저장하는 변수
    private bool isUITouch; // 이 터지가 UI였는지 검증


    public void TouchInputUpdate()
    {
        // 터치가 있는지 확인
        if (Input.touchCount > 0)
        {
            //if(TouchCheckUI())
            // 첫 번째 터치 가져오기
            Debug.Log("화면 터치됨");


            Touch touch = Input.GetTouch(0);
            Vector2 touchPositionWorld = Camera.main.ScreenToWorldPoint(touch.position);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    PointerEventData pointerEventData = new PointerEventData(eventSystem);
                    pointerEventData.position = touch.position;

                    // UI 요소와의 Raycast 결과 저장
                    List<RaycastResult> results = new List<RaycastResult>();
                    raycaster.Raycast(pointerEventData, results);
                    for (int i = 0; i < results.Count; i++)
                    {

                        to = results[i].gameObject.GetComponent<ITouchableObject>();
                        if (to != null)
                        {
                            if (to.isSubordinateTouch() == false)
                            {

                                to?.TouchThisObject(touch.position);
                                isDragging = true;
                                isUITouch = true;
                                return;
                            }
                        }
                    }






                    RaycastHit2D hit = Physics2D.Raycast(touchPositionWorld, Vector2.zero, 0, LayerMask.GetMask("UnitTouchCollider"));

                    if (hit.collider != null)
                    {
                        to = hit.collider.GetComponent<ITouchableObject>();
                        if (to != null)
                        {
                            to?.TouchThisObject(touch.position);
                            isDragging = true;
                            isUITouch = true;
                            return;
                        }
                    }

                    for (int i = 0; i < results.Count; i++)
                    {

                        to = results[i].gameObject.GetComponent<ITouchableObject>();
                        if (to != null)
                        {
                            if (to.isSubordinateTouch())
                            {

                                //Debug.Log(3+results[i].gameObject.name);
                                to?.TouchThisObject(touch.position);
                                isDragging = true;
                                isUITouch = true;
                                return;
                            }
                        }
                    }
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    // 드래그 중이면 오브젝트를 터치 위치에 따라 이동
                    if (isDragging)
                    {
                        if (isUITouch)
                        {
                            to?.Drag(touch.position);
                        }
                        else
                        {
                            to?.Drag(touchPositionWorld);
                        }
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    // 터치가 끝나거나 취소되면 드래그 중지
                    isDragging = false;

                    if (isUITouch)
                    {
                        to?.TouchEnd(touch.position);
                        to = null;
                        break;
                    }
                    else
                    {
                        to?.TouchEnd(touchPositionWorld);
                        to = null;
                        break;
                    }
            }
        }
    }

    public void Awake()
    {
        instance = this;
        instance.raycaster = GameObject.Find("Canvas").GetComponent<GraphicRaycaster>();
        instance.eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
    }
    public void Update()
    {
        TouchInputUpdate();
    }

    public bool CheckVecIsIn(Vector2 vec, GameObject go, bool isUI)
    {
        if (isUI)
        {
            PointerEventData pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = vec;

            // UI 요소와의 Raycast 결과 저장
            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(pointerEventData, results);

            for (int i = 0; i < results.Count; i++)
            {

                if (results[i].gameObject == go)
                {
                    return true;
                }
            }
        }
        else
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(vec, Vector2.zero);


            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null)
                {
                    if (hit.collider.gameObject == go)
                    {
                        return true;
                    }
                }
            }
            // 맞은 오브젝트가 스프라이트일 경우 드래그 시작 설정

        }

        return false;
    }
    public void OnDestroy()
    {
        instance = null;
    }
}


public class TouchUnit
{
    delegate void TouchStart(Vector2 vec);
    delegate void Drag(Vector2 vec);
    delegate void TouchEnd(Vector2 vec);
}

//후순위 터치 UI의 존재가능성

public interface ITouchableObject //UI또는 암튼 걔한테 부여됨
{
    public void TouchThisObject(Vector2 vec);
    public void Drag(Vector2 vec)
    {

    }
    public void TouchEnd(Vector2 vec)
    {

    }

    //무조건 screen좌표를 받으므로 월드의 경우 변환이 필요함!!

    //UI인 오브젝트 중에 subordinate
    public bool isSubordinateTouch()
    {
        return false;
    }
}