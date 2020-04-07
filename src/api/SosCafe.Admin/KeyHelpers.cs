namespace SosCafe
{
    public static class KeyHelpers
    {
        public static string CleanStringForPartitionKey(this string key)
        {
            return key
                .Replace('/', '|')
                .Replace('\\', '|')
                .Replace('#', '|')
                .Replace('_', '|');
        }
    }
}
