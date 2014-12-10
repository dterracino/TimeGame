﻿using System;
using System.Collections.Generic;
using GLX;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace TimeGame
{
    public class Player : Sprite
    {
        public bool alive;
        public List<Bullet> bullets;
        TimeSpan timeBetweenShots;
        bool canFire;

        GraphicsDeviceManager graphics;

        int shotDelay = 120;

        List<Line> debugLines;

        KeyboardState previousKeyboardState;
        GamePadState previousGamePadState;
        MouseState previousMouseState;

        public AudioListener audioListener;
        public Sound gunShotSound;

        public enum ControlScheme
        {
            KeyboardMouse,
            GamePad
        }

        public ControlScheme controlScheme;

        public Player(Texture2D loadedTex, GraphicsDeviceManager graphics) : base(loadedTex)
        {
            this.graphics = graphics;
            debugLines = new List<Line>();

            alive = true;
            bullets = new List<Bullet>();
            timeBetweenShots = TimeSpan.FromMilliseconds(shotDelay);
            canFire = true;

            controlScheme = ControlScheme.GamePad;

            previousKeyboardState = Keyboard.GetState();
            previousGamePadState = GamePad.GetState(PlayerIndex.One);
            previousMouseState = Mouse.GetState();
            audioListener = new AudioListener();
        }

        public void Fire()
        {
            if (canFire)
            {
                Bullet bullet = new Bullet(graphics);
                bullet.speed = 100;
                bullet.Fire(pos, rotation);
                bullets.Add(bullet);
                bool set = GamePad.SetVibration(PlayerIndex.One, 1, 1);
                gunShotSound.Play();
                canFire = false;
            }
        }

        public void Control(GameTimeWrapper gameTime, Camera camera)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular);

            Vector2 currentPos = pos;
            Vector2 futurePos = pos;

            if (gamePadState.IsConnected)
            {
                controlScheme = ControlScheme.GamePad;
            }
            else
            {
                controlScheme = ControlScheme.KeyboardMouse;
            }

            if (controlScheme == ControlScheme.KeyboardMouse)
            {
                if (keyboardState.IsKeyDown(Keys.W))
                {
                    futurePos.Y -= 5.0f * (float)gameTime.GameSpeed;
                }
                if (keyboardState.IsKeyDown(Keys.S))
                {
                    futurePos.Y += 5.0f * (float)gameTime.GameSpeed;
                }
                if (keyboardState.IsKeyDown(Keys.A))
                {
                    futurePos.X -= 5.0f * (float)gameTime.GameSpeed;
                }
                if (keyboardState.IsKeyDown(Keys.D))
                {
                    futurePos.X += 5.0f * (float)gameTime.GameSpeed;
                }
                Aim(mouseState, camera);
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    Fire();
                }
            }
            else if (controlScheme == ControlScheme.GamePad)
            {
                futurePos.X += gamePadState.ThumbSticks.Left.X * (5.0f * (float)gameTime.GameSpeed);
                futurePos.Y -= gamePadState.ThumbSticks.Left.Y * (5.0f * (float)gameTime.GameSpeed);
                if (gamePadState.Triggers.Right > 0.5f)
                {
                    Fire();
                }
            }
            pos = futurePos;
            Aim(gamePadState, ThumbStick.Right);

            bool intersection = false;
            debugLines.Clear();
        }

        public void Update(GameTimeWrapper gameTime, GraphicsDeviceManager graphics, Camera camera)
        {
            if (!canFire)
            {
                timeBetweenShots -= gameTime.ElapsedGameTime;
                if (timeBetweenShots <= TimeSpan.Zero)
                {
                    canFire = true;
                    timeBetweenShots = TimeSpan.FromMilliseconds(shotDelay);
                }
            }
            foreach (Bullet bullet in bullets)
            {
                if (bullet.visible)
                {
                    if (!camera.viewport.Bounds.Contains(bullet.point1) &&
                        !camera.viewport.Bounds.Contains(bullet.point2))
                    {
                        bullet.visible = false;
                        break;
                    }
                    bullet.Update(gameTime);
                }
            }
            audioListener.Position = pos.ToVector3();
            base.Update(gameTime, graphics);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach (Bullet bullet in bullets)
            {
                if (bullet.visible)
                {
                    bullet.Draw(spriteBatch);
                }
            }
            foreach (Line line in debugLines)
            {
                line.Draw(spriteBatch);
            }
            base.Draw(spriteBatch);
        }
    }
}
