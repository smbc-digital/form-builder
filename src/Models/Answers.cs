namespace form_builder.Models
{
    public class Answers
    {
        public Answers() { }

        public Answers(string questionId, dynamic response)
        {
            QuestionId = questionId;
            Response = response;
        }

        public string QuestionId { get; set; }

        public dynamic Response { get; set; }
    }
}