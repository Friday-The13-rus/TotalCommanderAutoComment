using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Comment
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			if (args.Length < 2)
				return;
			if (!File.Exists(args[0]))
				return;
			if (!Directory.Exists(args[1]))
				return;

			const string descriptionFileName = "descript.ion";

			string itemsCommentPath = args[0];
			string descriptionFilePath = Path.Combine(args[1], descriptionFileName);
			string commentValue = args.Length > 2 ? args[2] : String.Empty;

			IEnumerable<string> linesForComment = ReadLinesForComment(itemsCommentPath);
			IDictionary<string, string> descriptions = ReadDescriptions(descriptionFilePath);

			bool somethingChanged;

			if (string.IsNullOrEmpty(commentValue))
				somethingChanged = RemoveComments(linesForComment, descriptions);
			else
				somethingChanged = ReplaceDescriptions(linesForComment, commentValue, descriptions);

			if (somethingChanged)
				SaveDescriptions(descriptions, descriptionFilePath);
		}

		private static void SaveDescriptions(ICollection<KeyValuePair<string, string>> descriptions, string descriptionFilePath)
		{
			if (descriptions.Count == 0)
			{
				File.Delete(descriptionFilePath);
				return;
			}

			ShowFile(descriptionFilePath);

			using (StreamWriter writer = new StreamWriter(descriptionFilePath, false, Encoding.Default))
			{
				foreach (KeyValuePair<string, string> item in descriptions)
				{
					string pastePattern = item.Key.Contains(' ') ? "\"{0}\" {1}" : "{0} {1}";
					writer.WriteLine(pastePattern, item.Key, item.Value);
				}
			}

			HideFile(descriptionFilePath);
		}

		private static void HideFile(string descriptionFilePath)
		{
			FileAttributes fileAttributes = File.GetAttributes(descriptionFilePath);
			File.SetAttributes(descriptionFilePath, fileAttributes | FileAttributes.Hidden);
		}

		private static void ShowFile(string descriptionFilePath)
		{
			FileAttributes fileAttributes = File.GetAttributes(descriptionFilePath);
			File.SetAttributes(descriptionFilePath, fileAttributes & ~FileAttributes.Hidden);
		}

		private static bool ReplaceDescriptions(IEnumerable<string> linesForComment, string commentValue, IDictionary<string, string> descriptions)
		{
			bool somethingChanged = false;

			foreach (string line in linesForComment)
			{
				string description;
				if (descriptions.TryGetValue(line, out description))
				{
					if (description != commentValue)
					{
						descriptions[line] = commentValue;
						somethingChanged = true;
					}
					continue;
				}

				descriptions.Add(line, commentValue);
				somethingChanged = true;

			}
			return somethingChanged;
		}

		private static bool RemoveComments(IEnumerable<string> linesForComment, IDictionary<string, string> descriptions)
		{
			bool somethingChanged = false;

			foreach (string line in linesForComment)
			{
				descriptions.Remove(line);
				somethingChanged = true;
			}

			return somethingChanged;
		}

		private static IDictionary<string, string> ReadDescriptions(string descriptionsPath)
		{
			if (!File.Exists(descriptionsPath))
			{
				File.Create(descriptionsPath).Dispose();
				return new SortedDictionary<string, string>();
			}

			IEnumerable<string> readedDescriptions = File.ReadAllLines(descriptionsPath, Encoding.Default);

			var parsed = readedDescriptions.Select(ParseDescriptionLine).ToDictionary(el => el.Key, el => el.Value);
			return new SortedDictionary<string, string>(parsed);
		}

		private static IEnumerable<string> ReadLinesForComment(string filePath)
		{
			if (!File.Exists(filePath))
				return new List<string>();

			IEnumerable<string> readedLines = File.ReadAllLines(filePath);
			return readedLines.Select(line => line.TrimEnd('\\')).ToList();
		}

		private static KeyValuePair<string, string> ParseDescriptionLine(string line)
		{
			string key;
			string value;

			int keyEndIndex;

			if (line[0] == '"')
			{
				keyEndIndex = line.IndexOf('"', 1);
				key = line.Substring(1, keyEndIndex - 1);
				value = line.Substring(keyEndIndex + 2);
			}
			else
			{
				keyEndIndex = line.IndexOf(' ');
				key = line.Substring(0, keyEndIndex);
				value = line.Substring(keyEndIndex + 1);
			}
			return new KeyValuePair<string, string>(key, value);
		}
	}
}