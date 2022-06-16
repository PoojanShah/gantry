using UnityEngine;

public class ContourEditorUI : MonoBehaviour
{
    public ContourEditor ContourEdit;
    public Toolbar ToolbarMenu;
    public GameObject MainUI;
    public GameObject DensityOptions, ContourEditorMenu;
    public GameObject Darken1, Darken2;
    public GameObject[] OptionMenus;

    public void OnDisplayContourEditor()
    {
        MainUI.SetActive(false);
        DensityOptions.SetActive(true);
        ContourEditorMenu.SetActive(true);
        ShowBlackouts(true);
        ShowOptionMenu(-1); //Hide all option menus
    }

    public void OnHideContourEditor()
    {
        MainUI.SetActive(true);
        DensityOptions.SetActive(false);
        ContourEditorMenu.SetActive(false);
    }

    public void OnVertDensitySelected(int verts)
    {
        ShowBlackouts(false);
        DensityOptions.SetActive(false);
        ContourEditorMenu.SetActive(true);
        ShowOptionMenu(-1);
        ContourEdit.SetVertexAmount(verts);
    }

    public void OnVertexMode()
    {
        ShowOptionMenu(0);
    }

    public void OnRectangularSelection()
    {
        ToolbarMenu.menus[0].SelectItem(0, 0);
    }

    public void OnEllipticalSelection()
    {
        ToolbarMenu.menus[0].SelectItem(0, 1);
    }

    public void OnLassoSelection()
    {
        ToolbarMenu.menus[0].SelectItem(0, 2);
    }

    public void OnBlackoutMode()
    {
        ShowOptionMenu(1);
    }

    public void OnRectangularMask()
    {
        ToolbarMenu.menus[0].SelectItem(1, 0);
    }

    public void OnEllipticalMask()
    {
        ToolbarMenu.menus[0].SelectItem(1, 1);
    }

    public void OnLassoMask()
    {
        ToolbarMenu.menus[0].SelectItem(1, 2);
    }

    public void OnWhiteoutMode()
    {
        ShowOptionMenu(2);
    }

    public void OnRectangularWhiteout()
    {
        ToolbarMenu.menus[0].SelectItem(2, 0);
    }

    public void OnEllipticalWhiteout()
    {
        ToolbarMenu.menus[0].SelectItem(2, 1);
    }

    public void OnLassoWhiteoutButton()
    {
        ToolbarMenu.menus[0].SelectItem(2, 2);
    }

    public void OnScaleMode()
    {
        ShowOptionMenu(3);
    }

    public void OnScaleButton()
    {
        ToolbarMenu.menus[0].SelectItem(3, 0);
    }

    public void OnHorizontalScale()
    {
        ToolbarMenu.menus[0].SelectItem(3, 1);
    }

    public void OnVerticalScale()
    {
        ToolbarMenu.menus[0].SelectItem(3, 2);
    }

    public void OnBackgroundMode()
    {
        ShowOptionMenu(4);
    }

    public void SetContourBackground(int index)
    {
        ContourEdit.SetContourBackground(index);
    }

    public void OnFileMenu()
    {
        ShowOptionMenu(5);
    }

    public void OnEditMenu()
    {
        ShowOptionMenu(6);
    }

    public void Undo()
    {
        ToolbarMenu.menus[2].SelectItem(1, 1);
        //ContourEditor.Undo(-1);
    }

    //Not Working?
    public void Redo()
    {
        ToolbarMenu.menus[2].SelectItem(1, 2);
        //ContourEditor.Undo(1);
    }

    //Not Working?
    public void SelectAll()
    {
        ToolbarMenu.menus[0].SelectItem(2, 0);
        ToolbarMenu.menus[2].SelectItem(1, 3);
        
    }

    public void OnViewMenu()
    {
        ShowOptionMenu(7);
    }

    public void ToggleMirror(int val)
    {
        ContourEditor.ToggleMirror(val);
    }

    private void ShowBlackouts(bool show)
    {
        Darken1.SetActive(show);
        Darken2.SetActive(show);
    }

    private void ShowOptionMenu(int menu)
    {
        if (menu > OptionMenus.Length || menu < 0)
        {
            foreach (var item in OptionMenus)
            {
                item.SetActive(false);
            }
        }
        else
        {
            for (var index = 0; index < OptionMenus.Length; index++)
            {
                OptionMenus[index].SetActive((index == menu));
            }
        }
    }
}
