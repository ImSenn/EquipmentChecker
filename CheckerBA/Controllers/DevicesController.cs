using System.Text.Json;
using CheckerBA.Application.DTOs;
using CheckerBA.Application.Services;
using CheckerBA.Domain.Entities;
using CheckerBA.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CheckerBA.Controllers
{
    [ApiController]
    [Route("api/devices")]
    [Authorize]
    public class DevicesController : ControllerBase
    {
        private readonly DeviceManagementService _deviceService;
        private readonly IMqttService _mqttService;
        private readonly ILogger<DevicesController> _logger;

        public DevicesController(
            DeviceManagementService deviceService,
            IMqttService mqttService,
            ILogger<DevicesController> logger)
        {
            _deviceService = deviceService;
            _mqttService = mqttService;
            _logger = logger;
        }

        // ── GET /api/devices ──────────────────────────────────────────────
        [HttpGet]
        public async Task<ActionResult<List<DeviceDto>>> GetAll()
        {
            var devices = await _deviceService.GetAllDevicesAsync();
            return Ok(devices.Select(ToDto));
        }

        // ── GET /api/devices/{id} ─────────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<ActionResult<DeviceDto>> GetById(string id)
        {
            var device = await _deviceService.GetDeviceByIdAsync(id);
            if (device is null) return NotFound();
            return Ok(ToDto(device));
        }

        // ── POST /api/devices ─────────────────────────────────────────────
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<DeviceDto>> Create([FromBody] CreateDeviceRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.DeviceId))
                return BadRequest(new { message = "DeviceId không được để trống." });

            // Kiểm tra trùng
            var existing = await _deviceService.GetDeviceByIdAsync(req.DeviceId);
            if (existing is not null)
                return Conflict(new { message = $"Device '{req.DeviceId}' đã tồn tại." });

            var device = new Device
            {
                DeviceId = req.DeviceId,
                Name = req.Name,
                Type = req.Type,
                PowerRating = req.PowerRating
            };

            await _deviceService.AddDeviceAsync(device);
            _logger.LogInformation("[DEVICE] Tạo thiết bị mới: {DeviceId}", device.DeviceId);
            return CreatedAtAction(nameof(GetById), new { id = device.DeviceId }, ToDto(device));
        }

        // ── PUT /api/devices/{id} ─────────────────────────────────────────
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateDeviceRequest req)
        {
            var device = await _deviceService.GetDeviceByIdAsync(id);
            if (device is null) return NotFound();

            device.Name = req.Name;
            device.Type = req.Type;
            device.PowerRating = req.PowerRating;

            await _deviceService.UpdateDeviceAsync(device);
            return NoContent();
        }

        // ── DELETE /api/devices/{id} ──────────────────────────────────────
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var device = await _deviceService.GetDeviceByIdAsync(id);
            if (device is null) return NotFound();

            await _deviceService.DeleteDeviceAsync(id);
            _logger.LogInformation("[DEVICE] Xóa thiết bị: {DeviceId}", id);
            return NoContent();
        }

        // ── POST /api/devices/{id}/command ────────────────────────────────
        [HttpPost("{id}/command")]
        public async Task<IActionResult> SendCommand(string id, [FromBody] CommandRequest req)
        {
            var topic = $"device/{id}/command";
            var payload = JsonSerializer.Serialize(req, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await _mqttService.PublishAsync(topic, payload);
            _logger.LogInformation("[COMMAND] Gửi lệnh tới {DeviceId}: {Command}", id, req.Command);
            return Ok(new { message = "Lệnh đã được gửi.", deviceId = id, command = req.Command });
        }

        private static DeviceDto ToDto(Device d) =>
            new(d.DeviceId, d.Name, d.Type, d.PowerRating, d.CreatedAt);
    }
}
