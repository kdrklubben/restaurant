Customer client should do the following
* On start, provide a unique identifier
* Let user view the menu
* Let user place an order
	* send that order to the server
* Recieve notification when user's order is complete
	* client must listen for notifications


Execution plan:
Build the interfaces and methods for the above
Let the console manage input and map to data objects
Let SocketClient manage IO to the server
Let a repository act as "controller" to the console
	Reverse-engineer an interface for this repository in order to mock in unit tests?

Return feedback to user upon entering an order
Format return upon dish completion into sensible output