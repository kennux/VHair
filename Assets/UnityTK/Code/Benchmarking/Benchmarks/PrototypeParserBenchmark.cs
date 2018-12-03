using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityTK.Benchmarking;
using UnityTK.Prototypes;

namespace UnityTK.Editor.Benchmarking
{
    /// <summary>
    /// Example benchmark for UnityTK benchmarking
    /// </summary>
    public class PrototypeParserBenchmark : Benchmark
    {
		public class SimplePrototype : IPrototype
		{
			public string identifier { get; set; }
			public int someInt;
		}

		private PrototypeParser parser;

		private string xml;

        protected override void Prepare()
        {
            this.parser = new PrototypeParser();

			StringBuilder sb = new StringBuilder();
			sb.AppendLine("<PrototypeContainer Type=\"SimplePrototype\">");
			sb.AppendLine("	<Prototype Id=\"Base\">\n" +
				"		<someInt>32</someInt>\n" +
				"	</Prototype>");

			for (int i = 0; i < 10000; i++)
			{
				sb.Append("	<Prototype Id=\"Test");
				sb.Append(i.ToString());
				sb.AppendLine("\" Inherits=\"Base\">");
				sb.AppendLine("		<someInt>123</someInt>");
				sb.AppendLine("	</Prototype>");
			}

			sb.AppendLine("</PrototypeContainer>");
			this.xml = sb.ToString();
        }

        protected override void RunBenchmark(BenchmarkResult bRes)
        {
            bRes.BeginLabel("10k simple prototypes load");

			this.parser.Parse(this.xml, "TEST", new PrototypeParseParameters()
			{
				standardNamespace = "UnityTK.Editor.Benchmarking"
			});

            bRes.EndLabel();
        }
    }
}