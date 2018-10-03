using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

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
            if (state.IsKeyDown(keys))
            {
                switch (keys)
                {
                    case Keys.Down:
                        ManagerNetwork.Player._position.Y++;
                        break;
                    case Keys.Up:
                        ManagerNetwork.Player._position.Y--;
                        break;
                    case Keys.Left:
                        ManagerNetwork.Player._position.X--;
                        break;
                    case Keys.Right:
                        ManagerNetwork.Player._position.X++;
                        break;
                }
                ManagerNetwork.SendInput(keys);
            }
        }
    }
}
