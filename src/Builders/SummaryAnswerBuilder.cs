using form_builder.Enum;
using form_builder.Models;

namespace form_builder.Builders.Document
{
	public class SummaryAnswerBuilder
	{
		private List<string> _data = new List<string>();
		private List<string> _filesData = new List<string>();

		public void Add(string question, string answer, EElementType type)
		{
			if (string.IsNullOrWhiteSpace(answer))
				return;

			if (type.Equals(EElementType.FileUpload) || type.Equals(EElementType.MultipleFileUpload))
				_filesData.Add($"<b>{question.Replace("(optional)", "")}</b><br/>{answer}");
			else
				_data.Add($"<b>{question.Replace("(optional)", "")}</b><br/>{answer}");
		}

		public void AddQuestion(string question)
		{
			_data.Add($"{question}");
		}

		public void AddAnswer(string answer)
		{
			if (string.IsNullOrWhiteSpace(answer))
				return;

			_data.Add($"{answer}");
		}

		public List<string> Build()
		{
			if (!_filesData.Any()) return _data;

			_data.Add(string.Empty);
			_data.Add("Files:");
			_data.AddRange(_filesData);

			return _data;
		}

		public void AddBlankLine()
		{
			_data.Add(string.Empty);
		}

		public void AddPageTitle(string pageTitle)
		{
			_data.Add($"<h2>{pageTitle}</h2>");
		}
	}
}