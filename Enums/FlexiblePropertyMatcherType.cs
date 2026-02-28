namespace KinoshitaProductions.KnowledgePatch.Enums;
[Flags]
public enum FlexiblePropertyMatcherTypes
{
    Literal = 1,
    Contains = 2,
    Prefix = 3,
    Suffix = 4,
    Regex = 5,
    NegatedMatch = 32,
    AllListItemsMatchAllValues = 64,  // when iterating e.g., headers or annotations, this requires matching all for truthy result
    EachItemMatchesAllValues = 128,
    EachValueMatchesAtLeastOneListItem = 256,
}
