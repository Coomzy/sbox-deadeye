using Sandbox.Internal;
using System;

namespace Sandbox.Utils
{
	public class Rename : Attribute, ITitleProvider
	{
		public string Value { get; set; }

		public Rename(string value)
		{
			Value = value;
		}
	}
}
