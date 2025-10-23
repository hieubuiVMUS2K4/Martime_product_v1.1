// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'sync_item.dart';

// **************************************************************************
// TypeAdapterGenerator
// **************************************************************************

class SyncItemAdapter extends TypeAdapter<SyncItem> {
  @override
  final int typeId = 0;

  @override
  SyncItem read(BinaryReader reader) {
    final numOfFields = reader.readByte();
    final fields = <int, dynamic>{
      for (int i = 0; i < numOfFields; i++) reader.readByte(): reader.read(),
    };
    return SyncItem(
      id: fields[0] as String?,
      type: fields[1] as SyncItemType,
      data: (fields[2] as Map).cast<String, dynamic>(),
      createdAt: fields[3] as DateTime?,
      retryCount: fields[4] as int,
    );
  }

  @override
  void write(BinaryWriter writer, SyncItem obj) {
    writer
      ..writeByte(5)
      ..writeByte(0)
      ..write(obj.id)
      ..writeByte(1)
      ..write(obj.type)
      ..writeByte(2)
      ..write(obj.data)
      ..writeByte(3)
      ..write(obj.createdAt)
      ..writeByte(4)
      ..write(obj.retryCount);
  }

  @override
  int get hashCode => typeId.hashCode;

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is SyncItemAdapter &&
          runtimeType == other.runtimeType &&
          typeId == other.typeId;
}

class SyncItemTypeAdapter extends TypeAdapter<SyncItemType> {
  @override
  final int typeId = 1;

  @override
  SyncItemType read(BinaryReader reader) {
    switch (reader.readByte()) {
      case 0:
        return SyncItemType.taskComplete;
      case 1:
        return SyncItemType.taskStart;
      case 2:
        return SyncItemType.profileUpdate;
      default:
        return SyncItemType.taskComplete;
    }
  }

  @override
  void write(BinaryWriter writer, SyncItemType obj) {
    switch (obj) {
      case SyncItemType.taskComplete:
        writer.writeByte(0);
        break;
      case SyncItemType.taskStart:
        writer.writeByte(1);
        break;
      case SyncItemType.profileUpdate:
        writer.writeByte(2);
        break;
    }
  }

  @override
  int get hashCode => typeId.hashCode;

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is SyncItemTypeAdapter &&
          runtimeType == other.runtimeType &&
          typeId == other.typeId;
}
