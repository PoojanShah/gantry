using System;
using ContourEditorTool;
using Screens.ContourEditorScreen.PopUps;
using Screens.ContourEditorScreen.Toolbar;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Screens.ContourEditorScreen
{
	public class ContourEditorUI : MonoBehaviour
	{
		[Header("DensityPanel")]
		[SerializeField] private DensityButton[] _densityButtons;
		[SerializeField] private GameObject _densityPanel;
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

		private Action _hideLines;

		private void Start()
		{
			foreach (var b in _densityButtons)
			{
				b.Button.onClick.AddListener(() =>
				{
					ContourEditor.instance.SetDensity(b.Density);
					
					_densityPanel.SetActive(false);
					_toolBarTransform.gameObject.SetActive(true);
				});
			}
			
			_hideLines += () =>
			{
				foreach (var block in _toolBar)
					foreach (var line in block.Lines)
						line.HideLine();
			};
			
			foreach (var block in _toolBar)
			{
				block.Init(_hideLines,
					SetToolTipByID,
					OnPointerExit,
					_currentInstrument
				);
			}

			foreach (var button in _additionalButtons)
			{
				button.handler.OnPointerEnterAction += () => 
					SetToolTipByID(button.ID.x, button.ID.y, button.ID.z);
				button.handler.OnPointerExitAction += () =>_title.text = "";
			}

			_currentInstrument.sprite = _toolBar[0]
				.Lines[0]
				.Instruments[0]
				.Button
				.image
				.sprite;
			
			_saveButton.onClick.AddListener(() => 
				_savePopUp.gameObject.SetActive(true));
			
			_loadButton.onClick.AddListener(() =>
				_loadPopUp.gameObject.SetActive(true));
			
			_toolBarTransform.gameObject.SetActive(false);
			_savePopUp.gameObject.SetActive(false);
			_loadPopUp.gameObject.SetActive(false);
		}

		public void ShowDensityPanel()
		{
			_densityPanel.gameObject.SetActive(true);
		}

		private void SetToolTipByID(int block, int line, int id)
		{
			_title.text = ContourEditor.instance.toolbar.menus[block]
				.items[line][id]
				.buttonContent.tooltip;
		}
		
		private void OnPointerExit() =>
			_title.text = "";
	}
}