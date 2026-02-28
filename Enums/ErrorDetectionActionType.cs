namespace KinoshitaProductions.KnowledgePatch.Enums;
public enum ErrorDetectionActionType
{
    Undefined,
    NoAction = 1,
    RewriteStatusCode = 2,
    RequestAuthentication = 3,
    RequestChallenge = 4,
    PrintRawResponseBodyMessage = 5,
    PrintResponseBodyCustomMessage = 6,
    PrintCustomMessage = 7,
    Annotate = 8,
    RemoveAnnotation = 9,
    //PrintResponseBodyCustomMessage = 10, // not implemented for now
    //PrintCustomMessage = 11,
}