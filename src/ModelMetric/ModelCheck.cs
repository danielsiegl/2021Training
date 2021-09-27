using NUnit.Framework;
using System;
using System.IO;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;

namespace ModelMetric.Test
{
	public class ModelCheck
	{
		private readonly OleDbConnectionStringBuilder _builder = new OleDbConnectionStringBuilder();

		public TestReturn RunSQL(string command)
		{
			var result = new TestReturn();
			result.RecordCount = -1;
			result.ResultText = new System.Text.StringBuilder();
			var dataTable = new DataTable();
			using (var cn = new OleDbConnection { ConnectionString = _builder.ConnectionString })
			{
				var sql = command;
				using (var cmd = new OleDbCommand { CommandText = sql, Connection = cn })
				{
					/*
						<Title> 7 Actions have more than one incoming Control Flow</Title>
						<Description> Generate turnLeftCommand</Description >
						<Guid>{1E1129F9-AE37-449c-88AD-B6337778601B}</Guid >
						<EAFile>ea://PowerWindowController.eapx/%7bA2A184EF-3039-4091-B84E-E45CB26B9DC4%7d</EAFile>
						<EAWeb><![CDATA[https://test.lieberlieber.com/webea?m=1&o=1E1129F9-AE37-449c-88AD-B6337778601B]]>
					*/

					cn.Open();
					dataTable.Load(cmd.ExecuteReader());
					result.RecordCount = dataTable.Rows.Count;
					result.ResultText.AppendLine("<ul>");
					foreach (DataRow row in dataTable.Rows)
					{
						result.ResultText.AppendLine("<li>");
						string output = row[0].ToString();
						result.ResultText.AppendLine("<span>" + output + " " + row[1].ToString() + "</span>");
						string strippedGuid = output.Replace("{", "").Replace("}", "");

						result.ResultText.AppendLine("<ul><li>");
						string linkToWeb = $"<a href='{string.Format(Properties.Settings.Default.WebEAURL, strippedGuid)}'>Link To Web</a>";
						result.ResultText.AppendLine(linkToWeb + "</li>");

						string linkToEa = "<li>" + $"<a href='{string.Format(Properties.Settings.Default.EALink, strippedGuid)}'>Link To EA</a>";
						result.ResultText.AppendLine(linkToEa + "</li>");
						result.ResultText.AppendLine("</ul>");
						//ea://RegTest_cb_20.11+VORLAGE_155.eapx/%7b{0}%7d

						Console.WriteLine(output);
						Debug.WriteLine(output);

						result.ResultText.AppendLine("</li>");
					}
					result.ResultText.AppendLine("</ul>");

				}
			}

			return result;
		}

		[SetUp]
		public void Setup()
		{
			string assemblyFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			string modelPath = Path.Combine(assemblyFolder, Properties.Settings.Default.ModelPath);

			if (File.Exists(modelPath))
			{
				_builder.Provider = "Microsoft.Jet.OLEDB.4.0";
				_builder.DataSource = modelPath;
			}
			else
			{
				throw new FileNotFoundException($"The configured Model in \"ModelPath\" can't be found at {Properties.Settings.Default.ModelPath}");
			}
		}

		[Test]
		public void All_Requirements_NOT_connected_with_Realization_OR_trace_to_other_elements()
		{
			Assert.Pass();
			var testReturn = RunSQL(Properties.Settings.Default
				.All_Requirements_NOT_connected_with_Realisation_OR_trace_to_other_elements);
			if (testReturn.RecordCount == 0)
			{
				Assert.Pass("No Requirements are NOT connected with Realization OR trace to other elements");
			}
			else
			{
				var sb = new System.Text.StringBuilder();
				sb.AppendLine($"<br><b>{testReturn.RecordCount} Requirements are NOT connected with Realization OR trace to other elements</b>");
				sb.Append(testReturn.ResultText);

				Assert.Fail(sb.ToString());
			}
		}

		[Test]
		public void Duplicate_Name_in_Package()
		{
			Assert.Pass();
			var testReturn = RunSQL(Properties.Settings.Default.duplicate_Name_in_Package);
			if (testReturn.RecordCount == 0)
			{
				Assert.Pass("No duplicate Names occurred in the same package!");
			}
			else
			{
				var sb = new System.Text.StringBuilder();
				sb.AppendLine($"<br><b>{testReturn.RecordCount} duplicate names occurred in the packages</b>");
				sb.Append(testReturn.ResultText);

				Assert.Warn(sb.ToString());
			}
		}

		[Test]
		public void All_Actions_with_more_than_one_incoming_ControlFlow()
		{
			Assert.Pass();
			var testReturn = RunSQL(Properties.Settings.Default.All_Actions_with_more_than_one_incomming_ControlFlow);
			if (testReturn.RecordCount == 0)
			{
				Assert.Pass("No Actions have more than one incoming Control Flow!");
			}
			else
			{
				var sb = new System.Text.StringBuilder();
				sb.AppendLine($"<br><b>{testReturn.RecordCount} Actions have more than one incoming Control Flow</b>");
				sb.Append(testReturn.ResultText);

				Assert.Fail(sb.ToString());
			}
		}

		[Test]
		public void All_UseCases_without_Notes()
		{
			Assert.Pass();
			var testReturn = RunSQL(Properties.Settings.Default.All_UseCases_without_Notes);
			if (testReturn.RecordCount == 0)
			{
				Assert.Pass("No Uses Cases without Notes Found!");
			}
			else
			{
				var sb = new System.Text.StringBuilder();
				sb.AppendLine($"{testReturn.RecordCount} Uses Cases without Notes Found!");
				sb.Append(testReturn.ResultText);

				Assert.Fail(sb.ToString());
			}
		}
	}
}