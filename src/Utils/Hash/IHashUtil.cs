namespace form_builder.Utils.Hash
{
    public interface IHashUtil
    {
        string Hash(string reference);

        public bool Check(string reference, string hash);
    }
}
