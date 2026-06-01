using ii.Nairyt.Model;

namespace ii.Nairyt.Decoding;

internal class TyrianHdtEncoder
{
    private readonly BinaryDataWriter _writer;
    private readonly TyrianHdtHeader _header;

    public TyrianHdtEncoder(TyrianHdtData data)
    {
        _header = TyrianHdtHeader.FromItemCounts(data.Episode1DataOffset, data.ItemCounts);
        _writer = new BinaryDataWriter(_header.FileSize);
        _header.Write(_writer);
        _writer.Seek(_header.DataOffset);

        WriteWeapons(data.Weapons, _header.WeaponPatternEntryCount);
        WriteWeaponPorts(data.WeaponPorts, _header.WeaponPortEntryCount);
        _writer.WriteZeroBytes(_header.WeaponPortSectionPadding);
        WriteSpecialMoves(data.SpecialMoves, _header.SpecialMoveEntryCount);
        WritePowerGenerators(data.PowerGenerators, _header.PowerGeneratorEntryCount);
        WriteShips(data.Ships);
        _writer.WriteZeroBytes(_header.ShipToOptionSectionPadding);
        WriteSidekickOptions(data.SidekickOptions, _header.SidekickOptionEntryCount);
        WriteShields(data.Shields, _header.ShieldEntryCount);
        WriteEnemies(data.Enemies, _header.EnemyEntryCount);
    }

    public byte[] Encode() => _writer.ToArray();

    private static void ValidateCount<T>(IReadOnlyList<T> items, int expectedCount, string sectionName)
    {
        if (items.Count != expectedCount)
        {
            throw new InvalidDataException(
                $"Expected {expectedCount} {sectionName} entries, but received {items.Count}.");
        }
    }

    private void WriteWeapons(IReadOnlyList<WeaponPattern> weapons, int count)
    {
        ValidateCount(weapons, count, "weapon pattern");

        foreach (var weapon in weapons)
        {
            WriteWeaponPattern(weapon);
        }
    }

    private void WriteWeaponPattern(WeaponPattern weapon)
    {
        _writer.WriteUInt16(weapon.Drain);
        _writer.WriteByte(weapon.ShotRepeat);
        _writer.WriteByte(weapon.Multi);
        _writer.WriteUInt16(weapon.WeaponAnimationFrames);
        _writer.WriteByte(weapon.MaxPatternFrames);
        _writer.WriteByte(weapon.TargetX);
        _writer.WriteByte(weapon.TargetY);
        _writer.WriteByte(weapon.Aim);

        foreach (var frame in weapon.Frames)
        {
            _writer.WriteByte(frame.Attack);
        }

        foreach (var frame in weapon.Frames)
        {
            _writer.WriteByte(frame.Duration);
        }

        foreach (var frame in weapon.Frames)
        {
            _writer.WriteSByte(frame.SpeedX);
        }

        foreach (var frame in weapon.Frames)
        {
            _writer.WriteSByte(frame.SpeedY);
        }

        foreach (var frame in weapon.Frames)
        {
            _writer.WriteSByte(frame.OffsetX);
        }

        foreach (var frame in weapon.Frames)
        {
            _writer.WriteSByte(frame.OffsetY);
        }

        foreach (var frame in weapon.Frames)
        {
            _writer.WriteUInt16(frame.ShapeGraphic);
        }

        _writer.WriteSByte(weapon.Acceleration);
        _writer.WriteSByte(weapon.AccelerationX);
        _writer.WriteByte(weapon.CircleSize);
        _writer.WriteByte(weapon.Sound);
        _writer.WriteByte(weapon.Trail);
        _writer.WriteByte(weapon.ShipBlastFilter);
    }

    private void WriteWeaponPorts(IReadOnlyList<WeaponPort> weaponPorts, int count)
    {
        ValidateCount(weaponPorts, count, "weapon port");

        foreach (var weaponPort in weaponPorts)
        {
            WriteWeaponPort(weaponPort);
        }
    }

    private void WriteWeaponPort(WeaponPort weaponPort)
    {
        _writer.WritePascalString(weaponPort.Name, TyrianConstants.HdtNameFieldSize);
        _writer.WriteByte(weaponPort.FiringModeCount);

        foreach (var patterns in weaponPort.PatternsByPowerLevel)
        {
            foreach (var pattern in patterns)
            {
                _writer.WriteUInt16(pattern);
            }
        }

        _writer.WriteUInt16(weaponPort.Cost);
        _writer.WriteUInt16(weaponPort.ItemGraphic);
        _writer.WriteUInt16(weaponPort.PowerUse);
    }

    private void WriteSpecialMoves(IReadOnlyList<SpecialMove> specialMoves, int count)
    {
        ValidateCount(specialMoves, count, "special move");

        foreach (var specialMove in specialMoves)
        {
            WriteSpecialMove(specialMove);
        }
    }

    private void WriteSpecialMove(SpecialMove specialMove)
    {
        _writer.WritePascalString(specialMove.Name, TyrianConstants.HdtNameFieldSize);
        _writer.WriteUInt16(specialMove.ItemGraphic);
        _writer.WriteByte(specialMove.Power);
        _writer.WriteByte(specialMove.SpecialType);
        _writer.WriteUInt16(specialMove.WeaponIndex);
    }

    private void WritePowerGenerators(IReadOnlyList<PowerGenerator> powerGenerators, int count)
    {
        ValidateCount(powerGenerators, count, "power generator");

        foreach (var powerGenerator in powerGenerators)
        {
            WritePowerGenerator(powerGenerator);
        }
    }

    private void WritePowerGenerator(PowerGenerator powerGenerator)
    {
        _writer.WritePascalString(powerGenerator.Name, TyrianConstants.HdtNameFieldSize);
        _writer.WriteUInt16(powerGenerator.ItemGraphic);
        _writer.WriteByte(powerGenerator.Power);
        _writer.WriteSByte(powerGenerator.Speed);
        _writer.WriteUInt16(powerGenerator.Cost);
    }

    private void WriteShips(IReadOnlyList<ShipDefinition> ships)
    {
        if (!_header.IsExpandedFormat)
        {
            ValidateCount(ships, _header.FullShipEntryCount, "ship");

            foreach (var ship in ships)
            {
                WriteFullShip(ship);
            }

            return;
        }

        var expectedCount = _header.CompactShipEntryCount + _header.FullShipEntryCount;
        ValidateCount(ships, expectedCount, "ship");

        for (var i = 0; i < _header.CompactShipEntryCount; i++)
        {
            WriteCompactShip(ships[i]);
        }

        _writer.WriteZeroBytes(_header.ShipSectionPadding);

        for (var i = _header.CompactShipEntryCount; i < ships.Count; i++)
        {
            WriteFullShip(ships[i]);
        }
    }

    private void WriteCompactShip(ShipDefinition ship)
    {
        _writer.WritePascalString(ship.Name, TyrianConstants.HdtNameFieldSize);
        _writer.WriteUInt16(ship.ShipGraphic);
        _writer.WriteUInt16(ship.ItemGraphic);
        _writer.WriteUInt16(ship.Cost);
    }

    private void WriteFullShip(ShipDefinition ship)
    {
        _writer.WritePascalString(ship.Name, TyrianConstants.HdtNameFieldSize);
        _writer.WriteUInt16(ship.ShipGraphic);
        _writer.WriteUInt16(ship.ItemGraphic);
        _writer.WriteByte(ship.AnimationThreshold);
        _writer.WriteSByte(ship.Speed);
        _writer.WriteByte(ship.Damage);
        _writer.WriteUInt16(ship.Cost);
        _writer.WriteByte(ship.BigShipGraphic);
    }

    private void WriteSidekickOptions(IReadOnlyList<SidekickOption> sidekickOptions, int count)
    {
        ValidateCount(sidekickOptions, count, "sidekick option");

        foreach (var sidekickOption in sidekickOptions)
        {
            WriteSidekickOption(sidekickOption);
        }
    }

    private void WriteSidekickOption(SidekickOption sidekickOption)
    {
        _writer.WritePascalString(sidekickOption.Name, TyrianConstants.HdtNameFieldSize);
        _writer.WriteByte(sidekickOption.PowerStages);
        _writer.WriteUInt16(sidekickOption.ItemGraphic);
        _writer.WriteUInt16(sidekickOption.Cost);
        _writer.WriteByte(sidekickOption.TrackingMode);
        _writer.WriteByte(sidekickOption.AnimationMode);
        _writer.WriteSByte(sidekickOption.OptionSpeed);
        _writer.WriteByte(sidekickOption.AnimationFrameCount);

        foreach (var graphic in sidekickOption.Graphics)
        {
            _writer.WriteUInt16(graphic);
        }

        _writer.WriteByte(sidekickOption.WeaponPortIndex);
        _writer.WriteUInt16(sidekickOption.WeaponPatternIndex);
        _writer.WriteByte(sidekickOption.Ammo);
        _writer.WriteBoolean(sidekickOption.Stop);
        _writer.WriteByte(sidekickOption.IconGraphic);
    }

    private void WriteShields(IReadOnlyList<ShieldDefinition> shields, int count)
    {
        ValidateCount(shields, count, "shield");

        foreach (var shield in shields)
        {
            WriteShield(shield);
        }
    }

    private void WriteShield(ShieldDefinition shield)
    {
        _writer.WritePascalString(shield.Name, TyrianConstants.HdtNameFieldSize);
        _writer.WriteByte(shield.GeneratorPowerRequired);
        _writer.WriteByte(shield.Protection);
        _writer.WriteUInt16(shield.ItemGraphic);
        _writer.WriteUInt16(shield.Cost);
    }

    private void WriteEnemies(IReadOnlyList<EnemyDefinition> enemies, int count)
    {
        ValidateCount(enemies, count, "enemy");

        foreach (var enemy in enemies)
        {
            WriteEnemy(enemy);
        }
    }

    private void WriteEnemy(EnemyDefinition enemy)
    {
        _writer.WriteByte(enemy.AnimationFrameCount);

        foreach (var weaponIndex in enemy.TurretWeaponIndices)
        {
            _writer.WriteByte(weaponIndex);
        }

        foreach (var fireRate in enemy.TurretFireRates)
        {
            _writer.WriteByte(fireRate);
        }

        _writer.WriteSByte(enemy.MoveX);
        _writer.WriteSByte(enemy.MoveY);
        _writer.WriteSByte(enemy.AccelerationX);
        _writer.WriteSByte(enemy.AccelerationY);
        _writer.WriteSByte(enemy.CircularAccelerationX);
        _writer.WriteSByte(enemy.CircularAccelerationY);
        _writer.WriteInt16(enemy.StartX);
        _writer.WriteInt16(enemy.StartY);
        _writer.WriteSByte(enemy.StartCircularX);
        _writer.WriteSByte(enemy.StartCircularY);
        _writer.WriteByte(enemy.Armor);
        _writer.WriteByte(enemy.EnemySize);

        foreach (var graphic in enemy.Graphics)
        {
            _writer.WriteUInt16(graphic);
        }

        _writer.WriteByte(enemy.ExplosionType);
        _writer.WriteByte(enemy.Animate);
        _writer.WriteByte(enemy.ShapeBank);
        _writer.WriteSByte(enemy.ReverseX);
        _writer.WriteSByte(enemy.ReverseY);
        _writer.WriteUInt16(enemy.DropGraphic);
        _writer.WriteSByte(enemy.DropLevel);
        _writer.WriteSByte(enemy.DropAnimation);
        _writer.WriteByte(enemy.LaunchFrequency);
        _writer.WriteUInt16(enemy.LaunchType);
        _writer.WriteInt16(enemy.Value);
        _writer.WriteUInt16(enemy.EnemyDeathEffect);
    }
}