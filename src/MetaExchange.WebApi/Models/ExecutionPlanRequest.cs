using MetaExchange.Core.Enums;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MetaExchange.WebApi.Models;

public sealed record ExecutionPlanRequest(
    [FromRoute] TradeType TradeType,
    [FromRoute][Range(0.00000001, 1000, ErrorMessage = "Amount must be between 0.00000001 and 1000.")] decimal AmountBtc
);
