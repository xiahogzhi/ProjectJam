namespace Game.Tables
{
    /// <summary>
    /// Azcel 运行时引导（自动应用 TableLoader）
    /// </summary>
    public static class TableRegistry
    {
        public static void Apply(Azcel.AzcelSystem system)
        {
            if (system == null)
                return;

            if (system.TableLoader == null)
                system.SetTableLoader(Azcel.BinaryConfigTableLoader.Instance);

            system.RegisterTable(new LevelConfigTable());
        }
    }
}
