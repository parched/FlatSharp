// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace FlatSharpTests.Oracle
{

using global::System;
using global::System.Collections.Generic;
using global::FlatBuffers;

public struct UnionTable : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_1_12_0(); }
  public static UnionTable GetRootAsUnionTable(ByteBuffer _bb) { return GetRootAsUnionTable(_bb, new UnionTable()); }
  public static UnionTable GetRootAsUnionTable(ByteBuffer _bb, UnionTable obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public UnionTable __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public FlatSharpTests.Oracle.Union ValueType { get { int o = __p.__offset(4); return o != 0 ? (FlatSharpTests.Oracle.Union)__p.bb.Get(o + __p.bb_pos) : FlatSharpTests.Oracle.Union.NONE; } }
  public TTable? Value<TTable>() where TTable : struct, IFlatbufferObject { int o = __p.__offset(6); return o != 0 ? (TTable?)__p.__union<TTable>(o + __p.bb_pos) : null; }
  public string ValueAsString() { int o = __p.__offset(6); return o != 0 ? __p.__string(o + __p.bb_pos) : null; }

  public static Offset<FlatSharpTests.Oracle.UnionTable> CreateUnionTable(FlatBufferBuilder builder,
      FlatSharpTests.Oracle.Union Value_type = FlatSharpTests.Oracle.Union.NONE,
      int ValueOffset = 0) {
    builder.StartTable(2);
    UnionTable.AddValue(builder, ValueOffset);
    UnionTable.AddValueType(builder, Value_type);
    return UnionTable.EndUnionTable(builder);
  }

  public static void StartUnionTable(FlatBufferBuilder builder) { builder.StartTable(2); }
  public static void AddValueType(FlatBufferBuilder builder, FlatSharpTests.Oracle.Union ValueType) { builder.AddByte(0, (byte)ValueType, 0); }
  public static void AddValue(FlatBufferBuilder builder, int ValueOffset) { builder.AddOffset(1, ValueOffset, 0); }
  public static Offset<FlatSharpTests.Oracle.UnionTable> EndUnionTable(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<FlatSharpTests.Oracle.UnionTable>(o);
  }
};


}
