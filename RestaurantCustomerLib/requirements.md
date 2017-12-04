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

Add listener for SetAvailable
	Changes isAvailable value for the sent dishes
	# The following would be relevant if the customer could add dishes to an order before placing it
	#If an effected dish is in order, ask the user if s/he wish to remove it from the order
