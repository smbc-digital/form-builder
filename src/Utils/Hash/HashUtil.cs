using System;
using System.Security.Cryptography;
using System.Text;
using form_builder.Configuration;
using Microsoft.Extensions.Options;

namespace form_builder.Utils.Hash
{
    public class HashUtil : IHashUtil
    {
        private readonly HashConfiguration _hashConfiguration;

        public HashUtil(IOptions<HashConfiguration> hashConfiguration) => _hashConfiguration = hashConfiguration.Value;

        public string Hash(string reference)
        {
            using var sha256 = SHA256.Create();
            byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes($"{_hashConfiguration.Salt}{reference}"));

            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }

        public bool Check(string reference, string hash)
        {
            return hash.Equals(Hash(reference));
        }
    }
}
