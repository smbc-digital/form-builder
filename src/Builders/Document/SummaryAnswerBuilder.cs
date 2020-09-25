using System.Collections.Generic;
using System.Linq;
using form_builder.Enum;

namespace form_builder.Builders.Document
{
    public class SummaryAnswerBuilder
    {
        private List<string> _data = new List<string>();
        private List<string> _filesData = new List<string>();

        public void Add(string question, string answer, EElementType type){
            if(string.IsNullOrWhiteSpace(answer))
                return;

            if (type == EElementType.FileUpload || type == EElementType.MultipleFileUpload)
                _filesData.Add($"{question}: {answer}");
            else
                _data.Add($"{question}: {answer}");
        }

        public List<string> Build(){
            if (!_filesData.Any()) return _data;

            _data.Add(string.Empty);
            _data.Add("Files:");
            _data.AddRange(_filesData);

            return _data;
        }

        public void AddBlankLine()
        {
            _data.Add("");
        }
    }
}