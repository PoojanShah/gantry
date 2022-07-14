using System.Collections.Generic;
using UnityEngine;

namespace Core
{
	public static class Constants
	{
		public const string ZeroString = "0",
			Colon = ":",
			Comma = ",",
			ClosingBracket = ")",
			OpeningBracket = "(",
			CorrectAdminPassword = "Kim41",
			CorrectSuperPassword = "Jas375",
			BoyHash = "Boy",
			GirlHash = "Girl",
			ManHash = "Man",
			WomanHash = "Woman",
			ExtensionMeta = ".meta",
			GantryExtension = ".gantry",
			AllFilesPattern = "*.*";

		public const float ScrollbarDefaultValue = 1.0f;

		public static KeyValuePair<string, Color32>[] colorDefaults = {
			new("white",new Color32(255,255,255,255)),
			new("black",new Color32(0,0,0,255)),
			new("maroon",new Color32(128,0,0,255)),
			new("firebrick",new Color32(178,34,34,255)),
			new("crimson",new Color32(220,20,60,255)),
			new("red",new Color32(255,0,0,255)),
			new("tomato",new Color32(255,99,71,255)),
			new("coral",new Color32(255,127,80,255)),
			new("orange",new Color32(255,165,0,255)),
			new("gold",new Color32(255,215,0,255)),
			new("yellow",new Color32(255,255,0,255)),
			new("yellow green",new Color32(154,205,50,255)),
			new("green yellow",new Color32(173,255,47,255)),
			new("green",new Color32(0,128,0,255)),
			new("lime green",new Color32(50,205,50,255)),
			new("light green",new Color32(144,238,144,255)),
			new("spring green",new Color32(0,255,127,255)),
			new("medium aqua marine",new Color32(102,205,170,255)),
			new("light sea green",new Color32(32,178,170,255)),
			new("cyan",new Color32(0,255,255,255)),
			new("turquoise",new Color32(64,224,208,255)),
			new("pale turquoise",new Color32(175,238,238,255)),
			new("corn flower blue",new Color32(100,149,237,255)),
			new("deep sky blue",new Color32(0,191,255,255)),
			new("dodger blue",new Color32(30,144,255,255)),
			new("light sky blue",new Color32(135,206,250,255)),
			new("navy",new Color32(0,0,128,255)),
			new("blue",new Color32(0,0,255,255)),
			new("royal blue",new Color32(65,105,225,255)),
			new("indigo",new Color32(75,0,130,255)),
			new("medium slate blue",new Color32(123,104,238,255)),
			new("medium purple",new Color32(147,112,219,255)),
			new("dark magenta",new Color32(139,0,139,255)),
			new("dark orchid",new Color32(153,50,204,255))
		};
	}
}
