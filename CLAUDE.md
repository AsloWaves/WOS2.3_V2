# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

WOS2.3_V2 is a Unity-based naval MMO game focused on realistic ship physics and economy systems. The project uses Unity 6000.0.55f1 with the Universal Render Pipeline (URP) for 2D rendering.

## Core Architecture

### ScriptableObject System
The game uses ScriptableObjects for configuration-driven development:
- **ShipConfigurationSO**: Naval ship physics parameters (displacement, turning radius, stopping distance)
- **CameraSettingsSO**: Camera behavior configuration with speed-based effects
- **CargoConfigurationSO**: Tetris-style spatial inventory system with weight/balance physics
- **UIConfigurationSO**: Professional UI theming and maritime-specific context

### Input System Architecture
Uses Unity's Input System with naval-specific action maps:
- **Naval Controls**: Steering (A/D), Throttle (W/S with 8-speed system), Emergency Stop (Space)
- **Navigation**: Waypoint placement (Right-click), Autopilot (Z), Clear waypoints (X)
- **Camera**: Zoom (Mouse wheel), Pan (Middle mouse + drag)

### Performance Optimization
- **Unity Job System**: Used in CameraController and SimpleNavalController for parallel processing
- **NativeCollections**: Managing waypoints and navigation data
- **Target**: 60+ FPS with <100MB memory usage

## Development Commands

### Unity Editor Operations
- Open Unity Hub and launch with Unity 6000.0.55f1
- Ensure URP 2D template is active
- Use Package Manager to verify all required packages are installed

### Testing Workflow
1. Create test scene in `Assets/Scenes/`
2. Add ship prefab with SimpleNavalController component
3. Assign ShipConfigurationSO asset to ship
4. Add camera with CameraController component
5. Assign CameraSettingsSO asset to camera
6. Enter Play Mode to test naval physics

### ScriptableObject Creation
Create via Unity's Asset menu:
- `Create > WOS > Ship > Ship Configuration`
- `Create > WOS > Camera > Camera Settings`
- `Create > WOS > Economy > Cargo Configuration`
- `Create > WOS > UI > UI Configuration`

## Code Standards

### Namespace Structure
All scripts use the `WOS` namespace with subsystem organization:
- `WOS.Player` - Ship controllers and player systems
- `WOS.Camera` - Camera controllers and effects
- `WOS.Economy` - Trading and inventory systems
- `WOS.ScriptableObjects` - Configuration assets

### Unity Version Compatibility
Code uses Unity 2023+ APIs:
- `linearVelocity` instead of deprecated `velocity` for Rigidbody2D
- `FindFirstObjectByType<T>()` instead of deprecated `FindObjectOfType<T>()`
- Input System package for controls (not legacy Input Manager)

### Performance Guidelines
- Use Job System for computationally intensive operations
- Implement object pooling for frequently spawned objects (waypoints, UI elements)
- Cache component references in Awake() or Start()
- Use NativeCollections for data that needs Job System processing

## Current Implementation Status

**Phase 2 Complete**: Foundation systems fully implemented
- ✅ Core ScriptableObject configurations
- ✅ Naval physics with 8-speed throttle system
- ✅ Advanced camera with look-ahead and speed effects
- ✅ Input System with naval-specific controls
- ✅ Job System optimization

**Next Phase (Phase 3)**: Economy Foundation
- Ready to implement inventory system
- Trading mechanics preparation
- Port interaction systems

## Key Implementation Files

### Core Systems
- `Assets/Scripts/Player/SimpleNavalController.cs` - Naval physics and movement
- `Assets/Scripts/Camera/CameraController.cs` - Advanced camera system
- `Assets/InputSystem_Actions.inputactions` - Input configuration

### Configuration Assets
- `Assets/ScriptableObjects/Configs/ShipConfigurationSO.cs`
- `Assets/ScriptableObjects/Configs/CameraSettingsSO.cs`
- `Assets/ScriptableObjects/Configs/CargoConfigurationSO.cs`
- `Assets/ScriptableObjects/Configs/UIConfigurationSO.cs`

## Implementation Guide References
- Main guide: `WOS2.3_V2_IMPLEMENTATION_GUIDE.md`
- Current status: `IMPLEMENTATION_STATUS.md`
- Setup instructions: `SETUP_TEST_GUIDE.md`