using System.Collections.Generic;
using UnityEngine;

public class TabManager : MonoBehaviour
{
    [Header("Parameters to set")]
    [SerializeField] List<Tab> listTabs;

    [Header("Parameters to read")]
    [SerializeField] int currentTabIndex;

    private void Start()
    {
        UpdateVisual();
    }

    public void SetTab(int newTabIndex)
    {
        if (currentTabIndex != newTabIndex)
        {
            currentTabIndex = newTabIndex;
            UpdateVisual();
        }
    }

    private void UpdateVisual()
    {
        if (listTabs != null)
        {
            for (int tabIndex = 0; tabIndex < listTabs.Count; tabIndex++)
            {
                if (tabIndex == currentTabIndex)
                {
                    listTabs[tabIndex].SetSelect(true);
                }
                else
                {
                    listTabs[tabIndex].SetSelect(false);
                }
            }
        }
    }
}
