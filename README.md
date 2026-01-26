# Modbus-Developer-Toolkit
# Industrial IoT Toolkit

This toolkit provides essential tools for Industrial IoT development and testing.

## Included Tools
- [x] Modbus TCP Simulator: Asynchronous server simulation with customizable data generation.
- [ ] MQTT Simulator: (Coming Soon)
- [ ] Protocol Analyzer: (Planned)

## 🛠️ 2026-01-15 Architecture Update
- **Refactored with Strategy Pattern**: Decoupled waveform logic (Sine/Ramp) from the simulation engine.
- **Dynamic Configuration**: Fully data-driven register mapping via `AppConfig`.
- **High Efficiency**: Optimized data flow using `Span<short>` for Modbus register buffers.
