using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace NBDProject2024.Utilities
{
    public static class BuildMessages
    {
        public static string ErrorMessage(ModelStateDictionary modelState)
        {
            //Get Validation Errors:
            IEnumerable<ModelError> allErrors = modelState.Values.SelectMany
                (v => v.Errors);
            string errorMessage = "";
            foreach (var e in allErrors)
            {
                errorMessage += e.ErrorMessage + "|";
            }
            
            return errorMessage;
        }
    }
}
