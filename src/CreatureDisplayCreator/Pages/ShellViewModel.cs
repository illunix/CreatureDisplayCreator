using CreatureDisplayCreator.Infrastructure.Data;
using CreatureDisplayCreator.Infrastructure.Entities;
using Microsoft.Extensions.Configuration;
using Stylet;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
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
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

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

        protected override void OnViewLoaded()
        {
            var dbcPath = _configuration["dbcPath"];

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

            var creatureModelName = DirNameSameAsModelName ? @$"creature\{ModelName}\{ModelName}.m2" : $"{ModelName}.m2";

            var creatureModelData = _creatureModelDataDbc.Rows
                .Where(q => q.ModelName == creatureModelName)
                .FirstOrDefault();

            var modelId = 0;

            if (creatureModelData is not null)
            {
                modelId = creatureModelData.ID;
            }
            else
            {
                modelId = _creatureModelDataDbc.Rows[_creatureModelDataDbc.Rows.Count - 1].ID + 1;

                creatureModelData = new CreatureModelData
                {
                    ID = modelId,
                    Flags = 0,
                    ModelName = DirNameSameAsModelName ? @$"creature\{ModelName}\{ModelName}.m2" : $"{ModelName}.m2",
                    ModelScale = 1
                };

                _creatureModelDataDbc.Rows.Add(creatureModelData);

                DBReader.Write(_creatureModelDataDbc, _creatureModelDataDbcPath);
            }

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

            DBReader.Write(_creatureDisplayInfoDbc, _creatureDisplayInfoDbcPath);

            var creatureModelInfo = new CreatureModelInfo(displayInfoId);

            _context.CreatureModelInfos
                .Add(creatureModelInfo);

            await _context.SaveChangesAsync();

            Clipboard.SetText(displayInfoId.ToString());

            _windowManager.ShowMessageBox(displayInfoId.ToString());
        }
    }
}
