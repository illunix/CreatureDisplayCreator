using MySql.Data.MySqlClient;
using Stylet;
using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using WDBXLib.Definitions.WotLK;
using WDBXLib.Reader;
using WDBXLib.Storage;

namespace CreatureDisplayCreator.Pages
{
    public partial class ShellViewModel : Screen
    {
        private readonly IWindowManager _windowManager;
        private string _dbConnectionString;
        private string _creatureModelDataDbcPath;
        private string _creatureDisplayInfoDbcPath;
        private DBEntry<CreatureModelData> _creatureModelDataDbc;
        private DBEntry<CreatureDisplayInfo> _creatureDisplayInfoDbc;

        public string ModelName { get; set; }
        public float ModelScale { get; set; } = 1;
        public string TextureVariation_1 { get; set; } = "";
        public string TextureVariation_2 { get; set; } = "";
        public string TextureVariation_3 { get; set; } = "";
        public bool DirNameSameAsModelName { get; set; } = true;

        public ShellViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
            _dbConnectionString = ConfigurationManager.AppSettings.Get("DbConnectionString");
            var dbcPath = ConfigurationManager.AppSettings.Get("DbcPath");

            _creatureModelDataDbcPath = Path.Combine(dbcPath, "CreatureModelData.dbc");
            _creatureModelDataDbc = DBReader.Read<CreatureModelData>(_creatureModelDataDbcPath);
            _creatureDisplayInfoDbcPath = Path.Combine(dbcPath, "CreatureDisplayInfo.dbc");
            _creatureDisplayInfoDbc = DBReader.Read<CreatureDisplayInfo>(_creatureDisplayInfoDbcPath);
        }

        public async Task Add()
        {
            if (string.IsNullOrEmpty(ModelName))
            {
                _windowManager.ShowMessageBox("Please provide model name!");
                return;
            }

            var modelId = _creatureModelDataDbc.Rows[_creatureModelDataDbc.Rows.Count - 1].ID + 1;

            var creatureModelData = new CreatureModelData
            {
                ID = modelId,
                Flags = 0,
                ModelName = DirNameSameAsModelName ? @$"creature\{ModelName}\{ModelName}.m2" : $"{ModelName}.m2",
                ModelScale = 1
            };

            _creatureModelDataDbc.Rows.Add(creatureModelData);

            var displayInfoId = _creatureDisplayInfoDbc.Rows[_creatureDisplayInfoDbc.Rows.Count - 1].ID + 1;

            var creatureDisplayInfo = new CreatureDisplayInfo
            {
                ID = displayInfoId,
                ModelID = modelId,
                CreatureModelScale = 1,
                CreatureModelAlpha = 255,
                TextureVariation = new[] { TextureVariation_1, TextureVariation_2, TextureVariation_3 },
                PortraitTextureName = ""
            };

            _creatureDisplayInfoDbc.Rows.Add(creatureDisplayInfo);

            DBReader.Write(_creatureModelDataDbc, _creatureModelDataDbcPath);
            DBReader.Write(_creatureDisplayInfoDbc, _creatureDisplayInfoDbcPath);

            using var conn = new MySqlConnection(_dbConnectionString);

            await conn.OpenAsync();

            using var command = conn.CreateCommand();

            command.CommandText = "INSERT INTO `creature_model_info` VALUES (@ID, 0, 0, 2, 0);";
            command.Parameters.AddWithValue("@ID", displayInfoId);

            await command.ExecuteNonQueryAsync();

            await conn.CloseAsync();

            _windowManager.ShowMessageBox(displayInfoId.ToString());
        }
    }
}
