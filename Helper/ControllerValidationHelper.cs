using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Http;

namespace Shared.Helper
{
    public static class ControllerValidationHelper
    {
        public static void ValidateViewModel<TViewModel, TController>(this TController controller,
            TViewModel viewModelToValidate)
            where TController : ApiController
        {
            var validationContext = new ValidationContext(viewModelToValidate, null, null);
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(viewModelToValidate, validationContext, validationResults, true);
            foreach (var validationResult in validationResults)
            {
                controller.ModelState.AddModelError(validationResult.MemberNames.FirstOrDefault() ?? string.Empty,
                    validationResult.ErrorMessage);
            }
        }
    }
}