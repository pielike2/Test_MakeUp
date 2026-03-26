using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(WomanMakeUp))]
public class WomanFace : HandTarget, IPointerClickHandler
{
    [Header("Parameters to set")]
    [SerializeField] Transform faceMain;
    [SerializeField] Transform faceLips;
    [SerializeField] Transform faceEyebrows;

    private WomanMakeUp womanMakeUp;

    private void Start()
    {
        womanMakeUp = GetComponent<WomanMakeUp>();
    }

    //Метод нажатия на лицо из IPointerClickHandler
    public void OnPointerClick(PointerEventData eventData)
    {
        //Можно поменять на DI
        Hand.instance.SetTarget(this);
    }

    //Проверка, возможно ли использовать лицо (нужен любой инструмент в руках)
    public bool IsCanUse(Hand hand)
    {
        return hand != null && hand.GetCurrentTool() != null;
    }

    //Метод вызывается при достижении рукой этого места в качестве своей цели
    public override void HandUse(Hand hand)
    {
        if (hand != null)
        {
            Tool tool = hand.GetCurrentTool();
            if (tool != null)
            {
                Tool.Type toolType = tool.GetToolType();
                int indexMakeUp = tool.GetMakeUpIndex();

                //Меняем мейкап
                womanMakeUp.SetMakeUp(toolType, indexMakeUp);
            }
        }
    }

    //Возвращаем нужную позицию на экране
    //Позиция (часть лица) зависит от инструмента
    public override Vector2 GetScreenPosition(Hand hand)
    {
        if (hand != null)
        {
            Tool tool = hand.GetCurrentTool();
            if (tool != null)
            {
                Tool.Type toolType = tool.GetToolType();

                if (toolType == Tool.Type.lipstick && faceLips != null)
                    return Camera.main.WorldToScreenPoint(faceLips.position);
                if (toolType == Tool.Type.eyeshadow && faceEyebrows != null)
                    return Camera.main.WorldToScreenPoint(faceEyebrows.position);
            }
        }
        

        //Иначе возвращаем просто центр лица
        if (faceMain != null)
            return Camera.main.WorldToScreenPoint(faceMain.position);
        else
            return Vector2.zero;
    }

    
}
