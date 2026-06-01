using ii.Nairyt.Model;

namespace ii.Nairyt.Decoding;

internal class TyrianHdtDecoder
{
    private readonly BinaryDataReader _reader;
    private readonly TyrianHdtHeader _header;

    public TyrianHdtDecoder(byte[] data)
    {
        _reader = new BinaryDataReader(data);
        _header = TyrianHdtHeader.Read(_reader);
        _reader.Seek(_header.DataOffset);
    }

    public TyrianHdtData Decode()
    {
        var weapons = ReadWeapons(_header.WeaponPatternEntryCount);
        var weaponPorts = ReadWeaponPorts(_header.WeaponPortEntryCount);
        SkipBytes(_header.WeaponPortSectionPadding);
        var specialMoves = ReadSpecialMoves(_header.SpecialMoveEntryCount);
        var powerGenerators = ReadPowerGenerators(_header.PowerGeneratorEntryCount);
        var ships = ReadShips();
        SkipBytes(_header.ShipToOptionSectionPadding);
        var sidekickOptions = ReadSidekickOptions(_header.SidekickOptionEntryCount);
        var shields = ReadShields(_header.ShieldEntryCount);
        var enemies = ReadEnemies(_header.EnemyEntryCount);

        return new TyrianHdtData(
            _header.Episode1DataOffset,
            _header.ItemCounts,
            weapons,
            weaponPorts,
            specialMoves,
            powerGenerators,
            ships,
            sidekickOptions,
            shields,
            enemies);
    }

    private void SkipBytes(int count)
    {
        if (count > 0)
        {
            _reader.ReadBytes(count);
        }
    }

    private IReadOnlyList<WeaponPattern> ReadWeapons(int count)
    {
        var weapons = new WeaponPattern[count];

        for (var i = 0; i < weapons.Length; i++)
        {
            weapons[i] = ReadWeaponPattern();
        }

        return weapons;
    }

    private WeaponPattern ReadWeaponPattern()
    {
        var drain = _reader.ReadUInt16();
        var shotRepeat = _reader.ReadByte();
        var multi = _reader.ReadByte();
        var weaponAnimationFrames = _reader.ReadUInt16();
        var maxPatternFrames = _reader.ReadByte();
        var targetX = _reader.ReadByte();
        var targetY = _reader.ReadByte();
        var aim = _reader.ReadByte();

        var frames = new WeaponPatternFrame[TyrianConstants.HdtWeaponPatternFrames];
        var attacks = new byte[TyrianConstants.HdtWeaponPatternFrames];
        var durations = new byte[TyrianConstants.HdtWeaponPatternFrames];

        for (var i = 0; i < attacks.Length; i++)
        {
            attacks[i] = _reader.ReadByte();
        }

        for (var i = 0; i < durations.Length; i++)
        {
            durations[i] = _reader.ReadByte();
        }

        var speedX = new sbyte[TyrianConstants.HdtWeaponPatternFrames];
        var speedY = new sbyte[TyrianConstants.HdtWeaponPatternFrames];
        var offsetX = new sbyte[TyrianConstants.HdtWeaponPatternFrames];
        var offsetY = new sbyte[TyrianConstants.HdtWeaponPatternFrames];
        var shapeGraphics = new ushort[TyrianConstants.HdtWeaponPatternFrames];

        for (var i = 0; i < speedX.Length; i++)
        {
            speedX[i] = _reader.ReadSByte();
        }

        for (var i = 0; i < speedY.Length; i++)
        {
            speedY[i] = _reader.ReadSByte();
        }

        for (var i = 0; i < offsetX.Length; i++)
        {
            offsetX[i] = _reader.ReadSByte();
        }

        for (var i = 0; i < offsetY.Length; i++)
        {
            offsetY[i] = _reader.ReadSByte();
        }

        for (var i = 0; i < shapeGraphics.Length; i++)
        {
            shapeGraphics[i] = _reader.ReadUInt16();
        }

        for (var i = 0; i < frames.Length; i++)
        {
            frames[i] = new WeaponPatternFrame(
                attacks[i],
                durations[i],
                speedX[i],
                speedY[i],
                offsetX[i],
                offsetY[i],
                shapeGraphics[i]);
        }

        return new WeaponPattern(
            drain,
            shotRepeat,
            multi,
            weaponAnimationFrames,
            maxPatternFrames,
            targetX,
            targetY,
            aim,
            frames,
            _reader.ReadSByte(),
            _reader.ReadSByte(),
            _reader.ReadByte(),
            _reader.ReadByte(),
            _reader.ReadByte(),
            _reader.ReadByte());
    }

    private IReadOnlyList<WeaponPort> ReadWeaponPorts(int count)
    {
        var weaponPorts = new WeaponPort[count];

        for (var i = 0; i < weaponPorts.Length; i++)
        {
            weaponPorts[i] = ReadWeaponPort();
        }

        return weaponPorts;
    }

    private WeaponPort ReadWeaponPort()
    {
        var name = _reader.ReadPascalString(TyrianConstants.HdtNameFieldSize);
        var firingModeCount = _reader.ReadByte();
        var patternsByPowerLevel = new List<IReadOnlyList<ushort>>(TyrianConstants.HdtWeaponFiringModes);

        for (var modeIndex = 0; modeIndex < TyrianConstants.HdtWeaponFiringModes; modeIndex++)
        {
            var patterns = new ushort[TyrianConstants.HdtWeaponPowerLevels];

            for (var powerLevel = 0; powerLevel < patterns.Length; powerLevel++)
            {
                patterns[powerLevel] = _reader.ReadUInt16();
            }

            patternsByPowerLevel.Add(patterns);
        }

        return new WeaponPort(
            name,
            firingModeCount,
            patternsByPowerLevel,
            _reader.ReadUInt16(),
            _reader.ReadUInt16(),
            _reader.ReadUInt16());
    }

    private IReadOnlyList<SpecialMove> ReadSpecialMoves(int count)
    {
        var specialMoves = new SpecialMove[count];

        for (var i = 0; i < specialMoves.Length; i++)
        {
            specialMoves[i] = ReadSpecialMove();
        }

        return specialMoves;
    }

    private SpecialMove ReadSpecialMove()
    {
        return new SpecialMove(
            _reader.ReadPascalString(TyrianConstants.HdtNameFieldSize),
            _reader.ReadUInt16(),
            _reader.ReadByte(),
            _reader.ReadByte(),
            _reader.ReadUInt16());
    }

    private IReadOnlyList<PowerGenerator> ReadPowerGenerators(int count)
    {
        var powerGenerators = new PowerGenerator[count];

        for (var i = 0; i < powerGenerators.Length; i++)
        {
            powerGenerators[i] = ReadPowerGenerator();
        }

        return powerGenerators;
    }

    private PowerGenerator ReadPowerGenerator()
    {
        return new PowerGenerator(
            _reader.ReadPascalString(TyrianConstants.HdtNameFieldSize),
            _reader.ReadUInt16(),
            _reader.ReadByte(),
            _reader.ReadSByte(),
            _reader.ReadUInt16());
    }

    private IReadOnlyList<ShipDefinition> ReadShips()
    {
        if (!_header.IsExpandedFormat)
        {
            return ReadFullShips(_header.FullShipEntryCount);
        }

        var ships = new List<ShipDefinition>(_header.CompactShipEntryCount + _header.FullShipEntryCount);
        ships.AddRange(ReadCompactShips(_header.CompactShipEntryCount));
        SkipBytes(_header.ShipSectionPadding);
        ships.AddRange(ReadFullShips(_header.FullShipEntryCount));
        return ships;
    }

    private IReadOnlyList<ShipDefinition> ReadCompactShips(int count)
    {
        var ships = new ShipDefinition[count];

        for (var i = 0; i < ships.Length; i++)
        {
            ships[i] = ReadCompactShip();
        }

        return ships;
    }

    private IReadOnlyList<ShipDefinition> ReadFullShips(int count)
    {
        var ships = new ShipDefinition[count];

        for (var i = 0; i < ships.Length; i++)
        {
            ships[i] = ReadFullShip();
        }

        return ships;
    }

    private ShipDefinition ReadCompactShip()
    {
        return new ShipDefinition(
            _reader.ReadPascalString(TyrianConstants.HdtNameFieldSize),
            _reader.ReadUInt16(),
            _reader.ReadUInt16(),
            0,
            0,
            0,
            _reader.ReadUInt16(),
            0);
    }

    private ShipDefinition ReadFullShip()
    {
        return new ShipDefinition(
            _reader.ReadPascalString(TyrianConstants.HdtNameFieldSize),
            _reader.ReadUInt16(),
            _reader.ReadUInt16(),
            _reader.ReadByte(),
            _reader.ReadSByte(),
            _reader.ReadByte(),
            _reader.ReadUInt16(),
            _reader.ReadByte());
    }

    private IReadOnlyList<SidekickOption> ReadSidekickOptions(int count)
    {
        var sidekickOptions = new SidekickOption[count];

        for (var i = 0; i < sidekickOptions.Length; i++)
        {
            sidekickOptions[i] = ReadSidekickOption();
        }

        return sidekickOptions;
    }

    private SidekickOption ReadSidekickOption()
    {
        var name = _reader.ReadPascalString(TyrianConstants.HdtNameFieldSize);
        var powerStages = _reader.ReadByte();
        var itemGraphic = _reader.ReadUInt16();
        var cost = _reader.ReadUInt16();
        var trackingMode = _reader.ReadByte();
        var animationMode = _reader.ReadByte();
        var optionSpeed = _reader.ReadSByte();
        var animationFrameCount = _reader.ReadByte();
        var graphics = new ushort[TyrianConstants.HdtOptionAnimationFrames];

        for (var i = 0; i < graphics.Length; i++)
        {
            graphics[i] = _reader.ReadUInt16();
        }

        return new SidekickOption(
            name,
            powerStages,
            itemGraphic,
            cost,
            trackingMode,
            animationMode,
            optionSpeed,
            animationFrameCount,
            graphics,
            _reader.ReadByte(),
            _reader.ReadUInt16(),
            _reader.ReadByte(),
            _reader.ReadBoolean(),
            _reader.ReadByte());
    }

    private IReadOnlyList<ShieldDefinition> ReadShields(int count)
    {
        var shields = new ShieldDefinition[count];

        for (var i = 0; i < shields.Length; i++)
        {
            shields[i] = ReadShield();
        }

        return shields;
    }

    private ShieldDefinition ReadShield()
    {
        return new ShieldDefinition(
            _reader.ReadPascalString(TyrianConstants.HdtNameFieldSize),
            _reader.ReadByte(),
            _reader.ReadByte(),
            _reader.ReadUInt16(),
            _reader.ReadUInt16());
    }

    private IReadOnlyList<EnemyDefinition> ReadEnemies(int count)
    {
        var enemies = new EnemyDefinition[count];

        for (var i = 0; i < enemies.Length; i++)
        {
            enemies[i] = ReadEnemy();
        }

        return enemies;
    }

    private EnemyDefinition ReadEnemy()
    {
        var animationFrameCount = _reader.ReadByte();
        var turretWeaponIndices = new byte[TyrianConstants.HdtEnemyTurretDirections];
        var turretFireRates = new byte[TyrianConstants.HdtEnemyTurretDirections];

        for (var i = 0; i < turretWeaponIndices.Length; i++)
        {
            turretWeaponIndices[i] = _reader.ReadByte();
        }

        for (var i = 0; i < turretFireRates.Length; i++)
        {
            turretFireRates[i] = _reader.ReadByte();
        }

        var graphics = new ushort[TyrianConstants.HdtEnemyAnimationFrames];

        return new EnemyDefinition(
            animationFrameCount,
            turretWeaponIndices,
            turretFireRates,
            _reader.ReadSByte(),
            _reader.ReadSByte(),
            _reader.ReadSByte(),
            _reader.ReadSByte(),
            _reader.ReadSByte(),
            _reader.ReadSByte(),
            _reader.ReadInt16(),
            _reader.ReadInt16(),
            _reader.ReadSByte(),
            _reader.ReadSByte(),
            _reader.ReadByte(),
            _reader.ReadByte(),
            ReadUInt16Array(graphics),
            _reader.ReadByte(),
            _reader.ReadByte(),
            _reader.ReadByte(),
            _reader.ReadSByte(),
            _reader.ReadSByte(),
            _reader.ReadUInt16(),
            _reader.ReadSByte(),
            _reader.ReadSByte(),
            _reader.ReadByte(),
            _reader.ReadUInt16(),
            _reader.ReadInt16(),
            _reader.ReadUInt16());
    }

    private ushort[] ReadUInt16Array(ushort[] destination)
    {
        for (var i = 0; i < destination.Length; i++)
        {
            destination[i] = _reader.ReadUInt16();
        }

        return destination;
    }
}