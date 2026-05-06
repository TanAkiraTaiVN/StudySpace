using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudySpace.API.Helpers;
using StudySpace.Core.DTOs;
using StudySpace.Core.DTOs.Document;
using StudySpace.Core.Interfaces;

namespace StudySpace.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _docs;
    private readonly IWebHostEnvironment _env;

    public DocumentsController(IDocumentService docs, IWebHostEnvironment env)
    {
        _docs = docs;
        _env = env;
    }

    [HttpGet("by-group/{groupId}")]
    public async Task<ActionResult<ApiResponse<List<DocumentDto>>>> ListByGroup(int groupId, [FromQuery] string? search)
    {
        try
        {
            var uid = CurrentUser.RequireUserId(User);
            return Ok(ApiResponse<List<DocumentDto>>.Ok(await _docs.ListByGroupAsync(uid, groupId, search)));
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<List<DocumentDto>>.Fail(ex.Message)); }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<DocumentDto>>> Get(int id)
    {
        try
        {
            var uid = CurrentUser.RequireUserId(User);
            return Ok(ApiResponse<DocumentDto>.Ok(await _docs.GetByIdAsync(uid, id)));
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<DocumentDto>.Fail(ex.Message)); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<DocumentDto>.Fail(ex.Message)); }
    }

    [HttpPost("upload")]
    [RequestSizeLimit(60_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 60_000_000)]
    public async Task<ActionResult<ApiResponse<DocumentDto>>> Upload(
        [FromForm] int studyGroupId,
        [FromForm] string title,
        [FromForm] string? description,
        [FromForm] string? tags,
        [FromForm] IFormFile file)
    {
        try
        {
            if (file == null || file.Length <= 0)
                return BadRequest(ApiResponse<DocumentDto>.Fail("Vui lòng chọn tệp tải lên."));

            var uid = CurrentUser.RequireUserId(User);
            await using var stream = file.OpenReadStream();

            var publicBase = $"{Request.Scheme}://{Request.Host}";
            var dto = await _docs.UploadAsync(
                uid,
                studyGroupId,
                title,
                description,
                tags,
                file.FileName,
                file.ContentType,
                file.Length,
                stream,
                _env.WebRootPath,
                publicBase);

            return Ok(ApiResponse<DocumentDto>.Ok(dto, "Tải tài liệu thành công."));
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<DocumentDto>.Fail(ex.Message)); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<DocumentDto>.Fail(ex.Message)); }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<DocumentDto>>> Update(int id, [FromBody] UpdateDocumentRequest request)
    {
        try
        {
            var uid = CurrentUser.RequireUserId(User);
            return Ok(ApiResponse<DocumentDto>.Ok(await _docs.UpdateAsync(uid, id, request), "Cập nhật thành công."));
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<DocumentDto>.Fail(ex.Message)); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<DocumentDto>.Fail(ex.Message)); }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        try
        {
            var uid = CurrentUser.RequireUserId(User);
            await _docs.DeleteAsync(uid, id, _env.WebRootPath);
            return Ok(ApiResponse.Ok("Đã xoá tài liệu."));
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse.Fail(ex.Message)); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(int id)
    {
        try
        {
            var uid = CurrentUser.RequireUserId(User);
            var (path, name, contentType) = await _docs.GetDownloadAsync(uid, id, _env.WebRootPath);
            var bytes = await System.IO.File.ReadAllBytesAsync(path);
            return File(bytes, contentType, name);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse.Fail(ex.Message)); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (FileNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
    }
}
