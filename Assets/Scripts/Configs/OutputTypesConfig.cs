using System;
using System.Linq;
using UnityEngine;

namespace Configs
{
	public enum OutputType : byte
	{
		None,
		Both,
		Primary,
		Secondary
	}

	[Serializable]
	public class OutputTypeConfig
	{
		public OutputType OutputType;
		public Sprite TypeLogo;
	}

	[CreateAssetMenu(fileName = "OutputTypesConfig", menuName = "Configs/Create output types config")]
	public class OutputTypesConfig : ScriptableObject
	{
		public OutputTypeConfig[] OutputTypes;

		public Sprite GetSprite(OutputType type) =>
			(from ot in OutputTypes where ot.OutputType == type select ot.TypeLogo).FirstOrDefault();
	}
}
