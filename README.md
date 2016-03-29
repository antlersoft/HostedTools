# HostedTools
A framework for creating and using tools that are designed like command-line tools but work in a GUI (optionally)

I create little tools all the time.  For my purposes, a tool is something that takes some parameters and runs some code;
the actual running of the code is non-interactive, although the code can produce different kind of output while running.  Hosted
Tools is a set of libraries that allow you to create these tools in C# (or Python) without much code and run them inside a
GUI that lets you select them from a menu and provides an additional standard set of services that make all your tools more
useful.  (I am aware that in some respects this duplicates the functionality of PowerShell cmd-lets; IMO HostedTools puts more emphasis on GUIness and ease-of-use).  The libraries in this
project provide a WPF GUI for HostedTools, but the tools you write are by default GUI independent; a Web-based tool host is coming that
will allow you to run your tools in a web server and access them through a browser, if that's a good model for your tools.
Other GUIs (MonoTk) are possible and under consideration.
