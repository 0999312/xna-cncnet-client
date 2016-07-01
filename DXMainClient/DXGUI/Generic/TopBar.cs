﻿using Rampastring.XNAUI.XNAControls;
using System;
using System.Collections.Generic;
using Rampastring.XNAUI;
using Microsoft.Xna.Framework;
using Rampastring.XNAUI.Input;
using Microsoft.Xna.Framework.Input;
using DTAClient.Online;

namespace DTAClient.DXGUI.Generic
{
    /// <summary>
    /// A top bar that allows switching between various client windows.
    /// </summary>
    public class TopBar : XNAPanel
    {
        /// <summary>
        /// The number of seconds that the top bar will stay down after it has
        /// lost input focus.
        /// </summary>
        const double DOWN_TIME_WAIT_SECONDS = 1.0;
        const double EVENT_DOWN_TIME_WAIT_SECONDS = 2.0;
        const double STARTUP_DOWN_TIME_WAIT_SECONDS = 3.5;

        const double DOWN_MOVEMENT_RATE = 2.0;
        const double UP_MOVEMENT_RATE = 2.0;
        const int APPEAR_CURSOR_THRESHOLD_Y = 15;

        public TopBar(WindowManager windowManager, CnCNetManager connectionManager) : base(windowManager)
        {
            downTimeWaitTime = TimeSpan.FromSeconds(DOWN_TIME_WAIT_SECONDS);
            this.connectionManager = connectionManager;
        }

        List<ISwitchable> primarySwitches = new List<ISwitchable>();
        ISwitchable cncnetLobbySwitch;
        ISwitchable privateMessageSwitch;

        XNAButton btnMainButton;
        XNAButton btnCnCNetLobby;
        XNAButton btnPrivateMessages;
        XNAButton btnLogout;
        XNALabel lblTime;
        XNALabel lblDate;
        XNALabel lblConnectionStatus;

        CnCNetManager connectionManager;

        TimeSpan downTime = TimeSpan.FromSeconds(DOWN_TIME_WAIT_SECONDS - STARTUP_DOWN_TIME_WAIT_SECONDS);

        TimeSpan downTimeWaitTime;

        bool isDown = true;

        double locationY = -40.0;

        public void AddPrimarySwitchable(ISwitchable switchable)
        {
            primarySwitches.Add(switchable);
            btnMainButton.Text = switchable.GetSwitchName() + " (F2)";
        }

        public void RemovePrimarySwitchable(ISwitchable switchable)
        {
            primarySwitches.Remove(switchable);
            btnMainButton.Text = primarySwitches[primarySwitches.Count - 1].GetSwitchName() + " (F2)";
        }

        public void SetSecondarySwitch(ISwitchable switchable)
        {
            cncnetLobbySwitch = switchable;
        }

        public void SetTertiarySwitch(ISwitchable switchable)
        {
            privateMessageSwitch = switchable;
        }

        public override void Initialize()
        {
            Name = "TopBar";
            ClientRectangle = new Rectangle(0, -40, WindowManager.RenderResolutionX, 40);
            DrawMode = PanelBackgroundImageDrawMode.STRETCHED;
            BackgroundTexture = AssetLoader.CreateTexture(Color.Black, 1, 1);
            DrawBorders = false;

            btnMainButton = new XNAButton(WindowManager);
            btnMainButton.Name = "btnMainButton";
            btnMainButton.ClientRectangle = new Rectangle(12, 12, 160, 23);
            btnMainButton.FontIndex = 1;
            btnMainButton.Text = "Main Menu (F2)";
            btnMainButton.IdleTexture = AssetLoader.LoadTexture("160pxbtn.png");
            btnMainButton.HoverTexture = AssetLoader.LoadTexture("160pxbtn_c.png");
            btnMainButton.HoverSoundEffect = AssetLoader.LoadSound("button.wav");
            btnMainButton.LeftClick += BtnMainButton_LeftClick;

            btnCnCNetLobby = new XNAButton(WindowManager);
            btnCnCNetLobby.Name = "btnCnCNetLobby";
            btnCnCNetLobby.ClientRectangle = new Rectangle(184, 12, 160, 23);
            btnCnCNetLobby.FontIndex = 1;
            btnCnCNetLobby.Text = "CnCNet Lobby (F3)";
            btnCnCNetLobby.IdleTexture = AssetLoader.LoadTexture("160pxbtn.png");
            btnCnCNetLobby.HoverTexture = AssetLoader.LoadTexture("160pxbtn_c.png");
            btnCnCNetLobby.HoverSoundEffect = AssetLoader.LoadSound("button.wav");
            btnCnCNetLobby.LeftClick += BtnCnCNetLobby_LeftClick;

            btnPrivateMessages = new XNAButton(WindowManager);
            btnPrivateMessages.Name = "btnPrivateMessages";
            btnPrivateMessages.ClientRectangle = new Rectangle(356, 12, 160, 23);
            btnPrivateMessages.FontIndex = 1;
            btnPrivateMessages.Text = "Private Messages (F4)";
            btnPrivateMessages.IdleTexture = AssetLoader.LoadTexture("160pxbtn.png");
            btnPrivateMessages.HoverTexture = AssetLoader.LoadTexture("160pxbtn_c.png");
            btnPrivateMessages.HoverSoundEffect = AssetLoader.LoadSound("button.wav");
            btnPrivateMessages.LeftClick += BtnPrivateMessages_LeftClick;

            lblDate = new XNALabel(WindowManager);
            lblDate.Name = "lblDate";
            lblDate.FontIndex = 1;
            lblDate.Text = DateTime.Now.ToShortDateString();
            lblDate.ClientRectangle = new Rectangle(ClientRectangle.Width -
                (int)Renderer.GetTextDimensions(lblDate.Text, lblDate.FontIndex).X - 12, 20, 
                lblDate.ClientRectangle.Width, lblDate.ClientRectangle.Height);

            lblTime = new XNALabel(WindowManager);
            lblTime.Name = "lblTime";
            lblTime.FontIndex = 1;
            lblTime.Text = "99:99:99";
            lblTime.ClientRectangle = new Rectangle(ClientRectangle.Width -
                (int)Renderer.GetTextDimensions(lblTime.Text, lblTime.FontIndex).X - 12, 6,
                lblTime.ClientRectangle.Width, lblTime.ClientRectangle.Height);

            btnLogout = new XNAButton(WindowManager);
            btnLogout.Name = "btnLogout";
            btnLogout.ClientRectangle = new Rectangle(lblDate.ClientRectangle.Left - 87, 12, 75, 23);
            btnLogout.FontIndex = 1;
            btnLogout.Text = "Log Out";
            btnLogout.IdleTexture = AssetLoader.LoadTexture("75pxbtn.png");
            btnLogout.HoverTexture = AssetLoader.LoadTexture("75pxbtn_c.png");
            btnLogout.HoverSoundEffect = AssetLoader.LoadSound("button.wav");
            btnLogout.AllowClick = false;
            btnLogout.LeftClick += BtnLogout_LeftClick;

            lblConnectionStatus = new XNALabel(WindowManager);
            lblConnectionStatus.Name = "lblConnectionStatus";
            lblConnectionStatus.FontIndex = 1;
            lblConnectionStatus.Text = "OFFLINE";

            AddChild(btnMainButton);
            AddChild(btnCnCNetLobby);
            AddChild(btnPrivateMessages);
            AddChild(lblTime);
            AddChild(lblDate);
            AddChild(btnLogout);
            AddChild(lblConnectionStatus);

            lblConnectionStatus.CenterOnParent();

            base.Initialize();

            Keyboard.OnKeyPressed += Keyboard_OnKeyPressed;
            connectionManager.Connected += ConnectionManager_Connected;
            connectionManager.Disconnected += ConnectionManager_Disconnected;
            connectionManager.ConnectionLost += ConnectionManager_ConnectionLost;
            connectionManager.WelcomeMessageReceived += ConnectionManager_WelcomeMessageReceived;
            connectionManager.AttemptedServerChanged += ConnectionManager_AttemptedServerChanged;
            connectionManager.ConnectAttemptFailed += ConnectionManager_ConnectAttemptFailed;
        }

        private void ConnectionManager_ConnectionLost(object sender, Online.EventArguments.ConnectionLostEventArgs e)
        {
            ConnectionEvent("OFFLINE");
        }

        private void ConnectionManager_ConnectAttemptFailed(object sender, EventArgs e)
        {
            ConnectionEvent("OFFLINE");
        }

        private void ConnectionManager_AttemptedServerChanged(object sender, Online.EventArguments.AttemptedServerEventArgs e)
        {
            ConnectionEvent("CONNECTING...");
            BringDown();
        }

        private void ConnectionManager_WelcomeMessageReceived(object sender, Online.EventArguments.ServerMessageEventArgs e)
        {
            ConnectionEvent("CONNECTED");
        }

        private void ConnectionManager_Disconnected(object sender, EventArgs e)
        {
            btnLogout.AllowClick = false;
            ConnectionEvent("OFFLINE");
        }

        private void ConnectionEvent(string text)
        {
            lblConnectionStatus.Text = text;
            lblConnectionStatus.CenterOnParent();
            isDown = true;
            downTime = TimeSpan.FromSeconds(DOWN_TIME_WAIT_SECONDS - EVENT_DOWN_TIME_WAIT_SECONDS);
        }

        private void BtnLogout_LeftClick(object sender, EventArgs e)
        {
            connectionManager.Disconnect();
            SwitchToPrimary();
        }

        private void ConnectionManager_Connected(object sender, EventArgs e)
        {
            btnLogout.AllowClick = true;
        }

        public void SwitchToPrimary()
        {
            BtnMainButton_LeftClick(this, EventArgs.Empty);
        }

        public void SwitchToSecondary()
        {
            BtnCnCNetLobby_LeftClick(this, EventArgs.Empty);
        }

        private void BtnCnCNetLobby_LeftClick(object sender, EventArgs e)
        {
            primarySwitches[primarySwitches.Count - 1].SwitchOff();
            cncnetLobbySwitch.SwitchOn();
            privateMessageSwitch.SwitchOff();
        }

        private void BtnMainButton_LeftClick(object sender, EventArgs e)
        {
            cncnetLobbySwitch.SwitchOff();
            privateMessageSwitch.SwitchOff();
            primarySwitches[primarySwitches.Count - 1].SwitchOn();
        }

        private void BtnPrivateMessages_LeftClick(object sender, EventArgs e)
        {
            privateMessageSwitch.SwitchOn();
        }

        private void Keyboard_OnKeyPressed(object sender, KeyPressEventArgs e)
        {
            if (!Enabled || !WindowManager.HasFocus)
                return;

            if (e.PressedKey == Keys.F1)
            {
                BringDown();
            }
            else if (e.PressedKey == Keys.F2)
            {
                BtnMainButton_LeftClick(this, EventArgs.Empty);
            }
            else if (e.PressedKey == Keys.F3)
            {
                BtnCnCNetLobby_LeftClick(this, EventArgs.Empty);
            }
            else if (e.PressedKey == Keys.F4)
            {
                BtnPrivateMessages_LeftClick(this, EventArgs.Empty);
            }
        }

        public override void OnMouseOnControl(MouseEventArgs eventArgs)
        {
            if (Cursor.Location.Y > -1)
                BringDown();

            base.OnMouseOnControl(eventArgs);
        }

        void BringDown()
        {
            isDown = true;
            downTime = TimeSpan.Zero;
        }

        public void SetMainButtonText(string text)
        {
            btnMainButton.Text = text;
        }

        public override void Update(GameTime gameTime)
        {
            if (Cursor.Location.Y < APPEAR_CURSOR_THRESHOLD_Y && Cursor.Location.Y > -1)
            {
                BringDown();
            }

            if (isDown)
            {
                if (locationY < 0)
                {
                    locationY += DOWN_MOVEMENT_RATE;
                    ClientRectangle = new Rectangle(ClientRectangle.X, (int)locationY, 
                        ClientRectangle.Width, ClientRectangle.Height);
                }

                downTime += gameTime.ElapsedGameTime;

                isDown = downTime < downTimeWaitTime;
            }
            else
            {
                if (locationY > -ClientRectangle.Height - 1)
                {
                    locationY -= UP_MOVEMENT_RATE;
                    ClientRectangle = new Rectangle(ClientRectangle.X, (int)locationY,
                        ClientRectangle.Width, ClientRectangle.Height);
                }
                else
                    return; // Don't handle input when the cursor is above our game window
            }

            DateTime dtn = DateTime.Now;

            lblTime.Text = dtn.ToLongTimeString();
            if (lblDate.Text != dtn.ToShortDateString())
                lblDate.Text = dtn.ToShortDateString();

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Renderer.DrawRectangle(new Rectangle(ClientRectangle.X, ClientRectangle.Bottom, ClientRectangle.Width, 1), Color.Gray);
        }
    }
}