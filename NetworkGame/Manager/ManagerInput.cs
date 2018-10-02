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

        public void getInput()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                ManagerNetwork.Player.yPosition++;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                ManagerNetwork.Player.yPosition--;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                ManagerNetwork.Player.xPosition++;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                ManagerNetwork.Player.xPosition--;
            }
        }

        private void CheckKeyState(Keys keys, KeyboardState state)
        {
            if (state.IsKeyDown(keys))
            {
                ManagerNetwork.SendInput(keys);
            }
        }
    }
}
