using System;

namespace CgiResourceUpload.Models
{
    internal class ValidationCheck
    {
        public Func<bool> Validate { get; }
        public string FailMessage { get; }

        public ValidationCheck(Func<bool> validate, string failMessage)
        {
            Validate = validate;
            FailMessage = failMessage;
        }
    }
}
