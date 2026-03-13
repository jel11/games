using Microsoft.Xna.Framework;

namespace NarutoRoguelike.Core
{
    public static class Constants
    {
        // ── Display ──────────────────────────────────────────────────────────────
        public const int SCREEN_WIDTH  = 1280;
        public const int SCREEN_HEIGHT = 720;
        public const int TILE_SIZE     = 32;

        // ── Map ───────────────────────────────────────────────────────────────────
        public const int MAP_WIDTH        = 80;
        public const int MAP_HEIGHT       = 50;
        public const int VIEWPORT_TILES_X = 25;
        public const int VIEWPORT_TILES_Y = 18;

        // ── BSP generation ────────────────────────────────────────────────────────
        public const int BSP_MIN_ROOM_SIZE = 6;
        public const int BSP_MAX_ROOM_SIZE = 14;
        public const int BSP_MIN_SPLIT     = 10;

        // ── Player base stats ─────────────────────────────────────────────────────
        public const int PLAYER_HP_BASE        = 100;
        public const int PLAYER_CHAKRA_BASE    = 80;
        public const int PLAYER_ATTACK_BASE    = 12;
        public const int PLAYER_DEFENSE_BASE   = 5;
        public const int PLAYER_SPEED_BASE     = 10;
        public const int PLAYER_LEVEL_UP_BASE  = 100;  // XP needed for level 2
        public const float PLAYER_LEVEL_UP_SCALE = 1.5f;

        // ── Enemy stat tables (by rank) ───────────────────────────────────────────
        // Genin
        public const int GENIN_HP      = 30;
        public const int GENIN_ATTACK  = 8;
        public const int GENIN_DEFENSE = 2;
        public const int GENIN_XP      = 20;
        // Chunin
        public const int CHUNIN_HP      = 55;
        public const int CHUNIN_ATTACK  = 14;
        public const int CHUNIN_DEFENSE = 5;
        public const int CHUNIN_XP      = 50;
        // Jonin
        public const int JONIN_HP      = 100;
        public const int JONIN_ATTACK  = 22;
        public const int JONIN_DEFENSE = 10;
        public const int JONIN_XP      = 120;
        // Akatsuki
        public const int AKATSUKI_HP      = 200;
        public const int AKATSUKI_ATTACK  = 35;
        public const int AKATSUKI_DEFENSE = 15;
        public const int AKATSUKI_XP      = 300;

        // ── Chakra costs ──────────────────────────────────────────────────────────
        public const int CHAKRA_RASENGAN          = 25;
        public const int CHAKRA_SHADOW_CLONE      = 15;
        public const int CHAKRA_CHIDORI           = 30;
        public const int CHAKRA_FIREBALL          = 20;
        public const int CHAKRA_SUMMONING         = 40;
        public const int CHAKRA_EIGHT_TRIGRAMS    = 35;
        public const int CHAKRA_SAND_SHIELD       = 18;
        public const int CHAKRA_REGEN_PER_TURN    = 5;

        // ── Combat ────────────────────────────────────────────────────────────────
        public const float CRITICAL_HIT_CHANCE    = 0.10f;
        public const float CRITICAL_HIT_MULTIPLIER = 2.0f;
        public const int   DODGE_BASE_CHANCE      = 5;   // percent

        // ── FOV ───────────────────────────────────────────────────────────────────
        public const int FOV_RADIUS = 8;

        // ── UI layout ────────────────────────────────────────────────────────────
        public const int HUD_HEIGHT       = 80;   // pixels at bottom
        public const int LOG_WIDTH        = 320;  // pixels on right
        public const int LOG_MAX_MESSAGES = 50;

        // ── Color palette ─────────────────────────────────────────────────────────
        // Naruto orange
        public static readonly Color NarutoOrange   = new Color(255, 120,  30);
        // Sasuke dark
        public static readonly Color SasukeBlue     = new Color( 30,  30, 120);
        // Akatsuki dark red / cloud
        public static readonly Color AkatsukiRed    = new Color(160,  10,  10);
        public static readonly Color AkatsukiCloud  = new Color(220, 220, 220);
        // Chakra blue
        public static readonly Color ChakraBlue     = new Color( 80, 180, 255);
        // Map colors
        public static readonly Color FloorColor     = new Color( 60,  60,  60);
        public static readonly Color WallColor      = new Color( 30,  30,  30);
        public static readonly Color WallLitColor   = new Color( 90,  80,  70);
        public static readonly Color FloorLitColor  = new Color(100,  90,  80);
        public static readonly Color FogColor       = new Color( 20,  20,  20);
        // UI
        public static readonly Color UIBackground   = new Color( 15,  15,  15);
        public static readonly Color UIBorder       = new Color( 80,  80,  80);
        public static readonly Color UIText         = new Color(220, 220, 200);
        public static readonly Color UIHighlight    = new Color(255, 200,  50);
        // HP / Chakra bars
        public static readonly Color HPBarFull      = new Color( 20, 180,  20);
        public static readonly Color HPBarLow       = new Color(220,  40,  40);
        public static readonly Color ChakraBarColor = new Color( 40, 120, 220);
        // XP bar
        public static readonly Color XPBarColor     = new Color(180, 140,  20);
        // Item rarity
        public static readonly Color RarityCommon   = new Color(180, 180, 180);
        public static readonly Color RarityUncommon = new Color( 30, 200,  30);
        public static readonly Color RarityRare     = new Color( 30,  80, 220);
        public static readonly Color RarityLegendary= new Color(220, 140,  20);
    }
}
