namespace ii.Nairyt.Decoding;

internal class TyrianHdtHeader
{
    public int Episode1DataOffset { get; }
    public ushort RawWeaponCount { get; }
    public ushort RawWeaponPortCount { get; }
    public ushort RawPowerSystemCount { get; }
    public ushort RawShipCount { get; }
    public ushort RawOptionCount { get; }
    public ushort RawShieldCount { get; }
    public ushort RawEnemyCount { get; }

    public bool IsExpandedFormat => RawWeaponCount > TyrianConstants.HdtSharewareWeaponCount;

    public int DataOffset => Episode1DataOffset + TyrianConstants.HdtHeaderSize;

    public int WeaponPatternEntryCount => IsExpandedFormat ? ((RawWeaponCount + 1) * 2) + 1 : RawWeaponCount + 1;

    public int WeaponPortEntryCount => RawWeaponPortCount + 1;

    public int SpecialMoveEntryCount => TyrianConstants.HdtSpecialCount + 1;

    public int PowerGeneratorEntryCount => IsExpandedFormat ? RawPowerSystemCount : RawPowerSystemCount + 1;

    public int CompactShipEntryCount => RawShipCount - RawShieldCount;

    public int FullShipEntryCount => RawShipCount + 1;

    public int SidekickOptionEntryCount => IsExpandedFormat ? RawOptionCount : RawOptionCount + 1;

    public int ShipToOptionSectionPadding => IsExpandedFormat ? TyrianConstants.HdtExpandedShipToOptionPadding : 0;

    public int ShieldEntryCount => RawShieldCount + 1;

    public int EnemyEntryCount => RawEnemyCount + 1;

    public int WeaponPortSectionPadding => IsExpandedFormat ? TyrianConstants.HdtExpandedPortSectionPadding : 0;

    public int ShipSectionPadding => IsExpandedFormat ? TyrianConstants.HdtExpandedShipSectionPadding : 0;

    public IReadOnlyList<ushort> ItemCounts =>
    [
        RawWeaponCount,
        RawWeaponPortCount,
        RawPowerSystemCount,
        RawShipCount,
        RawOptionCount,
        RawShieldCount,
        RawEnemyCount,
    ];

    public int PayloadSize =>
        (WeaponPatternEntryCount * TyrianConstants.HdtWeaponPatternRecordSize) +
        (WeaponPortEntryCount * TyrianConstants.HdtWeaponPortRecordSize) +
        WeaponPortSectionPadding +
        (SpecialMoveEntryCount * TyrianConstants.HdtSpecialRecordSize) +
        (PowerGeneratorEntryCount * TyrianConstants.HdtPowerRecordSize) +
        ShipSectionSize +
        ShipToOptionSectionPadding +
        (SidekickOptionEntryCount * TyrianConstants.HdtSidekickRecordSize) +
        (ShieldEntryCount * TyrianConstants.HdtShieldRecordSize) +
        (EnemyEntryCount * TyrianConstants.HdtEnemyRecordSize);

    public int ShipSectionSize =>
        IsExpandedFormat
            ? (CompactShipEntryCount * TyrianConstants.HdtExpandedShipRecordSize) +
              ShipSectionPadding +
              (FullShipEntryCount * TyrianConstants.HdtShipRecordSize)
            : FullShipEntryCount * TyrianConstants.HdtShipRecordSize;

    public int FileSize => DataOffset + PayloadSize;

    public static TyrianHdtHeader FromItemCounts(int episode1DataOffset, IReadOnlyList<ushort> itemCounts)
    {
        if (itemCounts.Count != TyrianConstants.HdtItemCountFields)
        {
            throw new ArgumentException($"Expected {TyrianConstants.HdtItemCountFields} item counts, but received {itemCounts.Count}.", nameof(itemCounts));
        }

        return new TyrianHdtHeader(
            episode1DataOffset,
            itemCounts[0],
            itemCounts[1],
            itemCounts[2],
            itemCounts[3],
            itemCounts[4],
            itemCounts[5],
            itemCounts[6]);
    }

    public static TyrianHdtHeader Read(BinaryDataReader reader)
    {
        var episode1DataOffset = reader.ReadInt32();
        reader.Seek(episode1DataOffset);

        var rawWeaponCount = reader.ReadUInt16();
        var rawWeaponPortCount = reader.ReadUInt16();
        var rawPowerSystemCount = reader.ReadUInt16();
        var rawShipCount = reader.ReadUInt16();
        var rawOptionCount = reader.ReadUInt16();
        var rawShieldCount = reader.ReadUInt16();
        var rawEnemyCount = reader.ReadUInt16();
        reader.ReadUInt16();

        return new TyrianHdtHeader(
            episode1DataOffset,
            rawWeaponCount,
            rawWeaponPortCount,
            rawPowerSystemCount,
            rawShipCount,
            rawOptionCount,
            rawShieldCount,
            rawEnemyCount);
    }

    public void Write(BinaryDataWriter writer)
    {
        writer.WriteInt32(Episode1DataOffset);
        writer.Seek(Episode1DataOffset);
        writer.WriteUInt16(RawWeaponCount);
        writer.WriteUInt16(RawWeaponPortCount);
        writer.WriteUInt16(RawPowerSystemCount);
        writer.WriteUInt16(RawShipCount);
        writer.WriteUInt16(RawOptionCount);
        writer.WriteUInt16(RawShieldCount);
        writer.WriteUInt16(RawEnemyCount);
        writer.WriteUInt16(0);
    }

    private TyrianHdtHeader(
        int episode1DataOffset,
        ushort rawWeaponCount,
        ushort rawWeaponPortCount,
        ushort rawPowerSystemCount,
        ushort rawShipCount,
        ushort rawOptionCount,
        ushort rawShieldCount,
        ushort rawEnemyCount)
    {
        Episode1DataOffset = episode1DataOffset;
        RawWeaponCount = rawWeaponCount;
        RawWeaponPortCount = rawWeaponPortCount;
        RawPowerSystemCount = rawPowerSystemCount;
        RawShipCount = rawShipCount;
        RawOptionCount = rawOptionCount;
        RawShieldCount = rawShieldCount;
        RawEnemyCount = rawEnemyCount;
    }
}