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

        public void Update(double Gametime)
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
                ManagerNetwork.SendInput(keys);
            }
        }
    }
}
