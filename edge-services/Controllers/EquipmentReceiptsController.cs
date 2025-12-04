using MaritimeEdge.DTOs;
using MaritimeEdge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaritimeEdge.Controllers;

/// <summary>
/// API Controller cho quản lý phiếu nhập thiết bị
/// KHÔNG quản lý giá trị tài chính, chỉ tracking số lượng thiết bị
/// </summary>
[ApiController]
[Route("api/equipment/receipts")]
[Produces("application/json")]
public class EquipmentReceiptsController : ControllerBase
{
    private readonly EquipmentReceiptService _receiptService;
    private readonly ILogger<EquipmentReceiptsController> _logger;

    public EquipmentReceiptsController(
        EquipmentReceiptService receiptService,
        ILogger<EquipmentReceiptsController> logger)
    {
        _receiptService = receiptService;
        _logger = logger;
    }

    /// <summary>
    /// Preview dữ liệu import trước khi thực hiện nhập thiết bị
    /// </summary>
    /// <param name="dto">Dữ liệu phiếu nhập từ Excel</param>
    /// <returns>Kết quả preview với thông tin equipment sẽ được tạo mới/cập nhật</returns>
    /// <response code="200">Preview successful</response>
    /// <response code="400">Invalid data</response>
    [HttpPost("preview")]
    [ProducesResponseType(typeof(EquipmentReceiptPreviewResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EquipmentReceiptPreviewResponseDto>> PreviewImport([FromBody] ImportEquipmentReceiptDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var preview = await _receiptService.PreviewImportAsync(dto);
            
            // Nếu có lỗi nghiêm trọng, trả về BadRequest
            if (preview.Summary.ErrorItems > 0)
            {
                return BadRequest(new
                {
                    message = "Import data contains errors",
                    preview
                });
            }

            return Ok(preview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error previewing equipment import");
            return StatusCode(500, new { message = "Error previewing equipment import", error = ex.Message });
        }
    }

    /// <summary>
    /// Import phiếu nhập thiết bị - Tạo/cập nhật EquipmentItems và tạo Receipt records
    /// </summary>
    /// <param name="dto">Dữ liệu phiếu nhập từ Excel</param>
    /// <returns>Phiếu nhập đã được tạo với đầy đủ thông tin</returns>
    /// <response code="201">Import successful</response>
    /// <response code="400">Invalid data</response>
    /// <response code="500">Server error</response>
    [HttpPost("import")]
    [ProducesResponseType(typeof(EquipmentReceiptResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EquipmentReceiptResponseDto>> ImportReceipt([FromBody] ImportEquipmentReceiptDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var receipt = await _receiptService.ImportReceiptAsync(dto);
            
            _logger.LogInformation($"Equipment receipt {receipt.ReceiptCode} imported successfully with {receipt.ItemCount} items");

            return CreatedAtAction(
                nameof(GetReceiptById),
                new { id = receipt.Id },
                receipt
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing equipment receipt");
            return StatusCode(500, new { message = "Error importing equipment receipt", error = ex.Message });
        }
    }

    /// <summary>
    /// Lấy danh sách phiếu nhập thiết bị có phân trang và filter
    /// </summary>
    /// <param name="query">Query parameters cho filter và pagination</param>
    /// <returns>Danh sách phiếu nhập thiết bị</returns>
    /// <response code="200">Success</response>
    [HttpGet]
    [ProducesResponseType(typeof(EquipmentReceiptPagedResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<EquipmentReceiptPagedResponseDto>> GetReceipts([FromQuery] EquipmentReceiptQueryDto query)
    {
        try
        {
            var result = await _receiptService.GetReceiptsAsync(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting equipment receipts");
            return StatusCode(500, new { message = "Error getting equipment receipts", error = ex.Message });
        }
    }

    /// <summary>
    /// Lấy chi tiết một phiếu nhập thiết bị theo ID
    /// </summary>
    /// <param name="id">Receipt ID</param>
    /// <returns>Chi tiết phiếu nhập bao gồm tất cả equipment items</returns>
    /// <response code="200">Success</response>
    /// <response code="404">Equipment receipt not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EquipmentReceiptResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EquipmentReceiptResponseDto>> GetReceiptById(int id)
    {
        try
        {
            var receipt = await _receiptService.GetReceiptByIdAsync(id);
            
            if (receipt == null)
            {
                return NotFound(new { message = $"Equipment receipt with ID {id} not found" });
            }

            return Ok(receipt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting equipment receipt {id}");
            return StatusCode(500, new { message = "Error getting equipment receipt", error = ex.Message });
        }
    }

    /// <summary>
    /// Lấy thống kê tổng quan về phiếu nhập thiết bị
    /// </summary>
    /// <returns>Statistics về phiếu nhập thiết bị</returns>
    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetReceiptStats()
    {
        try
        {
            // TODO: Implement statistics aggregation
            return Ok(new
            {
                message = "Equipment receipt stats endpoint - To be implemented"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting equipment receipt stats");
            return StatusCode(500, new { message = "Error getting stats", error = ex.Message });
        }
    }
}
