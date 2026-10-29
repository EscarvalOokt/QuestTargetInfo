namespace QuestTargetInfo.Info.ViewModels
{
    internal sealed class WorldTargetInfoLineModel(string text, string label = null, string value = null)
    {
        public string Text { get; } = text;

        public string Label { get; } = label;

        public string Value { get; } = value;

        public bool HasText => !string.IsNullOrEmpty(Text);

        public bool HasLabelValue => !string.IsNullOrEmpty(Label) || !string.IsNullOrEmpty(Value);
    }
}