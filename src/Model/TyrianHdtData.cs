namespace ii.Nairyt.Model;

public record TyrianHdtData(
    int Episode1DataOffset,
    IReadOnlyList<ushort> ItemCounts,
    IReadOnlyList<WeaponPattern> Weapons,
    IReadOnlyList<WeaponPort> WeaponPorts,
    IReadOnlyList<SpecialMove> SpecialMoves,
    IReadOnlyList<PowerGenerator> PowerGenerators,
    IReadOnlyList<ShipDefinition> Ships,
    IReadOnlyList<SidekickOption> SidekickOptions,
    IReadOnlyList<ShieldDefinition> Shields,
    IReadOnlyList<EnemyDefinition> Enemies);

public record WeaponPatternFrame(
    byte Attack,
    byte Duration,
    sbyte SpeedX,
    sbyte SpeedY,
    sbyte OffsetX,
    sbyte OffsetY,
    ushort ShapeGraphic);

public record WeaponPattern(
    ushort Drain,
    byte ShotRepeat,
    byte Multi,
    ushort WeaponAnimationFrames,
    byte MaxPatternFrames,
    byte TargetX,
    byte TargetY,
    byte Aim,
    IReadOnlyList<WeaponPatternFrame> Frames,
    sbyte Acceleration,
    sbyte AccelerationX,
    byte CircleSize,
    byte Sound,
    byte Trail,
    byte ShipBlastFilter);

public record WeaponPort(
    string Name,
    byte FiringModeCount,
    IReadOnlyList<IReadOnlyList<ushort>> PatternsByPowerLevel,
    ushort Cost,
    ushort ItemGraphic,
    ushort PowerUse);

public record SpecialMove(
    string Name,
    ushort ItemGraphic,
    byte Power,
    byte SpecialType,
    ushort WeaponIndex);

public record PowerGenerator(
    string Name,
    ushort ItemGraphic,
    byte Power,
    sbyte Speed,
    ushort Cost);

public record ShipDefinition(
    string Name,
    ushort ShipGraphic,
    ushort ItemGraphic,
    byte AnimationThreshold,
    sbyte Speed,
    byte Damage,
    ushort Cost,
    byte BigShipGraphic);

public record SidekickOption(
    string Name,
    byte PowerStages,
    ushort ItemGraphic,
    ushort Cost,
    byte TrackingMode,
    byte AnimationMode,
    sbyte OptionSpeed,
    byte AnimationFrameCount,
    IReadOnlyList<ushort> Graphics,
    byte WeaponPortIndex,
    ushort WeaponPatternIndex,
    byte Ammo,
    bool Stop,
    byte IconGraphic);

public record ShieldDefinition(
    string Name,
    byte GeneratorPowerRequired,
    byte Protection,
    ushort ItemGraphic,
    ushort Cost);

public record EnemyDefinition(
    byte AnimationFrameCount,
    IReadOnlyList<byte> TurretWeaponIndices,
    IReadOnlyList<byte> TurretFireRates,
    sbyte MoveX,
    sbyte MoveY,
    sbyte AccelerationX,
    sbyte AccelerationY,
    sbyte CircularAccelerationX,
    sbyte CircularAccelerationY,
    short StartX,
    short StartY,
    sbyte StartCircularX,
    sbyte StartCircularY,
    byte Armor,
    byte EnemySize,
    IReadOnlyList<ushort> Graphics,
    byte ExplosionType,
    byte Animate,
    byte ShapeBank,
    sbyte ReverseX,
    sbyte ReverseY,
    ushort DropGraphic,
    sbyte DropLevel,
    sbyte DropAnimation,
    byte LaunchFrequency,
    ushort LaunchType,
    short Value,
    ushort EnemyDeathEffect);