﻿using System.Collections.Generic;

namespace AFPParser
{
    public static partial class CodePages
    {
        public static Dictionary<byte, string> C1252 = new Dictionary<byte, string>()
        {
            { 0x20, "SP010000" },
            { 0x21, "SP020000" },
            { 0x22, "SP040000" },
            { 0x23, "SM010000" },
            { 0x24, "SC030000" },
            { 0x25, "SM020000" },
            { 0x26, "SM030000" },
            { 0x27, "SP050000" },
            { 0x28, "SP060000" },
            { 0x29, "SP070000" },
            { 0x2A, "SM040000" },
            { 0x2B, "SA010000" },
            { 0x2C, "SP080000" },
            { 0x2D, "SP100000" },
            { 0x2E, "SP110000" },
            { 0x2F, "SP120000" },
            { 0x30, "ND100000" },
            { 0x31, "ND010000" },
            { 0x32, "ND020000" },
            { 0x33, "ND030000" },
            { 0x34, "ND040000" },
            { 0x35, "ND050000" },
            { 0x36, "ND060000" },
            { 0x37, "ND070000" },
            { 0x38, "ND080000" },
            { 0x39, "ND090000" },
            { 0x3A, "SP130000" },
            { 0x3B, "SP140000" },
            { 0x3C, "SA030000" },
            { 0x3D, "SA040000" },
            { 0x3E, "SA050000" },
            { 0x3F, "SP150000" },
            { 0x40, "SM050000" },
            { 0x41, "LA020000" },   // A
            { 0x42, "LB020000" },   // B
            { 0x43, "LC020000" },   // C
            { 0x44, "LD020000" },   // D
            { 0x45, "LE020000" },   // E
            { 0x46, "LF020000" },   // F
            { 0x47, "LG020000" },   // G
            { 0x48, "LH020000" },   // H
            { 0x49, "LI020000" },   // I
            { 0x4A, "LJ020000" },   // J
            { 0x4B, "LK020000" },   // K
            { 0x4C, "LL020000" },   // L
            { 0x4D, "LM020000" },   // M
            { 0x4E, "LN020000" },   // N
            { 0x4F, "LO020000" },   // O
            { 0x50, "LP020000" },   // P
            { 0x51, "LQ020000" },   // Q
            { 0x52, "LR020000" },   // R
            { 0x53, "LS020000" },   // S
            { 0x54, "LT020000" },   // T
            { 0x55, "LU020000" },   // U
            { 0x56, "LV020000" },   // V
            { 0x57, "LW020000" },   // W
            { 0x58, "LX020000" },   // X
            { 0x59, "LY020000" },   // Y
            { 0x5A, "LZ020000" },   // Z
            { 0x5B, "SM060000" },
            { 0x5C, "SM070000" },
            { 0x5D, "SM080000" },
            { 0x5E, "SD150000" },
            { 0x5F, "SP090000" },
            { 0x60, "SD130000" },
            { 0x61, "LA010000" },   // a
            { 0x62, "LB010000" },	// b
            { 0x63, "LC010000" },	// c
            { 0x64, "LD010000" },	// d
            { 0x65, "LE010000" },	// e
            { 0x66, "LF010000" },	// f
            { 0x67, "LG010000" },	// g
            { 0x68, "LH010000" },	// h
            { 0x69, "LI010000" },	// i
            { 0x6A, "LJ010000" },	// j
            { 0x6B, "LK010000" },	// k
            { 0x6C, "LL010000" },	// l
            { 0x6D, "LM010000" },	// m
            { 0x6E, "LN010000" },	// n
            { 0x6F, "LO010000" },	// o
            { 0x70, "LP010000" },	// p
            { 0x71, "LQ010000" },	// q
            { 0x72, "LR010000" },	// r
            { 0x73, "LS010000" },	// s
            { 0x74, "LT010000" },	// t
            { 0x75, "LU010000" },	// u
            { 0x76, "LV010000" },	// v
            { 0x77, "LW010000" },	// w
            { 0x78, "LX010000" },	// x
            { 0x79, "LY010000" },	// y
            { 0x7A, "LZ010000" },   // z
            { 0x7B, "SM110000" },
            { 0x7C, "SM130000" },
            { 0x7D, "SM140000" },
            { 0x7E, "SD190000" },
            { 0x80, "SC200000" },
            { 0x82, "SP260000" },
            { 0x83, "SC070000" },
            { 0x84, "SP230000" },
            { 0x85, "SV520000" },
            { 0x86, "SM340000" },
            { 0x87, "SM350000" },
            { 0x88, "SD150100" },
            { 0x89, "SM560000" },
            { 0x8A, "LS220000" },
            { 0x8B, "SP270000" },
            { 0x8C, "LO520000" },
            { 0x8E, "LZ220000" },
            { 0x91, "SP190000" },
            { 0x92, "SP200000" },
            { 0x93, "SP210000" },
            { 0x94, "SP220000" },
            { 0x95, "SM570000" },
            { 0x96, "SS680000" },
            { 0x97, "SM900000" },
            { 0x98, "SD190100" },
            { 0x99, "SM540000" },
            { 0x9A, "LS210000" },
            { 0x9B, "SP280000" },
            { 0x9C, "LO510000" },
            { 0x9E, "LZ210000" },
            { 0x9F, "LY180000" },
            { 0xA0, "SP300000" },
            { 0xA1, "SP030000" },
            { 0xA2, "SC040000" },
            { 0xA3, "SC020000" },
            { 0xA4, "SC010000" },
            { 0xA5, "SC050000" },
            { 0xA6, "SM650000" },
            { 0xA7, "SM240000" },
            { 0xA8, "SD170000" },
            { 0xA9, "SM520000" },
            { 0xAA, "SM210000" },
            { 0xAB, "SP170000" },
            { 0xAC, "SM660000" },
            { 0xAD, "SP320000" },
            { 0xAE, "SM530000" },
            { 0xAF, "SD310000" },
            { 0xB0, "SM190000" },
            { 0xB1, "SA020000" },
            { 0xB2, "ND021000" },
            { 0xB3, "ND031000" },
            { 0xB4, "SD110000" },
            { 0xB5, "SM170000" },
            { 0xB6, "SM250000" },
            { 0xB7, "SD630000" },
            { 0xB8, "SD410000" },
            { 0xB9, "ND011000" },
            { 0xBA, "SM200000" },
            { 0xBB, "SP180000" },
            { 0xBC, "NF040000" },
            { 0xBD, "NF010000" },
            { 0xBE, "NF050000" },
            { 0xBF, "SP160000" },
            { 0xC0, "LA140000" },
            { 0xC1, "LA120000" },
            { 0xC2, "LA160000" },
            { 0xC3, "LA200000" },
            { 0xC4, "LA180000" },
            { 0xC5, "LA280000" },
            { 0xC6, "LA520000" },
            { 0xC7, "LC420000" },
            { 0xC8, "LE140000" },
            { 0xC9, "LE120000" },
            { 0xCA, "LE160000" },
            { 0xCB, "LE180000" },
            { 0xCC, "LI140000" },
            { 0xCD, "LI120000" },
            { 0xCE, "LI160000" },
            { 0xCF, "LI180000" },
            { 0xD0, "LD640000" },
            { 0xD1, "LN200000" },
            { 0xD2, "LO140000" },
            { 0xD3, "LO120000" },
            { 0xD4, "LO160000" },
            { 0xD5, "LO200000" },
            { 0xD6, "LO180000" },
            { 0xD7, "SA070000" },
            { 0xD8, "LO620000" },
            { 0xD9, "LU140000" },
            { 0xDA, "LU120000" },
            { 0xDB, "LU160000" },
            { 0xDC, "LU180000" },
            { 0xDD, "LY120000" },
            { 0xDE, "LT640000" },
            { 0xDF, "LS610000" },
            { 0xE0, "LA130000" },
            { 0xE1, "LA110000" },
            { 0xE2, "LA150000" },
            { 0xE3, "LA190000" },
            { 0xE4, "LA170000" },
            { 0xE5, "LA270000" },
            { 0xE6, "LA510000" },
            { 0xE7, "LC410000" },
            { 0xE8, "LE130000" },
            { 0xE9, "LE110000" },
            { 0xEA, "LE150000" },
            { 0xEB, "LE170000" },
            { 0xEC, "LI130000" },
            { 0xED, "LI110000" },
            { 0xEE, "LI150000" },
            { 0xEF, "LI170000" },
            { 0xF0, "LD630000" },
            { 0xF1, "LN190000" },
            { 0xF2, "LO130000" },
            { 0xF3, "LO110000" },
            { 0xF4, "LO150000" },
            { 0xF5, "LO190000" },
            { 0xF6, "LO170000" },
            { 0xF7, "SA060000" },
            { 0xF8, "LO610000" },
            { 0xF9, "LU130000" },
            { 0xFA, "LU110000" },
            { 0xFB, "LU150000" },
            { 0xFC, "LU170000" },
            { 0xFD, "LY110000" },
            { 0xFE, "LT630000" },
            { 0xFF, "LY170000" }
        };
    }
}