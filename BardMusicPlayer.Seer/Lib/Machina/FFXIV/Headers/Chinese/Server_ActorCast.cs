﻿// Copyright © 2021 Ravahn - All Rights Reserved
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY. without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see<http://www.gnu.org/licenses/>.

using System.Runtime.InteropServices;

namespace Machina.FFXIV.Headers.Chinese;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Server_ActorCast
{
    public Server_MessageHeader MessageHeader; // 8 DWORDS
    public ushort ActionID;
    public byte SkillType;
    public byte Unknown;
    public uint Unknown1; // also action ID
    public float CastTime;
    public uint TargetID;
    public float Rotation; // in radians
    public uint Unknown2;
    public ushort PosX;
    public ushort PosY;
    public ushort PosZ;
    public ushort Unknown3;
}