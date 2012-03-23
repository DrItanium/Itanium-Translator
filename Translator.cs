//Copyright 2012 Joshua Scoggins. All rights reserved.
//
//Redistribution and use in source and binary forms, with or without modification, are
//permitted provided that the following conditions are met:
//
//   1. Redistributions of source code must retain the above copyright notice, this list of
//      conditions and the following disclaimer.
//
//   2. Redistributions in binary form must reproduce the above copyright notice, this list
//      of conditions and the following disclaimer in the documentation and/or other materials
//      provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY Joshua Scoggins ``AS IS'' AND ANY EXPRESS OR IMPLIED
//WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
//FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL Joshua Scoggins OR
//CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
//CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
//SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
//ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
//NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
//ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
//The views and conclusions contained in the software and documentation are those of the
//authors and should not be interpreted as representing official policies, either expressed
//or implied, of Joshua Scoggins. 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Libraries.Collections;
using Libraries.LexicalAnalysis;
using Libraries.Parsing;
using Libraries.Tycho;
using Libraries.Starlight;
using Libraries.Extensions;

namespace Applications.ItaniumTranslator 
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			List<string> str = new List<string>();
			EnhancedGrammar grammar = new EnhancedGrammar
			{
				(Rule)new RuleGenerator("S'")
				{
					{ "Rest", null },
				},
				(Rule)new RuleGenerator("Rest")
				{
					{ "Instructions", null },
				},
				(Rule)new RuleGenerator("Instructions")
				{
					{ "Instructions Instruction", (x) => { str.Add(x[1].ToString()); return null; } },
					{ "Instruction", (x) => { str.Add(x[0].ToString()); return null; } },
				},
				(Rule)new RuleGenerator("Instruction")
				{
					{ "Predicate Instruction'", (x) => string.Format(x[1].ToString(), x[0].ToString()) },
					{ "Instruction'", (x) => string.Format(x[0].ToString(), "p0") },
				},
				(Rule)new RuleGenerator("Instruction'")
				{
					{ "id", (x) => string.Format("({0} {1})", x[0].ToString(), "{0}") },
					{ "id id", (x) => string.Format("({0} {1} {2})", x[0].ToString(), "{0}", x[1].ToString()) },
					{ "id id , id", (x) => string.Format("({0} {1} {2} {3})", x[0].ToString(), "{0}", x[1].ToString(), x[3].ToString()) },
					{ "id id = SourceRegisters", (x) => string.Format("({0} {1} {2} {3})", 
							x[0].ToString(), "{0}", x[1].ToString(), x[3].ToString()) },
					{ "id id , id = SourceRegisters", (x) => string.Format("({0} {1} {2} {3} {4})", x[0].ToString(), "{0}", x[1].ToString(), x[3].ToString(), x[5].ToString()) },
				},
				(Rule)new RuleGenerator("SourceRegisters")
				{
					{ "id", (x) => x[0].ToString().Replace(";;","") },
					{ "id , id", (x) => string.Format("{0} {1}", x[0].ToString(), x[2].ToString()).Replace(";;","") },
					{ "id , id , id", (x) => string.Format("{0} {1} {2}", x[0].ToString(), x[2].ToString(), x[4].ToString().Replace(";;","")) },
					{ "id , id , id , id", (x) => string.Format("{0} {1} {2} {3}", x[0].ToString(), x[2].ToString(), x[4].ToString(), x[6].ToString().Replace(";;","")) },
					{ "id , id , id , id , id", (x) => string.Format("{0} {1} {2} {3} {4}", x[0].ToString(), x[2].ToString(), x[4].ToString(), x[6].ToString(), x[8].ToString().Replace(";;","")) },
				},
				(Rule)new RuleGenerator("Predicate")
				{
					{ "( id )", (x) => x[1] },
				},
			};
			EnhancedLR1ParsableLanguage elpl = new EnhancedLR1ParsableLanguage(
					"ItaniumConverter",
					"1.0",
					"\\[?[\\.\\-\\+0-9a-zA-Z_$]([\\.0-9a-zA-Z_$])*\\]?",
					new Comment[] 
					{ 
					},
					new Symbol[] 
					{
						new Symbol('\n', "Newline", "<new-line>"),
						new Symbol(' ', "Space", "<space>"),
						new Symbol('\t', "Tab", "<tab>"),
						new Symbol('\r', "Carriage Return", "<cr>"),
					},
					new RegexSymbol[]
					{
					//	new GenericInteger("Number", "num"),
					},
					new Keyword[]
					{
						new Keyword("="),
						new Keyword(","),
						new Keyword("("),
						new Keyword(")"),
					},
					LexicalExtensions.DefaultTypedStringSelector,
					grammar,
					"$",
					(x) => 
					{
						StringBuilder sb = new StringBuilder();
						foreach(var v in str)
							sb.AppendLine(v.ToString());
						return sb.ToString();
					},
					true,
					IsValid);
			string msg = Console.In.ReadToEnd().Replace(";;","").Replace("\r\n","\n");
			var result = elpl.Parse(msg).ToString().Replace("[","{").Replace("]","}");
			Console.WriteLine(result);
		}
		public static bool IsValid(Token<string> input)
		{
			switch(input.TokenType)
			{
        case "<cr>":
        case "<space>":
        case "<new-line>":
        case "<tab>":
          return false;
        default: 
          return true;
			}
		}
	}
}
