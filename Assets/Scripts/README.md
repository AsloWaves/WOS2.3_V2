# Scripts Organization

## Directory Structure

### 📁 **Camera/**
- `CameraController.cs` - Modern camera system with multiple modes
- `CameraSettingsSO.cs` - Camera configuration ScriptableObject

### 📁 **Economy/**
#### **Core/**
- `CargoSystem.cs` - Job system optimized spatial inventory
- `CargoConfigurationSO.cs` - Cargo system configuration
- `TradingSystem.cs` - Advanced trading with dynamic pricing
- `NPCTrader.cs` - AI trader with personality-driven behavior

#### **UI/**
- Economy UI components (to be implemented in Phase 5)

### 📁 **Environment/**
- `HomebaseObject.cs` - Port interaction system
- `DockingZone.cs` - Ship docking mechanics
- `HarborEffects.cs` - Weather and atmospheric effects

### 📁 **Player/**
- `SimpleNavalController.cs` - Enhanced naval physics
- `ShipConfigurationSO.cs` - Ship setup configuration
- `PlayerInventoryController.cs` - Player inventory management
- `PlayerInventoryConfigSO.cs` - Inventory configuration
- `PlayerAccountManager.cs` - Progression and reputation system

### 📁 **Scene/**
- `TransitionManager.cs` - Scene transition system
- `SceneTransitionSO.cs` - Scene configuration
- `SceneTransitionZone.cs` - Collision-based transitions

### 📁 **UI/**
- `TotalUIManager.cs` - Modern UI state management
- `UIPanel.cs` - Base panel class with animations
- `UIConfigurationSO.cs` - Theme and UI configuration

## Implementation Order
Follow the implementation guide phases:
1. **Foundation** - ScriptableObjects first
2. **Core Movement** - Player + Camera
3. **Economy** - Inventory + Trading
4. **World Systems** - Scenes + Environment
5. **UI** - Professional interface
6. **Polish** - Optimization + Effects

## Key Features
- **Job System** optimization for performance
- **Unity Input System** for modern controls
- **ScriptableObject** configuration for data-driven design
- **Event-driven** architecture for loose coupling
- **Accessibility** features in UI systems
- **Object pooling** for memory efficiency