using System.Collections.Generic;
using Axion.Tokens;

namespace Axion
{
	internal class ScriptFile
	{
		public readonly List<Token> Imports;
		public readonly List<Token> Tokens;

		public ScriptFile()
		{
			Imports = new List<Token>();
			Tokens  = new List<Token>();
		}
	}
}