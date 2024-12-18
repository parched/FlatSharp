﻿/*
 * Copyright 2020 James Courtney
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using FlatSharp.CodeGen;

namespace FlatSharpTests.Compiler;

public class PrecompiledSerializerTests
{
    [Fact]
    public void MonsterTest()
    {
        // https://github.com/google/flatbuffers/blob/master/samples/monster.fbs
        string schema = $@"
            {MetadataHelpers.AllAttributes}
            namespace MyGame;
            enum Color:byte {{ Red = 0, Green, Blue = 2 }}

            union Equipment {{ Weapon, Vec3 }} // Optionally add more tables.

            struct Vec3 {{
              x:float;
              y:float;
              z:float;
            }}

            table Monster ({MetadataKeys.SerializerKind}:""greedymutable"") {{
              pos:Vec3;
              mana:short = 150;
              hp:short = 100;
              name:string;
              friendly:bool = false ({MetadataKeys.Deprecated});
              inventory:[ubyte];
              color:Color = Blue;
              weapons:[Weapon];
              equipped:Equipment;
              path:[Vec3];
            }}

            table Weapon ({MetadataKeys.SerializerKind}:""lazy"") {{
              name:string;
              damage:short;
            }}

            root_type Monster;";

        Assembly asm = FlatSharpCompiler.CompileAndLoadAssembly(schema, new());

        Type weaponType = asm.GetType("MyGame.Weapon");
        Type monsterType = asm.GetTypes().Single(x => x.FullName == "MyGame.Monster");
        dynamic serializer = monsterType.GetProperty("Serializer", BindingFlags.Static | BindingFlags.Public).GetValue(null);

        object monster = Activator.CreateInstance(monsterType);
        dynamic dMonster = monster;

        Type vecType = asm.GetTypes().Single(x => x.FullName == "MyGame.Vec3");
        object vec = Activator.CreateInstance(vecType);
        dynamic dVec = vec;

        Assert.Equal((short)150, dMonster.Mana);
        Assert.Equal((short)100, dMonster.Hp);
        Assert.False(dMonster.Friendly);
        Assert.Equal("Blue", dMonster.Color.ToString());
        Assert.Null(dMonster.Pos);

        Assert.Equal(typeof(Memory<byte>?), monsterType.GetProperty("Inventory").PropertyType);
        Assert.Equal(typeof(IList<>).MakeGenericType(vecType), monsterType.GetProperty("Path").PropertyType);
        Assert.Equal(typeof(IList<>).MakeGenericType(weaponType), monsterType.GetProperty("Weapons").PropertyType);
        Assert.True(typeof(IFlatBufferUnion<,>).MakeGenericType(weaponType, vecType).IsAssignableFrom(Nullable.GetUnderlyingType(monsterType.GetProperty("Equipped").PropertyType)));
        Assert.Equal(typeof(string), monsterType.GetProperty("Name").PropertyType);
        Assert.True(monsterType.GetProperty("Friendly").GetCustomAttribute<FlatBufferItemAttribute>().Deprecated);

        var compiled = CompilerTestHelpers.CompilerTestSerializer.Compile(monster);
        var data = compiled.WriteToMemory(monster);
        dynamic parsedMonster = compiled.Parse(data);

        Assert.Equal("Blue", parsedMonster.Color.ToString());
    }

    [Fact]
    public void FlagsOptions_Greedy()
    {
        this.TestFlags(FlatBufferDeserializationOption.Greedy, $"{MetadataKeys.SerializerKind}:\"{nameof(FlatBufferDeserializationOption.Greedy)}\"");
    }

    [Fact]
    public void FlagsOptions_MutableGreedy()
    {
        this.TestFlags(FlatBufferDeserializationOption.GreedyMutable, $"{MetadataKeys.SerializerKind}:\"{nameof(FlatBufferDeserializationOption.GreedyMutable)}\"");
    }

    [Fact]
    public void FlagsOptions_Default()
    {
        this.TestFlags(FlatBufferDeserializationOption.Default, $"{MetadataKeys.SerializerKind}:\"{nameof(FlatBufferDeserializationOption.Default)}\"");
    }

    [Fact]
    public void FlagsOptions_Default_Implicit()
    {
        this.TestFlags(FlatBufferDeserializationOption.Default, $"{MetadataKeys.SerializerKind}");
    }

    [Fact]
    public void FlagsOptions_Lazy()
    {
        this.TestFlags(FlatBufferDeserializationOption.Lazy, $"{MetadataKeys.SerializerKind}:\"{nameof(FlatBufferDeserializationOption.Lazy)}\"");
    }

    [Fact]
    public void FlagsOptions_Progressive()
    {
        this.TestFlags(FlatBufferDeserializationOption.Progressive, $"{MetadataKeys.SerializerKind}:\"{nameof(FlatBufferDeserializationOption.Progressive)}\"");
    }

    [Fact]
    public void FlagsOptions_Invalid()
    {
        Assert.Throws<InvalidFbsFileException>(() => this.TestFlags(default, $"{MetadataKeys.SerializerKind}:banana"));
        Assert.Throws<InvalidFbsFileException>(() => this.TestFlags(default, $"{MetadataKeys.SerializerKind}:\"banana\""));
        Assert.Throws<InvalidFbsFileException>(() => this.TestFlags(default, $"{MetadataKeys.SerializerKind}:\"greedy|banana\""));
        Assert.Throws<InvalidFbsFileException>(() => this.TestFlags(default, $"{MetadataKeys.SerializerKind}:\"greedy|mutablegreedy\""));
    }

    private void TestFlags(FlatBufferDeserializationOption expectedFlags, string metadata)
    {
        string schema = $"{MetadataHelpers.AllAttributes} namespace Test; table FooTable ({metadata}) {{ foo:string; bar:string; }}";
        Assembly asm = FlatSharpCompiler.CompileAndLoadAssembly(schema, new());

        Type type = asm.GetType("Test.FooTable");
        Assert.NotNull(type);

        PropertyInfo serializerProp = type.GetProperty("Serializer", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(serializerProp);

        ISerializer serializer = (ISerializer)serializerProp.GetValue(null);
        Assert.Equal(expectedFlags, serializer.DeserializationOption);
    }
}

