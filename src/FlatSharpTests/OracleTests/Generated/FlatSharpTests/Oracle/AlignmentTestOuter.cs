// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace FlatSharpTests.Oracle
{

using global::System;
using global::System.Collections.Generic;
using global::FlatBuffers;

public struct AlignmentTestOuter : IFlatbufferObject
{
  private Struct __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public void __init(int _i, ByteBuffer _bb) { __p = new Struct(_i, _bb); }
  public AlignmentTestOuter __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public byte A { get { return __p.bb.Get(__p.bb_pos + 0); } }
  public ushort B { get { return __p.bb.GetUshort(__p.bb_pos + 2); } }
  public byte C { get { return __p.bb.Get(__p.bb_pos + 4); } }
  public uint D { get { return __p.bb.GetUint(__p.bb_pos + 8); } }
  public byte E { get { return __p.bb.Get(__p.bb_pos + 12); } }
  public ulong F { get { return __p.bb.GetUlong(__p.bb_pos + 16); } }
  public FlatSharpTests.Oracle.AlignmentTestInner G { get { return (new FlatSharpTests.Oracle.AlignmentTestInner()).__assign(__p.bb_pos + 24, __p.bb); } }

  public static Offset<FlatSharpTests.Oracle.AlignmentTestOuter> CreateAlignmentTestOuter(FlatBufferBuilder builder, byte A, ushort B, byte C, uint D, byte E, ulong F, byte g_A, int g_B, sbyte g_C) {
    builder.Prep(8, 40);
    builder.Pad(4);
    builder.Prep(4, 12);
    builder.Pad(3);
    builder.PutSbyte(g_C);
    builder.PutInt(g_B);
    builder.Pad(3);
    builder.PutByte(g_A);
    builder.PutUlong(F);
    builder.Pad(3);
    builder.PutByte(E);
    builder.PutUint(D);
    builder.Pad(3);
    builder.PutByte(C);
    builder.PutUshort(B);
    builder.Pad(1);
    builder.PutByte(A);
    return new Offset<FlatSharpTests.Oracle.AlignmentTestOuter>(builder.Offset);
  }
};


}
