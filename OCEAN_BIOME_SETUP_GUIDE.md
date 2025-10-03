# Ocean Biome System Setup Guide

## Quick Setup for Keeping Your Blue Tiles

### Option 1: Keep Your Current Blue Tiles (Recommended for Testing)

1. **In OceanChunkManager Inspector:**
   - Assign your existing ocean tile prefab to "Default Ocean Tile Prefab"
   - **UNCHECK** "Enable Depth Variations"
   - Leave "Biome Config" empty/null

2. **Result:** Your tiles will keep their original blue color from the prefab!

### Option 2: Use the New Biome System

1. **Create Ocean Biome Configuration:**
   ```
   Right-click in Project → Create → WOS → Environment → Ocean Biome Configuration
   ```

2. **Configure Basic Ocean Types:**
   - Click the "+" to add ocean tile types
   - **Shallow Ocean**: Min Depth: 0, Max Depth: 50, Base Color: Light Blue
   - **Deep Ocean**: Min Depth: 50, Max Depth: 500, Base Color: Dark Blue

3. **Assign to OceanChunkManager:**
   - Drag your biome config to "Biome Config" field
   - **CHECK** "Enable Depth Variations"
   - Assign your prefab to "Default Ocean Tile Prefab"

## Detailed Configuration

### Ocean Biome Configuration Settings

#### Basic Setup
- **Ocean Tile Types**: List of depth zones with their colors and settings
- **Max Ocean Depth**: 500m (controls the deepest areas)
- **Use Depth Gradient**: Enable for smooth color transitions
- **Depth Color Gradient**: 5 colors from shallow to deep

#### Procedural Noise (Default)
- **Primary Noise Scale**: 0.1 (larger scale = bigger depth areas)
- **Secondary Noise Scale**: 0.05 (detail layer)
- **Octaves**: 4 (complexity of noise)

#### Custom Depth Map (Advanced)
- **Use Custom Depth Map**: Check to use a texture
- **Custom Depth Map**: Assign a grayscale texture (white = deep, black = shallow)
- **Depth Map World Size**: How many world units the texture covers

### Ocean Tile Type Configuration

Each tile type has:
- **Tile Name**: Display name (e.g., "Shallow Water")
- **Depth Zone**: Coastal, Shallow, Medium, Deep, or Abyssal
- **Min/Max Depth**: Depth range in meters
- **Base Color**: Primary color for this depth
- **Variation Color**: Secondary color for variety
- **Color Variation**: How much to vary colors (0-1)
- **Tile Prefab**: Optional custom prefab for this depth
- **Possible Features**: Objects that can spawn (seaweed, rocks, etc.)

### Example Setup for Simple Blue Ocean

```
Tile Type 1: "Ocean Surface"
- Min Depth: 0m, Max Depth: 100m
- Base Color: RGB(32, 64, 128) - Your blue color
- Variation Color: RGB(24, 48, 96) - Slightly darker
- Color Variation: 0.1 (10% variation)

Tile Type 2: "Deep Ocean"
- Min Depth: 100m, Max Depth: 500m
- Base Color: RGB(16, 32, 64) - Darker blue
- Variation Color: RGB(8, 24, 48) - Even darker
- Color Variation: 0.05 (5% variation)
```

## Troubleshooting

### Issue: Tiles are white instead of blue
**Solution**: Either disable "Enable Depth Variations" OR configure tile colors in the biome config

### Issue: No variation between tiles
**Solution**:
1. Check "Enable Depth Variations" is checked
2. Verify biome config is assigned
3. Increase "Color Variation" values in tile types

### Issue: All tiles are the same color
**Solution**:
1. Check that tile types have different depth ranges
2. Verify "Use Depth Gradient" is enabled for smooth transitions
3. Check noise scale isn't too large (try 0.01-0.1)

### Issue: Depth pattern doesn't look good
**Solution**:
1. Adjust "Primary Noise Scale" (0.01 = large areas, 0.1 = small areas)
2. Try different "Octaves" (1-8, more = more detail)
3. Adjust "Persistence" (0.3-0.7, higher = more detail contrast)

## Performance Tips

1. **Limit Tile Types**: 3-5 types maximum for best performance
2. **Disable Features**: Don't use "Possible Features" unless needed
3. **Reduce Updates**: Increase "Update Interval" to 0.5s
4. **Lower Grid Radius**: Use 2 instead of 3 for fewer tiles

## Debug Options

- **Debug Depth Values**: Shows depth calculations in console
- **Visualize Biomes**: Colors tiles by depth zone (cyan=coastal, blue=medium, etc.)

## Quick Color Reference

```
Light Ocean Blue:   RGB(64, 128, 192)  or Color(0.25f, 0.5f, 0.75f)
Medium Ocean Blue:  RGB(32, 96, 160)   or Color(0.125f, 0.375f, 0.625f)
Deep Ocean Blue:    RGB(16, 64, 128)   or Color(0.06f, 0.25f, 0.5f)
Dark Ocean Blue:    RGB(8, 32, 96)     or Color(0.03f, 0.125f, 0.375f)
```