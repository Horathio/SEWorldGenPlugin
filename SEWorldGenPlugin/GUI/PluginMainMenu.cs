﻿using Sandbox;
using Sandbox.Engine.Networking;
using Sandbox.Engine.Utils;
using Sandbox.Game.Screens;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Game.World;
using Sandbox.Graphics;
using Sandbox.Graphics.GUI;
using SEWorldGenPlugin.http;
using SEWorldGenPlugin.Utilities;
using SpaceEngineers.Game.GUI;
using System;
using System.IO;
using System.Text;
using VRage;
using VRage.FileSystem;
using VRage.Game;
using VRage.Input;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.GUI
{
    public class PluginMainMenu : MyGuiScreenMainMenu
    {
        private static readonly StringBuilder PLUGIN_LOADED = new StringBuilder("SEWorldGenPlugin version {0} is loaded and by default ");
        private static readonly StringBuilder PLUGIN_ENABLED = new StringBuilder("ENABLED");
        private static readonly StringBuilder PLUGIN_DISABLED = new StringBuilder("DISABLED");

        private static bool OPENED_VERSION_NOTIFICATION = false;

        private MyGuiControlElementGroup m_elemtents;

        public PluginMainMenu() : this(false)
        {

        }

        public PluginMainMenu(bool pauseGame) : base(pauseGame)
        {
            m_pauseGame = pauseGame;
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            if (m_pauseGame) return;
            m_elemtents = new MyGuiControlElementGroup();
            m_elemtents.HighlightChanged += OnHighlightChange;
            MyGuiControlButton button = null;
            foreach (var c in Controls)
            {
                if(c is MyGuiControlButton)
                {
                    m_elemtents.Add(c);
                    if (((MyGuiControlButton)c).Text == MyTexts.GetString(MyCommonTexts.ScreenMenuButtonCampaign))
                        button = (MyGuiControlButton)c;
                }
            }
            if(button != null)
            {
                int index = Controls.IndexOf(button);
                MyGuiControlButton newGameButton = MakeButton(button.Position, MyCommonTexts.ScreenMenuButtonCampaign, OnNewGameClick);
                Controls.Add(newGameButton);
                m_elemtents.Add(newGameButton);
                newGameButton.Name = button.Name;
                Controls[index] = newGameButton;
                newGameButton.SetToolTip(button.Tooltips);
            }

            if (!VersionCheck.Static.IsNewest() && !OPENED_VERSION_NOTIFICATION)
            {
                OPENED_VERSION_NOTIFICATION = true;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, new StringBuilder(MyPluginTexts.MESSAGES.UPDATE_AVAILABLE_BOX), new StringBuilder(MyPluginTexts.MESSAGES.UPDATE_AVAILABLE_TITLE), null, null, null, null, OnUpdateNotifiactionMessageClose));
            }
        }

        private void OnUpdateNotifiactionMessageClose(MyGuiScreenMessageBox.ResultEnum r)
        {
            if(r == MyGuiScreenMessageBox.ResultEnum.YES)
            {
                MyGuiSandbox.OpenUrl(VersionCheck.Static.GetLatestVersionPage(), UrlOpenMode.SteamOrExternalWithConfirm);
            }
        }

        private void OnHighlightChange(MyGuiControlElementGroup obj)
        {
            foreach (var c in m_elemtents)
            {
                if (c.HasFocus && m_elemtents.SelectedIndex != obj.SelectedIndex)
                {
                    FocusedControl = c;
                    break;
                }
            }
        }

        private void OnNewGameClick(object sender)
        {
            if (!MyFakes.LIMITED_MAIN_MENU || MyInput.Static.ENABLE_DEVELOPER_KEYS)
            {
                RunWithTutorialCheck(delegate
                {
                    MyGuiSandbox.AddScreen(new PluginGuiScreenNewGame());
                });
            }
            else
            {
                QuickstartScenario("Red Ship");
            }
        }

        private void QuickstartScenario(string scenarioName)
        {
            string path = "CustomWorlds";
            string sessionPath = Path.Combine(MyFileSystem.ContentPath, path, scenarioName);
            ulong sizeInBytes;
            MyObjectBuilder_Checkpoint checkpoint = MyLocalCache.LoadCheckpoint(sessionPath, out sizeInBytes);
            if (checkpoint != null)
            {
                MySessionLoader.LoadSingleplayerSession(checkpoint, sessionPath, sizeInBytes, delegate
                {
                    MyAsyncSaving.Start(null, Path.Combine(MyFileSystem.SavesPath, checkpoint.SessionName.Replace(':', '-')));
                });
            }
        }

        private void RunWithTutorialCheck(Action afterTutorial)
        {
            if (MySandboxGame.Config.FirstTimeTutorials)
            {
                MyGuiSandbox.AddScreen(new MyGuiScreenTutorialsScreen(afterTutorial));
            }
            else
            {
                afterTutorial();
            }
        }

        public override bool Draw()
        {
            DrawPluginLoaded();
            return base.Draw();
        }

        private void DrawPluginLoaded()
        {
            Vector2 size;
            Vector2 textLeftBottomPosition = MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM, 8, 8);
            textLeftBottomPosition.Y -= 0 * TEXT_LINE_HEIGHT;

            StringBuilder pluginWithVersion = new StringBuilder();
            pluginWithVersion.AppendFormat(PLUGIN_LOADED.ToString(), VersionCheck.Static.GetVersion());

            MyGuiManager.DrawString(MyFontEnum.BuildInfo, pluginWithVersion, textLeftBottomPosition, 1, new Color(MyGuiConstants.LABEL_TEXT_COLOR * m_transitionAlpha, 1), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM);

            size = MyGuiManager.MeasureString(MyFontEnum.BuildInfo, pluginWithVersion, 1);

            textLeftBottomPosition.X += size.X;

            if (MySettings.Static.Settings.Enable)
            {
                MyGuiManager.DrawString(MyFontEnum.BuildInfoHighlight, PLUGIN_ENABLED, textLeftBottomPosition, 1, new Color(MyGuiConstants.LABEL_TEXT_COLOR * m_transitionAlpha, 1), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM);
            }
            else
            {
                MyGuiManager.DrawString(MyFontEnum.BuildInfoHighlight, PLUGIN_DISABLED, textLeftBottomPosition, 1, new Color(MyGuiConstants.LABEL_TEXT_COLOR * m_transitionAlpha, 1), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM);
            }
        }
    }
}
