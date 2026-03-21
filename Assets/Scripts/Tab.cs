using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Tab : MonoBehaviour
{
    [Header("Parameters to set")]
    [SerializeField] Sprite tabSelected;
    [SerializeField] Sprite tabUnselected;
    [SerializeField] RectTransform rectPage;

    [Header("Parameters to read")]
    [SerializeField] bool isSelected;

    private Image imageMain;

    private void Awake()
    {
        imageMain = GetComponent<Image>();
    }

    public void SetSelect(bool value)
    {
        if (value == true)
        {
            if (tabSelected != null)
                imageMain.sprite = tabSelected;

            if (rectPage != null)
                rectPage.gameObject.SetActive(true);
        }
        else
        {
            if (tabUnselected != null)
                imageMain.sprite = tabUnselected;

            if (rectPage != null)
                rectPage.gameObject.SetActive(false);
        }


        isSelected = value;
    }
}
