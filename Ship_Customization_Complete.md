# Ship Customization & Module System - Complete Specification

Advanced ship modification system allowing tactical specialization through modular equipment installation, creating unique vessel configurations optimized for specific combat roles and extraction-based missions.

---

## Core Philosophy

Ship customization in WOS2.3 follows a **dual-layer system** inspired by Navy Field's slot-based fitting combined with Escape from Tarkov's tetris-style inventory management. Players have complete freedom to configure ships within physical and weight constraints, with no artificial tier restrictions—if you can fit it and crew it, you can use it.

**Design Principles:**
- **Weight-Based Balancing**: Module and crew weight naturally limits over-fitting on smaller ships
- **Crew-Module Integration**: Every module requires appropriately classed crew card to function optimally
- **Visual Transparency**: Equipped modules visible on ship sprites, creating tactical intelligence opportunities
- **Economic Depth**: Multiple acquisition paths (crafting, market, salvage, black market) with distinct trade-offs
- **Progressive Complexity**: Simple at low tiers, deep optimization potential at high tiers

---

## Dual Module System Architecture

### Section A: Weapon Hardpoints (Visual 3D Positioning)

**Hardpoint System Overview:**
- **Fixed 3D Positions**: Each ship class has predetermined mounting locations designed into hull structure
- **Visual Integration**: Installed turrets/weapons appear on ship sprite with accurate scale and positioning
- **Drag-Drop Interface**: Players drag turret modules from inventory onto hardpoint positions on ship view
- **Tactical Transparency**: Enemy players can identify ship capabilities by visual inspection of equipped turrets

**Hardpoint Categories:**

**Main Battery Slots** (Primary armament):
- **Ship-Specific Count**: Destroyers (2-5 slots), Cruisers (3-5 slots), Battleships (3-4 slots)
- **Weight Capacity**: Varies by ship class and design era (see Ship Slot Capacity Evolution section)
- **Compatibility**: Accepts any main battery turret that fits weight/size limits
- **Crew Requirement**: Each main battery slot requires 1 Gunner crew card

**Secondary Battery Slots** (Mid-caliber guns):
- **Purpose**: Anti-destroyer defense, sustained fire support
- **Typical Count**: Cruisers (4-8 slots), Battleships (10-16 slots)
- **Caliber Range**: 4-inch to 6-inch guns typically
- **Crew Requirement**: Each secondary slot requires 1 Gunner crew card

**Tertiary/AA Slots** (Anti-aircraft defense):
- **Purpose**: Aircraft defense, close-range protection
- **High Slot Count**: 8-80+ depending on ship size and era
- **Module Types**: 20mm, 40mm, 5-inch dual-purpose mounts
- **Crew Requirement**: Each AA slot requires 1 AA Specialist crew card

**Special Hardpoints** (Class-specific systems):
- **Torpedo Tubes**: Destroyers (2-4 mounts), Submarines (4-6 tubes)
  - Crew: Torpedoman crew card required
- **Aircraft Catapults**: Cruisers (1-2 catapults), Battleships (2-4), Carriers (2-4)
  - Crew: Aviation crew card required per catapult
  - Requires: Aircraft Catapult Control module in Misc slot
- **Depth Charge Racks**: Destroyers (2-4 racks), Escorts (4-8 racks)
  - Crew: Damage Control crew card
- **Mine Rails**: Destroyers/Cruisers (1-2 rails)
  - Crew: Torpedoman crew card
  - Requires: Mining Equipment module

**Hardpoint Compatibility Rules:**
```
Turret Installation Check:
1. Module Type = Main/Secondary/Tertiary Battery
2. Module Physical Weight ≤ Hardpoint Capacity
3. Crew Card Assigned with matching class (Gunner/AA Specialist)
4. Total ship weight within limits after installation

Result: Green = Valid, Red = Invalid, Yellow = Over-weight warning
```

---

### Section B: Internal Ship Systems (Grid-Based Slot Fitting)

**Internal System Overview:**
- **Grid-Based Slots**: Fixed-size slots within ship's internal structure
- **Size Restrictions**: Modules must match slot size (1x1, 1x2, 2x2, 2x3, 3x3, etc.)
- **Category Restrictions**: Engine slots only accept engines, Support slots only accept support modules, Misc slots universal
- **Visual Interface**: Fitting screen shows ship cross-section with slot layout

**Internal System Categories:**

#### Engine Slots

**Slot Characteristics:**
- **Fixed Sizes**: Ship-specific (Small ships: 1x2, Medium ships: 2x3, Large ships: 3x4)
- **Multiple Bays**: Destroyers (2 slots), Cruisers (3 slots), Battleships (4-6 slots)
- **Accepts**: Engine modules ONLY
- **Crew Requirement**: Each engine slot requires 1 Engineer crew card

**Engine Slot Examples by Ship Class:**
| Ship Class | Engine Slots | Slot Size | Total Engine Capacity |
|------------|-------------|-----------|---------------------|
| T3 Destroyer | 2 slots | 1x2 each | Compact engines only |
| T5 Light Cruiser | 3 slots | 2x2 each | Medium engines |
| T7 Battleship | 4 slots | 3x4 each | Large/Heavy-Duty engines |
| T10 Carrier | 4 slots | 3x4 each | Maximum propulsion |

**Engine Configuration Strategy:**
- **Speed Build**: All high-performance engines (max speed, high fuel consumption)
- **Endurance Build**: All heavy-duty engines (moderate speed, excellent fuel economy)
- **Hybrid Build**: Mix of engine types (balanced performance)
- **Redundancy**: Extra engine bays provide backup if engines damaged in combat

**Engine Module Types** (See Module Progression Trees section for full details):
- Destroyer Engines: 28-38 knots capability, varying fuel efficiency
- Cruiser Engines: 28-38 knots, higher power requirements
- Battleship Engines: 23-32 knots, massive fuel consumption
- Specialized Variants: High-efficiency, experimental high-speed, endurance-optimized

---

#### Support Slots

**Slot Characteristics:**
- **Variable Sizes**: 1x1, 1x2, 2x2, 2x3, 3x3 depending on ship design
- **Quantity**: Scales with ship size (Destroyers: 3-8 slots, Battleships: 18-24 slots)
- **Accepts**: Support modules ONLY (crew welfare, engineering support, logistics)
- **Crew Requirement**: Varies by support module type (typically Engineer or Support crew)

**Support Module Categories:**

##### Crew Welfare Modules

**Crew Quarters**
- **Sizes**: 1x2 (Basic), 2x2 (Improved), 2x3 (Luxury)
- **Effects**:
  - Basic: +5 morale, stable morale decay
  - Improved: +10 morale, +5% crew efficiency
  - Luxury: +20 morale, +10% crew efficiency, +1 morale/hour at sea
- **Weight**: 15 tons (Basic), 25 tons (Improved), 40 tons (Luxury)
- **Crew**: 1 Support crew card required
- **Strategic Use**: Essential for long-range operations (7+ days at sea)

**Mess Hall**
- **Sizes**: 1x2 (Galley), 2x2 (Mess Hall), 2x3 (Officer's Mess)
- **Effects**:
  - Galley: +5 morale, basic food preparation
  - Mess Hall: +10 morale, +5% crew efficiency
  - Officer's Mess: +15 morale, +10% officer efficiency, improved crew coordination
- **Weight**: 12 tons (Galley), 22 tons (Mess Hall), 35 tons (Officer's Mess)
- **Crew**: 1 Support crew card required
- **Strategic Use**: Morale maintenance during extended deployments

**Medical Bay/Hospital**
- **Sizes**: 1x2 (Sick Bay), 2x3 (Medical Bay), 3x3 (Hospital)
- **Effects**:
  - Sick Bay: +10% crew injury recovery rate, treat 1 wounded
  - Medical Bay: +25% crew injury recovery, treat 2 wounded simultaneously
  - Hospital: +50% crew injury recovery, treat 5 wounded, surgery capability
- **Weight**: 10 tons (Sick Bay), 28 tons (Medical Bay), 50 tons (Hospital)
- **Crew**: 1 Medic crew card required (Medic class unlocked via research)
- **Strategic Use**: Critical for high-tier combat zones with crew casualty risk

**Recreation Room**
- **Size**: 1x2 (Basic), 2x2 (Advanced)
- **Effects**:
  - Basic: +10 morale, +5% crew efficiency during voyages >3 days
  - Advanced: +15 morale, +10% efficiency, reduces morale decay rate by 50%
- **Weight**: 18 tons (Basic), 30 tons (Advanced)
- **Crew**: 1 Support crew card required
- **Strategic Use**: Long-range extraction missions, extended combat operations

##### Engineering Support Modules

**Damage Control Station**
- **Sizes**: 1x2 (Basic), 2x2 (Advanced), 2x3 (Elite)
- **Effects**:
  - Basic: +10% fire suppression speed
  - Advanced: +25% fire suppression, +10% flooding control
  - Elite: +50% fire/flood control, automatic response systems, reduced crew casualties from damage
- **Weight**: 8 tons (Basic), 18 tons (Advanced), 32 tons (Elite)
- **Crew**: 1 Damage Control crew card required
- **Strategic Use**: Essential for high-tier combat, permadeath zone operations

**Machine Shop**
- **Sizes**: 2x2 (Basic), 2x3 (Advanced)
- **Effects**:
  - Basic: Field repairs 25% more effective, can craft basic ammunition if resources available
  - Advanced: Field repairs 50% more effective, can craft modules/parts, improves repair kit effectiveness
- **Weight**: 35 tons (Basic), 55 tons (Advanced)
- **Crew**: 1 Engineer crew card required
- **Strategic Use**: Extended operations far from friendly ports, self-sufficiency

**Auxiliary Generator**
- **Size**: 1x1 (Small), 1x2 (Large)
- **Effects**:
  - Small: Backup power if 1 engine damaged, +5% engine reliability
  - Large: Backup power if 2 engines damaged, +10% engine reliability, powers emergency systems
- **Weight**: 12 tons (Small), 22 tons (Large)
- **Crew**: 1 Engineer crew card required
- **Strategic Use**: Redundancy for critical systems (radar, fire control remain online if engines damaged)

**Fuel Purification System**
- **Size**: 1x2
- **Effects**: +10% fuel efficiency, allows use of lower-grade fuel without engine damage
- **Weight**: 15 tons
- **Crew**: 1 Engineer crew card required
- **Strategic Use**: Long-range missions, economic fuel savings

**Engine Governor**
- **Size**: 1x1
- **Effects**: Toggle between modes:
  - **Speed Mode**: +5% max speed, normal fuel consumption
  - **Economy Mode**: Normal speed, +15% fuel efficiency
- **Weight**: 8 tons
- **Crew**: 1 Engineer crew card required
- **Strategic Use**: Flexibility for different mission phases (speed for combat, economy for transit)

##### Storage & Logistics Modules

**Expanded Magazine**
- **Sizes**: 2x2 (Small), 2x3 (Medium), 3x3 (Large)
- **Effects**: Does NOT occupy cargo grid, adds separate ammunition storage slots
  - Small: +20 ammo storage slots, 40 tons weight
  - Medium: +40 ammo storage slots, 75 tons weight
  - Large: +60 ammo storage slots, 120 tons weight
- **Crew**: 1 Support crew card required
- **Strategic Use**: Extended combat operations, reduces resupply frequency

**Refrigeration Unit**
- **Sizes**: 1x2 (Basic), 2x2 (Advanced)
- **Effects**:
  - Basic: Extends supply lifetime by 100% (food/medical supplies don't spoil), +5 morale
  - Advanced: Extends supply lifetime by 200%, +10 morale, allows 30+ day voyages
- **Weight**: 20 tons (Basic), 35 tons (Advanced)
- **Crew**: 1 Support crew card required
- **Strategic Use**: Long-range operations, crew morale maintenance

**Cargo Expansion Module**
- **Sizes**: 2x2 (Small), 2x3 (Large)
- **Effects**: Adds +20-40 grid inventory cells (expands total cargo grid size)
  - Small: +20 cells, 50 tons weight penalty
  - Large: +40 cells, 95 tons weight penalty
- **Crew**: 1 Support crew card required
- **Strategic Use**: Trade ships, resource extraction missions, maximizing loot capacity

##### Specialized Support Modules

**Smoke Generator**
- **Sizes**: 1x1 (Basic), 1x2 (Advanced)
- **Effects**: Deployable smoke screen obscures ship from enemy vision
  - Basic: 3 charges, 60-second smoke duration, 5-minute cooldown
  - Advanced: 5 charges, 90-second smoke duration, 3-minute cooldown, larger smoke cloud
- **Weight**: 10 tons (Basic), 18 tons (Advanced)
- **Crew**: 1 Support crew card required
- **Refill**: Refillable at port (₡5,000 per charge)
- **Strategic Use**: Evasion during extraction, breaking line of sight, tactical positioning

**Decoy Launcher**
- **Size**: 1x1
- **Effects**: Launches chaff/decoys to confuse radar and torpedoes
  - 10 charges per module
  - 30% chance to deflect radar-guided weapons
  - 20% chance to decoy torpedoes
- **Weight**: 8 tons
- **Crew**: 1 Support crew card required
- **Refill**: ₡3,000 per charge
- **Strategic Use**: Defensive countermeasures, high-tier zone survival

**Salvage Equipment**
- **Sizes**: 2x3 (Basic), 3x3 (Advanced)
- **Effects**: Required to salvage wrecks in extraction zones
  - Basic: Salvage speed 100% (baseline), can salvage common/uncommon modules
  - Advanced: Salvage speed 200%, can salvage rare/exceptional modules, better loot quality
- **Weight**: 60 tons (Basic), 95 tons (Advanced)
- **Crew**: 1 Engineer crew card required
- **Strategic Use**: Dedicated salvage ships, extraction profession, wreck looting

**Research Lab** (Late-tier module)
- **Size**: 2x3 (Basic), 3x3 (Advanced)
- **Effects**:
  - Basic: Allows blueprint research at sea (50% slower than port), grants +10% research points
  - Advanced: Blueprint research at sea (normal speed), +25% research points, can analyze captured enemy equipment
- **Weight**: 40 tons (Basic), 70 tons (Advanced)
- **Crew**: 1 Support crew card required (Intelligence specialization)
- **Strategic Use**: Extended operations, faction progression optimization

---

#### Misc Slots

**Slot Characteristics:**
- **Universal Acceptance**: Can accept ANY module type (engines, support, or misc-specific modules)
- **Variable Sizes**: Ship-specific (1x1 to 3x3 possible)
- **Quantity**: Lower than support slots (Destroyers: 2-4, Battleships: 6-12)
- **Crew Requirement**: Varies by module (typically Electronics crew cards)

**Misc Module Categories:**

##### Detection & Electronics Modules

**Radar Systems** (See Module Progression Trees for full tech tree):
- **Sizes**: 1x1 (Basic), 1x2 (Improved), 2x2 (Advanced), 2x3 (Late-War), 3x3 (Experimental)
- **Progression**: Basic Radar (30km) → Improved (50km) → Advanced (80km) → Late-War Integrated (120km) → Experimental AEGIS (150km)
- **Effects**: Detection range, target tracking capability, IFF (Identify Friend/Foe), multi-target tracking
- **Weight**: 8-50 tons depending on tier
- **Crew**: 1 Radar Operator crew card required (Electronics class)
- **Strategic Use**: Vision control, fire control integration, air defense coordination

**Sonar Systems** (Destroyers/Submarines):
- **Sizes**: 1x1 (Passive), 1x2 (Active)
- **Types**:
  - **Passive Sonar**: Detects underwater contacts 5-15km, silent operation
  - **Active Sonar**: Detects underwater contacts 15-30km, reveals your position when pinging
  - **Advanced Sonar**: Better depth detection, tracks torpedo trajectories, 30km range
- **Weight**: 10 tons (Passive), 18 tons (Active), 28 tons (Advanced)
- **Crew**: 1 Radar Operator crew card required
- **Strategic Use**: Anti-submarine warfare, submarine hunting, torpedo defense

**Hydrophones** (Submarines):
- **Size**: 1x1
- **Effects**: Ultra-long-range passive detection (30-50km), detects engine noise and propeller signatures
  - Cannot determine exact position, only general bearing and approximate distance
  - Silent operation (no active pinging)
- **Weight**: 6 tons
- **Crew**: 1 Radar Operator crew card required
- **Strategic Use**: Submarine stealth operations, convoy detection, strategic positioning

##### Electronic Warfare Modules

**Radar Jammer**
- **Sizes**: 1x1 (Basic), 1x2 (Advanced)
- **Effects**:
  - Basic: Reduces enemy radar detection range by 20%, interferes with fire control radar (-10% enemy accuracy)
  - Advanced: Reduces enemy radar detection range by 50%, severe fire control interference (-25% enemy accuracy)
  - Active use alerts enemies to your presence
- **Weight**: 12 tons (Basic), 22 tons (Advanced)
- **Crew**: 1 Electronics crew card required
- **Strategic Use**: Stealth operations, countering radar-heavy enemies, extraction evasion

**Signal Intelligence (SIGINT) Module**
- **Size**: 1x2
- **Effects**:
  - Intercepts enemy radio communications
  - Detects enemy positions via radio triangulation (rough bearing, 50km range)
  - Provides early warning of enemy fleet movements
  - Can decode encrypted messages with time delay
- **Weight**: 15 tons
- **Crew**: 1 Electronics crew card required (SIGINT specialization)
- **Strategic Use**: Intelligence gathering, fleet coordination, strategic awareness

**Decoy Transmitter**
- **Size**: 1x1
- **Effects**:
  - Emits false radar signature (makes ship appear larger/different class)
  - 5 charges, refillable at port
  - Limited duration (5 minutes per charge)
  - Effective against AI, experienced players may detect deception
- **Weight**: 8 tons
- **Crew**: 1 Electronics crew card required
- **Refill**: ₡8,000 per charge
- **Strategic Use**: Psychological warfare, bluffing, extraction deception

##### Fire Control & Targeting Modules

**Optical Rangefinder** (See Module Progression Trees for details):
- **Sizes**: 1x1 (Basic), 1x2 (Advanced)
- **Effects**:
  - Basic: +5% accuracy for main battery, works when radar damaged
  - Advanced: +10% accuracy, better vs small/fast targets, weather-resistant
- **Weight**: 6 tons (Basic), 12 tons (Advanced)
- **Crew**: 1 Gunner crew card required (Fire Control specialization)
- **Stacking**: Stacks additively with fire control computers
- **Strategic Use**: Backup targeting, radar-denied environments, destroyer combat

**Fire Control Computer** (See Module Progression Trees for full tech tree):
- **Sizes**: 1x1 (Basic Optical), 1x2 (Mechanical), 2x2 (Electronic Analog), 2x3 (Digital), 3x3 (AI-Assisted)
- **Progression**: Basic (+5% accuracy) → Mechanical (+15%) → Electronic (+30%) → Digital (+50%) → AI-Assisted (+75%)
- **Multi-Target Capability**: Basic (1 target) → AI-Assisted (12 targets)
- **Weight**: 5-50 tons depending on tier
- **Crew**: 1 Gunner crew card required (Fire Control specialization)
- **Strategic Use**: Gunnery accuracy enhancement, multi-target engagements, competitive edge

**Torpedo Fire Control** (Destroyers/Submarines):
- **Size**: 1x1
- **Effects**:
  - Calculates torpedo intercept solutions automatically
  - +20% torpedo hit rate vs maneuvering targets
  - Can pre-program torpedo spread patterns
  - Shows predicted impact points on tactical display
- **Weight**: 8 tons
- **Crew**: 1 Torpedoman crew card required
- **Strategic Use**: Torpedo accuracy, destroyer combat effectiveness, submarine lethality

##### Defensive Systems

**Close-In Weapon System (CIWS)** (Late-tier technology):
- **Sizes**: 1x1 (Basic), 1x2 (Advanced)
- **Effects**:
  - Automated last-ditch defense vs aircraft and missiles
  - Basic: +30% AA effectiveness within 2km, 500 rounds ammo capacity
  - Advanced: +50% AA effectiveness within 3km, 1000 rounds, can engage missiles
  - Limited ammunition, refillable at port
- **Weight**: 15 tons (Basic), 28 tons (Advanced)
- **Crew**: 1 AA Specialist crew card required
- **Refill**: ₡15,000 per ammo reload
- **Strategic Use**: Carrier defense, high-tier zone air threats, missile defense

**Torpedo Defense System**
- **Sizes**: 1x2 (Passive), 2x2 (Active)
- **Types**:
  - **Passive (Torpedo Nets)**: +20% chance to deflect torpedoes, always active, no ammo
  - **Active (Counter-Torpedoes)**: Launches counter-torpedoes to intercept incoming torpedoes (40% intercept chance), 8 charges
- **Weight**: 25 tons (Passive), 45 tons (Active)
- **Crew**: 1 Damage Control crew card required
- **Refill**: ₡20,000 per counter-torpedo (Active only)
- **Strategic Use**: Battleship/carrier protection, submarine threat mitigation

**Countermeasure Dispenser**
- **Size**: 1x1
- **Effects**:
  - Launches chaff vs radar-guided weapons (50% effectiveness)
  - Launches flares vs heat-seeking weapons (60% effectiveness, late-war/modern)
  - 20 charges total, refillable
- **Weight**: 10 tons
- **Crew**: 1 Support crew card required
- **Refill**: ₡2,000 per charge
- **Strategic Use**: Missile defense (if game progresses to guided missile era)

##### Special Systems

**Submarine Periscope** (Submarines only):
- **Size**: 1x1
- **Effects**:
  - Required for surfaced/periscope depth observation
  - Basic: Visual observation only, 5km range
  - Advanced: Integrated rangefinder, 8km range, camera system for intelligence
- **Weight**: 4 tons (Basic), 7 tons (Advanced)
- **Crew**: 1 Command crew card required
- **Strategic Use**: Mandatory submarine module, periscope depth operations

**Snorkel System** (Submarines only):
- **Size**: 1x1
- **Effects**:
  - Allows diesel engine use while submerged at shallow depth (15m)
  - Increases submerged endurance dramatically (10x battery life)
  - Slight speed reduction vs surfaced (80% max speed)
  - Detectable by radar when snorkeling
- **Weight**: 6 tons
- **Crew**: 1 Engineer crew card required
- **Strategic Use**: Extended submerged operations, battery management

**Aircraft Catapult Control** (Ships with catapult hardpoints):
- **Size**: 1x1
- **Effects**:
  - Required to operate aircraft catapults
  - Can launch scout planes for reconnaissance
  - Provides over-horizon targeting data (+20km fire control range)
  - Aircraft return time: 10-20 minutes
- **Weight**: 8 tons
- **Crew**: 1 Aviation crew card required
- **Strategic Use**: Battleship/cruiser reconnaissance, spotting for long-range fire

**Mining Equipment** (Destroyers/Cruisers):
- **Sizes**: 2x2 (Basic), 2x3 (Advanced)
- **Effects**:
  - Allows deployment of naval mines
  - Basic: Stores 5 mines, basic contact mines
  - Advanced: Stores 20 mines, magnetic/acoustic mine options
  - Mines persist in world for 24 hours real-time or until detonated
- **Weight**: 45 tons (Basic), 80 tons (Advanced)
- **Crew**: 1 Torpedoman crew card required
- **Refill**: ₡25,000 per mine
- **Strategic Use**: Area denial, defensive operations, trap setting

---

## Armor Configuration System

**Navy Field-Inspired Armor System:**

WOS2.3 uses a detailed armor configuration interface allowing precise thickness adjustment for multiple armor zones. Armor thickness directly affects penetration mechanics and damage mitigation.

### Armor Interface Design

**Armor Configuration Screen:**
- **Visual Schematic**: Side and top-down view of ship showing armor zones
- **9 Armor Zones**: 3 deck zones, 3 side belt zones, 3 structural zones
- **Dual Input Methods**:
  - **Dropdown Menu**: Select armor type (material)
  - **Manual Entry**: Input exact thickness in inches (0.1" increments)
  - **Slider Control**: Quick adjustment with fine-tuning capability
- **Real-Time Feedback**: Weight, speed, cost calculations update instantly
- **MM Equivalents**: Shows millimeter conversion next to inch values

### Armor Zone Configuration (9 Total Zones)

**Deck Armor (Horizontal Protection):**
1. **Forward Deck** - [Armor Type] [Thickness in inches (mm)]
2. **Center Deck** - [Armor Type] [Thickness in inches (mm)]
3. **Aft Deck** - [Armor Type] [Thickness in inches (mm)]

**Side Belt Armor (Vertical Protection):**
4. **Forward Belt** - [Armor Type] [Thickness in inches (mm)]
5. **Center Belt** - [Armor Type] [Thickness in inches (mm)]
6. **Aft Belt** - [Armor Type] [Thickness in inches (mm)]

**Structural Armor (Critical Components):**
7. **Conning Tower** - [Armor Type] [Thickness in inches (mm)]
8. **Main Turrets** - [Armor Type] [Thickness in inches (mm)]
9. **Secondary Turrets** - [Armor Type] [Thickness in inches (mm)]

**Note**: Tertiary/AA turrets are unarmored and not configurable.

### Armor Type Options (Dropdown Selection)

**Rolled Homogeneous Armor (RHA)** - Standard:
- **Protection Coefficient**: 1.0 (baseline)
- **Weight per Inch**: 40 tons/inch (baseline)
- **Cost Multiplier**: 1.0x
- **Availability**: Universal, all nations
- **Best Use**: Balanced protection, general-purpose

**Face-Hardened Armor** - AP-Focused:
- **Protection Coefficient**: 1.3 vs AP shells, 0.8 vs HE shells
- **Weight per Inch**: 45 tons/inch (heavier)
- **Cost Multiplier**: 1.4x
- **Availability**: Common
- **Best Use**: Battleship duels, AP-heavy combat zones
- **Drawback**: Brittle vs HE, vulnerable to fire damage

**Krupp Cemented Armor** - German Specialty:
- **Protection Coefficient**: 1.4 vs AP shells, 1.1 vs HE shells
- **Weight per Inch**: 50 tons/inch (very heavy)
- **Cost Multiplier**: 2.0x
- **Availability**: German ports (expensive elsewhere)
- **Best Use**: Premium protection, high-stakes operations
- **Drawback**: Expensive, very heavy (speed penalty)

**Terni Steel** - Italian Specialty:
- **Protection Coefficient**: 0.9 vs AP shells, 1.2 vs HE shells
- **Weight per Inch**: 32 tons/inch (lighter)
- **Cost Multiplier**: 1.2x
- **Availability**: Italian ports, Mediterranean region
- **Best Use**: Cruisers, speed-focused builds
- **Advantage**: Lighter weight, good HE resistance

**Ducol Steel** - British Specialty:
- **Protection Coefficient**: 0.85 vs AP shells, 1.0 vs HE shells
- **Weight per Inch**: 28 tons/inch (very light)
- **Cost Multiplier**: 1.1x
- **Availability**: British ports, Commonwealth
- **Best Use**: Destroyers, speed-critical ships
- **Advantage**: Minimal weight penalty, good for light ships

**Special Treatment Steel (STS)** - US Specialty:
- **Protection Coefficient**: 1.15 vs AP shells, 1.15 vs HE shells
- **Weight per Inch**: 42 tons/inch (moderate)
- **Cost Multiplier**: 1.8x
- **Availability**: US ports (expensive elsewhere)
- **Best Use**: All-around protection, balanced builds
- **Advantage**: Excellent general-purpose protection

### Armor Thickness Configuration

**Input Methods:**
- **Manual Entry**: Type exact value (e.g., "10.5" for 10.5 inches / 266.7mm)
- **Slider**: Drag to adjust in 0.5" increments, fine-tune with arrow keys (0.1" per click)
- **Increment Buttons**: +/- 0.1" buttons for precise adjustment
- **Display Format**: "10.5" (266.7mm)" - inches primary, millimeters in parentheses

**Ship Class Armor Limits:**
| Ship Class | Maximum Deck | Maximum Belt | Maximum Turret | Maximum Conning Tower |
|------------|-------------|--------------|----------------|---------------------|
| Destroyer | 0.5" (12.7mm) | 2.0" (50.8mm) | 1.0" (25.4mm) | 0.5" (12.7mm) |
| Light Cruiser | 2.0" (50.8mm) | 4.0" (101.6mm) | 3.0" (76.2mm) | 2.0" (50.8mm) |
| Heavy Cruiser | 3.0" (76.2mm) | 8.0" (203.2mm) | 6.0" (152.4mm) | 4.0" (101.6mm) |
| Battleship | 7.0" (177.8mm) | 18.0" (457.2mm) | 17.0" (431.8mm) | 12.0" (304.8mm) |
| Carrier | 3.0" (76.2mm) | 4.0" (101.6mm) | N/A | 2.0" (50.8mm) |

### Armor Weight & Performance Impact

**Weight Calculation Formula:**
```
Zone Weight = Thickness (inches) × Armor Type Weight Multiplier × Zone Size Factor

Example (Iowa Battleship Center Belt):
- Thickness: 12.1" (307.3mm) RHA
- RHA Weight: 40 tons/inch
- Zone Size Factor: 45 (large zone)
- Total Weight: 12.1 × 40 × 45 = 21,780 tons for center belt alone
```

**Performance Impact:**
- **Speed Reduction**: Total armor weight affects max speed
  - Light armor (0-500 tons): No penalty
  - Moderate armor (500-2000 tons): -1 to -3 knots
  - Heavy armor (2000-5000 tons): -3 to -6 knots
  - Extreme armor (5000+ tons): -6 to -12 knots
- **Fuel Consumption**: Heavier ships consume more fuel per km traveled
- **Maneuverability**: Turn rate reduced proportional to total weight

**Armor Cost Calculation:**
```
Zone Cost = Base Cost × Thickness² × Armor Type Multiplier

Example (12" Belt RHA):
- Base Cost: ₡10,000 per zone
- Thickness: 12 inches
- Armor Type: RHA (1.0x multiplier)
- Total: ₡10,000 × 144 × 1.0 = ₡1,440,000 for one belt zone
```

### Armor Acquisition Methods

**Base Armor** (Inherent to Ship Design):
- Ships come with historical armor configuration by default
- Can be modified freely at any friendly port

**Armor Upgrades** (Unlocked via Research):
- **Basic Armor Schemes**: Default, available immediately
- **Improved Armor Schemes**: Unlock via ship research tree (e.g., "Iowa Modernized Armor Scheme")
- **Special Armor Types**: Unlock specific armor materials (Krupp, STS) via nation research
- **Blueprint-Based Schemes**: Find/earn special armor configurations (historical refits, experimental designs)

**Armor Configuration Constraints:**
- **Weight Budget**: Total armor + modules + crew ≤ ship maximum displacement
- **Ship Class Limits**: Cannot exceed max thickness per ship class
- **Historical Accuracy Mode** (Optional Toggle): Restricts armor to historically plausible ranges

### Example Armor Configurations

**T3 Fletcher-class Destroyer (Speed Build):**
```
Armor Type: Ducol Steel (lightweight British steel)
Forward Deck: 0.2" (5.1mm)
Center Deck: 0.3" (7.6mm)
Aft Deck: 0.2" (5.1mm)
Forward Belt: 0.5" (12.7mm)
Center Belt: 1.0" (25.4mm)
Aft Belt: 0.5" (12.7mm)
Conning Tower: 0.5" (12.7mm)
Main Turrets: 0.5" (12.7mm)
Secondary Turrets: 0" (unarmored)

Total Armor Weight: 285 tons
Speed Impact: No penalty (under 500 tons)
Total Cost: ₡125,000
```

**T8 Iowa-class Battleship (Balanced Build):**
```
Armor Type: Special Treatment Steel (US premium armor)
Forward Deck: 4.0" (101.6mm)
Center Deck: 6.0" (152.4mm)
Aft Deck: 4.0" (101.6mm)
Forward Belt: 10.0" (254.0mm)
Center Belt: 12.1" (307.3mm)
Aft Belt: 10.0" (254.0mm)
Conning Tower: 7.25" (184.2mm)
Main Turrets: 17.0" (431.8mm)
Secondary Turrets: 2.5" (63.5mm)

Total Armor Weight: 4,850 tons
Speed Impact: -5 knots (from base 33 knots to 28 knots)
Total Cost: ₡8,500,000
```

**T10 Yamato-class Battleship (Maximum Protection Build):**
```
Armor Type: Krupp Cemented Armor (German premium, ultimate protection)
Forward Deck: 7.0" (177.8mm)
Center Deck: 7.0" (177.8mm)
Aft Deck: 7.0" (177.8mm)
Forward Belt: 16.0" (406.4mm)
Center Belt: 18.0" (457.2mm) [MAXIMUM]
Aft Belt: 16.0" (406.4mm)
Conning Tower: 12.0" (304.8mm)
Main Turrets: 17.0" (431.8mm)
Secondary Turrets: 3.0" (76.2mm)

Total Armor Weight: 7,200 tons
Speed Impact: -10 knots (from base 27 knots to 17 knots)
Total Cost: ₡18,500,000
Strategic Trade-off: Maximum survivability, severe speed penalty
```

---

## Module-Crew Integration System

**Core Principle**: Every module requires an appropriately classed crew card to function at optimal efficiency.

### Crew-Module Assignment Mechanics

**Assignment Process:**
1. **Install Module** in ship slot (hardpoint or internal system slot)
2. **Assign Crew Card** to that module's crew slot
3. **Validation Checks**:
   - Crew class matches module type (Gunner → Turret, Engineer → Engine)
   - Crew card not already assigned to another module
   - Crew skill level meets module minimum requirements (if any)
4. **Result**: Module operates at efficiency based on crew level and specialization

**Crew Class → Module Type Compatibility:**

| Module Type | Required Crew Class | Notes |
|-------------|-------------------|-------|
| Main Battery Turret | Gunner | Any gunner works on any caliber |
| Secondary Battery | Gunner | Same crew pool as main battery |
| AA Mount | AA Specialist | Separate class from gunners |
| Torpedo Tubes | Torpedoman | Also operates mine rails |
| Engine | Engineer | Any engineer works on any engine |
| Radar System | Radar Operator (Electronics class) | |
| Sonar System | Radar Operator (Electronics class) | |
| Fire Control System | Gunner (Fire Control specialization) | |
| Damage Control Station | Damage Control (Engineer subclass) | |
| Medical Bay | Medic | Requires research unlock |
| Support Modules | Support Crew | Generic support class |
| Aircraft Catapult | Aviation | Carriers/cruisers/battleships |
| Submarine Periscope | Command | Submarine captains |

### Module Efficiency System

**Crew Level Matching:**

Each module has a **recommended crew level** for 100% efficiency. Crew level vs. module level determines performance:

**Efficiency Curve Formula:**
```
Efficiency % = 35% + (Crew Level / Module Level) × 65%
Capped at 130% maximum (Level 200 crew on Level 90 module)

Examples:
- Level 1 crew on Level 90 module = 36% efficiency (severe penalty)
- Level 45 crew on Level 90 module = 68% efficiency (half level penalty)
- Level 70 crew on Level 90 module = 86% efficiency (close but not optimal)
- Level 90 crew on Level 90 module = 100% efficiency (perfect match)
- Level 120 crew on Level 90 module = 115% efficiency (over-leveling bonus)
- Level 200 crew on Level 90 module = 130% efficiency (maximum bonus)
```

**Efficiency Impact by Module Type:**

**Main Battery Turret:**
- **Reload Speed**: Efficiency % directly affects reload time
  - 100% efficiency = 30 second base reload
  - 130% efficiency = 23 second reload (23% faster)
  - 50% efficiency = 60 second reload (100% slower)
- **Accuracy**: Efficiency % affects hit probability
  - 100% efficiency = 75% hit chance at optimal range
  - 130% efficiency = 97.5% hit chance
  - 50% efficiency = 37.5% hit chance

**Engine:**
- **Max Speed**: Efficiency % affects top speed
  - 100% efficiency = 33 knots
  - 130% efficiency = 35 knots (+6% speed bonus)
  - 50% efficiency = 16.5 knots (-50% speed)
- **Fuel Efficiency**: Higher efficiency = better fuel economy
- **Acceleration**: Efficiency affects time to reach max speed

**Radar System:**
- **Detection Range**: Efficiency % affects maximum detection distance
  - 100% efficiency = 80km base range
  - 130% efficiency = 88km range
  - 50% efficiency = 40km range
- **Update Rate**: Higher efficiency = faster target updates

**Damage Control Station:**
- **Fire Suppression Speed**: Efficiency % directly affects extinguishing time
- **Flooding Control**: Efficiency affects pumping effectiveness

### Unassigned Module Penalty

**Modules without assigned crew operate at 25% efficiency:**
- Main turret with no gunner: 75-second reload instead of 30-second
- Engine with no engineer: 8 knots max speed instead of 32 knots
- Radar with no operator: 20km range instead of 80km
- **Warning Indicator**: Red icon on module slot showing "No Crew Assigned"

**Strategic Implications:**
- Players must maintain adequate crew roster for all equipped modules
- Losing crew cards in battle creates immediate performance degradation
- Backup crew cards essential for high-tier operations

### Multiple Module Management

**Example: Battleship with 3 Main Turrets and 4 Engines:**
- **Turrets**: Requires 3 Gunner crew cards (one per turret)
- **Engines**: Requires 4 Engineer crew cards (one per engine)
- **Minimum Crew**: 7 specialized crew cards just for turrets and engines
- **Realistic Loadout**: 15-20 crew cards total (includes secondaries, AA, support modules)

**Crew Capacity Limits by Ship Class:**

| Ship Class | Total Crew Card Capacity | Typical Module Count |
|------------|------------------------|-------------------|
| T1 Destroyer | 8-12 crew cards | 5-8 modules |
| T3 Cruiser | 12-18 crew cards | 10-15 modules |
| T5 Battleship | 20-30 crew cards | 18-25 modules |
| T8 Carrier | 25-40 crew cards | 25-35 modules |
| T10 Super Battleship | 30-45 crew cards | 30-40 modules |

**Crew Weight Impact:**
- Each crew card has weight based on sailor count and level
- High-level crew cards can weigh 300-1200 tons
- Must balance crew quality vs. ship weight capacity
- Natural progression: bring veteran crew to larger ships as you tier up

---

## Module Acquisition & Progression

**Note**: This section references the existing detailed Module Unlock & Progression System already documented in the GDD (lines 3359-4182). Key points summarized below for integration.

### Acquisition Pathways

**1. Research & Development** (Unlock Path):
- Progressive unlock trees with credit, resource, and time investment
- Permanent blueprint ownership
- Craft unlimited quantities at production cost
- Long-term cost savings (30-70% cheaper than market)
- Example: 16" Iowa gun unlock costs ₡5M + 400 hours research, craft for ₡45K vs. ₡275K market price

**2. Market Purchase** (Direct Purchase Path):
- Buy from NPC vendors or player marketplace
- Immediate access, no research time
- Higher per-unit cost
- No blueprint retention
- Example: Buy 16" Iowa gun instantly for ₡275K without unlocks

**3. Black Market** (Unrestricted Access):
- Purchase any module regardless of nation or unlock status
- 200-400% markup over standard pricing
- Access to enemy nation technology
- Requires black market reputation
- Example: Japanese 18" turret costs ₡450K on black market (vs. ₡150K Japanese vendor price)

**4. Extraction Loot**:
- Salvage modules from defeated enemy ships
- Condition-based value (100% new → 20% broken)
- Damaged modules require repair before use
- High-quality RNG modules (120%+ stats) extremely valuable

**5. Crafting** (Blueprint-Based Production):
- Requires unlocked blueprint + resources + time
- Quality variance: 70-130% base stats (RNG roll)
- Crafting skill improves quality odds
- Progression: Novice → Legendary crafter (eliminates poor quality rolls)

### Module Progression Trees

**Detailed progression trees already documented for:**
- Main Battery Turrets (5", 6", 8", 14", 16", 18" families)
- Engine Systems (destroyer, cruiser, battleship families)
- Radar & Detection (Basic → Experimental AEGIS)
- Fire Control Systems (Optical → AI-Assisted)

**Refer to GDD Section: "Module Family Progression Trees" (lines 3434-3895) for complete tech trees.**

### Quality Variance & Crafting RNG

**Module Quality Mechanics:**
- **RNG Range**: 70-130% of base statistics
- **Distribution**: Bell curve centered on 100%
- **Probability**:
  - 70-85% (Poor): 15% chance
  - 86-95% (Below Average): 20% chance
  - 96-105% (Average): 30% chance
  - 106-115% (Above Average): 20% chance
  - 116-125% (Excellent): 12% chance
  - 126-130% (Exceptional): 3% chance

**Economic Impact:**
- Poor quality: Sells for 50-70% market value
- Exceptional quality: Sells for 180-250% market value, highly sought after
- Players craft multiple modules seeking high-quality rolls
- Exceptional modules become prestige items worth millions

**Crafting Skill Progression:**
- Novice (0-50 crafted): Standard RNG (70-130%)
- Skilled (51-200): Improved RNG (75-130%)
- Expert (201-500): Superior RNG (80-130%)
- Master (501-1000): Optimal RNG (85-130%)
- Legendary (1000+): Ultimate RNG (90-130%) - poor quality eliminated

---

## Module Insurance System

**Enhanced Insurance Mechanics:**

Insurance in WOS2.3 protects valuable modules and crew from permadeath at T5+ tiers. Multiple insurance providers offer different reliability and pricing tiers.

### Insurance Properties

**Core Characteristics:**
- ✅ **Permanent Coverage**: No expiration, sticks to item forever once insured
- ✅ **Global Coverage**: Works in all regions, no geographic restrictions
- ✅ **Port Independence**: Moving to new region does not reset insurance
- ✅ **Flexible Enrollment**: Can insure items anytime at any friendly port
- ✅ **Unlimited Claims**: No weekly or monthly claim limits
- ✅ **Distance-Based Return**: Return time based on distance from insured port

### Insurance Providers

#### NPC Insurance Companies

**Budget Marine Insurance** (Economy Option):
- **Premium Cost**: 50% of item value
- **Protection Chance**: 40-60% (item survives ship loss)
- **Base Return Time**: 48 hours from insured port
- **Reliability**: Medium (occasional claim disputes)
- **Best For**: Low-mid tier modules, backup equipment, common items

**Lloyd's of London** (Standard Option):
- **Premium Cost**: 100% of item value
- **Protection Chance**: 60-75% (item survives ship loss)
- **Base Return Time**: 36 hours from insured port
- **Reliability**: High (reputable, fast claims processing)
- **Best For**: Mid-tier modules, primary equipment, crew cards

**Premium Naval Underwriters** (Elite Option):
- **Premium Cost**: 200% of item value
- **Protection Chance**: 80-95% (item survives ship loss)
- **Base Return Time**: 24 hours from insured port
- **Reliability**: Maximum (guaranteed claims, priority service)
- **Best For**: High-tier modules, exceptional quality items, elite crew cards
- **Extra Benefit**: +25% value payout if item does drop (compensation bonus)

#### Player-Run Insurance

**Community Insurance System:**

Players can offer insurance contracts, creating an emergent profession and player-driven economy.

**How It Works:**
1. **Contract Creation**: Player insurer advertises insurance services (rates, terms, reputation)
2. **Customer Purchase**: Other player pays premium to insurer for coverage
3. **Ship Loss Event**: Customer's ship sinks, insurance claim triggered
4. **Salvage Requirement**: Insurer must physically travel to wreck and salvage insured items
5. **Return or Profit**:
   - **Success**: Insurer salvages items, returns to customer, keeps premium as profit
   - **Failure**: Wreck despawns, PvP interruption, or salvage fails → insurer keeps items + premium

**Player Insurer Economics:**
- **Profit Model**: Charge premium upfront, hope to salvage before payout timer expires
- **Risk**: Must physically salvage wreck (travel time, PvP danger, wreck despawn)
- **Reward**: Keep premium + items if salvage fails
- **Reputation System**: Reliable insurers build customer base, scammers get blacklisted
- **Specialization**: Some insurers focus on specific regions (low travel time = higher success rate)

**Example Player Insurance Contract:**
*Insurer "SalvageKing" offers services:*
- Coverage Area: US East Coast (Norfolk to New York)
- Premium: 75% of item value
- Protection: 70% success rate (based on historical performance)
- Return Time: 12-24 hours depending on location
- Reputation: 4.8/5 stars, 150 successful claims

**Player Insurance Advantages:**
- Often cheaper premiums than NPC insurers (competitive pricing)
- Faster return times in specialized regions
- Personalized service (negotiate terms, bulk discounts)
- Supporting player economy and community

**Player Insurance Risks:**
- Lower reliability than NPC companies
- Scam potential (insurer takes premium, never salvages)
- Insurer may be offline when claim occurs
- PvP threats to insurer during salvage missions

### Insurance Return Time System

**Base Return Time Formula:**
```
Base Return Time = 24 hours (from insured port)
Distance Modifier = +1 hour per 100km from insured port
Maximum Return Time = 48 hours total

Example:
- Item insured at Norfolk (US East Coast)
- Ship sinks 300km away in contested waters
- Return Time = 24h base + (300km / 100km) × 1h = 27 hours
```

**Premium Acceleration Option:**
- **Standard Service**: Base return time (as calculated)
- **Expedited Service**: Pay 2x premium → 50% return time reduction
- **Priority Service**: Pay 5x premium → 75% return time reduction

**Return Time Examples:**

| Scenario | Distance | Insurer | Base Time | Accelerated (2x) | Priority (5x) |
|----------|---------|---------|-----------|-----------------|---------------|
| Sank near home port | 50km | Lloyd's | 24.5 hours | 12.25 hours | 6.1 hours |
| Contested zone | 300km | Budget Marine | 51 hours | 25.5 hours | 12.75 hours |
| Deep enemy territory | 800km | Premium Naval | 32 hours | 16 hours | 8 hours |
| Maximum distance | 1200km+ | Any | 48 hours (cap) | 24 hours | 12 hours |

### Insurance Claim Process

**When Ship Sinks (T5+ tiers):**
1. **Death Roll**: Each insured item independently rolls for survival
   - Premium Naval (95%): 19 out of 20 items expected to survive
   - Lloyd's (75%): 15 out of 20 items expected to survive
   - Budget (60%): 12 out of 20 items expected to survive
2. **Surviving Items**: Marked as "In Transit" with countdown timer
3. **Countdown Timer**: Based on distance from insured port + insurer return time
4. **Claim Completion**: Items returned to player's inventory at insured port when timer expires
5. **Failed Items**: Items that failed survival roll drop as loot at wreck (other players can salvage)

**Player-Run Insurance Claims:**
1. **Death Event**: Insured player's ship sinks
2. **Notification**: Insurer receives claim notification with wreck location
3. **Salvage Window**: Insurer has limited time to reach wreck (30-60 minutes real-time)
4. **Salvage Process**: Insurer must use Salvage Equipment module to retrieve items
5. **Return Process**: Insurer delivers items to customer at agreed port
6. **Payment**: Insurer keeps premium as profit (already paid upfront)

### Insurance Strategy & Economics

**What to Insure:**
- **High Priority**: Exceptional quality modules (120%+ stats), elite crew cards (Level 100+)
- **Medium Priority**: Standard high-tier modules, mid-level crew cards
- **Low Priority**: Common modules, easily replaceable items, low-tier equipment

**Premium Cost Examples:**

| Item | Market Value | Budget Premium (50%) | Lloyd's Premium (100%) | Premium Naval (200%) |
|------|--------------|---------------------|----------------------|---------------------|
| Exceptional 16" Turret (128% quality) | ₡5,500,000 | ₡2,750,000 | ₡5,500,000 | ₡11,000,000 |
| Level 150 Gunner Crew Card | ₡8,000,000 | ₡4,000,000 | ₡8,000,000 | ₡16,000,000 |
| Advanced Radar System | ₡450,000 | ₡225,000 | ₡450,000 | ₡900,000 |
| Standard Engine Module | ₡180,000 | ₡90,000 | ₡180,000 | ₡360,000 |

**Insurance Math (T8 Battleship Full Loadout):**
- **Total Modules Value**: ₡45,000,000 (3 main turrets, engines, radar, fire control, support modules)
- **Lloyd's Premium (100%)**: ₡45,000,000 for 75% protection
- **Expected Survival**: 33.75M worth of modules survive on average
- **Net Insurance Value**: ₡33.75M return - ₡45M premium = -₡11.25M expected loss
- **Why Insure?**: Worst-case protection (prevent total loss), variance reduction (avoid losing all modules at once)

**Strategic Insurance Use:**
- **Selective Insurance**: Only insure irreplaceable items (exceptional quality, high-level crew)
- **Tier-Based Strategy**: No insurance T1-T4 (no permadeath), selective T5-T7, full coverage T8-T10
- **Premium Tier Selection**: Budget for common items, Premium Naval for exceptional/elite items
- **Player Insurance**: Use for regional operations (fast salvage), trusted community insurers

---

## Installation & Repair Time System

**Reduced Real-Time Timers for Fast-Paced Gameplay:**

WOS2.3 uses shortened installation and repair timers to keep players engaged and minimize downtime between combat sessions.

### Module Installation Times

**Installation at Port (Friendly Ports Only):**

| Module Size | Installation Time | Concurrent Installation | Examples |
|-------------|------------------|----------------------|----------|
| **Small** | 30 seconds - 2 minutes | Up to 5 simultaneous | AA mounts, sensors, 1x1 modules |
| **Medium** | 2-5 minutes | Up to 3 simultaneous | Secondary turrets, support modules, 1x2 to 2x2 |
| **Large** | 5-15 minutes | Up to 2 simultaneous | Main turrets, engines, 2x3 to 3x3 |
| **Massive** | 10-20 minutes | 1 at a time | Battleship triple 16" turrets, advanced systems |

**Concurrent Installation Mechanics:**
- **Base Capacity**: All ports support 1 concurrent installation by default
- **Hire Shipyard Workers**: Pay fee to unlock additional concurrent slots
  - **Small Port**: ₡5,000 → unlock 2 concurrent slots
  - **Medium Port**: ₡10,000 → unlock 3 concurrent slots
  - **Large Port**: ₡20,000 → unlock 5 concurrent slots
- **Strategic Use**: Install entire ship loadout in parallel (15 minutes vs. 2+ hours sequential)

**Installation Example (T7 Battleship Refit):**
```
Sequential Installation (1 worker):
- 3 main turrets: 15 min each = 45 minutes
- 4 engines: 10 min each = 40 minutes
- 8 support modules: 3 min each = 24 minutes
- 12 AA mounts: 1 min each = 12 minutes
Total: 121 minutes (2 hours 1 minute)

Parallel Installation (5 workers at Large Port):
- Wave 1: 3 main turrets + 2 engines = 15 minutes
- Wave 2: 2 engines + 3 support modules = 10 minutes
- Wave 3: 5 support modules + 5 AA mounts = 3 minutes
- Wave 4: 7 AA mounts = 1 minute
Total: 29 minutes (76% time saved)
```

### Port Repair Times

**At-Port Full Repair (Friendly/Neutral Ports):**

| Damage Severity | Repair Time | Acceleration (2x Cost) | Priority (5x Cost) | Example |
|----------------|-------------|---------------------|-------------------|---------|
| **Minor Module Damage** | 1-5 minutes | 30 sec - 2.5 min | 12 sec - 1 min | Radar 75% HP |
| **Major Module Damage** | 5-15 minutes | 2.5 - 7.5 min | 1 - 3 min | Engine 25% HP |
| **Hull Damage** | 10-30 minutes | 5 - 15 min | 2 - 6 min | 40% hull integrity |
| **Critical Rebuild** | 30 min - 2 hours | 15 min - 1 hour | 6 min - 24 min | Multiple destroyed modules |

**Repair Cost Scaling:**
- **Minor Damage**: ₡5,000 - ₡25,000
- **Major Damage**: ₡50,000 - ₡200,000
- **Hull Damage**: ₡100,000 - ₡500,000
- **Critical Rebuild**: ₡500,000 - ₡3,000,000

**Acceleration Options:**
- **Standard Repair**: Base time, base cost
- **Expedited Repair** (2x cost): 50% time reduction
- **Priority Repair** (5x cost): 75% time reduction

**Example Repair (T8 Battleship Post-Combat):**
```
Standard Repair:
- 2 main turrets (major damage): 10 min each = 20 minutes
- 1 engine (minor damage): 3 minutes
- Hull damage (45% → 100%): 25 minutes
- Total Time: 48 minutes
- Total Cost: ₡850,000

Expedited Repair (2x cost):
- Total Time: 24 minutes
- Total Cost: ₡1,700,000

Priority Repair (5x cost):
- Total Time: 12 minutes
- Total Cost: ₡4,250,000
```

### At-Sea Emergency Repairs

**Field Repair Limitations:**
- **Cannot Fully Repair**: Maximum 75% HP restoration per module
- **Requires Consumables**: Repair Kits consumed from inventory
- **Time-Consuming**: 30 seconds - 2 minutes per repair attempt
- **Cooldown**: 5-10 minutes between repair attempts on same module

**Repair Kit Types:**

**Small Repair Kit** (1x1 inventory):
- **Restores**: 10-25% module HP
- **Repair Time**: 30 seconds
- **Cooldown**: 5 minutes
- **Weight**: 0.5 tons per kit
- **Cost**: ₡5,000 per kit
- **Best For**: Minor damage, quick fixes, AA/secondary systems

**Large Repair Kit** (2x2 inventory):
- **Restores**: 25-50% module HP
- **Repair Time**: 90 seconds
- **Cooldown**: 10 minutes
- **Weight**: 2 tons per kit
- **Cost**: ₡20,000 per kit
- **Best For**: Major damage, main turrets, engines, critical systems

**Emergency Patch** (1x2 inventory):
- **Restores**: Temporary 50% function (degrades over time)
- **Repair Time**: 45 seconds
- **Cooldown**: None (can spam if have kits)
- **Duration**: Module breaks again after 30-60 minutes
- **Weight**: 1 ton per kit
- **Cost**: ₡8,000 per kit
- **Best For**: Emergency situations, temporary fixes to reach port

**Machine Shop Module Bonus:**
- If equipped, all repair kit effectiveness increased by 25-50%
- Small Kit: Restores 12.5-37.5% (instead of 10-25%)
- Large Kit: Restores 31.25-75% (instead of 25-50%)
- Reduces cooldown by 20%

**Example At-Sea Repair (T6 Cruiser Mid-Combat):**
```
Situation: Main turret #2 destroyed (0% HP), need firepower to continue combat

Option 1 (Large Repair Kit):
- Use Large Repair Kit: 90 seconds repair time
- Result: Turret restored to 45% HP (functional but reduced performance)
- Can fire but 55% longer reload, 45% accuracy
- Cooldown: 10 minutes before can repair again

Option 2 (Emergency Patch):
- Use Emergency Patch: 45 seconds repair time
- Result: Turret restored to 50% temporary function
- Can fire normally for 30-60 minutes
- Turret breaks again after timer expires (must use another kit or retreat to port)

Strategic Decision: Emergency patch if close to port (30 min away), Large Kit if extended combat expected
```

---

## Offline Crew Training System

**Passive Progression While Logged Out:**

WOS2.3 implements an offline training system allowing crew cards to gain experience while players are logged out, reducing grind and rewarding long-term progression.

### Core Training Mechanics

**Offline XP Accumulation:**
- **Accumulation Rate**: 1 XP per 10 minutes offline
- **Maximum Cap**: 300 XP per crew card
- **Cap Duration**: ~50 hours offline (3000 minutes ÷ 60 = 50 hours)
- **Claim Requirement**: Must manually claim XP on login (button/notification prompt)

**Training Assignment System:**

**Pre-Logout Assignment:**
1. **Access Training Interface** at any friendly port before logging out
2. **Select Crew Cards** to train (drag crew cards to training slots)
3. **Choose Skill Track** for each crew card:
   - Gunnery (Main Battery, Secondary, AA specializations)
   - Engineering (Propulsion, Damage Control, Repair)
   - Navigation (Piloting, Chart Reading, Weather Prediction)
   - Electronics (Radar, Sonar, Electronic Warfare)
   - Aviation (Aircraft Operations, Carrier Coordination)
   - Command (Leadership, Tactics, Crisis Management)
4. **Confirm Training** and log out

**Post-Login Claim:**
1. **Login Notification**: "5 crew cards have completed training! 300 XP available."
2. **Click Claim**: XP distributed to trained crew cards
3. **Result**: Each crew card gains 300 XP in assigned skill track

**Per-Crew Card XP:**
- Each crew card assigned to training gets **full 300 XP** (NOT shared)
- If 3 crew cards training → Each gets 300 XP = 900 XP total distributed
- If 10 crew cards training → Each gets 300 XP = 3,000 XP total distributed

**Strategic Implications:**
- Incentive to train multiple crew cards simultaneously
- No penalty for training full crew roster
- Encourages maintaining diverse crew library (gunners, engineers, specialists)

### Premium Account Enhancement

**Premium Account Benefits (Optional Monetization):**
- **Standard Account**: Train up to 5 crew cards simultaneously
- **Premium Account**: Train up to 15 crew cards simultaneously (3x capacity)
- **Elite Premium Account**: Train up to 30 crew cards simultaneously (6x capacity)
- **Premium Bonus**: +50% offline XP accumulation rate (1.5 XP per 10 minutes = 450 XP cap)

**Example Comparison (48 hours offline):**

| Account Type | Crew Trained | XP per Crew | Total XP Gained |
|-------------|--------------|-------------|-----------------|
| Standard | 5 crew | 300 XP | 1,500 XP |
| Premium | 15 crew | 450 XP | 6,750 XP |
| Elite Premium | 30 crew | 450 XP | 13,500 XP |

**Premium Value Proposition:**
- Faster crew development for players with limited playtime
- Supports maintaining multiple ship loadouts (multiple specialized crews)
- Optional (not pay-to-win, just time-saver)

### Training Enhancements

**Training Facility Quality (Port-Based):**

Ports have varying training facility quality, affecting XP gain rates:

| Port Tier | Facility Quality | Offline XP Rate | Cap Increase |
|-----------|-----------------|----------------|--------------|
| T0-T1 (Small Port) | Basic | 1 XP / 10 min | 300 XP (base) |
| T2-T3 (Medium Port) | Improved | 1.2 XP / 10 min | 360 XP |
| T4-T5 (Large Port) | Advanced | 1.5 XP / 10 min | 450 XP |
| T6+ (Capital Port) | Elite | 2 XP / 10 min | 600 XP |

**Strategic Port Selection:**
- Logout at capital ports for maximum training benefit
- Trade-off: Capital ports may be in contested zones (risk vs. reward)
- Long-term players establish "training bases" at high-tier friendly ports

**Naval Academy Module** (Ship Module Enhancement):

**Naval Academy Support Module:**
- **Size**: 2x3 (support slot)
- **Effects**:
  - +50% offline XP bonus (stacks with port bonuses)
  - Allows training 3 additional crew cards beyond account limit
  - Reduces XP cost for classification unlocks by 25%
- **Weight**: 45 tons
- **Crew**: 1 Support crew card required
- **Cost**: ₡850,000 (expensive, late-game module)
- **Strategic Use**: Dedicated training ships, crew development focus

**Combined Bonuses Example:**
```
Premium Account + Capital Port + Naval Academy Module:

Base XP Rate: 1 XP / 10 min
Premium Bonus: +50% (1.5 XP / 10 min)
Capital Port: +100% (3 XP / 10 min)
Naval Academy: +50% of base (3.5 XP / 10 min)

Result: 3.5 XP per 10 minutes = 1,050 XP cap (~50 hours offline)
Training Capacity: 15 (Premium) + 3 (Naval Academy) = 18 crew cards
Total XP Distributed: 18 × 1,050 = 18,900 XP per 50-hour offline period
```

**Skill Books** (Consumable XP Boosters):

**Gunnery Manual** (Consumable Item):
- **Effect**: +500 XP instant grant to Gunnery skill
- **Inventory**: 1x1 slot
- **Weight**: 0.1 tons
- **Cost**: ₡50,000 (NPC vendors), ₡35,000-45,000 (player market)
- **Acquisition**: Mission rewards, salvage drops, crafting
- **Strategic Use**: Fast-track specific skills, push crew over level thresholds

**Available Skill Books:**
- Gunnery Manual (Gunner skill)
- Engineering Handbook (Engineer skill)
- Navigation Charts (Navigator skill)
- Electronics Technical Manual (Electronics skill)
- Aviation Operations Guide (Aviation skill)
- Command Leadership Principles (Command skill)

**Veteran Mentorship System** (Crew-to-Crew Training):

**How It Works:**
1. **Assign Veteran Crew** (Level 50+) as mentor
2. **Assign Rookie Crew** (Level 1-49) as student
3. **Offline Training**: Both assigned to same skill track
4. **XP Distribution**:
   - Veteran: Receives 50% normal XP (150 XP at cap)
   - Rookie: Receives 150% normal XP (450 XP at cap)
5. **Mentorship Bonus**: Rookie gains +50% XP, Veteran sacrifices 50% for teaching

**Strategic Use:**
- Fast-track new crew cards to operational levels
- Utilize high-level crew cards that are near cap (Level 180+)
- Efficient use of offline XP pool

**Example Mentorship Setup:**
```
Mentor: Level 150 Master Gunnery Officer (near level cap, slow progression)
Student: Level 15 Rookie Gunner (needs fast development)

Offline Training (48 hours at T3 Port):
- Normal XP Cap: 360 XP
- Mentor Receives: 180 XP (50% normal) → Level 150 → 150.18 (+0.18 levels)
- Student Receives: 540 XP (150% normal) → Level 15 → 20.4 (+5.4 levels)

Result: Rookie crew rapidly promoted to operational levels, veteran crew still progresses (slower but acceptable)
```

### Future Training Enhancements (Potential Updates)

**Possible Future Features:**
- **Raise XP Cap**: Increase from 300 XP to 500 XP or 1,000 XP (balancing required)
- **Faster Accumulation**: Premium tiers with 2 XP / 10 min or 5 XP / 10 min
- **Specialized Training Programs**: Event-based training bonuses (double XP weekends)
- **Cross-Skill Training**: Train multiple skills simultaneously (reduced efficiency)
- **Crew Exchange Program**: Send crew to allied faction ports for unique specializations

---

## Ship Fitting Interface (UI/UX Design)

**Multi-Screen Interface for Complete Ship Configuration:**

### Screen 1: Weapon Hardpoint View

**Visual Layout:**
- **3D Ship Sprite**: Top-down view of ship with hardpoints highlighted
- **Non-Rotatable**: Fixed top-down perspective (2D game, no need for rotation)
- **Hardpoint Indicators**: Green circles showing available hardpoint positions
  - Main Battery: Large circles (bow, stern, centerline)
  - Secondary Battery: Medium circles (sides, superstructure)
  - Tertiary/AA: Small circles (deck positions)
  - Special Hardpoints: Unique icons (torpedo tubes, catapults)

**Interaction:**
- **Drag-Drop**: Drag turret/weapon from inventory panel → Drop onto hardpoint
- **Visual Feedback**:
  - Green = Valid placement (weight OK, crew assigned)
  - Yellow = Valid but over-weight warning (speed penalty preview)
  - Red = Invalid (exceeds weight capacity, no crew, incompatible)
- **Hover Tooltips**: Show hardpoint capacity, current turret stats, crew assignment

**Right Panel - Turret Inventory:**
- **Filter Options**: Main Battery, Secondary, AA, Special Weapons
- **Sort Options**: By weight, caliber, damage, fire rate
- **Turret Cards**: Visual cards showing:
  - Turret model/name
  - Weight (tons)
  - Damage/ROF stats
  - Crew requirement (icon)
  - Quality indicator (70-130% color-coded)

**Bottom Panel - Real-Time Stats:**
- **Firepower Rating**: Total damage output (aggregated)
- **AA Rating**: Anti-aircraft effectiveness
- **Total Weight**: Hardpoint weight sum
- **Ship Speed**: Max speed after weight penalties
- **Warnings**: Over-weight alerts, missing crew alerts

**Example Layout:**
```
┌─────────────────────────────────────────────────────────────┐
│ WEAPON HARDPOINT CONFIGURATION          [Save] [Reset] [?]  │
├────────────────────────────────┬────────────────────────────┤
│                                │ TURRET INVENTORY           │
│    [Ship Top-Down View]        │ ┌──────────────────────┐  │
│                                │ │ 16"/50 Mk.7 (128%)   │  │
│    ◉ Main #1 (assigned)        │ │ Weight: 285 tons     │  │
│    ◉ Main #2 (assigned)        │ │ Damage: 12,800       │  │
│    ◉ Main #3 (EMPTY)           │ │ ROF: 2.5/min         │  │
│    ⬤ Secondary #1-8            │ │ [Gunner Required]    │  │
│    • AA #1-40                  │ └──────────────────────┘  │
│                                │ [Filter: Main Battery ▼]  │
│                                │ [Sort: By Weight ▼]       │
├────────────────────────────────┴────────────────────────────┤
│ Firepower: 8,500 | AA: 450 | Weight: 2,850t | Speed: 28kn  │
│ ⚠️ Warning: Main #3 unassigned (-33% firepower potential)   │
└─────────────────────────────────────────────────────────────┘
```

---

### Screen 2: Ship Systems Fitting Screen

**Visual Layout:**
- **Grid-Based Slot Layout**: Ship cross-section showing internal systems
  - **Engine Section**: Left side, 2-6 engine bay slots (size-specific)
  - **Support Section**: Center, variable support module slots
  - **Misc Section**: Right side, universal misc slots
- **Color-Coded Slots**:
  - Orange = Engine slots (engines only)
  - Blue = Support slots (support modules only)
  - Purple = Misc slots (universal acceptance)

**Interaction:**
- **Drag-Drop**: Drag module from inventory → Drop into compatible slot
- **Size Validation**: Module size must match slot size
  - 1x1 module can fit 1x1, 2x2, or 3x3 slot (inefficient, wastes space)
  - 2x2 module CANNOT fit 1x1 slot (invalid)
- **Visual Feedback**: Same green/yellow/red system as hardpoint screen
- **Crew Assignment**: After module placement, assign crew card to module's crew slot

**Module Inventory Panel:**
- **Tabbed Categories**: [Engines] [Support] [Misc] [All]
- **Filter Options**: By size (1x1, 1x2, 2x2, etc.), by function, by weight
- **Module Cards**: Show name, size (grid visual), weight, effects, crew requirement

**Crew Assignment Sub-Panel:**
- **Crew Card List**: All owned crew cards
- **Filter by Class**: Show only Gunners, Engineers, etc.
- **Drag-Drop**: Drag crew card onto module's crew slot
- **Validation**: Check class compatibility, availability (not assigned elsewhere)

**Real-Time Stats Panel:**
- **Speed**: Max speed with current engines
- **Fuel Efficiency**: km per fuel unit
- **Crew Morale**: Aggregate morale bonus from support modules
- **Total Weight**: All modules + crew weight
- **Warnings**: Over-weight, missing crew, incompatible modules

**Example Layout:**
```
┌──────────────────────────────────────────────────────────────┐
│ SHIP SYSTEMS FITTING                     [Save] [Reset] [?]  │
├─────────────────────────────────┬────────────────────────────┤
│ ENGINE BAYS (Orange)            │ MODULE INVENTORY           │
│ ┌───┬───┐ ┌───┬───┐           │ [Engines] [Support] [Misc] │
│ │ E │ E │ │ E │ E │ (4 bays)  │ ┌────────────────────────┐ │
│ │ 1 │ 2 │ │ 3 │ 4 │           │ │ High-Eff Turbine       │ │
│ └───┴───┘ └───┴───┘           │ │ Size: 2x3   ⚡         │ │
│                                 │ │ +32kn, -20% fuel      │ │
│ SUPPORT SLOTS (Blue)            │ │ [Engineer Required]    │ │
│ ┌──┬──┐ ┌──┬──┬──┐            │ └────────────────────────┘ │
│ │S1│S2│ │S3│S4│S5│            │                            │
│ └──┴──┘ └──┴──┴──┘            │ CREW ASSIGNMENT            │
│ ┌───┬───┬───┐                 │ ┌────────────────────────┐ │
│ │ S │ S │ S │ (8 slots)      │ │ Module: Engine Bay #1  │ │
│ │ 6 │ 7 │ 8 │                 │ │ Requires: Engineer     │ │
│ └───┴───┴───┘                 │ │ [Drag Crew Card Here]  │ │
│                                 │ │                        │ │
│ MISC SLOTS (Purple)             │ │ Available Crew:        │ │
│ ┌──┐ ┌──┐ ┌──┬──┐            │ │ - Engr Lvl 85 (450t)  │ │
│ │M1│ │M2│ │M3│M4│            │ │ - Engr Lvl 120 (720t) │ │
│ └──┘ └──┘ └──┴──┘            │ └────────────────────────┘ │
├─────────────────────────────────┴────────────────────────────┤
│ Speed: 32kn | Fuel Eff: 1.2km/unit | Morale: +25 | Wt: 4,200t│
│ ✅ All systems operational | 18/20 crew assigned             │
└──────────────────────────────────────────────────────────────┘
```

---

### Screen 3: Armor Configuration

**Visual Layout:**
- **Schematic Views**: Side profile and top-down view showing armor zones
- **9 Armor Zone Controls**: One dropdown + input field per zone
- **Color-Coded Zones**: Visual heat map showing armor thickness
  - Blue = Thin armor (0-2")
  - Green = Moderate armor (2-6")
  - Yellow = Heavy armor (6-12")
  - Red = Maximum armor (12-18")

**Per-Zone Controls:**
```
Forward Deck Armor:
[Armor Type: RHA ▼] [Thickness: 4.5" (114.3mm) ] [+0.1] [-0.1]
```

**Interaction:**
- **Dropdown**: Select armor type (RHA, Face-Hardened, Krupp, Terni, Ducol, STS)
- **Manual Entry**: Type exact thickness (supports decimals, e.g., "10.5")
- **Increment Buttons**: +/- 0.1" for fine adjustment
- **Slider** (below each zone): Quick adjustment, 0.5" increments

**Real-Time Stats Panel:**
- **Total Armor Weight**: Sum of all zones
- **Speed Impact**: Max speed reduction from armor weight
- **Protection Rating**: Aggregate armor effectiveness
- **Total Cost**: Credit cost for current armor configuration
- **Weight Remaining**: Available weight budget after armor

**Armor Scheme Presets:**
- **Save Custom Scheme**: Save current configuration as preset
- **Load Scheme**: Historical refits, player-saved presets
  - "Iowa Standard Armor"
  - "Iowa Maximum Protection"
  - "Iowa Speed Build (Lightweight)"
- **Quick Apply**: One-click armor configuration

**Example Layout:**
```
┌──────────────────────────────────────────────────────────────┐
│ ARMOR CONFIGURATION                      [Save] [Reset] [?]  │
├─────────────────────────────────┬────────────────────────────┤
│ [Ship Side Profile View]        │ ARMOR ZONES                │
│  Showing armor thickness        │                            │
│  color-coded heat map           │ DECK ARMOR                 │
│                                  │ Forward Deck:              │
│                                  │ [RHA ▼] [4.0" (101.6mm)]  │
│ [Ship Top-Down View]            │ Center Deck:               │
│  Showing deck armor zones       │ [RHA ▼] [6.0" (152.4mm)]  │
│                                  │ Aft Deck:                  │
│                                  │ [RHA ▼] [4.0" (101.6mm)]  │
│                                  │                            │
│                                  │ SIDE BELT ARMOR            │
│                                  │ Forward Belt:              │
│                                  │ [STS ▼] [10.0" (254.0mm)] │
│                                  │ Center Belt:               │
│                                  │ [STS ▼] [12.1" (307.3mm)] │
│                                  │ Aft Belt:                  │
│                                  │ [STS ▼] [10.0" (254.0mm)] │
│                                  │                            │
│                                  │ STRUCTURAL ARMOR           │
│                                  │ Conning Tower:             │
│                                  │ [STS ▼] [7.25" (184.2mm)] │
│                                  │ Main Turrets:              │
│                                  │ [STS ▼] [17.0" (431.8mm)] │
│                                  │ Secondary Turrets:         │
│                                  │ [STS ▼] [2.5" (63.5mm)]   │
├─────────────────────────────────┴────────────────────────────┤
│ Armor Weight: 4,850t | Speed: 28kn (-5kn) | Protection: 9.2/10│
│ Total Cost: ₡8,500,000 | Weight Budget: 2,150t remaining    │
│ [Load Preset ▼] [Save As New Preset]                        │
└──────────────────────────────────────────────────────────────┘
```

---

### Screen 4: Cargo Grid Inventory

**Visual Layout:**
- **Tetris-Style Grid**: Ship's cargo hold represented as grid cells
- **Grid Size**: Ship-class specific (DD: 10x12, CL: 12x14, BB: 16x20, CV: 18x22)
- **Item Representation**: Visual blocks showing item shapes and sizes
- **Color-Coding**:
  - Grey = Empty cells
  - Yellow = Ammunition
  - Green = Resources
  - Blue = Modules/equipment
  - Purple = Special items

**Interaction:**
- **Drag-Drop**: Drag items from inventory list → Place in grid
- **Rotation**: Press R key to rotate item 90° before placement
- **Auto-Sort**: Button to automatically organize items efficiently (Tetris solver algorithm)
- **Collision Detection**: Can't place items overlapping or outside grid

**Weight Display:**
- **Current Weight**: Real-time total cargo weight
- **Max Weight**: Ship's cargo capacity limit
- **Visual Meter**: Progress bar showing weight percentage
- **Color-Coded**:
  - Green (0-70%): Safe load
  - Yellow (70-90%): Moderate load
  - Red (90-100%): Maximum load
  - Flashing Red (100%+): Over-weight (can't undock)

**Item List Panel:**
- **Owned Items**: All items in port storage not on ship
- **Filter**: By type (ammo, resources, modules, etc.)
- **Sort**: By weight, size, value, name
- **Search**: Text search for specific items

**Example Layout:**
```
┌──────────────────────────────────────────────────────────────┐
│ CARGO GRID INVENTORY                     [Save] [Reset] [?]  │
├─────────────────────────────────┬────────────────────────────┤
│ CARGO GRID (16x20 cells)       │ ITEM LIST                  │
│ ┌─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┐ │ [Filter: All ▼]           │
│ │█│█│ │ │ │ │ │ │ │ │ │ │ │ │ │ ┌────────────────────────┐ │
│ │█│█│ │▓│▓│▓│ │ │ │ │ │ │ │ │ │ │ 16" AP Shells (100)    │ │
│ │ │ │ │▓│▓│▓│ │ │ │ │ │ │ │ │ │ │ Size: 2x3  Weight: 8t  │ │
│ │ │ │ │▓│▓│▓│ │ │ │ │ │ │ │ │ │ │ Value: ₡50,000         │ │
│ │ │ │ │ │ │ │ │░│░│ │ │ │ │ │ │ └────────────────────────┘ │
│ │ │ │ │ │ │ │ │░│░│ │ │ │ │ │ │ ┌────────────────────────┐ │
│ │ │ │ │ │ │ │ │ │ │ │ │ │ │ │ │ │ Steel (500 tons)       │ │
│ │ │ │ │ │ │ │ │ │ │ │ │ │ │ │ │ │ Size: 5x4  Weight: 500t│ │
│ │ │ │ │ │ │ │ │ │ │ │ │ │ │ │ │ │ Value: ₡50,000         │ │
│ └─┴─┴─┴─┴─┴─┴─┴─┴─┴─┴─┴─┴─┴─┘ │ └────────────────────────┘ │
│                                  │                            │
│ █ = 16" AP Shells (2x2)         │ [Auto-Sort Inventory]      │
│ ▓ = Steel Resources (5x4)       │ [Rotate Item: R key]       │
│ ░ = Radar Module (2x2)          │                            │
├─────────────────────────────────┴────────────────────────────┤
│ Weight: 2,450t / 5,000t (49%) ░░░░░░░░░░░░░░░░░░░ [SAFE]   │
│ Grid Usage: 128 / 320 cells (40%)                            │
└──────────────────────────────────────────────────────────────┘
```

---

### Screen 5: Loadout Presets (Future Feature)

**Purpose**: Save and load complete ship configurations for quick refitting.

**Features:**
- **Save Current Loadout**: Saves turrets, modules, armor, cargo as preset
- **Load Preset**: One-click to apply entire configuration
- **Name & Description**: Custom names for loadouts (e.g., "Anti-Air Build", "Maximum Firepower")
- **Share with Players**: Export loadout code for sharing with community
- **Community Loadout Library**: Browse top-rated player loadouts

**UI Elements:**
- **Preset List**: Saved loadouts displayed as cards
- **Preview**: Shows turret configuration, modules, stats
- **Quick Comparison**: Compare stats between current and preset
- **One-Click Apply**: Apply preset, purchase missing modules automatically

**Example Loadout Card:**
```
┌────────────────────────────────────────┐
│ LOADOUT: "Iowa Maximum Firepower"     │
│ By: PlayerName | Rating: 4.8/5 ⭐     │
├────────────────────────────────────────┤
│ Turrets: 3x Triple 16"/50 Mk.7 (130%) │
│ Engines: 4x High-Performance Turbines  │
│ Armor: Maximum Protection (STS)        │
│ Support: Damage Control, Medical Bay   │
│ Misc: Advanced Radar, Fire Control     │
├────────────────────────────────────────┤
│ Stats: Firepower 10/10 | Speed 6/10   │
│       Armor 9/10 | AA 7/10             │
├────────────────────────────────────────┤
│ Cost to Apply: ₡12,500,000             │
│ [Preview] [Apply] [Share] [Delete]     │
└────────────────────────────────────────┘
```

---

## Accessibility & User Experience Features

**Interface Accessibility:**
- **Colorblind Modes**: Alternative color schemes for valid/invalid/warning states
- **Keyboard Shortcuts**:
  - Q/E: Cycle through slots
  - R: Rotate module/item
  - Tab: Switch between screens
  - Ctrl+S: Save configuration
  - Ctrl+Z: Undo last change
- **Tooltips**: Hover over any element for detailed information
- **Comparison Tooltips**: Hover module shows stat changes if equipped
- **Undo/Redo**: Revert configuration changes without restarting

**Performance Optimization:**
- **Lazy Loading**: Load module lists progressively (display 20, load more on scroll)
- **Caching**: Cache ship configurations locally for instant loading
- **Batch Operations**: Apply multiple changes simultaneously (reduce server calls)

---

## Summary & Integration

**Complete Ship Customization System Includes:**

✅ **Dual Module System**: Weapon hardpoints (visual 3D drag-drop) + Internal systems (grid-based slots)
✅ **9-Zone Armor Configuration**: Inches with mm equivalents, dropdown armor types, real-time weight/cost/speed feedback
✅ **Comprehensive Module Categories**: 40+ module types across turrets, engines, support, and misc systems
✅ **Crew-Module Integration**: Every module requires appropriately classed crew card for optimal performance
✅ **Module Progression Trees**: Detailed unlock paths already documented in GDD (referenced)
✅ **Quality Variance System**: 70-130% RNG for crafted modules with crafting skill progression
✅ **Enhanced Insurance System**: NPC companies + player-run insurance with distance-based return times
✅ **Reduced Real-Time Timers**: 30 seconds - 20 minutes for installation, 1 minute - 2 hours for repairs
✅ **Offline Training System**: 300 XP per crew card, premium account options, training facility bonuses
✅ **Complete UI Design**: 5 fitting screens with detailed interaction flows and accessibility features

**Integration with Existing GDD Systems:**
- **Crew System** (lines 163-495): Module efficiency tied to crew level and class
- **Combat System** (lines 498-891): Modules affect combat stats, damage compartmentalization
- **Economy System** (lines 893-1146): Module acquisition, crafting, insurance economics
- **Module Progression Trees** (lines 3359-4182): Unlock paths, resource requirements, quality variance

---

**End of Ship Customization & Module System Specification**
