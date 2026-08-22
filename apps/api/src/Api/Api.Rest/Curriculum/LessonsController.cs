using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Milese.Common.Shared;
using Milese.Common.Types.ValueTypes.Curriculum;
using Milese.Common.Types.ValueTypes.Identity;
using Milese.Services.Core.Curriculum;

namespace Milese.Api.Rest.Curriculum;

[ApiController]
[Route("api/lessons")]
#pragma warning disable S6960 // One controller per REST resource; the Read/Update split happens at the Service layer, not here.
public sealed class LessonsController : ControllerBase
#pragma warning restore S6960
{
    private readonly LessonsReadService lessonsReadService;
    private readonly LessonsUpdateService lessonsUpdateService;

    public LessonsController(LessonsReadService lessonsReadService, LessonsUpdateService lessonsUpdateService)
    {
        this.lessonsReadService = lessonsReadService;
        this.lessonsUpdateService = lessonsUpdateService;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
#pragma warning disable VSTHRD103 // Parse is a synchronous value type method, not an async operation.
        var lessonId = LessonId.Parse(id);
#pragma warning restore VSTHRD103
        if (lessonId.IsFailure)
            return Problem(lessonId.Error.ClientMessage(), statusCode: 400);

        var lesson = await lessonsReadService.GetByIdAsync(lessonId.Value);
        return lesson.Match<IActionResult>(
            Ok,
            error => Problem(error.ClientMessage(), statusCode: 404));
    }

    [HttpGet]
    public async Task<IActionResult> ListByConceptAsync([FromQuery] int conceptId)
    {
#pragma warning disable VSTHRD103 // Parse is a synchronous value type method, not an async operation.
        var parsedConceptId = ConceptId.Parse(conceptId);
#pragma warning restore VSTHRD103
        if (parsedConceptId.IsFailure)
            return Problem(parsedConceptId.Error.ClientMessage(), statusCode: 400);

        var lessons = await lessonsReadService.ListByConceptAsync(parsedConceptId.Value);
        return Ok(lessons);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateLessonRequest request)
    {
#pragma warning disable VSTHRD103 // Parse is a synchronous value type method, not an async operation.
        var parsed =
            from conceptId in ConceptId.Parse(request.ConceptId)
            from title in LessonTitle.Parse(request.Title)
            from estimatedMinutes in EstimatedMinutes.Parse(request.EstimatedMinutes)
            from order in SortOrder.Parse(request.Order)
            select (conceptId, title, estimatedMinutes, order);
#pragma warning restore VSTHRD103

        if (parsed.IsFailure)
            return Problem(parsed.Error.ClientMessage(), statusCode: 400);

        var (parsedConceptId, parsedTitle, parsedEstimatedMinutes, parsedOrder) = parsed.Value;
        var lesson = await lessonsUpdateService.CreateAsync(
            parsedConceptId, parsedTitle, parsedEstimatedMinutes, parsedOrder);

        return CreatedAtAction("GetById", new { id = lesson.Id.Value }, lesson);
    }
}

public sealed record CreateLessonRequest
{
    public required int ConceptId { get; init; }

    public required string Title { get; init; }

    public required int EstimatedMinutes { get; init; }

    public required int Order { get; init; }
}
