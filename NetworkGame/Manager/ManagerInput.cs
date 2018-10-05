using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using NetworkGameLibrary;

namespace NetworkGame.Manager
{
    class ManagerInput
    {
        private ManagerNetwork ManagerNetwork;
        public ManagerInput(ManagerNetwork manager)
        {
            ManagerNetwork = manager;
        }

        public void Update()
        {
            var state = Keyboard.GetState();
            CheckKeyState(Keys.Down, state);
            CheckKeyState(Keys.Up, state);
            CheckKeyState(Keys.Left, state);
            CheckKeyState(Keys.Right, state);

        }

        private void CheckKeyState(Keys keys, KeyboardState state)
        {
            bool collided = false;
            if (state.IsKeyDown(keys))
            {
                ManagerNetwork.SendInput(keys);
                switch (keys)
                {
                    case Keys.Down:

                        foreach (var item in ManagerNetwork.OtherPlayers)
                        {
                            if (ManagerNetwork.Player != item)
                            {
                                if (IsTouchingBottom(ManagerNetwork.Player, item) || IsTouchingTop(ManagerNetwork.Player, item))
                                {
                                    ManagerNetwork.Player._position.Y = 0f;
                                    collided = true;
                                    continue;
                                }
                            }
                        }
                        if (!collided)
                        {
                            ManagerNetwork.Player.yPosition++;
                        }
                        break;
                    case Keys.Up:
                        foreach (var item in ManagerNetwork.OtherPlayers)
                        {
                            if (ManagerNetwork.Player != item)
                            {
                                if (IsTouchingBottom(ManagerNetwork.Player, item) || IsTouchingTop(ManagerNetwork.Player, item))
                                {
                                    ManagerNetwork.Player._position.Y = 0f;
                                    collided = true;
                                    continue;
                                }
                            }
                        }
                        if (!collided)
                        {
                            ManagerNetwork.Player._position.Y--;
                        }

                        break;
                    case Keys.Left:
                        foreach (var item in ManagerNetwork.OtherPlayers)
                        {
                            if (ManagerNetwork.Player != item)
                            {
                                if (IsTouchingRight(ManagerNetwork.Player, item) || IsTouchingLeft(ManagerNetwork.Player, item))
                                {
                                    ManagerNetwork.Player._position.X = 0f;
                                    collided = true;
                                    continue;
                                }
                            }
                        }
                        if (!collided)
                        {
                            ManagerNetwork.Player._position.X--;
                        }
                        break;
                    case Keys.Right:
                        foreach (var item in ManagerNetwork.OtherPlayers)
                        {
                            if (ManagerNetwork.Player != item)
                            {
                                if (IsTouchingLeft(ManagerNetwork.Player, item) || IsTouchingRight(ManagerNetwork.Player, item))
                                {
                                    ManagerNetwork.Player._position.X = 0f;
                                    collided = true;
                                    continue;
                                }
                            }
                        }
                        if (!collided)
                        {
                            ManagerNetwork.Player._position.X++;
                        }
                        break;
                }

            }
        }

        protected bool IsTouchingLeft(Player player1, Player player2)
        {
            return player1.BoundingBox.Right + 1 > player2.BoundingBox.Left &&
                   player1.BoundingBox.Left < player2.BoundingBox.Left &&
                   player1.BoundingBox.Bottom > player2.BoundingBox.Top &&
                   player1.BoundingBox.Top < player2.BoundingBox.Bottom;
        }

        protected bool IsTouchingRight(Player player1, Player player2)
        {
            return player1.BoundingBox.Left - 1 < player2.BoundingBox.Right &&
                   player1.BoundingBox.Right > player2.BoundingBox.Right &&
                   player1.BoundingBox.Bottom > player2.BoundingBox.Top &&
                   player1.BoundingBox.Top < player2.BoundingBox.Bottom;
        }

        protected bool IsTouchingTop(Player player1, Player player2)
        {
            return player1.BoundingBox.Bottom + 1 > player2.BoundingBox.Top &&
                   player1.BoundingBox.Top < player2.BoundingBox.Top &&
                   player1.BoundingBox.Right > player2.BoundingBox.Left &&
                   player1.BoundingBox.Left < player2.BoundingBox.Right;
        }

        protected bool IsTouchingBottom(Player player1, Player player2)
        {
            return player1.BoundingBox.Top - 1 < player2.BoundingBox.Bottom &&
                   player1.BoundingBox.Bottom > player2.BoundingBox.Bottom &&
                   player1.BoundingBox.Right > player2.BoundingBox.Left &&
                   player1.BoundingBox.Left < player2.BoundingBox.Right;
        }
    }
}
