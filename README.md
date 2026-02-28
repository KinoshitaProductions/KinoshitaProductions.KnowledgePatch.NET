# KinoshitaProductions.KnowledgePatch.NET

This is our API request and response patching library.

It takes an HTTP request message and, based in a set of rules and conditions, it will add headers, replace parts of the request, or handle/detect errors to take a variety of actions (e.g. show a human readable message or instructions for the user).

It can also patch responses if needed to correct or adjust the data contained from them.

The current implementation is pretty simplistic, but flexible enough for the known use cases, but it'd be nice to streamline further in the future for a v2.
