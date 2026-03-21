using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class Hand : MonoBehaviour
{
    private enum State
    {
        Idle,
        TakingTool,
        ReturningTool,
        TargetFace,
        UsingTool
    }

    public static Hand instance { get; private set; }

    [Header("Parameters to set")]
    [SerializeField] Image imageHand;
    [SerializeField] Image imageTool;
    [SerializeField] Image imageCoverFinger;
    [SerializeField] float speed = 10f;

    [Header("Parameters to read")]
    [SerializeField] private State currentState = State.Idle;
    [SerializeField] bool isShown = true;

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
            Vector2 screenPosition = newTarget.GetScreenPosition(this);
            Vector2 localPosition = GetLocalPoint(screenPosition);

            if (newTarget is Tool tool)
            {
                //Если новая цель - инструмент, то проверяем, нет ли в руках друго инструмента, если есть, то сначала возвращаем (требуемый инстурменрт остается в переменной target)
                //Если рука пустая, то просто начинаем движение к цели, чтобы его взять
                if (currentTool?.handTarget != null && currentTool.handTarget != tool)
                {
                    SetState(State.ReturningTool);
                    target = new TargetData(newTarget, localPosition);
                }
                else
                {
                    SetState(State.TakingTool);
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
                if (currentTool?.handTarget == null)
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
            //Далее Рука будет свободна с интрументов в руках
            case State.TakingTool:
                {
                    if (target != null)
                    {
                        UpdatePosition(target.localPosition, out bool reached);
                        if (reached)
                        {
                            target.handTarget.HandUse(this);
                            target = null;
                            SetState(State.Idle);
                        }
                    }
                    else
                        SetState(State.Idle);
                }
                break;

            //Рука движется вернуть инструмент на место (где он и лежал изначально) (текущий интрумент в currentTool)
            //Далее, если есть цель взять новый инструмент, то отдаём приказ в виде состояния. Иначе просто освобождаем руку
            case State.ReturningTool:
                {
                    if (currentTool != null)
                    {
                        UpdatePosition(currentTool.localPosition, out bool reached);
                        if (reached)
                        {
                            currentTool.handTarget.HandUse(this);

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
        if (Vector3.Distance(rectMain.anchoredPosition, targetPosition) > 25f)
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
        if (imageTool != null)
            imageTool.gameObject.SetActive(true);
        if (imageCoverFinger != null)
            imageCoverFinger.gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(SmoothShow());

        isShown = true;
    }
    private IEnumerator SmoothShow()
    {
        if (imageHand != null)
        {
            float alpha = imageHand.color.a;

            int errorCount = 0;
            while (alpha < 1 && errorCount < 9999)
            {
                alpha += Time.deltaTime * 10;
                imageHand.color = new Color(1, 1, 1, alpha);
                yield return null;
            }
        }
    }
    private void Hide()
    {
        if (imageTool != null)
            imageTool.gameObject.SetActive(false);
        if (imageCoverFinger != null)
            imageCoverFinger.gameObject.SetActive(false);

        StopAllCoroutines();
        StartCoroutine(SmootHide());

        isShown = false;
    }
    private IEnumerator SmootHide()
    {
        if (imageHand != null)
        {
            float alpha = imageHand.color.a;

            int errorCount = 0;
            while (alpha > 0 && errorCount < 9999)
            {
                alpha -= Time.deltaTime * 10;
                imageHand.color = new Color(1, 1, 1, alpha);
                yield return null;
            }
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

    private void OnDestroy()
    {
        //Очищаем синглтон на выходе на всякий случай
        instance = null;
    }
}
