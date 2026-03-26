using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image), typeof(Button))]
public class Tool : HandTarget
{
    public enum Type { cream, lipstick, blush, eyeshadow }

    [Header("Parameters to set")]
    [SerializeField] Type type;
    [SerializeField] int indexMakeUp;
    [SerializeField] Image actualTool;

    [Header("Parameters to read")]
    [SerializeField] bool isTaken;

    private void Start()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(OnToolClick);
    }

    public Type GetToolType()
    {
        return type;
    }
    public int GetMakeUpIndex()
    {
        return indexMakeUp;
    }
    public bool IsColorable()
    {
        return actualTool != null;
    }

    //ћетод нажати€ на инструменот как на кнопку
    private void OnToolClick()
    {
        //ћожно помен€ть на DI
        Hand.instance.SetTarget(this);
    }

    //Ѕерем инструмент с места (делаем едва видимым)
    //¬ случае, если фактический инструмент это не тот, на который нажимает игрок (кисточка), то мен€ем спрайт actualTool
    public void TakeTool()
    {
        if (actualTool != null)
            actualTool.color = new Color(1, 1, 1, 0.25f);
        else
            GetComponent<Image>().color = new Color(1, 1, 1, 0.25f);

        isTaken = true;
    }

    //¬озвращаем инструмент на место (делаем полностью видимым)
    //¬ случае, если фактический инструмент это не тот, на который нажимает игрок (кисточка), то мен€ем спрайт actualTool
    public void ReturnTool()
    {
        if (actualTool != null)
            actualTool.color = Color.white;
        else
            GetComponent<Image>().color = Color.white;

        isTaken = false;
    }

    //¬озвращаем спрайт инструмента
    //¬ случае, если фактический инструмент это не тот, на который нажимает игрок (кисточка), то возвращаем —прайт actualTool
    public Sprite GetSprite()
    {
        if (actualTool != null)
            return actualTool.sprite;
        else
            return GetComponent<Image>().sprite;
    }

    //ћетод вызываетс€ при достижении рукой этого места в качестве своей цели
    //Ѕерем инструмент в руки либо ставим его на место
    public override void HandUse(Hand hand)
    {
        if (hand != null)
        {
            if (hand.GetCurrentTool() == this)
            {
                ReturnTool();
                hand.SetTool(null);
            }
            else
            {
                TakeTool();
                hand.SetTool(this);
            }
        }
    }

    //¬озвращаем нужную позицию на экране
    //¬ случае, если фактический инструмент это не тот, на который нажимает игрок (кисточка), то возвращаем позицию через actualTool
    public override Vector2 GetScreenPosition(Hand hand)
    {
        if (actualTool != null)
        {
            return actualTool.GetComponent<RectTransform>().position;
        }

        return GetComponent<RectTransform>().position;
    }

    //—пециальный метод дл€ инстурмента, который возвращаем именно свою позицию
    //¬ случае паллетки, возвращает позицию паллетки
    public Vector2 GetPalleteScreenPosition(Hand hand)
    {
        return GetComponent<RectTransform>().position;
    }
}
