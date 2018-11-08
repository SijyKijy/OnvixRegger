using System;

namespace OnvixRegger.Engine
{
    [Serializable]
    class SaveData
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }
}
