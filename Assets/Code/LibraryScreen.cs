using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LibraryScreen : MonoBehaviour
{
    public Image ExampleFile;
    public Text Boy, Girl, Man, Woman;
    public RectTransform ContentHolder;
    private LinkedList<LibraryFile> _libFiles = new LinkedList<LibraryFile>();
    private bool _reloadLib;
    private bool _showFileExtensions,_simpleMenu=false;
    private int _ticksToLoad = 5;
    private int _curTickCount;

    public void ShowFileExtensions()
    {
        _showFileExtensions = !_showFileExtensions;
        int count = 0;

        foreach (var file in _libFiles)
        {
            file.FileNameText.text = _showFileExtensions ? 
                Settings.library[count] : Path.GetFileNameWithoutExtension(Settings.library[count]);
            count++;
        }
    }
    public void SimpleMenu(){
        Settings.simpleMenu=GameObject.Find("SimpleMenuToggle").GetComponent<Toggle>().isOn;
    }

    public void AllButtonClicked()
    {
        foreach (var file in _libFiles)
        {
            file.BoyToggle.isOn = file.GirlToggle.isOn = file.ManToggle.isOn = file.WomanToggle.isOn = true;
        }

        UpdateToggleAmounts();
    }

    public void NoneButtonClicked()
    {
        foreach (var file in _libFiles)
        {
            file.BoyToggle.isOn = file.GirlToggle.isOn = file.ManToggle.isOn = file.WomanToggle.isOn = false;
        }

        UpdateToggleAmounts();
    }

    public void NextColorClicked(GameObject callingObj)
    {
        var index = Int32.Parse(callingObj.transform.parent.name);
        var libFile = callingObj.transform.parent.GetComponent<LibraryFile>();
        ChangeColor(index, libFile, true);
    }

    public void PrevColorClicked(GameObject callingObj)
    {
        var index = Int32.Parse(callingObj.transform.parent.name);
        var libFile = callingObj.transform.parent.GetComponent<LibraryFile>();
        ChangeColor(index, libFile, false);
    }

    public void ToggleChanged(GameObject objChanged)
    {
        var toggle = objChanged.GetComponent<Toggle>();
        var catIndex = toggle.name.Equals("BoyToggle") ? 0 :
            toggle.name.Equals("GirlToggle") ? 1 :
            toggle.name.Equals("ManToggle") ? 2 : 3;
        var libIndex = Int32.Parse(objChanged.transform.parent.name);

        if (toggle.isOn)
        {
            if (!Settings.categories[catIndex].Contains(Settings.library[libIndex]))
            {
                Settings.categories[catIndex].Concat(new[] { Settings.library[libIndex] });
            }
        }
        else
        {
            if (Settings.categories[catIndex].Contains(Settings.library[libIndex]))
            {
                Settings.categories[catIndex] =
                    Settings.categories[catIndex].Where(w => w != Settings.library[libIndex]).ToArray();
            }
        }

        UpdateToggleAmounts();
    }

    public void SaveLibrarySettings()
    {
        UpdateCategoryData();
        StreamWriter sw;
        try
        {
            sw = new StreamWriter(Settings.categoryFile);

            foreach (var category in Settings.categories)
            {
                List<string> crossChecked = new List<string>();
                for (int j = 0; j < category.Length; j++) if (Settings.library.Contains(category[j])) crossChecked.Add(category[j]);
                sw.WriteLine(string.Join(",", crossChecked.ToArray()));
            }

            sw.Close();
        }
        catch (Exception e)
        {
            Debug.LogError("Error writing file " + Settings.categoryFile + ": " + e);
        }
        try
        {
            sw = new StreamWriter(Settings.movieColorFile);
            foreach (KeyValuePair<string, string> kvp in Settings.videoColor) sw.WriteLine(kvp.Key + ":" + kvp.Value);
            sw.Close();
        }
        catch (Exception e)
        {
            Debug.LogError("Error writing file " + Settings.movieColorFile + ": " + e);
        }

        gameObject.SetActive(false);
    }

    void Update()
    {
        if (_reloadLib && _curTickCount >= _ticksToLoad)
        {
            var ratioFitter = GetComponent<AspectRatioFitter>();
            if (ratioFitter != null)
            {
                Destroy(ratioFitter);
            }

            _reloadLib = false;

            if (_libFiles.Count > 0)
            {
                _libFiles.RemoveFirst();

                while (_libFiles.Count > 1)
                {
                    var fileGObject = _libFiles.Last.Value.gameObject;                    
                    Destroy(fileGObject);
                    _libFiles.RemoveLast();
                }

                ContentHolder.sizeDelta = new Vector2(0, 300);
            }

            var height = ExampleFile.rectTransform.rect.height;
            var spacing = height * 0.2f;
            var genNewFiles = Settings.library.Length;
            var totalHeightNeeded = CalcTotalHeight(genNewFiles);

            if (genNewFiles == 0)
            {
                ExampleFile.gameObject.SetActive(false);
                return;
            }
            ExampleFile.gameObject.SetActive(true);

            if (ContentHolder.rect.height < totalHeightNeeded)
            {
                ContentHolder.sizeDelta = new Vector2(0, totalHeightNeeded);
            }

            //Populate first file's data.
            var exLibFile = ExampleFile.GetComponent<LibraryFile>();
            exLibFile.name = "0";
            exLibFile.FileNameText.text = Path.GetFileNameWithoutExtension(Settings.library[0]);
            exLibFile.ColorText.text = Settings.videoColor[Settings.library[0]];
            exLibFile.ColorText.color = Settings.colorDefaults.FirstOrDefault(cd => cd.Key == Settings.videoColor[Settings.library[0]]).Value;
            exLibFile.BoyToggle.isOn = Settings.categories[0].Contains(Settings.library[0]);
            exLibFile.GirlToggle.isOn = Settings.categories[1].Contains(Settings.library[0]);
            exLibFile.ManToggle.isOn = Settings.categories[2].Contains(Settings.library[0]);
            exLibFile.WomanToggle.isOn = Settings.categories[3].Contains(Settings.library[0]);
            _libFiles.AddLast(exLibFile);

            for (int i = 1; i < genNewFiles; i++)
            {
                var clone = Instantiate(ExampleFile).GetComponent<Image>();
                var libFile = clone.GetComponent<LibraryFile>();
                _libFiles.AddLast(libFile);
                libFile.name = "" + i;

                try
                {
                    libFile.FileNameText.text = Path.GetFileNameWithoutExtension(Settings.library[i]);
                    libFile.ColorText.text = Settings.videoColor[Settings.library[i]];
                    libFile.ColorText.color = Settings.colorDefaults.FirstOrDefault(cd => cd.Key == Settings.videoColor[Settings.library[i]]).Value;
                    libFile.BoyToggle.isOn = Settings.categories[0].Contains(Settings.library[i]);
                    libFile.GirlToggle.isOn = Settings.categories[1].Contains(Settings.library[i]);
                    libFile.ManToggle.isOn = Settings.categories[2].Contains(Settings.library[i]);
                    libFile.WomanToggle.isOn = Settings.categories[3].Contains(Settings.library[i]);
                    // categories[j].Contains(library[i]);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                
                clone.transform.parent = ContentHolder.transform;
                clone.rectTransform.anchoredPosition = ExampleFile.rectTransform.anchoredPosition;
                clone.rectTransform.offsetMin = ExampleFile.rectTransform.offsetMin;
                clone.rectTransform.offsetMax = ExampleFile.rectTransform.offsetMax;
                clone.rectTransform.ForceUpdateRectTransforms();
                clone.rectTransform.anchoredPosition = new Vector2(ExampleFile.rectTransform.anchoredPosition.x, ExampleFile.rectTransform.anchoredPosition.y - (i * height + i * spacing));
            }

            UpdateToggleAmounts();
        }
        
        if (_reloadLib)
        {
            _curTickCount++;
        }
    }

    public void ShowLibraryOptions()
    {
        if (_libFiles.Count > 0 && _libFiles.Count == Settings.library.Length)
        {
            var notUnique = true;
            var count = 0;

            foreach (var libFile in _libFiles)
            {
                if ((_showFileExtensions ? 
                    Settings.library[count] : Path.GetFileNameWithoutExtension(Settings.library[count]))
                    .Equals(libFile.FileNameText.text))
                {
                    notUnique = false;
                    break;
                }

                count++;
            }

            if (notUnique) return;
        }

        _curTickCount = 0;
        _reloadLib = true;
        GameObject.Find("SimpleMenuToggle").GetComponent<Toggle>().isOn=Settings._simpleMenu;//SRS
    }

    private float CalcTotalHeight(int amount)
    {
        var fileHeight = ExampleFile.rectTransform.rect.height;
        var spacing = fileHeight * 0.2f;
        var totalHeight = 0f;

        for (var i = 0; i < amount; i++)
        {
            totalHeight += fileHeight;
            if (i != (amount - 1))
            {
                totalHeight += spacing;
            }
        }

        return totalHeight;
    }

    private void ChangeColor(int index, LibraryFile libFile, bool next)
    {
        Settings.videoColor[Settings.library[index]] = Settings.colorDefaults[SRSUtilities.Wrap(Settings.colorDefaults.IndexOfFirstMatch(cd => cd.Key == Settings.videoColor[Settings.library[index]]) + (next ? 1 : -1), Settings.colorDefaults.Length)].Key;
        libFile.ColorText.text = Settings.videoColor[Settings.library[index]];
        libFile.ColorText.color = Settings.colorDefaults.FirstOrDefault(cd => cd.Key == Settings.videoColor[Settings.library[index]]).Value;
    }

    private void UpdateToggleAmounts()
    {
        int boy = 0, girl = 0, man = 0, woman = 0;

        foreach (var file in _libFiles)
        {
            if (file.BoyToggle.isOn) boy++;
            if (file.GirlToggle.isOn) girl++;
            if (file.ManToggle.isOn) man++;
            if (file.WomanToggle.isOn) woman++;
        }

        Boy.text = "Boy\n(" + boy + ")";
        Girl.text = "Girl\n(" + girl + ")";
        Man.text = "Man\n(" + man + ")";
        Woman.text = "Woman\n(" + woman + ")";
    }

    private void UpdateCategoryData()
    {
        var boy = new LinkedList<string>();
        var girl = new LinkedList<string>();
        var man = new LinkedList<string>();
        var woman = new LinkedList<string>();
        var count = 0;

        foreach (var file in _libFiles)
        {
            if (file.BoyToggle.isOn) boy.AddLast(Settings.library[count]);
            if (file.GirlToggle.isOn) girl.AddLast(Settings.library[count]);
            if (file.ManToggle.isOn) man.AddLast(Settings.library[count]);
            if (file.WomanToggle.isOn) woman.AddLast(Settings.library[count]);

            count++;
        }

        Settings.categories[0] = boy.ToArray();
        Settings.categories[1] = girl.ToArray();
        Settings.categories[2] = man.ToArray();
        Settings.categories[3] = woman.ToArray();
    }
}
