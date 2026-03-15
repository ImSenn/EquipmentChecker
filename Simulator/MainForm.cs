using System.Text;
using System.Text.Json;
using MQTTnet;
using MQTTnet.Protocol;

namespace Simulator;

public class MainForm : Form
{
    private readonly TextBox _txtHost = new() { Text = "localhost", Width = 140 };
    private readonly NumericUpDown _numPort = new() { Minimum = 1, Maximum = 65535, Value = 1883, Width = 80 };
    private readonly TextBox _txtClientId = new() { Text = "simulator-ui-01", Width = 140 };
    private readonly Button _btnConnect = new() { Text = "Connect MQTT", Width = 120 };
    private readonly Label _lblConnection = new() { Text = "Disconnected", AutoSize = true };

    private readonly ComboBox _cmbDeviceId = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
    private readonly NumericUpDown _numTemp = new() { Minimum = -20, Maximum = 150, DecimalPlaces = 1, Increment = 0.1M, Value = 42.6M, Width = 80 };
    private readonly NumericUpDown _numVibration = new() { Minimum = 0, Maximum = 10, DecimalPlaces = 3, Increment = 0.001M, Value = 0.021M, Width = 80 };
    private readonly NumericUpDown _numPower = new() { Minimum = 0, Maximum = 10000, DecimalPlaces = 0, Increment = 10, Value = 520, Width = 100 };
    private readonly ComboBox _cmbRunState = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
    private readonly ComboBox _cmbPowerState = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
    private readonly NumericUpDown _numInterval = new() { Minimum = 1, Maximum = 3600, Value = 5, Width = 80 };
    private readonly Button _btnSendTelemetry = new() { Text = "Send Telemetry Now", Width = 150 };
    private readonly Button _btnToggleAuto = new() { Text = "Start Auto", Width = 100 };
    private readonly Button _btnApplyProfile = new() { Text = "Apply Profile", Width = 110 };
    private readonly CheckBox _chkRandomize = new() { Text = "Randomize around sliders", Checked = true, AutoSize = true };
    private readonly NumericUpDown _numTempJitter = new() { Minimum = 0, Maximum = 30, DecimalPlaces = 1, Increment = 0.1M, Value = 2.0M, Width = 70 };
    private readonly NumericUpDown _numVibrationJitter = new() { Minimum = 0, Maximum = 1, DecimalPlaces = 3, Increment = 0.001M, Value = 0.005M, Width = 70 };
    private readonly NumericUpDown _numPowerJitter = new() { Minimum = 0, Maximum = 5000, DecimalPlaces = 0, Increment = 5, Value = 30, Width = 70 };

    private readonly ComboBox _cmbEventType = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 160 };
    private readonly ComboBox _cmbSeverity = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
    private readonly NumericUpDown _numEventValue = new() { Minimum = 0, Maximum = 10000, DecimalPlaces = 1, Value = 78.5M, Width = 100 };
    private readonly TextBox _txtEventMessage = new() { Width = 300, Text = "Temperature exceeded threshold" };
    private readonly Button _btnSendEvent = new() { Text = "Trigger Event", Width = 120 };
    private readonly Button _btnHeartbeat = new() { Text = "Send Heartbeat", Width = 120 };

    private readonly ComboBox _cmbCommand = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
    private readonly TextBox _txtCommandValue = new() { Width = 120, Text = "10" };
    private readonly Button _btnSendCommand = new() { Text = "Send Command", Width = 120 };

    private readonly TextBox _txtLog = new()
    {
        Multiline = true,
        ScrollBars = ScrollBars.Vertical,
        ReadOnly = true,
        Dock = DockStyle.Fill,
        Font = new Font("Consolas", 10)
    };

    private readonly System.Windows.Forms.Timer _telemetryTimer = new();
    private readonly Random _random = new();
    private readonly Dictionary<string, DeviceProfile> _profiles = new();

    private IMqttClient? _mqttClient;
    private long _uptimeSeconds;

    public MainForm()
    {
        Text = "EquipmentChecker Simulator";
        Width = 1024;
        Height = 720;
        StartPosition = FormStartPosition.CenterScreen;

        InitializeOptions();
        InitializeLayout();
        WireEvents();

        _telemetryTimer.Interval = (int)_numInterval.Value * 1000;
    }

    private sealed class DeviceProfile
    {
        public required decimal Temperature { get; init; }
        public required decimal Vibration { get; init; }
        public required decimal EstimatedPower { get; init; }
        public required string RunState { get; init; }
        public required string PowerState { get; init; }
    }

    private void InitializeOptions()
    {
        _cmbDeviceId.Items.AddRange(new object[] { "MACHINE_01", "MACHINE_02", "MACHINE_03" });
        _cmbDeviceId.SelectedIndex = 0;

        _cmbRunState.Items.AddRange(new object[] { "RUNNING", "STOPPED" });
        _cmbRunState.SelectedIndex = 0;

        _cmbPowerState.Items.AddRange(new object[] { "ON", "OFF" });
        _cmbPowerState.SelectedIndex = 0;

        _cmbEventType.Items.AddRange(new object[] { "OVERHEAT", "HIGH_VIBRATION", "POWER_FAILURE" });
        _cmbEventType.SelectedIndex = 0;

        _cmbSeverity.Items.AddRange(new object[] { "LOW", "MEDIUM", "HIGH", "CRITICAL" });
        _cmbSeverity.SelectedIndex = 3;

        _cmbCommand.Items.AddRange(new object[] { "SET_INTERVAL", "RESTART", "SET_POWER" });
        _cmbCommand.SelectedIndex = 0;

        SeedProfiles();
        ApplyProfileForCurrentDevice();
    }

    private void SeedProfiles()
    {
        _profiles["MACHINE_01"] = new DeviceProfile
        {
            Temperature = 42.6M,
            Vibration = 0.021M,
            EstimatedPower = 520M,
            RunState = "RUNNING",
            PowerState = "ON"
        };

        _profiles["MACHINE_02"] = new DeviceProfile
        {
            Temperature = 38.4M,
            Vibration = 0.013M,
            EstimatedPower = 460M,
            RunState = "RUNNING",
            PowerState = "ON"
        };

        _profiles["MACHINE_03"] = new DeviceProfile
        {
            Temperature = 55.2M,
            Vibration = 0.037M,
            EstimatedPower = 740M,
            RunState = "STOPPED",
            PowerState = "OFF"
        };
    }

    private void InitializeLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var controls = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            WrapContents = true,
            Padding = new Padding(8)
        };

        AddLabeled(controls, "Host", _txtHost);
        AddLabeled(controls, "Port", _numPort);
        AddLabeled(controls, "ClientId", _txtClientId);
        controls.Controls.Add(_btnConnect);
        controls.Controls.Add(_lblConnection);

        controls.Controls.Add(new Label { Width = 20 });
        AddLabeled(controls, "Device", _cmbDeviceId);
        AddLabeled(controls, "Temp", _numTemp);
        AddLabeled(controls, "Vibration", _numVibration);
        AddLabeled(controls, "Power", _numPower);
        AddLabeled(controls, "RunState", _cmbRunState);
        AddLabeled(controls, "PowerState", _cmbPowerState);
        AddLabeled(controls, "Interval(s)", _numInterval);
        controls.Controls.Add(_btnSendTelemetry);
        controls.Controls.Add(_btnToggleAuto);
        controls.Controls.Add(_btnApplyProfile);

        controls.Controls.Add(new Label { Width = 20 });
        controls.Controls.Add(_chkRandomize);
        AddLabeled(controls, "dTemp", _numTempJitter);
        AddLabeled(controls, "dVib", _numVibrationJitter);
        AddLabeled(controls, "dPower", _numPowerJitter);

        controls.Controls.Add(new Label { Width = 20 });
        AddLabeled(controls, "Event", _cmbEventType);
        AddLabeled(controls, "Severity", _cmbSeverity);
        AddLabeled(controls, "Value", _numEventValue);
        AddLabeled(controls, "Message", _txtEventMessage);
        controls.Controls.Add(_btnSendEvent);
        controls.Controls.Add(_btnHeartbeat);

        controls.Controls.Add(new Label { Width = 20 });
        AddLabeled(controls, "Command", _cmbCommand);
        AddLabeled(controls, "Value", _txtCommandValue);
        controls.Controls.Add(_btnSendCommand);

        var logGroup = new GroupBox { Text = "Log", Dock = DockStyle.Fill, Padding = new Padding(8) };
        logGroup.Controls.Add(_txtLog);

        root.Controls.Add(controls, 0, 0);
        root.Controls.Add(logGroup, 0, 1);
        Controls.Add(root);
    }

    private void WireEvents()
    {
        _btnConnect.Click += async (_, _) => await ToggleConnectAsync();
        _btnSendTelemetry.Click += async (_, _) => await SendTelemetryAsync();
        _btnToggleAuto.Click += (_, _) => ToggleAutoSend();
        _btnApplyProfile.Click += (_, _) => ApplyProfileForCurrentDevice();
        _btnSendEvent.Click += async (_, _) => await SendEventAsync();
        _btnHeartbeat.Click += async (_, _) => await SendHeartbeatAsync();
        _btnSendCommand.Click += async (_, _) => await SendCommandAsync();

        _numInterval.ValueChanged += (_, _) =>
        {
            _telemetryTimer.Interval = (int)_numInterval.Value * 1000;
            Log($"[AUTO] Interval = {_numInterval.Value}s");
        };

        _cmbDeviceId.SelectedIndexChanged += async (_, _) =>
        {
            ApplyProfileForCurrentDevice();
            await SubscribeCommandsAsync();
        };

        _telemetryTimer.Tick += async (_, _) => await SendTelemetryAsync();

        FormClosing += async (_, _) =>
        {
            try
            {
                _telemetryTimer.Stop();
                if (_mqttClient is { IsConnected: true })
                {
                    await _mqttClient.DisconnectAsync();
                }
            }
            catch
            {
                // Ignore dispose errors during shutdown.
            }
        };
    }

    private static void AddLabeled(Control parent, string label, Control control)
    {
        parent.Controls.Add(new Label
        {
            Text = label,
            AutoSize = true,
            Margin = new Padding(8, 10, 4, 4)
        });
        parent.Controls.Add(control);
    }

    private void ApplyProfileForCurrentDevice()
    {
        if (!_profiles.TryGetValue(CurrentDeviceId, out var profile))
        {
            return;
        }

        _numTemp.Value = ClampDecimal(profile.Temperature, _numTemp.Minimum, _numTemp.Maximum);
        _numVibration.Value = ClampDecimal(profile.Vibration, _numVibration.Minimum, _numVibration.Maximum);
        _numPower.Value = ClampDecimal(profile.EstimatedPower, _numPower.Minimum, _numPower.Maximum);

        _cmbRunState.SelectedItem = profile.RunState;
        _cmbPowerState.SelectedItem = profile.PowerState;

        Log($"[PROFILE] Applied {CurrentDeviceId}");
    }

    private static decimal ClampDecimal(decimal value, decimal min, decimal max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    private string CurrentDeviceId => _cmbDeviceId.SelectedItem?.ToString() ?? "MACHINE_01";

    private async Task ToggleConnectAsync()
    {
        if (_mqttClient is { IsConnected: true })
        {
            await _mqttClient.DisconnectAsync();
            _lblConnection.Text = "Disconnected";
            _btnConnect.Text = "Connect MQTT";
            Log("[MQTT] Disconnected");
            return;
        }

        var factory = new MqttClientFactory();
        _mqttClient = factory.CreateMqttClient();

        _mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            Log($"[RECV] {topic} | {payload}");

            if (topic.EndsWith("/command", StringComparison.OrdinalIgnoreCase))
            {
                await SendAckForCommandAsync(topic, payload);
            }

            await Task.CompletedTask;
        };

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(_txtHost.Text.Trim(), (int)_numPort.Value)
            .WithClientId(_txtClientId.Text.Trim())
            .WithCleanSession(true)
            .Build();

        await _mqttClient.ConnectAsync(options);
        _lblConnection.Text = "Connected";
        _btnConnect.Text = "Disconnect";
        Log($"[MQTT] Connected to {_txtHost.Text}:{_numPort.Value}");

        await SubscribeCommandsAsync();
    }

    private async Task SubscribeCommandsAsync()
    {
        if (_mqttClient is not { IsConnected: true })
        {
            return;
        }

        var topic = $"device/{CurrentDeviceId}/command";
        var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
            .WithTopicFilter(filter => filter.WithTopic(topic))
            .Build();

        await _mqttClient.SubscribeAsync(subscribeOptions);
        Log($"[MQTT] Subscribed command topic: {topic}");
    }

    private async Task SendTelemetryAsync()
    {
        if (_mqttClient is not { IsConnected: true })
        {
            Log("[WARN] MQTT chưa connect.");
            return;
        }

        _uptimeSeconds += (long)_numInterval.Value;

        var tempValue = (double)_numTemp.Value;
        var vibrationValue = (double)_numVibration.Value;
        var powerValue = (double)_numPower.Value;

        if (_chkRandomize.Checked)
        {
            tempValue = RandomAround(tempValue, (double)_numTempJitter.Value, (double)_numTemp.Minimum, (double)_numTemp.Maximum);
            vibrationValue = RandomAround(vibrationValue, (double)_numVibrationJitter.Value, (double)_numVibration.Minimum, (double)_numVibration.Maximum);
            powerValue = RandomAround(powerValue, (double)_numPowerJitter.Value, (double)_numPower.Minimum, (double)_numPower.Maximum);
        }

        var payload = new
        {
            deviceId = CurrentDeviceId,
            timestamp = DateTime.UtcNow,
            metrics = new
            {
                temperature = Math.Round(tempValue, 1),
                vibration = Math.Round(vibrationValue, 3),
                estimatedPower = Math.Round(powerValue, 0)
            },
            status = new
            {
                runState = _cmbRunState.SelectedItem?.ToString() ?? "RUNNING",
                powerState = _cmbPowerState.SelectedItem?.ToString() ?? "ON"
            },
            system = new
            {
                uptime = _uptimeSeconds,
                wifiRssi = _random.Next(-80, -40),
                freeHeap = _random.Next(100000, 250000)
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var topic = $"device/{CurrentDeviceId}/telemetry";
        await PublishAsync(topic, json);
        Log($"[SEND] {topic} | temp={payload.metrics.temperature:0.0} | vib={payload.metrics.vibration:0.000} | power={payload.metrics.estimatedPower:0}");
    }

    private double RandomAround(double center, double jitter, double min, double max)
    {
        if (jitter <= 0)
        {
            return center;
        }

        var val = center + ((_random.NextDouble() * 2.0) - 1.0) * jitter;
        if (val < min) return min;
        if (val > max) return max;
        return val;
    }

    private async Task SendEventAsync()
    {
        if (_mqttClient is not { IsConnected: true })
        {
            Log("[WARN] MQTT chưa connect.");
            return;
        }

        var payload = new
        {
            deviceId = CurrentDeviceId,
            timestamp = DateTime.UtcNow,
            @event = new
            {
                type = _cmbEventType.SelectedItem?.ToString() ?? "OVERHEAT",
                severity = _cmbSeverity.SelectedItem?.ToString() ?? "CRITICAL",
                value = (double)_numEventValue.Value,
                message = _txtEventMessage.Text.Trim()
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var topic = $"device/{CurrentDeviceId}/event";
        await PublishAsync(topic, json);
        Log($"[SEND] {topic} | {payload.@event.type}/{payload.@event.severity}");
    }

    private async Task SendHeartbeatAsync()
    {
        if (_mqttClient is not { IsConnected: true })
        {
            Log("[WARN] MQTT chưa connect.");
            return;
        }

        var payload = new
        {
            deviceId = CurrentDeviceId,
            timestamp = DateTime.UtcNow,
            status = "online"
        };

        var json = JsonSerializer.Serialize(payload);
        var topic = $"device/{CurrentDeviceId}/heartbeat";
        await PublishAsync(topic, json);
        Log($"[SEND] {topic}");
    }

    private async Task SendCommandAsync()
    {
        if (_mqttClient is not { IsConnected: true })
        {
            Log("[WARN] MQTT chưa connect.");
            return;
        }

        var commandName = _cmbCommand.SelectedItem?.ToString() ?? "RESTART";
        var valueRaw = _txtCommandValue.Text.Trim();

        object payload;
        if (string.Equals(commandName, "RESTART", StringComparison.OrdinalIgnoreCase))
        {
            payload = new { command = commandName };
        }
        else if (string.Equals(commandName, "SET_POWER", StringComparison.OrdinalIgnoreCase))
        {
            var powerState = string.IsNullOrWhiteSpace(valueRaw) ? "OFF" : valueRaw;
            payload = new { command = commandName, value = powerState };
        }
        else
        {
            if (!int.TryParse(valueRaw, out var interval))
            {
                interval = 10;
            }
            payload = new { command = commandName, value = interval };
        }

        var topic = $"device/{CurrentDeviceId}/command";
        var json = JsonSerializer.Serialize(payload);
        await PublishAsync(topic, json);
        Log($"[SEND] {topic} | {json}");
    }

    private async Task SendAckForCommandAsync(string commandTopic, string commandPayload)
    {
        if (_mqttClient is not { IsConnected: true })
        {
            return;
        }

        var parts = commandTopic.Split('/');
        if (parts.Length < 3)
        {
            return;
        }

        var deviceId = parts[1];
        var commandName = "UNKNOWN";

        try
        {
            using var doc = JsonDocument.Parse(commandPayload);
            if (doc.RootElement.TryGetProperty("command", out var commandNode))
            {
                commandName = commandNode.GetString() ?? "UNKNOWN";
            }
        }
        catch
        {
            // Keep UNKNOWN command name.
        }

        var ack = new
        {
            deviceId,
            timestamp = DateTime.UtcNow,
            commandAck = new
            {
                command = commandName,
                status = "SUCCESS"
            }
        };

        var ackTopic = $"device/{deviceId}/ack";
        await PublishAsync(ackTopic, JsonSerializer.Serialize(ack));
        Log($"[SEND] {ackTopic} | ACK {commandName}");
    }

    private async Task PublishAsync(string topic, string payload)
    {
        if (_mqttClient is not { IsConnected: true })
        {
            return;
        }

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        await _mqttClient.PublishAsync(message);
    }

    private void ToggleAutoSend()
    {
        if (_telemetryTimer.Enabled)
        {
            _telemetryTimer.Stop();
            _btnToggleAuto.Text = "Start Auto";
            Log("[AUTO] Stopped");
        }
        else
        {
            _telemetryTimer.Start();
            _btnToggleAuto.Text = "Stop Auto";
            Log($"[AUTO] Started ({_numInterval.Value}s)");
        }
    }

    private void Log(string message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}";

        if (InvokeRequired)
        {
            BeginInvoke(() => _txtLog.AppendText(line));
            return;
        }

        _txtLog.AppendText(line);
    }
}
