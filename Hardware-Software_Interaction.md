## Communication Protocol

$header:cmd#

Header: code of asyncronous minigame or syncronous control

cmd: function x y z ... 

## Timer

| Command            | Code            | Values                                             |
|--------------------|-----------------|----------------------------------------------------|
| Set remaining time | $TMR:set x#     | x: Time in seconds to set timer to                 |
| Flash timer        | $TMR:flash x y# | x: Number of times to flash y: Delay of flash (ms) |
| Set timer on/off   | $TMR:on x#      | x: 1 = on, 0 = off                                 |

## Asyncronous Minigames

Asyncronous minigames' logic will be onboard. Status of the minigame can be queries through the following commands. Any changes made to the hardware of the minigame **will not** trigger a status update. Changes of the state of the minigame (not started, in progress, failed, completed) **will** trigger a status update without requiring a request. Reset will return the minigame to its original state, although physical hardware will not be affected. 

| Command        | Code       | Values              | Response    |
|----------------|------------|---------------------|-------------|
| Status Request | $X,status# | X: Code of minigame | $X,status y |
| Set Status     | $X,set y#  | X: Code of minigame | $X,status y |
| Reset          | $X,reset#  | X: Code of minigame | $X,status y |

## Syncronous Minigames

Syncronous minigames will give hardware level access to unity through the following commands. Any changes made to the hardware component will trigger a status response without requiring a request. Reset will do its best to reset the status of any components although physical switches will not be effected.

| Command        | Code       | Values                        | Response    |
|----------------|------------|-------------------------------|-------------|
| Status Request | $X,status# | X: Code of hardware component | $X,status y |
| Reset          | $X,reset#  | X: Code of hardware component | $X,status y |
