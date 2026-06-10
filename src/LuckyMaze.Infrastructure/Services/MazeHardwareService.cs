using System;
using System.IO.Ports;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using LuckyMaze.Domain;
using LuckyMaze.Domain.Enums;

namespace LuckyMaze.Infrastructure.Services
{
    public class MazeHardwareService : IMazeHardwareService, IDisposable
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<MazeHardwareService> _logger;
        
        private readonly string? _picoPortName;
        private readonly string? _moonrakerUrl;
        private readonly decimal _cellSizeMm;

        private SerialPort? _serialPort;
        private bool _isSerialInitialized = false;

        public MazeHardwareService(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<MazeHardwareService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;

            _picoPortName = configuration["Hardware:PicoPort"];
            _moonrakerUrl = configuration["Hardware:MoonrakerUrl"];
            _cellSizeMm = configuration.GetValue<decimal>("Hardware:CellSizeMm", 30.0m);

            InitializeSerialPort();
        }

        private void InitializeSerialPort()
        {
            if (string.IsNullOrWhiteSpace(_picoPortName))
            {
                _logger.LogInformation("Hardware Pico Serial Port not configured. Running Pico in MOCK mode.");
                return;
            }

            try
            {
                _serialPort = new SerialPort(_picoPortName, 115200, Parity.None, 8, StopBits.One);
                _serialPort.ReadTimeout = 500;
                _serialPort.WriteTimeout = 500;
                _serialPort.Open();
                _isSerialInitialized = true;
                _logger.LogInformation("Successfully opened serial connection to Pico on {Port} at 115200 baud.", _picoPortName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to open serial port {Port}. Pico will run in MOCK mode.", _picoPortName);
                _serialPort = null;
                _isSerialInitialized = false;
            }
        }

        private async Task SendSerialCommandAsync(string command)
        {
            if (!_isSerialInitialized || _serialPort == null)
            {
                _logger.LogInformation("[HARDWARE MOCK (Pico Serial)] Sending command: {Cmd}", command.Trim());
                return;
            }

            try
            {
                // SerialPort write is synchronous but fast. We run it in Task to keep execution non-blocking
                await Task.Run(() => 
                {
                    lock (_serialPort)
                    {
                        if (_serialPort.IsOpen)
                        {
                            _serialPort.WriteLine(command);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed writing serial command '{Cmd}' to Pico.", command.Trim());
            }
        }

        private async Task SendGCodeAsync(string gcode)
        {
            if (string.IsNullOrWhiteSpace(_moonrakerUrl))
            {
                _logger.LogInformation("[HARDWARE MOCK (Klipper GCode)] Executing: {GCode}", gcode);
                return;
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var endpoint = $"{_moonrakerUrl.TrimEnd('/')}/printer/gcode/script";
                var payload = new { script = gcode };

                _logger.LogDebug("Sending G-Code to Moonraker: {GCode}", gcode);
                var response = await client.PostAsJsonAsync(endpoint, payload);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Moonraker API returned non-success status: {Code}. Details: {Details}", response.StatusCode, errorMsg);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send G-Code command '{GCode}' to Klipper/Moonraker.", gcode);
            }
        }

        #region IMazeHardwareService implementation

        public async Task InitializeAsync(Maze maze)
        {
            _logger.LogInformation("Initializing physical maze layout.");

            // 1. Tell Pico to draw the new maze grid on the 64x64 panel
            // We serialize the maze cells list to send to the Pico or send a simple dimensions setup command
            await SendSerialCommandAsync($"INIT {maze.Width} {maze.Height}");

            // Send cells in a structured format: CELL X Y North East South West
            var cells = JsonSerializer.Deserialize<System.Collections.Generic.List<MazeCell>>(maze.GridData) ?? new();
            foreach (var cell in cells)
            {
                int n = cell.North ? 1 : 0;
                int e = cell.East ? 1 : 0;
                int s = cell.South ? 1 : 0;
                int w = cell.West ? 1 : 0;
                await SendSerialCommandAsync($"CELL {cell.X} {cell.Y} {n} {e} {s} {w}");
            }

            // Tell Pico cells list is fully sent
            await SendSerialCommandAsync("GRID_COMPLETE");

            // 2. Home Klipper printer and prepare coordinates
            await SendGCodeAsync("G28"); // Home all axes
            await SendGCodeAsync("G90"); // Absolute positioning
            
            // Move toolhead (magnetic carriage) to start cell (center of the maze)
            decimal startX = (maze.Width / 2) * _cellSizeMm;
            decimal startY = (maze.Height / 2) * _cellSizeMm;
            await SendGCodeAsync($"G1 X{startX:F1} Y{startY:F1} F3000");
        }

        public async Task ShowStepAsync(int x, int y, Direction direction)
        {
            _logger.LogInformation("Pushed step to hardware: ({X}, {Y}) moving {Direction}", x, y, direction);

            // 1. Tell Pico to light up/update path leading to the new cell
            await SendSerialCommandAsync($"STEP {x} {y} {direction}");

            // 2. Send G-Code to move physical carriage to the coordinates
            decimal posX = x * _cellSizeMm;
            decimal posY = y * _cellSizeMm;
            await SendGCodeAsync($"G1 X{posX:F1} Y{posY:F1} F2400");
        }

        public async Task FlashWinnerAsync(string exitName)
        {
            _logger.LogInformation("Flashing winner exit: {ExitName}", exitName);

            // Tell Pico to trigger a celebratory flash sequence for this exit
            await SendSerialCommandAsync($"WIN {exitName}");

            // Flash stepper motors or do a victory dance G-code (e.g. wiggle back and forth)
            await SendGCodeAsync("G1 E5 F200"); // Example extruder click or small wiggle
            await SendGCodeAsync("G1 X10 F4000");
            await SendGCodeAsync("G1 X0 F4000");
        }

        public async Task ResetAsync()
        {
            _logger.LogInformation("Resetting physical maze components.");

            // Clear Pico display
            await SendSerialCommandAsync("RESET");

            // Move stepper motors to safe park coordinates (0, 0)
            await SendGCodeAsync("G1 X0 Y0 F3000");
        }

        #endregion

        public void Dispose()
        {
            if (_serialPort != null)
            {
                try
                {
                    if (_serialPort.IsOpen)
                    {
                        _serialPort.Close();
                    }
                    _serialPort.Dispose();
                }
                catch
                {
                    // Silent close
                }
            }
        }
    }
}
