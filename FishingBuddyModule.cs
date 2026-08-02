using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Oberyn.FishingBuddy.Controls;
using Oberyn.FishingBuddy.Services;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Oberyn.FishingBuddy
{
    [Export(typeof(Module))]
    public class FishingBuddyModule : Module
    {
        private static readonly Logger Logger = Logger.GetLogger(typeof(FishingBuddyModule));
        internal static FishingBuddyModule Instance { get; private set; }
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;

        internal AchievementProgressService AchievementProgress { get; private set; }

        private CornerIcon _cornerIcon;
        private StandardWindow _mainWindow;
        private MainView _mainView;
        private System.Threading.Timer _bannerRefreshTimer;

        [ImportingConstructor]
        public FishingBuddyModule([Import("ModuleParameters")] ModuleParameters moduleParameters)
            : base(moduleParameters)
        {
            Instance = this;
        }
        protected override void DefineSettings(SettingCollection settings)
        {
        }
        protected override void Initialize()
        {
            try
            {
                Logger.Info("Fishing Buddy initializing.");
                AchievementProgress = new AchievementProgressService();
            }
            catch (Exception ex)
            {
                // debugging cause the module didn't load during texting, this is to find it easily in Blish's logs
                Logger.Error(ex, "Fishing Buddy failed during Initialize().");
                throw;
            }
        }
        protected override async Task LoadAsync()
        {
            try
            {
                Logger.Info("Fishing Buddy loaded.");

                BuildWindow();
                BuildContent();
                BuildCornerIcon();

                _bannerRefreshTimer = new System.Threading.Timer(
                    _ => GameService.Graphics.QueueMainThreadRender(__ => _mainView.RefreshBanners()),
                    null,
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(1)
                );

                Gw2ApiManager.SubtokenUpdated += async (sender, e) => await AchievementProgress.RefreshAsync();
                await AchievementProgress.RefreshAsync();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Fishing Buddy failed during LoadAsync().");
                throw;
            }
        }

        private void BuildWindow()
        {
            var windowBackground = ContentsManager.GetTexture("icons/background2.png");

            _mainWindow = new StandardWindow(
                windowBackground,
                new Rectangle(0, 0, 1172, 667),
                new Rectangle(60, 50, 1052, 568))
            {
                Parent = GameService.Graphics.SpriteScreen,
                Title = "Fishing Buddy",
                Location = new Point(50, 50),
                SavesPosition = true,
                Id = "FishingBuddy_MainWindow",
            };
        }

        private const int ContentWidth = 1052;
        private const int ContentHeight = 568;

        private void BuildContent()
        {
            _mainView = new MainView(ContentsManager, AchievementProgress, ContentHeight)
            {
                Parent = _mainWindow,
                Width = ContentWidth,
                Height = ContentHeight,
            };
        }

        private void BuildCornerIcon()
        {
            _cornerIcon = new CornerIcon
            {
                Icon = ContentsManager.GetTexture("icons/icon.png"),
                HoverIcon = ContentsManager.GetTexture("icons/icon_hover.png"),
                BasicTooltipText = "Fishing Buddy",
                Parent = GameService.Graphics.SpriteScreen,
            };

            _cornerIcon.Click += (s, e) =>
            {
                if (_mainWindow.Visible)
                    _mainWindow.Hide();
                else
                    _mainWindow.Show();
            };
        }

        protected override void Update(GameTime gameTime)
        {
        }

        protected override void Unload()
        {
            _bannerRefreshTimer?.Dispose();
            _cornerIcon?.Dispose();
            _mainWindow?.Dispose();
            Instance = null;
        }
    }
}