using System.Collections.Generic;
using UnityEngine;

public class WomanMakeUp : MonoBehaviour
{
    [Header("Parameters to set")]
    [SerializeField] SpriteRenderer dirtFace;
    [SerializeField] List<SpriteRenderer> listLipsticks;
    [SerializeField] List<SpriteRenderer> listEyebrows;
    [SerializeField] List<SpriteRenderer> listBlush;

    [Header("Parameters to read")]
    [SerializeField] bool dirtface = true;
    [SerializeField] int currentLipsticks = -1;
    [SerializeField] int currentEyebrows = -1;
    [SerializeField] int currentBlush = -1;

    //Установить какой-либо мейкап
    //type - тип мейкапа (помада, крем и т.д.)
    //indexMakeUp - номер цвета
    //в случае крема есть только две позиции: нанести крем (0), убрать крем (не 0)
    public void SetMakeUp(Tool.Type type, int indexMakeUp)
    {
        switch (type)
        {
            case Tool.Type.cream:

                if (dirtFace != null)
                {
                    if (indexMakeUp == 0)
                    {
                        dirtFace.enabled = false;
                        dirtface = false;
                    }
                    else
                    {
                        dirtFace.enabled = true;
                        dirtface = true;
                    }
                }
                break;

            case Tool.Type.lipstick:

                if (listLipsticks != null)
                {
                    for (int i = 0; i < listLipsticks.Count; i++)
                    {
                        if (i == indexMakeUp)
                            listLipsticks[i].enabled = true;
                        else
                            listLipsticks[i].enabled = false;
                    }
                }
                currentLipsticks = indexMakeUp;
                break;

            case Tool.Type.blush:

                if (listBlush != null)
                {
                    for (int i = 0; i < listBlush.Count; i++)
                    {
                        if (i == indexMakeUp)
                            listBlush[i].enabled = true;
                        else
                            listBlush[i].enabled = false;
                    }
                }
                currentBlush = indexMakeUp;
                break;

            case Tool.Type.eyeshadow:

                if (listEyebrows != null)
                {
                    for (int i = 0; i < listEyebrows.Count; i++)
                    {
                        if (i == indexMakeUp)
                            listEyebrows[i].enabled = true;
                        else
                            listEyebrows[i].enabled = false;
                    }
                }
                currentEyebrows = indexMakeUp;
                break;

            default:
                break;
        };
    }

    //Убрать весь мейкап
    public void ClearAllMakeUp()
    {
        SetMakeUp(Tool.Type.cream, -1);
        SetMakeUp(Tool.Type.lipstick, -1);
        SetMakeUp(Tool.Type.eyeshadow, -1);
        SetMakeUp(Tool.Type.blush, -1);
    }
}
