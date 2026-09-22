using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;

namespace HomeMarket.Api.Controllers
{
    // How a refused Result becomes an HTTP answer, in one place: NotFound
    // 404, Forbidden 403, Unauthorized 401, Invalid 400 with the errors
    // field by field, Conflict 409, and Error 422 unless a controller
    // says otherwise. Every one of them is a Problem Details document.
    public static class ResultTranslation
    {
        public static ActionResult Refuse(this ControllerBase controller, Ardalis.Result.IResult result)
        {
            var reason = string.Join(" ", result.Errors);
            return result.Status switch
            {
                ResultStatus.NotFound => reason.Length == 0
                    ? controller.NotFound()
                    : controller.Problem(reason, statusCode: StatusCodes.Status404NotFound),
                ResultStatus.Forbidden => controller.Forbid(),
                ResultStatus.Unauthorized => controller.Problem(reason, statusCode: StatusCodes.Status401Unauthorized),
                ResultStatus.Invalid => controller.ValidationProblem(new ValidationProblemDetails(
                    result.ValidationErrors
                        .GroupBy(e => e.Identifier)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()))),
                ResultStatus.Conflict => controller.Problem(reason, statusCode: StatusCodes.Status409Conflict),
                _ => controller.Problem(reason, statusCode: StatusCodes.Status422UnprocessableEntity),
            };
        }
    }
}
