# Combat Entity System
Base layer for combat entity lifecycles, stats, and resources. Implements a data pipeline via request structs and iterating systems, optimized for RPG request volumes (up to 400/frame), with linear scaling into the thousands before full ECS or parallel job-based implementations become the better tradeoff.

## Scope
This package defines the standard RPG combat pipeline: entity lifecycle management, stat resolution, resource mutation, and request processing. It is intended as a foundation - domain-specific mechanics and ui implimentation are built on top rather than embedded here.

## Architecture
The pipeline processes combat requests linearly through Burst-compiled static classes that operate directly on arrays of entity data. This approach was chosen over job-based parallelism because at the ~400 req/frame target typical of RPG combat, Unity Job System overhead is measurable and consistent enough to negate throughput gains. Jobs and parallelism become the better trade-off at approximately 1,000+ requests per frame, where the scheduling cost amortizes. For the request volumes common to RPG combat, static Burst-compiled iteration is faster in practice.

## Expandable Definitions
Core types are defined through enums to keep the system extensible without structural changes.

**Stats** - entity statistics (MaxHealth, Attack Power, Haste, etc.). Actual values are stored split across arrays by access pattern rather than per-entity structs, keeping frequently co-accessed stats contiguous in memory. Base values with applied modifiers are stored in per-stat arrays separately and recalculated on change.

**Resources** - pooled values tracked per entity (Health, Armor, Mana, etc.). Separated from stats due to a difference in access patterns. Stored together with their max values.

## Interceptors
Support for passive effects (specialized shields, damage reduction, triggers) and modifiable extensions is implemented through Interceptors. There are defined spots in the pipeline, exposed through interfaces, where interceptor classes can be inserted. All instances are created at startup through reflection.

Interceptors work the same way as pipeline systems - they iterate over requests and modify values or cause needed side effects. This approach maintains Burst compatibility and allows straightforward implementation of combat interception effects. Addition/Removal of entities to/from the Interceptor is done via addition to per Interceptor List.

## Usage
Most interaction is done through CombatCommands and direct array access. A basic example is provided in TestDamageScript.
GameObjects must be authored with CombatEntityAuthoring or created manually through CombatCommands to be registered in the system. All communication between systems is done through EntityID, which is later mapped to an array index via IndexerSystem or directly through the GetIndex method.
Interceptor definition can be referenced in ShieldAbsorb, which demonstrates the three-part structure: a ScriptableObject for editor authoring, a static system class containing the processor method, and an instantiated coordinator class.
