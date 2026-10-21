using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using MovieStreaming.Services;

namespace MovieStreaming.Controllers;

[ApiController]
[Route("api/vsmov")]
public class VsmovController : ControllerBase
{
    private readonly VsmovService _service;
    private readonly ILogger<VsmovController> _logger;

    public VsmovController(
        VsmovService service,
        ILogger<VsmovController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("raw")]
    public async Task<IActionResult> Raw(
        [FromQuery] int page = 1)
    {
        try
        {
            return Ok(await _service.GetRawLatestAsync(page));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "VSMOV HTTP error");
            return StatusCode(502,
                "Không kết nối được API VSMOV.");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "VSMOV JSON error");
            return StatusCode(502,
                "VSMOV trả về JSON không hợp lệ.");
        }
    }

    [HttpGet("latest")]
    public async Task<IActionResult> Latest(
        [FromQuery] int page = 1)
    {
        try
        {
            var movies = await _service.GetLatestAsync(page);
            return Ok(movies);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "VSMOV HTTP error");
            return StatusCode(502,
                "Không kết nối được API VSMOV.");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "VSMOV schema error");
            return StatusCode(502,
                "Cấu trúc JSON VSMOV chưa được hỗ trợ.");
        }
    }

    [HttpGet("detail/raw/{slug}")]
    public async Task<IActionResult> RawDetail(string slug)
    {
        try
        {
            var data = await _service.GetRawDetailAsync(slug);
            return Ok(data);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "VSMOV detail HTTP error");
            return StatusCode(502, "Không lấy được chi tiết phim.");
        }
        catch (System.Text.Json.JsonException ex)
        {
            _logger.LogError(ex, "VSMOV detail JSON error");
            return StatusCode(502, "JSON chi tiết phim không hợp lệ.");
        }
    }

    [HttpGet("detail/{slug}")]
    public async Task<IActionResult> Detail(string slug)
    {
        try
        {
            var movie = await _service.GetDetailAsync(slug);
            return Ok(movie);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "VSMOV detail HTTP error");
            return StatusCode(502, "Không lấy được chi tiết phim.");
        }
        catch (System.Text.Json.JsonException ex)
        {
            _logger.LogError(ex, "VSMOV detail schema error");
            return StatusCode(502, "Cấu trúc JSON chi tiết phim chưa được hỗ trợ.");
        }
    }
    [HttpGet("search")]
    public async Task<IActionResult> Search(
    [FromQuery] string? keyword,
    [FromQuery] int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return BadRequest(new
            {
                message = "Vui lòng nhập từ khóa tìm kiếm."
            });
        }

        if (keyword.Length > 100)
        {
            return BadRequest(new
            {
                message = "Từ khóa không được vượt quá 100 ký tự."
            });
        }

        try
        {
            var movies = await _service.SearchAsync(
             keyword,
            limit
            );

            return Ok(movies);
        }
        catch (HttpRequestException)
        {
            return StatusCode(502, new
            {
                message = "Không thể kết nối API VSMOV."
            });
        }
        catch (JsonException)
        {
            return StatusCode(502, new
            {
                message = "Dữ liệu tìm kiếm VSMOV không đúng định dạng."
            });
        }
    }

}