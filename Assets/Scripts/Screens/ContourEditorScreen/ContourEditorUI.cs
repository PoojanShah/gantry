using System;
using ContourEditorTool;
using Core;
using Screens.ContourEditorScreen.PopUps;
using Screens.ContourEditorScreen.PopUps.DensityPanel;
using Screens.ContourEditorScreen.Toolbar;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Screens.ContourEditorScreen
{
	public class ContourEditorUI : MonoBehaviour
	{
		[SerializeField] private Transform _canvas;
		[Header("DensityPanel")]
		[SerializeField] private DensityPanel _densityPanel;
		[Header("Toolbar")] 
		[SerializeField] private ToolBarLinesBlock[] _toolBar;
		[SerializeField] private Transform _toolBarTransform;
		[SerializeField] private TMP_Text _title;
		[SerializeField] private Image _currentInstrument;

		[Header("Save popup")] 
		[SerializeField] private SavePopUp _savePopUp;
		[SerializeField] private Button _saveButton;
		[Header("Load popup")] 
		[SerializeField] private LoadPopUp _loadPopUp;
		[SerializeField] private Button _loadButton;

		[Header("Additional buttons")] 
		[SerializeField] private AdditionalButton[] _additionalButtons;

		private ICommonFactory _commonFactory;

		public void Init(ICommonFactory commonFactory)
		{
			_commonFactory = commonFactory;

			foreach (var block in _toolBar)
				block.Init(CloseToolbarSections, SetToolTipByID, OnPointerExit, _currentInstrument);

			foreach (var button in _additionalButtons)
			{
				button.handler.OnPointerEnterAction += () => SetToolTipByID(button.ID.x, button.ID.y, button.ID.z);
				button.handler.OnPointerExitAction += () =>_title.text = string.Empty;
			}

			_currentInstrument.sprite = _toolBar[0].Lines[0].Instruments[0].Button.image.sprite;

			_saveButton.onClick.AddListener(ShowSavePopUp);
			_loadButton.onClick.AddListener(ShowLoadPopUp);
			
			ShowToolbar(false);
		}

		private void ShowToolbar(bool isShow)
		{
			_toolBarTransform.gameObject.SetActive(isShow);

			ContourEditor.IsToolsBlocked = !isShow;

			ContourEditor.ShowLassoObjects(isShow);
		}

		public void ShowDensityPanel()
		{
			_commonFactory.InstantiateObject<DensityPanel>(_densityPanel.gameObject, _canvas)
				.Init(() =>
				{
					ContourEditor.HideGUI = false;

					ShowToolbar(true);
				});
		}

		private void SetToolTipByID(int block, int line, int id)
		{
			_title.text = ContourEditor.instance.toolbar.menus[block]
				.items[line][id]
				.buttonContent.tooltip;
		}

		private void ShowSavePopUp()
		{
			CloseToolbarSections();
			
			ShowToolbar(false);

			var savePopup = _commonFactory.InstantiateObject<SavePopUp>(_savePopUp.gameObject, _canvas);
			savePopup.Init(() => ShowToolbar(true));
		}

		private void ShowLoadPopUp()
		{
			CloseToolbarSections();

			ShowToolbar(false);

			_commonFactory.InstantiateObject<LoadPopUp>(_loadPopUp.gameObject, _canvas)
				.Init(_commonFactory, () => ShowToolbar(true));
		}

		private void CloseToolbarSections()
		{
			foreach (var block in _toolBar)
			foreach (var line in block.Lines)
				line.HideLine();
		}

		private void OnPointerExit() => _title.text = string.Empty;

		private void OnDestroy()
		{
			_saveButton.onClick.RemoveAllListeners();
			_loadButton.onClick.RemoveAllListeners();
		}
	}
}