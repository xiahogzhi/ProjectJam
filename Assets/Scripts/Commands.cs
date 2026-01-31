using Azathrix.Framework.Core;
using Framework.Commands.Attributes;

namespace DefaultNamespace
{
    public static class Commands
    {

        [Command("level.goto")]
        public static void GotoLevel(int id)
        {
            AzathrixFramework.GetSystem<GamePlaySystem>().GotoLevel(id);
        }
        
    }
}