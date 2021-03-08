using System.Threading.Tasks;
using form_builder.Models;

namespace form_builder.Validators.IntegrityChecks
{
    public class IntegrityCheckResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }        
    }
}