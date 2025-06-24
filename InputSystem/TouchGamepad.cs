using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace InputSystem
{
    public class TouchGamepad : DrawableGameComponent
    {

        public class TouchButton
        {
            public Rectangle Bounds;
            internal ButtonState PreviousState;
            internal ButtonState State;
            internal string Text;

            public virtual void Draw(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont spriteFont)
            {
                Color drawColor = Color.White;
                if (State == ButtonState.Pressed)
                    drawColor = Color.Red;

                spriteBatch.Draw(pixel, Bounds, drawColor * opacity);

                
                

                spriteBatch.DrawString(
                    spriteFont,
                    Text,
                    new Vector2(
                        Bounds.Left,
                        Bounds.Top
                    ),
                    Color.Yellow,
                    0,
                    Vector2.Zero,
                    4,
                    SpriteEffects.None,
                    0
                );
            }
        }

        public SpriteBatch spriteBatch;
        public SpriteFont spriteFont;

        ContentManager content;

        public Dictionary<Buttons, TouchButton> buttons = new Dictionary<Buttons, TouchButton>();
        public Texture2D pixel;

        Matrix globalTransformation;
        const float opacity = 0.3f;

        public TouchGamepad(Game game)
            : base(game)
        {
            this.content = Game.Content;
        }

        #region Life Cycle

        protected override void LoadContent()
        {
            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new Color[] { Color.White });
            spriteBatch = new SpriteBatch(GraphicsDevice);            

            spriteFont = content.Load<SpriteFont>("Fonts\\Junicode");

            SetupButtons();
        }

        protected override void UnloadContent()
        {
            content.Unload();
        }

        void SetupButtons()
        {
            var viewport = GraphicsDevice.Viewport;
            int screenWidth = viewport.Width;
            int screenHeight = viewport.Height;

            int buttonSize = (int)(screenHeight * 0.16f);
            int spacing = (int)(screenHeight * 0.02f);

            int dpadX = (int)(screenWidth * 0.15f);
            int dpadY = (int)(screenHeight * 0.80f);

            AddButton(Buttons.DPadUp, dpadX, dpadY - buttonSize - spacing, buttonSize, buttonSize, "^");
            AddButton(Buttons.DPadDown, dpadX, dpadY, buttonSize, buttonSize, "v");
            AddButton(Buttons.DPadLeft, dpadX - buttonSize - spacing, dpadY, buttonSize, buttonSize, "<");
            AddButton(Buttons.DPadRight, dpadX + buttonSize + spacing, dpadY, buttonSize, buttonSize, ">");

            int actionX = (int)(screenWidth * 0.85f);
            int actionY = (int)(screenHeight * 0.80f);

            AddButton(Buttons.Y, actionX, actionY - buttonSize - spacing, buttonSize, buttonSize, "Y");
            AddButton(Buttons.X, actionX - buttonSize - spacing, actionY, buttonSize, buttonSize, "X");
            AddButton(Buttons.A, actionX, actionY, buttonSize, buttonSize, "A");
            AddButton(Buttons.B, actionX + buttonSize + spacing, actionY, buttonSize, buttonSize, "B");

            int menuButtonWidth = (int)(screenWidth * 0.18f);
            int menuButtonHeight = (int)(screenHeight * 0.1f);
            int menuButtonY = (int)(screenHeight * 0.02f);
            int menuSpacing = (int)(screenWidth * 0.02f);
            int menuStartX = (screenWidth / 2) - (menuButtonWidth + menuSpacing / 2);

            AddButton(Buttons.Back, menuStartX, menuButtonY, menuButtonWidth, menuButtonHeight, "Back");
            AddButton(Buttons.Start, menuStartX + menuButtonWidth + menuSpacing, menuButtonY, menuButtonWidth, menuButtonHeight, "Start");
        }

        public override void Update(GameTime gameTime)
        {
            TouchCollection touches = TouchPanel.GetState();

            foreach (TouchButton button in buttons.Values)
            {
                button.PreviousState = button.State;
                button.State = ButtonState.Released;

                foreach (TouchLocation touch in touches)
                {
                    Vector2 transformedPosition = TransformTouchPosition(touch.Position);

                    if (button.Bounds.Contains(transformedPosition))
                    {
                        button.State = ButtonState.Pressed;
                        break;
                    }
                }
            }
        }

        public Vector2 TransformTouchPosition(Vector2 touchPosition)
        {
            var screenScaleX = (float)GraphicsDevice.Viewport.Width / this.Game.Window.ClientBounds.Width;
            var screenScaleY = (float)GraphicsDevice.Viewport.Height / this.Game.Window.ClientBounds.Height;
            globalTransformation = Matrix.CreateScale(screenScaleX, screenScaleY, 1.0f);
            
            return Vector2.Transform(touchPosition, Matrix.Invert(globalTransformation));
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            foreach (TouchButton button in buttons.Values)
            {
                button.Draw(spriteBatch, pixel, spriteFont);
            }

            spriteBatch.End();
        }

        #endregion

        #region Getters

        public bool IsDown(Buttons action)
        {
            if (!IsValidButton(action))
                return false;

            return buttons[action].State == ButtonState.Pressed;
        }

        public bool IsUp(Buttons action)
        {
            if (!IsValidButton(action))
                return false;

            return buttons[action].State == ButtonState.Released;
        }

        public bool WasDown(Buttons action)
        {
            if (!IsValidButton(action))
                return false;

            return buttons[action].PreviousState == ButtonState.Pressed;
        }

        public bool WasUp(Buttons action)
        {
            if (!IsValidButton(action))
                return false;

            return buttons[action].PreviousState == ButtonState.Released;
        }

        public bool JustPressed(Buttons action)
        {
            if (!IsValidButton(action))
                return false;

            return buttons[action].State == ButtonState.Pressed &&
                buttons[action].PreviousState == ButtonState.Released;
        }

        public bool JustReleased(Buttons action)
        {
            if (!IsValidButton(action))
                return false;

            return buttons[action].State == ButtonState.Released &&
                buttons[action].PreviousState == ButtonState.Pressed;
        }

        #endregion

        #region Setters

        public void AddButton(Buttons action, int x, int y, int width, int height, string text = "")
        {
            buttons.Add(action, new TouchButton
            {
                Bounds = new Rectangle(x, y, width, height),
                Text = text
            });
        }

        public void RemoveButton(Buttons action)
        {
            if (!IsValidButton(action))
                throw new Exception("Attempting to remove button not registered by the touch gamepad!");

            buttons.Remove(action);
        }

        #endregion

        #region Utilities

        bool IsValidButton(Buttons button)
        {
            return buttons.ContainsKey(button);
        }

        #endregion
    }
}
