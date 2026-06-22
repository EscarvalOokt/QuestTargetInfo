namespace QuestTargetInfo
{
    internal sealed class WorldTargetInfoLineModel
    {
        public WorldTargetInfoLineModel(
            string text,
            string label = null,
            string value = null)
        {
            Text = text;
            Label = label;
            Value = value;
        }

        public string Text { get; }

        public string Label { get; }

        public string Value { get; }

        public bool HasText => !string.IsNullOrEmpty(Text);

        public bool HasLabelValue =>
            !string.IsNullOrEmpty(Label) || !string.IsNullOrEmpty(Value);
    }
}