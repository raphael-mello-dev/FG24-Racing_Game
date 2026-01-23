Milo is responsible for the contents of this folder. Ask her if you have questions or issues.

PROGRAMMING NOTES FOR USING THESE

FOR TRACK
	Make sure to Instantiate the CheckpointNode prefabs and put their transforms in an array in the order that the player would cross them. The last checkpoint would need to have the IsLapFlag turned true. 
	
FOR PLAYER / UI
	To get lap and position/placement info for the race then call CheckpointManager.GetRacerInfo(transform) to get a Racer info object that you can then access info from.
	A "respawn" function can easily be made by getting the racer info then calling Racer.GetCheckpoint() and teleporting the player to its position.