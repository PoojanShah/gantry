using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Core
{
	public static class Extensions
	{
		private const string NullAtring = "(null)";
		private const string EmptyArray = "[]";

		public static string Stringify(this int[] ints)
		{
			if (ints == null)
				return NullAtring;
			else if (ints.Length < 1)
				return EmptyArray;

			string str = Constants.SquareBracerOpen;

			for (int i = 0; i < ints.Length; i++)
				str += ints[i] + Constants.Coma;

			return str.Substring(0, str.Length - 1) + Constants.SquareBracerClose + " (" + ints.Length + ")";
		}

		public static Vector2 ToVector2XZ(this Vector3 v) => new Vector2(v.x, v.z);
		public static Vector2 FlipY(this Vector2 v) => new Vector2(v.x, Screen.height - v.y);
		public static Rect FlipY(this Rect r) => new Rect(r.x, Screen.height - r.y, r.width, r.height);
		public static float[] Elements(this Rect r) => new float[] { r.x, r.y, r.width, r.height };
		public static Color WithAlpha(this Color c, float a) => new Color(c.r, c.g, c.b, a);
	}
}