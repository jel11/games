using System;
using System.IO;
using System.Text.Json;
using NarutoRoguelike.Data;

namespace NarutoRoguelike.Managers
{
    public static class SaveManager
    {
        private static readonly string SaveDir  = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NarutoRoguelike");

        private static readonly string SaveFile = Path.Combine(SaveDir, "save.json");

        private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters    = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };

        // ── Save ──────────────────────────────────────────────────────────────────

        public static bool Save(SaveGame data)
        {
            try
            {
                Directory.CreateDirectory(SaveDir);
                data.SaveTime = DateTime.UtcNow;
                string json   = JsonSerializer.Serialize(data, JsonOpts);
                File.WriteAllText(SaveFile, json);
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[SaveManager] Save failed: {ex.Message}");
                return false;
            }
        }

        // ── Load ──────────────────────────────────────────────────────────────────

        public static SaveGame? Load()
        {
            if (!File.Exists(SaveFile)) return null;

            try
            {
                string json = File.ReadAllText(SaveFile);
                return JsonSerializer.Deserialize<SaveGame>(json, JsonOpts);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[SaveManager] Load failed: {ex.Message}");
                return null;
            }
        }

        public static bool HasSave() => File.Exists(SaveFile);

        // ── Delete ────────────────────────────────────────────────────────────────

        public static void DeleteSave()
        {
            if (File.Exists(SaveFile))
                File.Delete(SaveFile);
        }
    }
}
