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
                        ManagerNetwork.Player.yPosition++;
                        break;
                    case Keys.Up:
                        ManagerNetwork.Player.yPosition--;
                        break;
                    case Keys.Left:
                        ManagerNetwork.Player.xPosition--;
                        break;
                    case Keys.Right:
                        ManagerNetwork.Player.xPosition++;
                        break;
                }
                ManagerNetwork.SendInput(keys);
            }
        }
    }
}
