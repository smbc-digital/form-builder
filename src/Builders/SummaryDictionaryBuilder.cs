﻿using form_builder.Enum;

namespace form_builder.Builders
{
    public class SummaryDictionaryBuilder
    {

        private Dictionary<string, string> _data = new Dictionary<string, string>();
        private List<string> _filesData = new List<string>();

        public void Add(string question, string answer, EElementType type)
        {
            if (string.IsNullOrWhiteSpace(answer))
                return;

            if (type.Equals(EElementType.FileUpload) || type.Equals(EElementType.MultipleFileUpload))
                _filesData.Add($"{question}: {answer}");
            else
                _data.Add(question, answer);
        }

        public Dictionary<string, string> Build()
        {
            if (_filesData.Any())
                _data.Add("Files:", string.Join(", ", _filesData.ToArray()));

            return _data;
        }
    }
}