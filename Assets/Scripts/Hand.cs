using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class Hand : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private enum State
    {
        Idle,
        TakingTool,
        ColoringTool,
        ReturningTool,
        TargetFace,
        UsingTool
    }

    public static Hand instance { get; private set; }

    [Header("Parameters to set")]
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] Image imageHand;
    [SerializeField] Image imageTool;
    [SerializeField] float speed = 10f;

    [Header("Parameters to read")]
    [SerializeField] private State currentState = State.Idle;
    [SerializeField] bool isShown = true;
    [SerializeField] bool isDragged;

    //Текущий инструмент в руке
    [SerializeField] private TargetData currentTool;

    private HandAnimation handAnimation;    
    private RectTransform rectMain;

    //Позиция по-умолчанию для готовности руки к использованию
    private Vector2 originalPosition;

    //Текущая цель для Руки
    private TargetData target;

    //Класс для хранения объекта, с которым может взаимодейсвтовать рука, и его позиции относительно Руки
    [Serializable] private class TargetData
    {
        public HandTarget handTarget;
        public Vector2 localPosition;
        public TargetData(HandTarget handTarget, Vector2 position)
        {
            this.handTarget = handTarget;
            this.localPosition = position;
        }
    }

    private void Awake()
    {
        rectMain = GetComponent<RectTransform>();
        handAnimation = GetComponent<HandAnimation>();

        //Устанавлаиаем Синглтон для отправки команды SetTarget(HandTarget newTarget)
        if (instance != null)
            Destroy(instance);
        instance = this;
    }

    private void Start()
    {
        //Устанавливаем позицию по-умолчанию для готовности руки к использованию
        originalPosition = rectMain.anchoredPosition;

        Hide();
    }

    #region CurrentTool
    //Установить инструмент в руку. null - убрать инструмент
    public void SetTool(Tool newTool)
    {
        if (newTool != null)
        {
            Vector2 screenPosition = newTool.GetScreenPosition(this);
            Vector2 localPosition = GetLocalPoint(screenPosition);
            currentTool = new TargetData(newTool, localPosition);

            if (imageTool != null)
            {
                imageTool.enabled = true;
                imageTool.sprite = newTool.GetSprite();
            }
        }
        else
        {
            //убрать инструмент
            currentTool = null;

            if (imageTool != null)
                imageTool.enabled = false;
        }
    }

    //Возвращаем текущий инструмент в руках
    public Tool GetCurrentTool()
    {
        if (currentTool != null && currentTool.handTarget is Tool tool)
            return tool;
        return null;
    }
    #endregion

    //Установка новой цели, если сейчас нет таковой
    public void SetTarget(HandTarget newTarget)
    {
        if (target == null)
        {
            if (newTarget is Tool tool)
            {
                //Если новая цель - инструмент, то проверяем, нет ли в руках друго инструмента

                if (GetCurrentTool() == null)   //Рука пустая, просто начинаем движение к цели, чтобы его взять
                {
                    SetState(State.TakingTool);

                    Vector2 screenPosition = tool.GetScreenPosition(this);
                    Vector2 localPosition = GetLocalPoint(screenPosition);
                    target = new TargetData(newTarget, localPosition);
                }
                else if (GetCurrentTool() == tool)  //В руке уже стоит этот инструент, возвращаем его
                {
                    SetState(State.ReturningTool);
                }
                else if (GetCurrentTool().GetToolType() == tool.GetToolType() && tool.IsColorable())    //В руке есть другой интрумент, но этого же типа и может быть перекрашен, перекрашиваем
                {
                    SetState(State.ColoringTool);

                    Vector2 screenPosition = tool.GetPalleteScreenPosition(this);
                    Vector2 localPosition = GetLocalPoint(screenPosition);
                    target = new TargetData(newTarget, localPosition);
                }
                else //В руке полностью другой инструмент, сначала возвращаем (требуемый инструмент остается в переменной target), а потом берём новый
                {
                    SetState(State.ReturningTool);

                    Vector2 screenPosition = tool.GetScreenPosition(this);
                    Vector2 localPosition = GetLocalPoint(screenPosition);
                    target = new TargetData(newTarget, localPosition);
                }
            }
            else if (newTarget is WomanFace face)
            {
                //Если новая цель - лицо, то проверяем, можем ли мы им воспользоваться в данный момент
                //Если всё ок, то просто начинаем движение к цели, чтобы прмиенить инстурмент на лицо
                if (face.IsCanUse(this))
                {
                    SetState(State.TargetFace);

                    Vector2 screenPosition = newTarget.GetScreenPosition(this);
                    Vector2 localPosition = GetLocalPoint(screenPosition);
                    target = new TargetData(newTarget, localPosition);
                }
            }
        }
    }

    //Находим локальную позицию цели на экране относительно Руки
    private Vector2 GetLocalPoint(Vector2 screenPosition)
    {
        RectTransform parentRect = transform.parent as RectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            screenPosition,
            null,
            out Vector2 localPoint
        );

        return localPoint;
    }

    //Установить состояние Руки
    //Взависимости от состояния меняем отображение Руки
    private void SetState(State newState)
    {
        if (currentState != newState)
        {
            currentState = newState;

            if (newState != State.Idle)
            {
                if (!isShown)
                    Show();
            }
            else
            {
                if (GetCurrentTool() == null)
                {
                    if (isShown)
                        Hide();
                }
            }
        }
    }

    #region Update (Дейсвуем в зависимости от состояния Руки)
    private void Update()
    {
        if (isDragged)
            return;

        switch (currentState)
        {
            //Рука свободна в данный момент. Ждёт приказа в виде Цели
            //Если цели нет, то возвращается в позицию по-умолчанию (на груди). Если в руке будет инструмент, то рука будет выглядет готовой к использованию, если инструмента нет, то она всё-равно скроется
            case State.Idle:
                {
                    if (target == null)
                    {
                        UpdatePosition(originalPosition, out _);
                    }
                }
                break;

            //Рука движется к новому инструменту (предположительно он в target)
            //Если инсутрмент нужно дополнительно покрасить, то движемся к краске, иначе Рука будет свободна с интрументом в руках
            case State.TakingTool:
                {
                    if (target != null)
                    {
                        UpdatePosition(target.localPosition, out bool reached);
                        if (reached)
                        {
                            target.handTarget.HandUse(this);

                            if (target.handTarget is Tool tool && tool.IsColorable())
                            {
                                Vector2 screenPosition = tool.GetPalleteScreenPosition(this);
                                target.localPosition = GetLocalPoint(screenPosition);
                                SetState(State.ColoringTool);
                            }
                            else
                            {
                                target = null;
                                SetState(State.Idle);
                            }
                        }
                    }
                    else
                        SetState(State.Idle);
                }
                break;

            //Рука движется к краске (предположительно он в target)
            //Далее Рука будет свободна с интрументов в руках
            case State.ColoringTool:
                {
                    if (target != null)
                    {
                        UpdatePosition(target.localPosition, out bool reached);

                        if (reached)
                        {
                            if (GetCurrentTool() != target.handTarget)
                                target.handTarget.HandUse(this);

                            target = null;
                            SetState(State.Idle);
                        }
                    }
                    else
                    {
                        SetState(State.Idle);
                    }
                }
                break;

            //Рука движется вернуть инструмент на место (где он и лежал изначально) (текущий интрумент в currentTool)
            //Далее, если есть цель взять новый инструмент, то отдаём приказ в виде состояния. Иначе просто освобождаем руку
            case State.ReturningTool:
                {
                    if (GetCurrentTool() != null)
                    {
                        UpdatePosition(currentTool.localPosition, out bool reached);
                        if (reached)
                        {
                            GetCurrentTool().HandUse(this);

                            if (target != null)
                                SetState(State.TakingTool);
                            else
                                SetState(State.Idle);
                        }
                    }
                    else
                        SetState(State.Idle);
                }
                break;

            //Рука движется к лицу прмиенить инструмент (предположительно лицо в target)
            //Далее, если всё впорядке (в руках сейчас есть инструмент), запускаем анимацию использования интрумента на лицо, после чего возвращаем инстурмент на место
            //Состояние UsingTool означает ожидание, пока инструмент закончит использование
            case State.TargetFace:
                {
                    if (target != null)
                    {
                        UpdatePosition(target.localPosition, out bool reached);
                        if (reached)
                        {
                            Tool tool = GetCurrentTool();
                            if (tool != null)
                            {
                                Tool.Type toolType = tool.GetToolType();

                                Action actionOnClipEnd = () =>
                                {
                                    target.handTarget.HandUse(this);
                                    target = null;

                                    //Возвращаем инструмент
                                    SetState(State.ReturningTool);
                                };

                                SetState(State.UsingTool);

                                if (toolType == Tool.Type.lipstick)
                                    RunAnimation(HandAnimation.Type.lipstick, actionOnClipEnd);
                                else if (toolType == Tool.Type.eyeshadow)
                                    RunAnimation(HandAnimation.Type.eyeshadows, actionOnClipEnd);
                                else
                                    RunAnimation(HandAnimation.Type.brush, actionOnClipEnd);
                            }
                            else
                            {
                                target.handTarget.HandUse(this);
                                target = null;

                                SetState(State.Idle);
                            }
                        }
                    }
                    else
                        SetState(State.Idle);
                }
                break;

            default:
                break;
        }
    }

    //Метод перемещения руки
    //Возвращаем isReached: true, если достиг своей цели
    //Сейчас стоит минимальное растояние до объекта 25f
    private void UpdatePosition(Vector2 targetPosition, out bool isReached)
    {
        if (Vector3.Distance(rectMain.anchoredPosition, targetPosition) > 10f)
        {
            rectMain.anchoredPosition = Vector3.Lerp(
            rectMain.anchoredPosition,
            targetPosition,
            Time.deltaTime * speed);

            isReached = false;
        }
        else
        {
            isReached = true;
        }
    }
    #endregion

    #region Show\Hide (Методы плавного скрывания и отображения руки)
    private void Show()
    {
        StopAllCoroutines();
        StartCoroutine(SmoothShow());

        isShown = true;
    }
    private IEnumerator SmoothShow()
    {
        if (imageHand != null)
        {
            float alpha = canvasGroup.alpha;

            int errorCount = 0;
            while (alpha < 1 && errorCount < 9999)
            {
                alpha += Time.deltaTime * 10;
                canvasGroup.alpha = alpha;
                yield return null;
            }

            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
    }
    private void Hide()
    {
        StopAllCoroutines();
        StartCoroutine(SmootHide());

        isShown = false;
    }
    private IEnumerator SmootHide()
    {
        if (imageHand != null)
        {
            float alpha = canvasGroup.alpha;

            int errorCount = 0;
            while (alpha > 0 && errorCount < 9999)
            {
                alpha -= Time.deltaTime * 10;
                canvasGroup.alpha = alpha;
                yield return null;
            }

            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }
    #endregion

    //Запустить анимацию инструмента
    public void RunAnimation(HandAnimation.Type type, Action actionOnClipEnd)
    {
        if (handAnimation != null)
            handAnimation.RunAnimation(type, actionOnClipEnd);
        else
            actionOnClipEnd?.Invoke();
    }

    #region Drag
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragged = true;
    }
    public void OnDrag(PointerEventData eventData)
    {
        var touchPosition = InputManager.instance.currentPosition;
        Vector2 localPos = GetLocalPoint(touchPosition);
        rectMain.anchoredPosition = localPos;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        isDragged = false;

        var touchPosition = InputManager.instance.currentPosition;

        //Проверяем, если ли лицо под пальцем
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(touchPosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Face") && hit.collider.TryGetComponent(out WomanFace womanFace))
            {
                SetTarget(womanFace);
            }
        }
    }
    #endregion

    private void OnDestroy()
    {
        //Очищаем синглтон на выходе на всякий случай
        instance = null;
    }
}
